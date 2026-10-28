using BE;
using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace BLL
{
    /// <summary>
    /// BLL de Orden de Compra — RFN2 CU08 "Generar Orden de Compra".
    /// La OC se arma a partir de una Solicitud de Cotización (CU06): se toman sus líneas,
    /// el encargado completa el precio unitario cotizado por el proveedor y la fecha de entrega.
    /// Persistencia atómica (encabezado + líneas + marcar SC como Cotizada) vía DAL.
    /// </summary>
    public class BLLOrdenCompra_GO44
    {
        private readonly DALOrdenCompra_GO44 _dal;
        private readonly DALSolicitudCotizacion_GO44 _dalSC;

        public BLLOrdenCompra_GO44()
        {
            _dal   = new DALOrdenCompra_GO44();
            _dalSC = new DALSolicitudCotizacion_GO44();
        }

        /// <summary>Resultados posibles de generar una OC (éxito o motivo del rechazo).</summary>
        public enum ResultadoGeneracion
        {
            Exitoso,
            SinLineas,
            SinProveedor,
            SinSesion,
            PrecioInvalido,
            FechaInvalida,
            Error
        }

        /// <summary>Resultado de GenerarOrden: estado, la OC generada (con su número) y un mensaje.</summary>
        public class GeneracionVO
        {
            public ResultadoGeneracion Resultado { get; set; }
            public BE_OrdenCompra_GO44 Orden { get; set; }
            public string Mensaje { get; set; }
        }

        /// <summary>Registra un evento del módulo Compras en la bitácora (best-effort).</summary>
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

        /// <summary>
        /// Genera una Orden de Compra (CU08). Valida líneas, proveedor, sesión, precios (&gt; 0), cantidades
        /// y fecha de entrega (no pasada). Recalcula el total y persiste encabezado + líneas en UNA transacción;
        /// si la OC viene de una SC, en la misma tx marca esa SC como "Cotizada". Tras el commit toma el número
        /// de OC generado por el SP y audita. Ante cualquier fallo, rollback total.
        /// </summary>
        public GeneracionVO GenerarOrden(BE_OrdenCompra_GO44 oc)
        {
            var vo = new GeneracionVO();

            if (oc == null || oc.EstaVacia())
            {
                vo.Resultado = ResultadoGeneracion.SinLineas;
                vo.Mensaje = "La orden de compra no tiene componentes cargados";
                return vo;
            }
            if (oc.Proveedor == null || string.IsNullOrWhiteSpace(oc.Proveedor.CUIT))
            {
                vo.Resultado = ResultadoGeneracion.SinProveedor;
                vo.Mensaje = "Debe seleccionar un proveedor";
                return vo;
            }

            var usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (usuario == null)
            {
                vo.Resultado = ResultadoGeneracion.SinSesion;
                vo.Mensaje = "No hay sesión activa";
                return vo;
            }
            oc.LoginEncargado = usuario.Login;

            // Validar precios (> 0) y fecha de entrega (no anterior a hoy)
            foreach (var l in oc.Lineas)
            {
                if (l.PrecioUnitario <= 0m)
                {
                    vo.Resultado = ResultadoGeneracion.PrecioInvalido;
                    vo.Mensaje = "El precio unitario de " + l.ComponenteCodigo + " debe ser mayor a 0";
                    return vo;
                }
                if (l.Cantidad < 1)
                {
                    vo.Resultado = ResultadoGeneracion.PrecioInvalido;
                    vo.Mensaje = "La cantidad de " + l.ComponenteCodigo + " debe ser mayor a 0";
                    return vo;
                }
            }
            if (oc.FechaEntrega.Date < DateTime.Now.Date)
            {
                vo.Resultado = ResultadoGeneracion.FechaInvalida;
                vo.Mensaje = "La fecha de entrega no puede ser anterior a hoy";
                return vo;
            }

            oc.RecalcularTotal();

            SqlTransaction tx = null;
            try
            {
                // La BLL habla SOLO con la DAL — la DAL encapsula el Acceso.
                tx = _dal.IniciarTransaccion();

                int idOC = _dal.InsertarEncabezadoEnTx(oc, tx);
                if (idOC <= 0) throw new Exception("No se pudo generar el Id de la orden de compra");
                oc.Id = idOC;

                foreach (var linea in oc.Lineas)
                {
                    int filas = _dal.InsertarLineaEnTx(idOC, linea, tx);
                    if (filas == 0) throw new Exception("No se pudo insertar línea para " + linea.ComponenteCodigo);
                }

                // Si la OC nace de una SC, marcarla como Cotizada (ya se procesó)
                if (oc.IdSolicitud > 0)
                    _dalSC.CambiarEstadoEnTx(oc.IdSolicitud, BE_SolicitudCotizacion_GO44.EstadoSolicitud.Cotizada.ToString(), tx);

                _dal.ConfirmarTransaccion(tx);
                tx = null;

                // Recuperar la OC completa (para tener NumeroOC generado por el SP)
                var completa = _dal.BuscarPorId(idOC);
                if (completa != null) oc.NumeroOC = completa.NumeroOC;

                Auditar("Orden de compra generada",
                        "OC " + oc.NumeroOC + " · Proveedor " + oc.Proveedor.CUIT +
                        " · " + oc.Lineas.Count + " componentes · Total $" + oc.Total.ToString("N2"),
                        "Media");

                vo.Resultado = ResultadoGeneracion.Exitoso;
                vo.Orden = oc;
                vo.Mensaje = "Orden de compra " + oc.NumeroOC + " generada correctamente";
                return vo;
            }
            catch (Exception ex)
            {
                try { _dal.CancelarTransaccion(tx); } catch { }
                Auditar("Orden de compra cancelada", "Rollback por error: " + ex.Message, "Alta");
                vo.Resultado = ResultadoGeneracion.Error;
                vo.Mensaje = "Error al generar la orden de compra: " + ex.Message;
                return vo;
            }
        }

        /// <summary>Lista todas las OC.</summary>
        public List<BE_OrdenCompra_GO44> Listar() => _dal.Listar();

        /// <summary>Lista las OC en un estado dado (ej. Pagada, para la pantalla de Recepción). Convierte el enum a texto para la DAL.</summary>
        public List<BE_OrdenCompra_GO44> ListarPorEstado(BE_OrdenCompra_GO44.EstadoOrden estado)
            => _dal.ListarPorEstado(estado.ToString());

        /// <summary>Trae una OC completa por Id.</summary>
        public BE_OrdenCompra_GO44 BuscarPorId(int id) => _dal.BuscarPorId(id);
    }
}
