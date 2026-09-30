using BE;
using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace BLL
{
    /// <summary>
    /// BLL de Facturación — CU04 Generar Factura.
    /// El CU05 Cobrar Venta se maneja en BLLCobro_GO44, pero esta clase provee
    /// también el método GenerarFacturaYCobrar que integra ambos en una única
    /// transacción atómica (encabezado + líneas + cobro + marcar cobrada + descontar stock).
    /// </summary>
    public class BLLFactura_GO44
    {
        private const decimal ALICUOTA_IVA = 0.21m;

        private readonly DALFactura_GO44 _dalFactura;
        private readonly DALCarrito_GO44 _dalCarrito;
        private readonly DALComponente_GO44 _dalComponente;
        private readonly BLLIntegridad_GO44 _bllIntegridad;

        public BLLFactura_GO44()
        {
            _dalFactura    = new DALFactura_GO44();
            _dalCarrito    = new DALCarrito_GO44();
            _dalComponente = new DALComponente_GO44();
            _bllIntegridad = new BLLIntegridad_GO44();
        }

        public enum ResultadoFactura
        {
            Exitoso,
            SinCarritoPendiente,
            CarritoInvalido,
            SinVendedor,
            StockInsuficiente,
            ErrorCobro,
            Error
        }

        public class FacturaVO
        {
            public ResultadoFactura Resultado { get; set; }
            public BE_Factura_GO44 Factura { get; set; }
            public BE_Cobro_GO44 Cobro { get; set; }
            public string Mensaje { get; set; }
        }

        // ============ Consultas para pantalla ============

        public BE_Carrito_GO44 ObtenerCarritoPendiente(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni)) return null;
            return _dalCarrito.BuscarPendientePorDNI(dni);
        }

        public BE_Factura_GO44 BuscarPorId(int id)
        {
            return _dalFactura.BuscarPorId(id);
        }

        public List<BE_Factura_GO44> ListarConFiltros(DateTime? desde, DateTime? hasta, string dni, string estado)
        {
            return _dalFactura.ListarConFiltros(desde, hasta, dni, estado);
        }

        // ============ CU04 + CU05 unificados en transacción atómica ============

        /// <summary>
        /// Genera la factura para el carrito indicado y registra el cobro en una única transacción.
        /// Retorna VO con Resultado, la Factura completa y el Cobro registrado.
        /// </summary>
        public FacturaVO GenerarFacturaYCobrar(BE_Carrito_GO44 carrito, BE_Cobro_GO44 cobro)
        {
            FacturaVO vo = new FacturaVO();
            if (carrito == null || carrito.EstaVacio())
            {
                vo.Resultado = ResultadoFactura.CarritoInvalido;
                vo.Mensaje = IdiomaManager_GO44.T("bll.factura.sinLineas");
                return vo;
            }
            if (carrito.Cliente == null || string.IsNullOrWhiteSpace(carrito.Cliente.DNI))
            {
                vo.Resultado = ResultadoFactura.CarritoInvalido;
                vo.Mensaje = IdiomaManager_GO44.T("bll.factura.sinCliente");
                return vo;
            }

            Usuario_GO44 usuarioActual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                vo.Resultado = ResultadoFactura.SinVendedor;
                vo.Mensaje = IdiomaManager_GO44.T("bll.factura.sinSesion");
                return vo;
            }

            // Calcular totales
            decimal subtotal = carrito.Total;
            decimal iva      = Math.Round(subtotal * ALICUOTA_IVA, 2);
            decimal total    = subtotal + iva;

            // Armar factura
            BE_Factura_GO44 factura = new BE_Factura_GO44
            {
                NumeroFactura = GenerarNumeroFactura(),
                IdCarrito     = carrito.Id,
                Cliente       = carrito.Cliente,
                LoginCajero   = usuarioActual.Login,
                Subtotal      = subtotal,
                IVA           = iva,
                Total         = total,
                Estado        = BE_Factura_GO44.EstadoFactura.Pendiente
            };
            foreach (BE_LineaCarrito_GO44 lc in carrito.Lineas)
            {
                factura.Lineas.Add(new BE_LineaFactura_GO44(lc.Componente, lc.Cantidad, lc.PrecioUnitario));
            }

            // Validar monto del cobro
            if (cobro == null || cobro.Monto < total)
            {
                vo.Resultado = ResultadoFactura.ErrorCobro;
                vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.factura.montoNoCoincide"),
                    (cobro != null ? cobro.Monto.ToString("N2") : "0.00"), total.ToString("N2"));
                return vo;
            }

            // Verificación fail-fast de stock (fuera de tx) — el stock se descuenta abajo en la misma
            // tx que la factura y el cobro. Puede haber cambiado entre el CU01 y este momento si otro
            // vendedor vendió las mismas unidades a otro cliente.
            foreach (BE_LineaFactura_GO44 lf in factura.Lineas)
            {
                BE_Componente_GO44 compActual = _dalComponente.BuscarPorId(lf.Componente.Id);
                if (compActual == null || !compActual.Activo)
                {
                    vo.Resultado = ResultadoFactura.StockInsuficiente;
                    vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.factura.componenteInactivo"), lf.ComponenteCodigo);
                    return vo;
                }
                if (compActual.StockActual < lf.Cantidad)
                {
                    vo.Resultado = ResultadoFactura.StockInsuficiente;
                    vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.factura.stockInsuficiente"),
                        lf.ComponenteCodigo, compActual.StockActual, lf.Cantidad);
                    return vo;
                }
            }

            // ---- Transacción atómica ----
            SqlTransaction tx = null;
            try
            {
                tx = Acceso.Instancia.IniciarTransaccion();

                // 1) Insertar encabezado factura
                int idFactura = _dalFactura.InsertarEncabezadoEnTx(factura, tx);
                if (idFactura <= 0) throw new Exception("No se pudo generar el ID de factura");
                factura.Id = idFactura;

                // 2) Insertar cada línea
                foreach (BE_LineaFactura_GO44 lf in factura.Lineas)
                {
                    int filas = _dalFactura.InsertarLineaEnTx(idFactura, lf, tx);
                    if (filas == 0) throw new Exception("No se pudo insertar línea de factura para " + lf.ComponenteCodigo);
                }

                // 3) Registrar cobro
                cobro.IdFactura = idFactura;
                int idCobro = _dalFactura.RegistrarCobroEnTx(cobro, tx);
                if (idCobro <= 0) throw new Exception("No se pudo registrar el cobro");
                cobro.Id = idCobro;

                // 4) Marcar factura como Cobrada
                _dalFactura.MarcarCobradaEnTx(idFactura, tx);
                factura.Estado = BE_Factura_GO44.EstadoFactura.Cobrada;

                // 5) Descontar stock definitivo dentro de la misma tx. El SP tiene protección
                //    atómica contra stock negativo (WHERE StockActual >= @Cantidad); si devuelve
                //    0 filas es porque hubo condición de carrera → rollback completo.
                foreach (BE_LineaFactura_GO44 lf in factura.Lineas)
                {
                    SqlParameter[] pDesc = {
                        new SqlParameter("@Id",       lf.Componente.Id),
                        new SqlParameter("@Cantidad", lf.Cantidad)
                    };
                    int filasStock = Acceso.Instancia.escribirSPEnTx("sp_Componente_DescontarStock_GO44", pDesc, tx);
                    if (filasStock == 0)
                        throw new Exception("Condición de carrera: se acabó el stock de " + lf.ComponenteCodigo + " durante el cobro");
                }

                // 6) Commit
                Acceso.Instancia.ConfirmarTransaccion(tx);
                tx = null;

                // 6) Post-commit: bitácora + DVH (best-effort)
                try
                {
                    BLLBitacora_GO44.Instancia.RegistrarEvento(
                        usuarioActual.Login, "Factura", "Factura generada",
                        "Factura " + factura.NumeroFactura + " · Total $" + total.ToString("N2") +
                        " · Cliente " + factura.Cliente.DNI, "Baja");

                    BLLBitacora_GO44.Instancia.RegistrarEvento(
                        usuarioActual.Login, "Cobro", "Cobro registrado",
                        "Método " + cobro.Metodo + " · Monto $" + cobro.Monto.ToString("N2") +
                        " · Factura " + factura.NumeroFactura, "Baja");
                }
                catch { }

                _dalFactura.RecalcularIntegridadPostCobro();
                try { _bllIntegridad.RecalcularTabla("Componentes"); } catch { }   // por el descuento de stock

                vo.Resultado = ResultadoFactura.Exitoso;
                vo.Factura = factura;
                vo.Cobro = cobro;
                vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.factura.generadaOk"), factura.NumeroFactura, total.ToString("N2"));
                return vo;
            }
            catch (Exception ex)
            {
                try { Acceso.Instancia.CancelarTransaccion(tx); } catch { }
                try
                {
                    BLLBitacora_GO44.Instancia.RegistrarEvento(
                        usuarioActual.Login, "Factura", "Factura anulada",
                        "Rollback por error: " + ex.Message, "Alta");
                }
                catch { }
                vo.Resultado = ResultadoFactura.Error;
                vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.factura.errGenerar"), ex.Message);
                return vo;
            }
        }

        // ============ Utilidades ============

        /// <summary>
        /// Genera un número de factura único con formato F-yyyyMMddHHmmssfff.
        /// (Simple, sin punto de venta real — para un TP alcanza).
        /// </summary>
        private static string GenerarNumeroFactura()
        {
            return "F-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        /// <summary>
        /// Retorna el total con IVA para un carrito (para mostrar en el preview de la pantalla).
        /// </summary>
        public decimal CalcularTotalConIVA(BE_Carrito_GO44 carrito)
        {
            if (carrito == null) return 0m;
            decimal subtotal = carrito.Total;
            decimal iva = Math.Round(subtotal * ALICUOTA_IVA, 2);
            return subtotal + iva;
        }

        public decimal CalcularIVA(BE_Carrito_GO44 carrito)
        {
            if (carrito == null) return 0m;
            return Math.Round(carrito.Total * ALICUOTA_IVA, 2);
        }

        public decimal AlicuotaIVA { get { return ALICUOTA_IVA; } }
    }
}
