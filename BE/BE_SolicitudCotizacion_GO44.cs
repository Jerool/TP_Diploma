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
        public string LoginEncargado { get; set; }
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

        public bool EstaVacia()
        {
            return Lineas == null || Lineas.Count == 0;
        }

        /// <summary>
        /// Agrega una línea. Si el componente ya está, actualiza la cantidad
        /// (requerimiento del CU06 paso 4 — no duplicar líneas).
        /// </summary>
        public void AgregarLinea(BE_Componente_GO44 componente, int cantidad)
        {
            if (componente == null || cantidad <= 0) return;
            foreach (var l in Lineas)
            {
                if (l.Componente != null && l.Componente.Id == componente.Id)
                {
                    l.Cantidad = cantidad;   // actualizar
                    return;
                }
            }
            Lineas.Add(new BE_LineaSolicitudCotizacion_GO44(componente, cantidad));
        }

        public void QuitarLinea(int idComponente)
        {
            if (Lineas == null) return;
            for (int i = Lineas.Count - 1; i >= 0; i--)
            {
                if (Lineas[i].Componente != null && Lineas[i].Componente.Id == idComponente)
                    Lineas.RemoveAt(i);
            }
        }
    }
}
