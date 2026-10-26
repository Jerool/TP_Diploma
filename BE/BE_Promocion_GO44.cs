namespace BE
{
    /// <summary>
    /// Sugerencia de promoción/combo generada por el Reporte Inteligente.
    /// Tipo: Combo · Liquidación · Reposición.
    /// </summary>
    public class BE_Promocion_GO44
    {
        public string Tipo { get; set; }
        public string Titulo { get; set; }
        public string Detalle { get; set; }
        /// <summary>% de descuento sugerido (0 si no aplica, ej. reposición).</summary>
        public decimal Descuento { get; set; }

        public string DescuentoTxt { get { return Descuento > 0 ? Descuento.ToString("N0") + "%" : "—"; } }
    }
}
