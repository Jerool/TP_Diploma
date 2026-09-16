using BE;
using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace BLL
{
    /// <summary>
    /// BLL de Cliente — implementa el CU02 Registrar Cliente.
    /// </summary>
    public class BLLCliente_GO44
    {
        private readonly DALCliente_GO44 _dalCliente;
        private readonly BLLIntegridad_GO44 _bllIntegridad;

        public BLLCliente_GO44()
        {
            _dalCliente    = new DALCliente_GO44();
            _bllIntegridad = new BLLIntegridad_GO44();
        }

        public enum ResultadoRegistroCliente
        {
            Exitoso,
            DNIVacio,
            DNIDuplicado,
            DatosIncompletos,
            EmailInvalido,
            Error
        }

        private void Auditar(string modulo, string tipoEvento, string detalle, string criticidad)
        {
            Usuario_GO44 usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            string login = usuario != null ? usuario.Login : "SISTEMA";
            BLLBitacora_GO44.Instancia.RegistrarEvento(login, modulo, tipoEvento, detalle, criticidad);
        }

        private void RecalcularCliente()
        {
            try { _bllIntegridad.RecalcularTabla("Clientes"); } catch { }
        }

        private static bool EmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            }
            catch { return false; }
        }

        // ============ CU02 Registrar Cliente ============

        public ResultadoRegistroCliente RegistrarCliente(string dni, string apellido, string nombre, string email, string telefono)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dni))
                    return ResultadoRegistroCliente.DNIVacio;

                if (string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(nombre))
                    return ResultadoRegistroCliente.DatosIncompletos;

                if (!EmailValido(email))
                    return ResultadoRegistroCliente.EmailInvalido;

                if (_dalCliente.ExisteDNI(dni))
                {
                    Auditar("Cliente", "DNI duplicado", "Intento de alta con DNI existente: " + dni, "Media");
                    return ResultadoRegistroCliente.DNIDuplicado;
                }

                BE_Cliente_GO44 nuevo = new BE_Cliente_GO44(dni, apellido, nombre, email, telefono);
                int filas = _dalCliente.Insertar(nuevo);

                if (filas == 0)
                {
                    Auditar("Cliente", "Cliente registrado", "Falló el INSERT para DNI " + dni, "Alta");
                    return ResultadoRegistroCliente.Error;
                }

                Auditar("Cliente", "Cliente registrado", "DNI: " + dni + " — " + apellido + ", " + nombre, "Baja");
                RecalcularCliente();
                return ResultadoRegistroCliente.Exitoso;
            }
            catch (Exception ex)
            {
                Auditar("Cliente", "Cliente registrado", "Error: " + ex.Message, "Alta");
                return ResultadoRegistroCliente.Error;
            }
        }

        // ============ Consultas y utilidades ============

        public bool ExisteDNI(string dni)
        {
            return _dalCliente.ExisteDNI(dni);
        }

        public BE_Cliente_GO44 BuscarPorDNI(string dni)
        {
            return _dalCliente.BuscarPorDNI(dni);
        }

        public List<BE_Cliente_GO44> Listar()
        {
            return _dalCliente.Listar();
        }

        public bool ModificarEmail(string dni, string email)
        {
            if (!EmailValido(email)) return false;
            int filas = _dalCliente.ModificarEmail(dni, email);
            if (filas > 0)
            {
                Auditar("Cliente", "Cliente modificado", "Email actualizado para DNI " + dni, "Media");
                RecalcularCliente();
            }
            return filas > 0;
        }

        public bool ActivarDesactivar(string dni, bool activo)
        {
            int filas = _dalCliente.ActivarDesactivar(dni, activo);
            if (filas > 0)
            {
                string accion = activo ? "Cliente activado" : "Cliente desactivado";
                Auditar("Cliente", "Cliente modificado", accion + " — DNI " + dni, "Media");
                RecalcularCliente();
            }
            return filas > 0;
        }

        /// <summary>
        /// Variante para uso dentro del CU01 Cargar Carrito, cuando el vendedor decide
        /// registrar un cliente nuevo desde la misma pantalla del carrito.
        /// El llamador (BLLCarrito) inicia y confirma la transacción.
        /// </summary>
        internal int RegistrarClienteEnTx(BE_Cliente_GO44 cliente, SqlTransaction tx)
        {
            return _dalCliente.InsertarEnTx(cliente, tx);
        }
    }
}
