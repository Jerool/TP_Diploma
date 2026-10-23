using System;
using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Recepción de mercadería (RFN2 — CU10). Actor: Almacenista.
    /// Al recibir la mercadería de una OC pagada, el almacenista carga la cantidad
    /// efectivamente recibida de cada componente. El sistema suma esas cantidades al stock
    /// y detecta discrepancias respecto a la OC (faltantes).
    /// </summary>
    public class BE_RecepcionMercaderia_GO44
    {
        public int Id { get; set; }
        public int IdOrdenCompra { get; set; }
        public string NumeroOC { get; set; }
        public string LoginAlmacenista { get; set; }
        public DateTime FechaRecepcion { get; set; }
        /// <summary>True si al menos una línea se recibió con menos unidades de las pedidas.</summary>
        public bool ConFaltantes { get; set; }
        public string Observaciones { get; set; }
        public List<BE_LineaRecepcion_GO44> Lineas { get; set; }

        public BE_RecepcionMercaderia_GO44()
        {
            FechaRecepcion = DateTime.Now;
            Lineas = new List<BE_LineaRecepcion_GO44>();
        }

        public bool EstaVacia()
        {
            return Lineas == null || Lineas.Count == 0;
        }

        /// <summary>Recalcula ConFaltantes a partir de las líneas (recibida &lt; pedida).</summary>
        public bool TieneFaltantes()
        {
            if (Lineas == null) return false;
            foreach (var l in Lineas)
                if (l.CantidadRecibida < l.CantidadPedida) return true;
            return false;
        }
    }
}
