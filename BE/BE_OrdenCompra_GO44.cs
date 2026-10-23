using System;
using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Orden de Compra (OC) — RFN2, CU08.
    /// Se genera a partir de una Solicitud de Cotización (CU06): se toman sus líneas,
    /// el encargado completa el precio unitario de cada componente y la fecha de entrega.
    /// No modifica stock; el stock se actualiza recién al recibir la mercadería (CU10).
    /// </summary>
    public class BE_OrdenCompra_GO44
    {
        public enum EstadoOrden
        {
            Pendiente,              // Generada, falta pagarla
            Pagada,                 // Se registró el pago al proveedor (CU09)
            Recibida,               // Entró toda la mercadería (CU10)
            RecibidaConFaltantes,   // Entró con discrepancias respecto a la OC (CU10)
            Cancelada               // Se descartó
        }

        public int Id { get; set; }
        /// <summary>Formato: OC-000001. Lo arma el SP a partir del Id autoincremental.</summary>
        public string NumeroOC { get; set; }
        /// <summary>SC de origen (CU06). Puede ser 0 si se generó sin SC.</summary>
        public int IdSolicitud { get; set; }
        public string NumeroSC { get; set; }
        public BE_Proveedor_GO44 Proveedor { get; set; }
        public string LoginEncargado { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaEntrega { get; set; }
        public decimal Total { get; set; }
        public EstadoOrden Estado { get; set; }
        public List<BE_LineaOrdenCompra_GO44> Lineas { get; set; }

        public BE_OrdenCompra_GO44()
        {
            FechaEmision = DateTime.Now;
            FechaEntrega = DateTime.Now.AddDays(7);
            Estado = EstadoOrden.Pendiente;
            Lineas = new List<BE_LineaOrdenCompra_GO44>();
        }

        public bool EstaVacia()
        {
            return Lineas == null || Lineas.Count == 0;
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

        /// <summary>Recalcula el total sumando los subtotales de las líneas.</summary>
        public decimal RecalcularTotal()
        {
            decimal t = 0m;
            if (Lineas != null)
                foreach (var l in Lineas) t += l.Subtotal;
            Total = t;
            return t;
        }
    }
}
