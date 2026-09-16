using BE;
using DAL;
using Servicios;
using System;
using System.Data.SqlClient;

namespace BLL
{
    /// <summary>
    /// BLL de Carrito — implementa el CU01 Cargar Carrito (transacción atómica).
    /// El vendedor arma el carrito en memoria (agregando componentes vía CU03),
    /// y al confirmar esta clase inserta encabezado + líneas + descuenta stock + audita
    /// TODO dentro de una única transacción.
    /// </summary>
    public class BLLCarrito_GO44
    {
        private readonly DALCarrito_GO44 _dalCarrito;
        private readonly DALComponente_GO44 _dalComponente;
        private readonly BLLIntegridad_GO44 _bllIntegridad;
        private readonly BLLCliente_GO44 _bllCliente;

        public BLLCarrito_GO44()
        {
            _dalCarrito    = new DALCarrito_GO44();
            _dalComponente = new DALComponente_GO44();
            _bllIntegridad = new BLLIntegridad_GO44();
            _bllCliente    = new BLLCliente_GO44();
        }

        public enum ResultadoConfirmacion
        {
            Exitoso,
            SinLineas,
            SinCliente,
            SinVendedor,
            StockInsuficiente,
            ClienteInexistente,
            Error
        }

        public class ConfirmacionCarritoVO
        {
            public ResultadoConfirmacion Resultado { get; set; }
            public int IdCarritoGenerado { get; set; }
            public string ComponenteConProblema { get; set; }
            public string Mensaje { get; set; }
        }

        private void Auditar(string tipoEvento, string detalle, string criticidad)
        {
            Usuario_GO44 usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            string login = usuario != null ? usuario.Login : "SISTEMA";
            BLLBitacora_GO44.Instancia.RegistrarEvento(login, "Carrito", tipoEvento, detalle, criticidad);
        }

        // ============ CU01 Cargar Carrito — confirmación ============

        /// <summary>
        /// Toma un BE_Carrito_GO44 armado en memoria y lo persiste en la BD dentro de una
        /// única transacción: encabezado + N líneas + N descuentos de stock + auditoría.
        /// Si algo falla, rollback completo.
        /// </summary>
        public ConfirmacionCarritoVO ConfirmarCarrito(BE_Carrito_GO44 carrito)
        {
            ConfirmacionCarritoVO vo = new ConfirmacionCarritoVO();

            // --- Validaciones previas ---
            if (carrito == null || carrito.EstaVacio())
            {
                vo.Resultado = ResultadoConfirmacion.SinLineas;
                vo.Mensaje = "El carrito no tiene líneas";
                return vo;
            }

            if (carrito.Cliente == null || string.IsNullOrWhiteSpace(carrito.Cliente.DNI))
            {
                vo.Resultado = ResultadoConfirmacion.SinCliente;
                vo.Mensaje = "El carrito no tiene cliente asignado";
                return vo;
            }

            Usuario_GO44 usuarioActual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                vo.Resultado = ResultadoConfirmacion.SinVendedor;
                vo.Mensaje = "No hay sesión activa";
                return vo;
            }
            carrito.LoginVendedor = usuarioActual.Login;

            // --- Verificación de stock (fuera de tx, para fail-fast) ---
            foreach (BE_LineaCarrito_GO44 linea in carrito.Lineas)
            {
                BE_Componente_GO44 comp = _dalComponente.BuscarPorId(linea.Componente.Id);
                if (comp == null || !comp.Activo)
                {
                    vo.Resultado = ResultadoConfirmacion.StockInsuficiente;
                    vo.ComponenteConProblema = linea.ComponenteCodigo;
                    vo.Mensaje = "Componente inexistente o inactivo: " + linea.ComponenteCodigo;
                    return vo;
                }
                if (comp.StockActual < linea.Cantidad)
                {
                    Auditar("Stock insuficiente",
                            "Pidieron " + linea.Cantidad + " de " + comp.Codigo + " y hay " + comp.StockActual,
                            "Alta");
                    vo.Resultado = ResultadoConfirmacion.StockInsuficiente;
                    vo.ComponenteConProblema = comp.Codigo;
                    vo.Mensaje = "Stock insuficiente para " + comp.Codigo + ". Disponible: " + comp.StockActual;
                    return vo;
                }
            }

