using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{

    public class BLLUsuario_GO44
    {
        private readonly DALUsuario_GO44 _DALUsuario;
        private readonly BLLIntegridad_GO44 _bllIntegridad;
        public const int MAX_INTENTOS = 3;
        private static readonly TimeSpan VENTANA_INTENTOS = TimeSpan.FromHours(1);

        public BLLUsuario_GO44()
        {
            _DALUsuario = new DALUsuario_GO44();
            _bllIntegridad = new BLLIntegridad_GO44();
        }

        private void RecalcularUsuario()
        {
            try { _bllIntegridad.RecalcularTabla("Usuario"); } catch { }
        }

        public enum ResultadoLogin
        {
            Exitoso,
            SesionActiva,
            UsuarioInexistente,
            UsuarioBloqueado,
            UsuarioInactivo,
            ContrasenaIncorrecta,
            BloqueadoPorIntentos,
            Error
        }

        private void Auditar(string login, string modulo, string tipoEvento, string detalle, string criticidad)
        {

            BLLBitacora_GO44.Instancia.RegistrarEvento(login, modulo, tipoEvento, detalle, criticidad);

        }

        public ResultadoLogin IntentarLogin(string login, string contrasena)
        {
            try
            {
                if (SessionManager_GO44.Instancia.HaySesionActiva())
                {
                    Auditar(login, "Usuario", "Intento de login con sesión ya activa", null, "Media");
                    return ResultadoLogin.SesionActiva;
                }

                Usuario_GO44 usuario = _DALUsuario.BuscarPorLogin(login);

                if (usuario == null)
                {
                    try
                    {
                        Auditar(login, "Usuario", "Usuario inexistente", "el usuario no existe", "Alta");
                    }
                    catch
                    {
                        return ResultadoLogin.UsuarioInexistente;
                    }
                }

                if (usuario.Bloqueo)
                {
                    Auditar(login, "Usuario", "Usuario bloqueado", "Usuario bloqueado correctamente", "Alta");
                    return ResultadoLogin.UsuarioBloqueado;
                }

                if (!usuario.Activo)
                {
                    Auditar(login, "Usuario", "Usuario inactivo", "el usuario esta inactivo", "Alta");
                    return ResultadoLogin.UsuarioInactivo;
                }

                string contrasenaCifrada = Encriptador_GO44.Instancia.EncriptarContrasena(contrasena);
                if (usuario.Contrasena != contrasenaCifrada)
                {

                    int nuevosIntentos = CalcularNuevosIntentos(usuario);
                    DateTime ahora = DateTime.Now;

                    if (nuevosIntentos >= MAX_INTENTOS)
                    {

                        _DALUsuario.ActualizarIntentosFallidos(login, nuevosIntentos, ahora);
                        _DALUsuario.Bloquear(login);
                        Auditar(login, "Usuario", "Usuario bloqueado por intentos fallidos", $"{MAX_INTENTOS} intentos fallidos consecutivos dentro de {VENTANA_INTENTOS.TotalMinutes:0} min", "Alta");
                        return ResultadoLogin.BloqueadoPorIntentos;
                    }
                    else
                    {
                        _DALUsuario.ActualizarIntentosFallidos(login, nuevosIntentos, ahora);
                        Auditar(login, "Usuario", "Contraseña incorrecta", $"Intento {nuevosIntentos}/{MAX_INTENTOS}", "Media");
                        return ResultadoLogin.ContrasenaIncorrecta;
                    }
                }

                _DALUsuario.ResetearIntentosFallidos(login);
                BLLPermisos_GO44 bllPermisos = new BLLPermisos_GO44();
                if (usuario.Rol != null)
                {
                    usuario.Rol = bllPermisos.ObtenerArbolRol(usuario.Rol.Id);
                }
                bool sesionIniciada = SessionManager_GO44.Instancia.IniciarSesion(usuario);
                if (!sesionIniciada)
                {
                    Auditar(login, "Usuario", "Intento de login con sesión ya activa", "Intento de login con sesión ya activa", "Alta");
                    return ResultadoLogin.SesionActiva;
                }
                Auditar(login, "Usuario", "Login exitoso", "Login correcto", "Baja");

                if (!string.IsNullOrWhiteSpace(usuario.Idioma))
                    IdiomaManager_GO44.Instancia.CambiarIdioma(usuario.Idioma);

                return ResultadoLogin.Exitoso;

            }
            catch
            {
                return ResultadoLogin.Error;
            }

        }

        private int CalcularNuevosIntentos(Usuario_GO44 usuario)
        {
            try
            {
                if (usuario.UltimoIntentoFallido == null)
                    return 1;

                DateTime ultimo = usuario.UltimoIntentoFallido.Value;
                bool dentroDeLaVentana = (DateTime.Now - ultimo) <= VENTANA_INTENTOS;

                if (dentroDeLaVentana)
                    return usuario.IntentosFallidos + 1;
                else
                    return 1;
            }
            catch { throw new Exception(IdiomaManager_GO44.T("err.errorGenerico")); }

        }

        public List<Usuario_GO44> ListarActivos() => _DALUsuario.ListarActivos();
        public List<Usuario_GO44> ListarTodos() => _DALUsuario.ListarTodos();

        public List<Rol_GO44> ListarRoles() => _DALUsuario.ListarRoles();

        public Usuario_GO44 BuscarPorLogin(string login) => _DALUsuario.BuscarPorLogin(login);

        public bool ExisteDNI(string dni) => _DALUsuario.ExisteDNI(dni);

        public void CambiarIdioma(string codigoIdioma)
        {

            Usuario_GO44 actual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            string idiomaAnterior = actual != null
                ? (actual.Idioma ?? IdiomaManager_GO44.Instancia.IdiomaActual)
                : IdiomaManager_GO44.Instancia.IdiomaActual;

            IdiomaManager_GO44.Instancia.CambiarIdioma(codigoIdioma);

            if (actual != null)
            {
                _DALUsuario.GuardarIdioma(actual.Login, codigoIdioma);
                actual.Idioma = codigoIdioma;

                if (!string.Equals(idiomaAnterior, codigoIdioma, StringComparison.OrdinalIgnoreCase))
                {
                    Auditar(actual.Login, "Usuario", "Idioma cambiado",
                            $"{idiomaAnterior} -> {codigoIdioma}", "Baja");
                }
            }
        }

        public void Desbloquear(string dni, string login)
        {
            Usuario_GO44 usuario = _DALUsuario.BuscarPorLogin(login);

            string ultimos3 = dni.Length >= 3 ? dni.Substring(dni.Length - 3) : dni;
            string contrasenaPlana = usuario.Nombre.ToLower() + ultimos3;
            string contrasenaCifrada = Encriptador_GO44.Instancia.EncriptarContrasena(contrasenaPlana);
            _DALUsuario.Desbloquear(dni, contrasenaCifrada);
            Auditar(SessionManager_GO44.Instancia.ObtenerUsuarioActual().Login, "Admin", "Usuario desbloqueado", $"Usuario {login} desbloqueado y contraseña reseteada", "Media");
            RecalcularUsuario();
        }

        public void ActivarDesactivar(string dni, bool activo)
        {
            _DALUsuario.ActivarDesactivar(dni, activo);
            string accion = activo ? "Usuario activado" : "Usuario desactivado";
            Auditar(SessionManager_GO44.Instancia.ObtenerUsuarioActual().Login, "Admin", accion, $"DNI: {dni}", "Media");
            RecalcularUsuario();
        }

        public void ModificarEmail(string dni, string email)
        {
            _DALUsuario.ModificarEmail(dni, email);
            Auditar(SessionManager_GO44.Instancia.ObtenerUsuarioActual().Login, "Admin", "Email modificado", $"DNI: {dni}", "Media");
            RecalcularUsuario();
        }

        public void ModificarRol(string dni, Rol_GO44 rol)
        {
            if (rol == null) throw new Exception(IdiomaManager_GO44.T("err.rolValido"));
            _DALUsuario.ModificarRol(dni, rol.Id);
            Auditar(SessionManager_GO44.Instancia.ObtenerUsuarioActual().Login, "Admin","Rol modificado", $"DNI {dni} -> rol {rol.Nombre}", "Media");
            RecalcularUsuario();
        }
        public void CrearUsuario(string dni, string apellido, string nombre, string email, Rol_GO44 rol)
        {
            if (rol == null)
                throw new Exception(IdiomaManager_GO44.T("err.rolSeleccionar"));

            if (_DALUsuario.ExisteDNI(dni))
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.dniDuplicado"), dni));
            string ultimos3 = dni.Length >= 3 ? dni.Substring(dni.Length - 3) : dni;
            string contrasenaPlana = nombre.ToLower() + ultimos3;
            string contrasenaCifrada = Encriptador_GO44.Instancia.EncriptarContrasena(contrasenaPlana);
            string login = nombre.ToLower() + ultimos3;

            if (_DALUsuario.BuscarPorLogin(login) != null)
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.usuarioLoginDuplicado"), login));

            Usuario_GO44 u = new Usuario_GO44
            {
                DNI = dni,
                Apellido = apellido,
                Nombre = nombre,
                Login = login,
                Contrasena = contrasenaCifrada,
                Rol = rol,
                Email = email
            };

            int filas = _DALUsuario.AgregarUsuario(u);
            if (filas == 0)
                throw new Exception(IdiomaManager_GO44.T("err.insertFallido"));
            Auditar(SessionManager_GO44.Instancia.ObtenerUsuarioActual().Login, "Admin","Usuario creado", $"Login: {login}", "Baja");
            RecalcularUsuario();
        }

        public enum ResultadoCambioContrasena
        {
            Exitoso,
            ContrasenaActualIncorrecta,
            ContrasenasNoCoinciden,
            UsuarioInexistente,
            NuevaIgualActual
        }

        public ResultadoCambioContrasena CambiarContrasena(string login, string contrasenaActual, string nuevaContrasena, string confirmarContrasena)
        {
            try
            {
                if (nuevaContrasena != confirmarContrasena)
                    return ResultadoCambioContrasena.ContrasenasNoCoinciden;
                if (nuevaContrasena == contrasenaActual)
                    return ResultadoCambioContrasena.NuevaIgualActual;

                Usuario_GO44 usuario = _DALUsuario.BuscarPorLogin(login);
                if (usuario == null)
                    return ResultadoCambioContrasena.UsuarioInexistente;

                string contrasenaActualCifrada = Encriptador_GO44.Instancia.EncriptarContrasena(contrasenaActual);
                if (usuario.Contrasena != contrasenaActualCifrada)
                    return ResultadoCambioContrasena.ContrasenaActualIncorrecta;

                string nuevaContrasenaCifrada = Encriptador_GO44.Instancia.EncriptarContrasena(nuevaContrasena);
                if (usuario.Contrasena == nuevaContrasenaCifrada)
                    return ResultadoCambioContrasena.NuevaIgualActual;

                _DALUsuario.CambiarContrasena(login, nuevaContrasenaCifrada);

                Auditar(login, "Usuario", "Contraseña cambiada exitosamente", "Cambio de contraseña", "Baja");
                return ResultadoCambioContrasena.Exitoso;
            }
            catch
            {
                throw new Exception(IdiomaManager_GO44.T("err.errorGenerico"));
            }
        }

        public static void CerrarSesión()
        {
            BLLUsuario_GO44 bll = new BLLUsuario_GO44();
            bll.Auditar(SessionManager_GO44.Instancia.ObtenerUsuarioActual().Login,"Usuario", "Logout realizado", "LogOut", "Alta");
            SessionManager_GO44.Instancia.CerrarSesion();
            IdiomaManager_GO44.Instancia.CambiarIdioma(IdiomaManager_GO44.ES);
        }
    }
}
