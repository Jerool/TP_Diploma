using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicios
{

    public class SessionManager_GO44
    {
        private static SessionManager_GO44 _instancia;

        private Usuario_GO44 _usuarioActual = null;

        private SessionManager_GO44() { }

        public static SessionManager_GO44 Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new SessionManager_GO44();
                return _instancia;
            }
        }

        public bool IniciarSesion(Usuario_GO44 usuario)
        {
            if (_usuarioActual != null)
                return false;

            _usuarioActual = usuario;
            return true;
        }

        public void CerrarSesion()
        {
            _usuarioActual = null;
        }

        public bool HaySesionActiva()
        {
            return _usuarioActual != null;
        }

        public Usuario_GO44 ObtenerUsuarioActual()
        {
            return _usuarioActual;
        }
    }
}
