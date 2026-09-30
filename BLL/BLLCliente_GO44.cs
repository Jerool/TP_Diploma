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
            EmailDuplicado,
            TelefonoInvalido,
            TelefonoDuplicado,
            Error
        }

        private void Auditar(string modulo, string tipoEvento, string detalle, string criticidad)
        {
            try
            {
                Usuario_GO44 usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
                string login = usuario != null ? usuario.Login : "SISTEMA";
                BLLBitacora_GO44.Instancia.RegistrarEvento(login, modulo, tipoEvento, detalle, criticidad);
            }
            catch { /* la bitácora es best-effort; no debe romper la operación principal */ }
        }

        private void RecalcularCliente()
        {
            try { _bllIntegridad.RecalcularTabla("Clientes"); } catch { }
        }

        // Reutiliza el validador de Servicios (única fuente de verdad para las reglas)
        private static bool EmailValido(string email) => Validaciones_GO44.EsEmailValido(email);
        private static bool DniValido(string dni)     => Validaciones_GO44.EsDniValido(dni);
        private static bool TelefonoValido(string t)  => string.IsNullOrWhiteSpace(t) || Validaciones_GO44.EsTelefonoValido(t);

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

                // Validar duplicado de email y teléfono contra el resto de clientes
                if (ExisteEmail(email))
                {
                    Auditar("Cliente", "Email duplicado", "Intento de alta con email existente: " + email, "Media");
                    return ResultadoRegistroCliente.EmailDuplicado;
                }

                if (!string.IsNullOrWhiteSpace(telefono))
                {
                    if (!TelefonoValido(telefono))
                        return ResultadoRegistroCliente.TelefonoInvalido;

                    if (ExisteTelefono(telefono))
                    {
                        Auditar("Cliente", "Teléfono duplicado", "Intento de alta con teléfono existente: " + telefono, "Media");
                        return ResultadoRegistroCliente.TelefonoDuplicado;
                    }
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
                _ultimoErrorMensaje = ex.Message;
                return ResultadoRegistroCliente.Error;
            }
        }

        /// <summary>Último mensaje de error interno (para debug/diagnóstico).</summary>
        public string UltimoErrorMensaje { get { return _ultimoErrorMensaje; } }
        private string _ultimoErrorMensaje;

        // ============ Consultas y utilidades ============

        public bool ExisteDNI(string dni)
        {
            return _dalCliente.ExisteDNI(dni);
        }

        /// <summary>
        /// Chequea si algún OTRO cliente ya tiene ese email. Ignora al cliente con DNI = dniExcluir
        /// (útil para modificar un email sin que se autodetecte como duplicado).
        /// Tolerante: si falla el desencriptado de un cliente puntual, sigue con los demás.
        /// </summary>
        public bool ExisteEmail(string email, string dniExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var lista = _dalCliente.Listar();
                foreach (var c in lista)
                {
                    try
                    {
                        if (c.DNI == dniExcluir) continue;
                        if (!string.IsNullOrEmpty(c.Email) &&
                            c.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    catch { /* saltear cliente con datos corruptos */ }
                }
            }
            catch { /* si falla el listado entero, asumimos que no hay duplicado */ }
            return false;
        }

        /// <summary>
        /// Chequea si algún OTRO cliente ya tiene ese teléfono.
        /// </summary>
        public bool ExisteTelefono(string telefono, string dniExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(telefono)) return false;
            try
            {
                var lista = _dalCliente.Listar();
                foreach (var c in lista)
                {
                    try
                    {
                        if (c.DNI == dniExcluir) continue;
                        if (!string.IsNullOrEmpty(c.Telefono) && c.Telefono == telefono)
                            return true;
                    }
                    catch { /* saltear cliente con datos corruptos */ }
                }
            }
            catch { /* si falla el listado entero, asumimos que no hay duplicado */ }
            return false;
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

            // No permitir email que ya use otro cliente (excluye al que estamos modificando)
            if (ExisteEmail(email, dniExcluir: dni))
            {
                Auditar("Cliente", "Email duplicado", "Intento de modificación con email en uso: " + email, "Media");
                return false;
            }

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
