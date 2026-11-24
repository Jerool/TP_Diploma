using BE;
using DAL;
using Servicios;
using System;
using System.Data.SqlClient;
using System.Linq;

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

        /// <summary>Cada forma en que puede terminar la confirmación del carrito (éxito o el motivo del rechazo).</summary>
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

        /// <summary>Resultado de ConfirmarCarrito: el estado, el Id generado, el componente problemático (si hubo) y un mensaje para la UI.</summary>
        public class ConfirmacionCarritoVO
        {
            public ResultadoConfirmacion Resultado { get; set; }
            public int IdCarritoGenerado { get; set; }
            public string ComponenteConProblema { get; set; }
            public string Mensaje { get; set; }
        }

        /// <summary>Registra un evento en la bitácora (best-effort: si falla, no interrumpe la venta).</summary>
        private void Auditar(string tipoEvento, string detalle, string criticidad)
        {
            try
            {
                Usuario_GO44 usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
                string login = usuario != null ? usuario.Login : "SISTEMA";
                BLLBitacora_GO44.Instancia.RegistrarEvento(login, "Carrito", tipoEvento, detalle, criticidad);
            }
            catch { /* la bitácora es best-effort; no debe romper la operación principal */ }
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
            if (carrito == null || EstaVacio(carrito))
            {
                vo.Resultado = ResultadoConfirmacion.SinLineas;
                vo.Mensaje = IdiomaManager_GO44.T("bll.carrito.sinLineas");
                return vo;
            }

            if (carrito.Cliente == null || string.IsNullOrWhiteSpace(carrito.Cliente.DNI))
            {
                vo.Resultado = ResultadoConfirmacion.SinCliente;
                vo.Mensaje = IdiomaManager_GO44.T("bll.carrito.sinCliente");
                return vo;
            }

            Usuario_GO44 usuarioActual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                vo.Resultado = ResultadoConfirmacion.SinVendedor;
                vo.Mensaje = IdiomaManager_GO44.T("bll.carrito.sinSesion");
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
                    vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.carrito.componenteInactivo"), linea.ComponenteCodigo);
                    return vo;
                }
                if (comp.StockActual < linea.Cantidad)
                {
                    Auditar("Stock insuficiente",
                            "Pidieron " + linea.Cantidad + " de " + comp.Codigo + " y hay " + comp.StockActual,
                            "Alta");
                    vo.Resultado = ResultadoConfirmacion.StockInsuficiente;
                    vo.ComponenteConProblema = comp.Codigo;
                    vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.carrito.stockInsuficiente"), comp.Codigo, comp.StockActual);
                    return vo;
                }
            }

            // --- Verificación de cliente existe ---
            if (!_bllCliente.ExisteDNI(carrito.Cliente.DNI))
            {
                vo.Resultado = ResultadoConfirmacion.ClienteInexistente;
                vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.carrito.clienteNoRegistrado"), carrito.Cliente.DNI);
                return vo;
            }

            // --- Transacción atómica ---
            SqlTransaction tx = null;
            try
            {
                carrito.Estado = BE_Carrito_GO44.EstadoCarrito.Confirmado;

                tx = _dalCarrito.IniciarTransaccion();

                // 1) Insertar encabezado carrito
                int idCarrito = _dalCarrito.InsertarEncabezadoEnTx(carrito, tx);
                if (idCarrito <= 0)
                    throw new Exception("No se pudo generar el ID del carrito");

                carrito.Id = idCarrito;

                // 2) Insertar cada línea (SIN descontar stock — el descuento se hace en CU05 Cobrar Venta,
                //    dentro de la misma tx que la factura y el cobro. Acá solo se persiste la reserva
                //    del carrito. Si el cliente no llega a cobrar, el stock nunca se descuenta.)
                foreach (BE_LineaCarrito_GO44 linea in carrito.Lineas)
                {
                    int filasLinea = _dalCarrito.InsertarLineaEnTx(idCarrito, linea, tx);
                    if (filasLinea == 0)
                        throw new Exception("No se pudo insertar línea para componente " + linea.ComponenteCodigo);
                }

                // 3) Commit atómico (encabezado + líneas)
                _dalCarrito.ConfirmarTransaccion(tx);
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
                vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.carrito.confirmadoOk"), idCarrito, carrito.Total.ToString("0.00"));
                return vo;
            }
            catch (Exception ex)
            {
                try { _dalCarrito.CancelarTransaccion(tx); } catch { }
                Auditar("Carrito cancelado", "Rollback por error: " + ex.Message, "Alta");
                vo.Resultado = ResultadoConfirmacion.Error;
                vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.carrito.errConfirmar"), ex.Message);
                return vo;
            }
        }

        // ============ Operaciones sobre las líneas del carrito (antes vivían en la BE) ============

        /// <summary>
        /// Agrega un componente al carrito en memoria. Si ese componente YA estaba, no duplica la línea:
        /// le suma la cantidad a la línea existente (comportamiento típico de "agregar al carrito").
        /// El precio unitario se "congela" tomando el del componente en este instante.
        /// </summary>
        public void AgregarLinea(BE_Carrito_GO44 carrito, BE_Componente_GO44 componente, int cantidad)
        {
            // Guardas defensivas: ignorar entradas inválidas en vez de romper.
            if (carrito == null || carrito.Lineas == null) return;
            if (componente == null) return;
            if (cantidad <= 0) return;

            // Busca si ya hay una línea para ese mismo componente (comparando por Id).
            BE_LineaCarrito_GO44 existente = carrito.Lineas.FirstOrDefault(l => l.Componente != null && l.Componente.Id == componente.Id);
            if (existente != null)
            {
                // Ya existía: solo acumula la cantidad.
                existente.Cantidad += cantidad;
            }
            else
            {
                // No existía: crea una línea nueva con el precio congelado del momento.
                carrito.Lineas.Add(new BE_LineaCarrito_GO44
                {
                    Componente = componente,
                    Cantidad = cantidad,
                    PrecioUnitario = componente.Precio
                });
            }
        }

        /// <summary>Quita del carrito la línea del componente indicado. Devuelve false si no estaba.</summary>
        public bool QuitarLinea(BE_Carrito_GO44 carrito, int idComponente)
        {
            if (carrito == null || carrito.Lineas == null) return false;
            BE_LineaCarrito_GO44 linea = carrito.Lineas.FirstOrDefault(l => l.Componente != null && l.Componente.Id == idComponente);
            if (linea == null) return false; // no había nada que quitar
            carrito.Lineas.Remove(linea);
            return true;
        }

        /// <summary>true si el carrito no tiene líneas; se usa para impedir facturar un carrito vacío.</summary>
        public bool EstaVacio(BE_Carrito_GO44 carrito)
        {
            return carrito == null || carrito.Lineas == null || carrito.Lineas.Count == 0;
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

        /// <summary>Trae un carrito persistido por Id (delega en la DAL).</summary>
        public BE_Carrito_GO44 BuscarPorId(int id)
        {
            return _dalCarrito.BuscarPorId(id);
        }
    }
}
