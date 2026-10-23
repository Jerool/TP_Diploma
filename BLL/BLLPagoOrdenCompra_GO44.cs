using BE;
using DAL;
using Servicios;
using System;
using System.Data.SqlClient;

namespace BLL
{
    /// <summary>
    /// BLL de Pago de Orden de Compra — RFN2 CU09 "Pagar OC".
    /// Registra el pago al proveedor y, en la misma transacción, pasa la OC a estado "Pagada".
    /// </summary>
    public class BLLPagoOrdenCompra_GO44
    {
        private readonly DALPagoOrdenCompra_GO44 _dal;
        private readonly DALOrdenCompra_GO44 _dalOC;

        public BLLPagoOrdenCompra_GO44()
        {
            _dal   = new DALPagoOrdenCompra_GO44();
            _dalOC = new DALOrdenCompra_GO44();
        }

        public enum ResultadoPago
        {
            Exitoso,
            SinOrden,
            OrdenNoPendiente,
            MontoInvalido,
            SinComprobante,
            SinSesion,
            Error
        }

        public class PagoVO
        {
            public ResultadoPago Resultado { get; set; }
            public BE_PagoOrdenCompra_GO44 Pago { get; set; }
            public string Mensaje { get; set; }
        }

        private void Auditar(string tipoEvento, string detalle, string criticidad)
        {
            try
            {
                var u = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
                string login = u != null ? u.Login : "SISTEMA";
                BLLBitacora_GO44.Instancia.RegistrarEvento(login, "Compras", tipoEvento, detalle, criticidad);
            }
            catch { }
        }

        public PagoVO PagarOrden(BE_OrdenCompra_GO44 orden, BE_PagoOrdenCompra_GO44 pago)
        {
            var vo = new PagoVO();

            if (orden == null || orden.Id <= 0)
            {
                vo.Resultado = ResultadoPago.SinOrden;
                vo.Mensaje = "Debe seleccionar una orden de compra";
                return vo;
            }
            if (orden.Estado != BE_OrdenCompra_GO44.EstadoOrden.Pendiente)
            {
                vo.Resultado = ResultadoPago.OrdenNoPendiente;
                vo.Mensaje = "La orden " + orden.NumeroOC + " no está pendiente de pago (estado: " + orden.Estado + ")";
                return vo;
            }

            var usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (usuario == null)
            {
                vo.Resultado = ResultadoPago.SinSesion;
                vo.Mensaje = "No hay sesión activa";
                return vo;
            }

            if (pago == null || pago.MontoAbonado <= 0m || pago.MontoAbonado < orden.Total)
            {
                vo.Resultado = ResultadoPago.MontoInvalido;
                vo.Mensaje = string.Format("El monto abonado ({0}) debe cubrir el total de la orden ({1})",
                    (pago != null ? pago.MontoAbonado.ToString("N2") : "0.00"), orden.Total.ToString("N2"));
                return vo;
            }
            if (string.IsNullOrWhiteSpace(pago.NroComprobante))
            {
                vo.Resultado = ResultadoPago.SinComprobante;
                vo.Mensaje = "Debe ingresar el número de comprobante de pago";
                return vo;
            }

            pago.IdOrdenCompra = orden.Id;
            pago.LoginEncargado = usuario.Login;
            // Datos bancarios del proveedor (snapshot al momento del pago)
            if (orden.Proveedor != null)
            {
                if (string.IsNullOrWhiteSpace(pago.CVU))     pago.CVU     = orden.Proveedor.CVU;
                if (string.IsNullOrWhiteSpace(pago.Banco))   pago.Banco   = orden.Proveedor.Banco;
                if (string.IsNullOrWhiteSpace(pago.Titular)) pago.Titular = orden.Proveedor.Titular;
            }

            SqlTransaction tx = null;
            try
            {
                tx = _dal.IniciarTransaccion();

                int idPago = _dal.InsertarEnTx(pago, tx);
                if (idPago <= 0) throw new Exception("No se pudo registrar el pago");
                pago.Id = idPago;

                _dalOC.CambiarEstadoEnTx(orden.Id, BE_OrdenCompra_GO44.EstadoOrden.Pagada.ToString(), tx);

                _dal.ConfirmarTransaccion(tx);
                tx = null;

                orden.Estado = BE_OrdenCompra_GO44.EstadoOrden.Pagada;

                Auditar("Orden de compra pagada",
                        "OC " + orden.NumeroOC + " · Monto $" + pago.MontoAbonado.ToString("N2") +
                        " · Comprobante " + pago.NroComprobante, "Media");

                vo.Resultado = ResultadoPago.Exitoso;
                vo.Pago = pago;
                vo.Mensaje = "Pago de la orden " + orden.NumeroOC + " registrado correctamente";
                return vo;
            }
            catch (Exception ex)
            {
                try { _dal.CancelarTransaccion(tx); } catch { }
                Auditar("Pago de orden cancelado", "Rollback por error: " + ex.Message, "Alta");
                vo.Resultado = ResultadoPago.Error;
                vo.Mensaje = "Error al registrar el pago: " + ex.Message;
                return vo;
            }
        }

        public BE_PagoOrdenCompra_GO44 BuscarPagoDeOrden(int idOrdenCompra) => _dal.BuscarPorOrden(idOrdenCompra);
    }
}
