using System;
using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Solicitud de Cotización (SC) — RFN2, CU06.
    /// Documento que el Encargado de Compras envía al proveedor para pedirle
    /// precios de componentes a reponer. No modifica stock ni genera obligaciones
    /// hasta que se convierte en Orden de Compra (CU08).
    /// </summary>
    public class BE_SolicitudCotizacion_GO44
    {
        public enum EstadoSolicitud
        {
            Pendiente,    // Recién creada, esperando respuesta del proveedor
            Cotizada,     // El proveedor respondió con precios/condiciones
            Cancelada     // Se descartó
        }

        public int Id { get; set; }
        /// <summary>Formato: SC-000001. Lo arma el SP a partir del Id autoincremental.</summary>
        public string NumeroSC { get; set; }
        public DateTime FechaEmision { get; set; }
        public BE_Proveedor_GO44 Proveedor { get; set; }
        public string UserNameEncargado { get; set; }
        public EstadoSolicitud Estado { get; set; }
        public List<BE_LineaSolicitudCotizacion_GO44> Lineas { get; set; }

        public BE_SolicitudCotizacion_GO44()
        {
            FechaEmision = DateTime.Now;
            Estado = EstadoSolicitud.Pendiente;
            Lineas = new List<BE_LineaSolicitudCotizacion_GO44>();
        }

        public int CantidadItems
        {
            get
            {
                int total = 0;
                if (Lineas != null)
                    foreach (var l in Lineas) total += l.Cantidad;
                return total;
            }
        }

    }
}