            // --- Verificación de cliente existe ---
            if (!_bllCliente.ExisteDNI(carrito.Cliente.DNI))
            {
                vo.Resultado = ResultadoConfirmacion.ClienteInexistente;
                vo.Mensaje = "El cliente DNI " + carrito.Cliente.DNI + " no está registrado. Registre el cliente primero (CU02).";
                return vo;
            }

            // --- Transacción atómica ---
            SqlTransaction tx = null;
            try
            {
                carrito.Estado = BE_Carrito_GO44.EstadoCarrito.Confirmado;

                tx = Acceso.Instancia.IniciarTransaccion();

                // 1) Insertar encabezado carrito
                int idCarrito = _dalCarrito.InsertarEncabezadoEnTx(carrito, tx);
                if (idCarrito <= 0)
                    throw new Exception("No se pudo generar el ID del carrito");

                carrito.Id = idCarrito;

                // 2) Insertar cada línea + descontar stock
                foreach (BE_LineaCarrito_GO44 linea in carrito.Lineas)
                {
                    int filasLinea = _dalCarrito.InsertarLineaEnTx(idCarrito, linea, tx);
                    if (filasLinea == 0)
                        throw new Exception("No se pudo insertar línea para componente " + linea.ComponenteCodigo);

                    // El SP descuenta y devuelve 0 filas si hubo condición de carrera → protección adicional
                    SqlParameter[] pDesc = {
                        new SqlParameter("@Id",       linea.Componente.Id),
                        new SqlParameter("@Cantidad", linea.Cantidad)
                    };
                    int filasStock = Acceso.Instancia.escribirSPEnTx("sp_Componente_DescontarStock_GO44", pDesc, tx);
                    if (filasStock == 0)
                        throw new Exception("Condición de carrera: se acabó el stock de " + linea.ComponenteCodigo);
                }

                // 3) Commit atómico (encabezado + líneas + descuento stock)
                Acceso.Instancia.ConfirmarTransaccion(tx);
                tx = null;

                // 4) Auditoría post-commit (la venta ya está persistida; log en modo best-effort)
                try
                {
                    BLLBitacora_GO44.Instancia.RegistrarEvento(
                        usuarioActual.Login, "Carrito", "Carrito confirmado",
                        "IdCarrito=" + idCarrito + " Cliente=" + carrito.Cliente.DNI +
                        " Items=" + carrito.CantidadItems + " Total=" + carrito.Total.ToString("0.00"),
                        "Baja");

                    foreach (BE_LineaCarrito_GO44 linea in carrito.Lineas)
                    {
                        BLLBitacora_GO44.Instancia.RegistrarEvento(
                            usuarioActual.Login, "Componente", "Componente vendido",
                            linea.ComponenteCodigo + " x" + linea.Cantidad + " @ " + linea.PrecioUnitario.ToString("0.00"),
                            "Baja");
                    }
                }
                catch { /* si falla la bitácora la venta ya está commiteada */ }

                // 5) Recalcular DVH post-commit (fuera de tx)
                _dalCarrito.RecalcularIntegridadPostConfirmacion();
                try { _bllIntegridad.RecalcularTabla("Componentes"); } catch { }

                vo.Resultado = ResultadoConfirmacion.Exitoso;
                vo.IdCarritoGenerado = idCarrito;
                vo.Mensaje = "Carrito Nº " + idCarrito + " confirmado. Total: $" + carrito.Total.ToString("0.00");
                return vo;
            }
            catch (Exception ex)
            {
                try { Acceso.Instancia.CancelarTransaccion(tx); } catch { }
                Auditar("Carrito cancelado", "Rollback por error: " + ex.Message, "Alta");
                vo.Resultado = ResultadoConfirmacion.Error;
                vo.Mensaje = "Error al confirmar carrito: " + ex.Message;
                return vo;
            }
        }

        // ============ Utilidades para la UI ============

        /// <summary>
        /// Crea un carrito nuevo en memoria (no persiste). El vendedor le va agregando líneas.
        /// </summary>
        public BE_Carrito_GO44 NuevoCarrito(BE_Cliente_GO44 cliente)
        {
            BE_Carrito_GO44 c = new BE_Carrito_GO44();
            c.Cliente = cliente;
            Usuario_GO44 u = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            c.LoginVendedor = u != null ? u.Login : string.Empty;
            return c;
        }

        public BE_Carrito_GO44 BuscarPorId(int id)
        {
            return _dalCarrito.BuscarPorId(id);
        }
    }
}
