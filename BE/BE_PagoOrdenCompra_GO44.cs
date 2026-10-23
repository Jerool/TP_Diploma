using System;

namespace BE
{
    /// <summary>
    /// Pago de una Orden de Compra (RFN2 — CU09 "Pagar OC").
    /// Registra el pago al proveedor usando sus datos bancarios (CVU, Banco, Titular).
    /// Al registrarse, la OC pasa a estado "Pagada".
    /// </summary>
    public class BE_PagoOrdenCompra_GO44
    {
        public int Id { get; set; }
        public int IdOrdenCompra { get; set; }
        public string CVU { get; set; }
        public string Banco { get; set; }
        public string Titular { get; set; }
        public decimal MontoAbonado { get; set; }
        public DateTime FechaOperacion { get; set; }
        public string NroComprobante { get; set; }
        /// <summary>Siempre "Pagado" en esta versión (pago único y total).</summary>
        public string Estado { get; set; }
        public string LoginEncargado { get; set; }

        public BE_PagoOrdenCompra_GO44()
        {
            FechaOperacion = DateTime.Now;
            Estado = "Pagado";
        }
    }
}
