using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Servicios
{

    public class Encriptador_GO44
    {
        private static Encriptador_GO44 _instancia;

        private Encriptador_GO44() { }

        public static Encriptador_GO44 Instancia
        {
            get
            {
                if (_instancia == null)
                _instancia = new Encriptador_GO44();
                return _instancia;
            }
        }

        public string EncriptarContrasena(string contrasenaPlana)
        {
            if (string.IsNullOrEmpty(contrasenaPlana))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contrasenaPlana));

                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
