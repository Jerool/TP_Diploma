using Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL del USUARIO del sistema (seguridad/login). Además del CRUD, maneja lo relacionado a la
    /// autenticación: bloqueo de cuenta, conteo de intentos fallidos, obligación de cambiar contraseña
    /// e idioma preferido. La contraseña viaja ya cifrada desde la BLL; el email se cifra/descifra acá.
    /// Tras cada escritura recalcula el dígito verificador de la tabla Usuario.
    /// </summary>
    public class DALUsuario_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALIntegridad_GO44 _dalIntegridad;

        // SELECT reutilizado por todas las lecturas: une Usuario con Roles para traer el nombre del rol.
        private const string SELECT_BASE =
            "SELECT U.DNI, U.Apellido, U.Nombre, U.UserName, U.Contrasena, " +
            "       U.IdRol, R.Nombre AS RolNombre, " +
            "       U.Email, U.Bloqueo, U.Activo, " +
            "       U.IntentosFallidos, U.UltimoIntentoFallido, " +
            "       U.DebeCambiarContrasena, U.Idioma " +
            "FROM Usuario U " +
            "INNER JOIN Roles R ON R.Id = U.IdRol";

        public DALUsuario_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalIntegridad = new DALIntegridad_GO44();
        }

        /// <summary>Recalcula el dígito verificador de la tabla Usuario tras una escritura (best-effort).</summary>
        private void RecalcularIntegridadUsuario()
        {
            if (DALIntegridad_GO44.IntegridadConocidamenteRota) return;
            try { _dalIntegridad.RecalcularTabla("Usuario"); } catch { }
        }

        /// <summary>Indica si ya existe un usuario con ese DNI (COUNT &gt; 0).</summary>
        public bool ExisteDNI(string dni)
        {
            string query = "SELECT COUNT(1) FROM Usuario WHERE DNI = @DNI";
            SqlParameter[] parametros = { new SqlParameter("@DNI", dni) };
            object resultado = _acceso.leerEscalar(query, parametros);
            return resultado != null && Convert.ToInt32(resultado) > 0;
        }

        /// <summary>Busca un usuario por su nombre de login. Devuelve null si no existe. Base del proceso de autenticación.</summary>
        public Usuario_GO44 BuscarPorLogin(string login)
        {
            string query = SELECT_BASE + " WHERE U.UserName = @UserName";

            SqlParameter[] parametros = { new SqlParameter("@UserName", login) };

            DataTable dt = _acceso.leer(query, parametros);

            if (dt.Rows.Count == 0)
                return null;

            return MapearFila(dt.Rows[0]);
        }

        /// <summary>Lista solo los usuarios activos.</summary>
        public List<Usuario_GO44> ListarActivos()
        {
            string query = SELECT_BASE + " WHERE U.Activo = 1";
            return MapearLista(_acceso.leer(query, null));
        }

        /// <summary>Lista todos los usuarios, incluidos los inactivos (para administración).</summary>
        public List<Usuario_GO44> ListarTodos()
        {
            return MapearLista(_acceso.leer(SELECT_BASE, null));
        }

        /// <summary>Bloquea la cuenta (Bloqueo = 1). Se dispara al superar el máximo de intentos fallidos.</summary>
        public void Bloquear(string login)
        {
            string query = "UPDATE Usuario SET Bloqueo = 1 WHERE UserName = @UserName";
            SqlParameter[] parametros = {
                new SqlParameter("@UserName", login)
            };
            _acceso.escribir(query, parametros);
            RecalcularIntegridadUsuario();
        }

        /// <summary>
        /// Desbloquea una cuenta: la habilita, le pone una contraseña nueva (ya cifrada), resetea los
        /// intentos fallidos y la marca para que el usuario deba cambiar la clave en el próximo login.
        /// </summary>
        public void Desbloquear(string dni, string contrasenaCifrada)
        {
            string query =
                "UPDATE Usuario " +
                "SET Bloqueo = 0, " +
                "    Contrasena = @Contrasena, " +
                "    IntentosFallidos = 0, " +
                "    UltimoIntentoFallido = NULL, " +
                "    DebeCambiarContrasena = 1 " +
                "WHERE DNI = @DNI";
            SqlParameter[] p = {
                new SqlParameter("@Contrasena", contrasenaCifrada),
                new SqlParameter("@DNI", dni)
            };
            _acceso.escribir(query, p);
            RecalcularIntegridadUsuario();
        }

        /// <summary>Activa o desactiva (baja lógica) un usuario.</summary>
        public void ActivarDesactivar(string dni, bool activo)
        {
            string query = "UPDATE Usuario SET Activo = @Activo WHERE DNI = @DNI";
            SqlParameter[] p = {
                new SqlParameter("@Activo", activo),
                new SqlParameter("@DNI",    dni)
            };
            _acceso.escribir(query, p);
            RecalcularIntegridadUsuario();
        }

        /// <summary>Cambia el email del usuario (se guarda cifrado).</summary>
        public void ModificarEmail(string dni, string email)
        {
            string emailCifrado = EncriptadorReversible_GO44.Instancia.Encriptar(email);
            string query = "UPDATE Usuario SET Email = @Email WHERE DNI = @DNI";
            SqlParameter[] p = {
                new SqlParameter("@Email", emailCifrado),
                new SqlParameter("@DNI",   dni)
            };
            _acceso.escribir(query, p);
            RecalcularIntegridadUsuario();
        }

        /// <summary>Cambia el rol (permisos) del usuario.</summary>
        public void ModificarRol(string dni, int idRol)
        {
            string query = "UPDATE Usuario SET IdRol = @IdRol WHERE DNI = @DNI";
            SqlParameter[] p = {
                new SqlParameter("@IdRol", idRol),
                new SqlParameter("@DNI",   dni)
            };
            _acceso.escribir(query, p);
            RecalcularIntegridadUsuario();
        }

        /// <summary>Guarda la nueva contraseña (ya cifrada) y apaga el flag "debe cambiar contraseña".</summary>
        public void CambiarContrasena(string login, string contrasenaCifrada)
        {
            string query =
                "UPDATE Usuario " +
                "SET Contrasena = @Contrasena, " +
                "    DebeCambiarContrasena = 0 " +
                "WHERE UserName = @Login";
            SqlParameter[] p = {
                new SqlParameter("@Contrasena", contrasenaCifrada),
                new SqlParameter("@Login", login)
            };
            _acceso.escribir(query, p);
            RecalcularIntegridadUsuario();
        }

        /// <summary>Guarda el nuevo conteo de intentos fallidos y el momento del último. La BLL lo usa para decidir el bloqueo.</summary>
        public void ActualizarIntentosFallidos(string login, int nuevosIntentos, DateTime momentoIntento)
        {
            string query =
                "UPDATE Usuario " +
                "SET IntentosFallidos = @Intentos, " +
                "    UltimoIntentoFallido = @Momento " +
                "WHERE UserName = @Login";
            SqlParameter[] p = {
                new SqlParameter("@Intentos", nuevosIntentos),
                new SqlParameter("@Momento",  momentoIntento),
                new SqlParameter("@Login",    login)
            };
            _acceso.escribir(query, p);
            RecalcularIntegridadUsuario();
        }

        /// <summary>Guarda el idioma preferido del usuario (es/en) para recordarlo entre sesiones.</summary>
        public void GuardarIdioma(string login, string idioma)
        {
            string query = "UPDATE Usuario SET Idioma = @Idioma WHERE UserName = @Login";
            SqlParameter[] p = {
                new SqlParameter("@Idioma", idioma),
                new SqlParameter("@Login",  login)
            };
            _acceso.escribir(query, p);
            RecalcularIntegridadUsuario();
        }

        /// <summary>Pone en cero los intentos fallidos (se llama tras un login exitoso).</summary>
        public void ResetearIntentosFallidos(string login)
        {
            string query =
                "UPDATE Usuario " +
                "SET IntentosFallidos = 0, " +
                "    UltimoIntentoFallido = NULL " +
                "WHERE UserName = @Login";
            SqlParameter[] p = { new SqlParameter("@Login", login) };
            _acceso.escribir(query, p);
            RecalcularIntegridadUsuario();
        }

        /// <summary>
        /// Da de alta un usuario. Exige rol (si no, lanza excepción). Lo crea habilitado, sin bloqueo,
        /// con 0 intentos fallidos y marcado para que cambie la contraseña en el primer ingreso.
        /// El email se cifra; la contraseña ya viene cifrada desde la BLL.
        /// </summary>
        public int AgregarUsuario(Usuario_GO44 usuario)
        {
            if (usuario.Rol == null)
                throw new Exception(IdiomaManager_GO44.T("err.usuarioSinRol"));
            string query = "INSERT INTO Usuario " +
                  "(DNI, Apellido, Nombre, UserName, Contrasena, IdRol, Email, Bloqueo, Activo, IntentosFallidos, UltimoIntentoFallido, DebeCambiarContrasena) " +
                  "VALUES (@DNI, @Ape, @Nom, @Login, @Clave, @IdRol, @Email, 0, 1, 0, NULL, 1)";
            SqlParameter[] p = {
                new SqlParameter("@DNI",   usuario.DNI),
                new SqlParameter("@Ape",   usuario.Apellido),
                new SqlParameter("@Nom",   usuario.Nombre),
                new SqlParameter("@Login", usuario.Login),
                new SqlParameter("@Clave", usuario.Contrasena),
                new SqlParameter("@IdRol", usuario.Rol.Id),
                new SqlParameter("@Email", EncriptadorReversible_GO44.Instancia.Encriptar(usuario.Email))
            };
            int filas = _acceso.escribir(query, p);
            RecalcularIntegridadUsuario();
            return filas;
        }

        /// <summary>Convierte un DataTable de usuarios en lista de entidades.</summary>
        private List<Usuario_GO44> MapearLista(DataTable dt)
        {
            List<Usuario_GO44> lista = new List<Usuario_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(MapearFila(row));
            }
            return lista;
        }

        /// <summary>
        /// Mapea una fila a entidad Usuario. El email se descifra; el idioma cae a "es" si viene NULL;
        /// UltimoIntentoFallido puede ser NULL. La contraseña se deja tal cual (cifrada) para que la compare la BLL.
        /// </summary>
        private Usuario_GO44 MapearFila(DataRow row)
        {
            return new Usuario_GO44
            {
                DNI = row["DNI"].ToString(),
                Apellido = row["Apellido"].ToString(),
                Nombre = row["Nombre"].ToString(),
                Login = row["UserName"].ToString(),
                Contrasena = row["Contrasena"].ToString(),

                Rol = new Rol_GO44
                {
                    Id = Convert.ToInt32(row["IdRol"]),
                    Nombre = row["RolNombre"].ToString()
                },

                Email = EncriptadorReversible_GO44.Instancia.Desencriptar(row["Email"].ToString()),
                Bloqueo = Convert.ToBoolean(row["Bloqueo"]),
                Activo = Convert.ToBoolean(row["Activo"]),
                IntentosFallidos = Convert.ToInt32(row["IntentosFallidos"]),
                UltimoIntentoFallido = row["UltimoIntentoFallido"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(row["UltimoIntentoFallido"]),
                DebeCambiarContrasena = Convert.ToBoolean(row["DebeCambiarContrasena"]),
                Idioma = row["Idioma"] == DBNull.Value ? "es" : row["Idioma"].ToString()
            };
        }

        /// <summary>Lista los roles disponibles (para asignarlos a un usuario).</summary>
        public List<Rol_GO44> ListarRoles()
        {
            string query = "SELECT Id, Nombre FROM Roles ORDER BY Nombre";
            DataTable dt = _acceso.leer(query, null);

            List<Rol_GO44> lista = new List<Rol_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Rol_GO44
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString()
                });
            }
            return lista;
        }

        /// <summary>Busca un rol por nombre exacto. Devuelve null si no existe.</summary>
        public Rol_GO44 BuscarPorNombre(string nombre)
        {
            string query = "SELECT Id, Nombre FROM Roles WHERE Nombre = @Nombre";
            SqlParameter[] p = { new SqlParameter("@Nombre", nombre) };
            DataTable dt = _acceso.leer(query, p);

            if (dt.Rows.Count == 0) return null;

            return new Rol_GO44
            {
                Id = Convert.ToInt32(dt.Rows[0]["Id"]),
                Nombre = dt.Rows[0]["Nombre"].ToString()
            };
        }
    }
}
