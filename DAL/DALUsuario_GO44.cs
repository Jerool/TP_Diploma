using Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{

    public class DALUsuario_GO44
    {

        private readonly Acceso _acceso;
        private readonly DALIntegridad_GO44 _dalIntegridad;
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

        private void RecalcularIntegridadUsuario()
        {
            if (DALIntegridad_GO44.IntegridadConocidamenteRota) return;
            try { _dalIntegridad.RecalcularTabla("Usuario"); } catch { }
        }

        public bool ExisteDNI(string dni)
        {
            string query = "SELECT COUNT(1) FROM Usuario WHERE DNI = @DNI";
            SqlParameter[] parametros = { new SqlParameter("@DNI", dni) };
            object resultado = _acceso.leerEscalar(query, parametros);
            return resultado != null && Convert.ToInt32(resultado) > 0;
        }

        public Usuario_GO44 BuscarPorLogin(string login)
        {
            string query = SELECT_BASE + " WHERE U.UserName = @UserName";

            SqlParameter[] parametros = { new SqlParameter("@UserName", login) };

            DataTable dt = _acceso.leer(query, parametros);

            if (dt.Rows.Count == 0)
                return null;

            return MapearFila(dt.Rows[0]);
        }

        public List<Usuario_GO44> ListarActivos()
        {
            string query = SELECT_BASE + " WHERE U.Activo = 1";
            return MapearLista(_acceso.leer(query, null));
        }

        public List<Usuario_GO44> ListarTodos()
        {
            return MapearLista(_acceso.leer(SELECT_BASE, null));
        }

        public void Bloquear(string login)
        {
            string query = "UPDATE Usuario SET Bloqueo = 1 WHERE UserName = @UserName";
            SqlParameter[] parametros = {
                new SqlParameter("@UserName", login)
            };
            _acceso.escribir(query, parametros);
            RecalcularIntegridadUsuario();
        }

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

        private List<Usuario_GO44> MapearLista(DataTable dt)
        {
            List<Usuario_GO44> lista = new List<Usuario_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(MapearFila(row));
            }
            return lista;
        }

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
