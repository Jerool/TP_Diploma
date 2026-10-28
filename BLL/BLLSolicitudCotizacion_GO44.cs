using BE;
using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace BLL
{
    /// <summary>
    /// BLL de Solicitud de Cotización — CU06 "Generar solicitud de cotización".
    /// Persistencia atómica (encabezado + líneas en una sola transacción).
    /// </summary>
    public class BLLSolicitudCotizacion_GO44
    {
        private readonly DALSolicitudCotizacion_GO44 _dal;

        public BLLSolicitudCotizacion_GO44()
        {
            _dal = new DALSolicitudCotizacion_GO44();
        }

        /// <summary>Resultados posibles de generar una SC (éxito o motivo del rechazo).</summary>
        public enum ResultadoGeneracion
        {
            Exitoso,
            SinLineas,
            SinProveedor,
            SinSesion,
            CantidadInvalida,
            Error
        }

        /// <summary>Resultado de GenerarSolicitud: estado, la SC generada (con su número) y un mensaje para la UI.</summary>
        public class GeneracionVO
        {
            public ResultadoGeneracion Resultado { get; set; }
            public BE_SolicitudCotizacion_GO44 Solicitud { get; set; }
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
        /// Genera una Solicitud de Cotización (CU06). Valida que tenga líneas, proveedor, sesión activa y
        /// cantidades entre 1 y 999; luego persiste encabezado + líneas en UNA transacción (o rollback total).
        /// Tras el commit recupera la SC para obtener el número (NumeroSC) que generó el SP, y audita.
        /// </summary>
        public GeneracionVO GenerarSolicitud(BE_SolicitudCotizacion_GO44 sc)
        {
            var vo = new GeneracionVO();

            if (sc == null || sc.EstaVacia())
            {
                vo.Resultado = ResultadoGeneracion.SinLineas;
                vo.Mensaje = "La solicitud no tiene componentes cargados";
                return vo;
            }
            if (sc.Proveedor == null || string.IsNullOrWhiteSpace(sc.Proveedor.CUIT))
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
            sc.LoginEncargado = usuario.Login;

            // Validar cantidades (1-999 según CU06)
            foreach (var l in sc.Lineas)
            {
                if (l.Cantidad < 1 || l.Cantidad > 999)
                {
                    vo.Resultado = ResultadoGeneracion.CantidadInvalida;
                    vo.Mensaje = "La cantidad por línea debe estar entre 1 y 999";
                    return vo;
                }
            }

            SqlTransaction tx = null;
            try
            {
                // La BLL habla SOLO con la DAL — la DAL encapsula el Acceso.
                tx = _dal.IniciarTransaccion();

                int idSC = _dal.InsertarEncabezadoEnTx(sc, tx);
                if (idSC <= 0) throw new Exception("No se pudo generar el Id de la solicitud");
                sc.Id = idSC;

                foreach (var linea in sc.Lineas)
                {
                    int filas = _dal.InsertarLineaEnTx(idSC, linea, tx);
                    if (filas == 0) throw new Exception("No se pudo insertar línea para " + linea.ComponenteCodigo);
                }

                _dal.ConfirmarTransaccion(tx);
                tx = null;

                // Recuperar la SC completa (para tener NumeroSC generado por el SP)
                var completa = _dal.BuscarPorId(idSC);
                if (completa != null) sc.NumeroSC = completa.NumeroSC;

                Auditar("Solicitud de cotización generada",
                        $"SC {sc.NumeroSC} · Proveedor {sc.Proveedor.CUIT} · {sc.Lineas.Count} componentes",
                        "Baja");

                vo.Resultado = ResultadoGeneracion.Exitoso;
                vo.Solicitud = sc;
                vo.Mensaje = $"Solicitud {sc.NumeroSC} generada correctamente";
                return vo;
            }
            catch (Exception ex)
            {
                try { _dal.CancelarTransaccion(tx); } catch { }
                vo.Resultado = ResultadoGeneracion.Error;
                vo.Mensaje = "Error al generar solicitud: " + ex.Message;
                return vo;
            }
        }

        /// <summary>Lista todas las SC (delega en la DAL).</summary>
        public List<BE_SolicitudCotizacion_GO44> Listar() => _dal.Listar();

        /// <summary>Trae una SC completa por Id (para generar la OC a partir de ella).</summary>
        public BE_SolicitudCotizacion_GO44 BuscarPorId(int id) => _dal.BuscarPorId(id);
    }
}
