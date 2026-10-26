using BE;
using DAL;
using Servicios;
using System;
using System.Data.SqlClient;

namespace BLL
{
    /// <summary>
    /// BLL de Recepción de mercadería — RFN2 CU10. Actor: Almacenista.
    /// Al recibir la mercadería de una OC pagada, suma al stock la cantidad recibida de cada
    /// componente y detecta discrepancias (faltantes) respecto a la OC. Todo atómico vía DAL:
    /// encabezado + líneas + aumento de stock + cambio de estado de la OC.
    /// </summary>
    public class BLLRecepcionMercaderia_GO44
    {
        private readonly DALRecepcionMercaderia_GO44 _dal;
        private readonly DALOrdenCompra_GO44 _dalOC;
        private readonly DALComponente_GO44 _dalComponente;
        private readonly BLLIntegridad_GO44 _bllIntegridad;

        public BLLRecepcionMercaderia_GO44()
        {
            _dal           = new DALRecepcionMercaderia_GO44();
            _dalOC         = new DALOrdenCompra_GO44();
            _dalComponente = new DALComponente_GO44();
            _bllIntegridad = new BLLIntegridad_GO44();
        }

        public enum ResultadoRecepcion
        {
            Exitoso,
            SinOrden,
            OrdenNoPagada,
            SinLineas,
            CantidadInvalida,
            SinSesion,
            Error
        }

        public class RecepcionVO
        {
            public ResultadoRecepcion Resultado { get; set; }
            public BE_RecepcionMercaderia_GO44 Recepcion { get; set; }
            public bool ConFaltantes { get; set; }
            public string Mensaje { get; set; }
        }

        private void Auditar(string tipoEvento, string detalle, string criticidad)
        {
            try
            {
                var u = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
                string login = u != null ? u.Login : "SISTEMA";
                BLLBitacora_GO44.Instancia.RegistrarEvento(login, "Almacen", tipoEvento, detalle, criticidad);
            }
            catch { }
        }

        /// <summary>Recepción registrada de una OC (null si todavía no se recibió). Usado por el reporte RFN2.</summary>
        public BE_RecepcionMercaderia_GO44 BuscarPorOrden(int idOrdenCompra) => _dal.BuscarPorOrden(idOrdenCompra);

        public RecepcionVO RecibirMercaderia(BE_OrdenCompra_GO44 orden, BE_RecepcionMercaderia_GO44 recepcion)
        {
            var vo = new RecepcionVO();

            if (orden == null || orden.Id <= 0)
            {
                vo.Resultado = ResultadoRecepcion.SinOrden;
                vo.Mensaje = "Debe seleccionar una orden de compra";
                return vo;
            }
            if (orden.Estado != BE_OrdenCompra_GO44.EstadoOrden.Pagada)
            {
                vo.Resultado = ResultadoRecepcion.OrdenNoPagada;
                vo.Mensaje = "Solo se puede recibir mercadería de una orden pagada (estado actual: " + orden.Estado + ")";
                return vo;
            }
            if (recepcion == null || recepcion.EstaVacia())
            {
                vo.Resultado = ResultadoRecepcion.SinLineas;
                vo.Mensaje = "La recepción no tiene líneas cargadas";
                return vo;
            }

            var usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (usuario == null)
            {
                vo.Resultado = ResultadoRecepcion.SinSesion;
                vo.Mensaje = "No hay sesión activa";
                return vo;
            }

            // Validar cantidades recibidas (0 a pedida; no se puede recibir más de lo pedido)
            foreach (var l in recepcion.Lineas)
            {
                if (l.CantidadRecibida < 0 || l.CantidadRecibida > l.CantidadPedida)
                {
                    vo.Resultado = ResultadoRecepcion.CantidadInvalida;
                    vo.Mensaje = "La cantidad recibida de " + l.ComponenteCodigo +
                                 " debe estar entre 0 y la pedida (" + l.CantidadPedida + ")";
                    return vo;
                }
            }

            recepcion.IdOrdenCompra = orden.Id;
            recepcion.LoginAlmacenista = usuario.Login;
            recepcion.ConFaltantes = recepcion.TieneFaltantes();

            SqlTransaction tx = null;
            try
            {
                tx = _dal.IniciarTransaccion();

                int idRec = _dal.InsertarEncabezadoEnTx(recepcion, tx);
                if (idRec <= 0) throw new Exception("No se pudo generar el Id de la recepción");
                recepcion.Id = idRec;

                foreach (var linea in recepcion.Lineas)
                {
                    int filas = _dal.InsertarLineaEnTx(idRec, linea, tx);
                    if (filas == 0) throw new Exception("No se pudo insertar línea de recepción para " + linea.ComponenteCodigo);

                    // Sumar al stock lo efectivamente recibido (si recibió 0, no toca stock)
                    if (linea.CantidadRecibida > 0)
                    {
                        int fStock = _dalComponente.AumentarStockEnTx(linea.Componente.Id, linea.CantidadRecibida, tx);
                        if (fStock == 0)
                            throw new Exception("No se pudo actualizar el stock de " + linea.ComponenteCodigo);
                    }
                }

                // Estado final de la OC según discrepancias
                var nuevoEstado = recepcion.ConFaltantes
                    ? BE_OrdenCompra_GO44.EstadoOrden.RecibidaConFaltantes
                    : BE_OrdenCompra_GO44.EstadoOrden.Recibida;
                _dalOC.CambiarEstadoEnTx(orden.Id, nuevoEstado.ToString(), tx);

                _dal.ConfirmarTransaccion(tx);
                tx = null;

                orden.Estado = nuevoEstado;

                // Recalcular DVH de Componentes post-commit (cambió el stock) — best-effort
                try { _bllIntegridad.RecalcularTabla("Componentes"); } catch { }

                Auditar(recepcion.ConFaltantes ? "Recepción con faltantes" : "Recepción de mercadería",
                        "OC " + orden.NumeroOC + " · " + recepcion.Lineas.Count + " componentes" +
                        (recepcion.ConFaltantes ? " · CON FALTANTES" : ""),
                        recepcion.ConFaltantes ? "Alta" : "Media");

                vo.Resultado = ResultadoRecepcion.Exitoso;
                vo.Recepcion = recepcion;
                vo.ConFaltantes = recepcion.ConFaltantes;
                vo.Mensaje = recepcion.ConFaltantes
                    ? "Mercadería recibida con faltantes. La orden " + orden.NumeroOC + " quedó en estado 'Recibida con faltantes'."
                    : "Mercadería recibida correctamente. La orden " + orden.NumeroOC + " quedó en estado 'Recibida'.";
                return vo;
            }
            catch (Exception ex)
            {
                try { _dal.CancelarTransaccion(tx); } catch { }
                Auditar("Recepción cancelada", "Rollback por error: " + ex.Message, "Alta");
                vo.Resultado = ResultadoRecepcion.Error;
                vo.Mensaje = "Error al registrar la recepción: " + ex.Message;
                return vo;
            }
        }
    }
}
