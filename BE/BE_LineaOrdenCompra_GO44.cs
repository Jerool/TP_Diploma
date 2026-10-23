namespace BE
{
    /// <summary>
    /// Línea de una Orden de Compra (RFN2 — CU08).
    /// Reutiliza el patrón de BE_LineaSolicitudCotizacion_GO44, agregando precio unitario
    /// (lo cotiza el proveedor) y subtotal.
    /// </summary>
    public class BE_LineaOrdenCompra_GO44
    {
        public int Id { get; set; }
        public int IdOrdenCompra { get; set; }
        public BE_Componente_GO44 Componente { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get { return Cantidad * PrecioUnitario; } }

        // Snapshot para la grilla (evita null si el componente se da de baja después)
        public string ComponenteCodigo { get { return Componente != null ? Componente.Codigo : ""; } }
        public string ComponenteNombre { get { return Componente != null ? Componente.Nombre : ""; } }
        public string ComponenteCategoria { get { return Componente != null ? Componente.Categoria : ""; } }

        public BE_LineaOrdenCompra_GO44() { }

        public BE_LineaOrdenCompra_GO44(BE_Componente_GO44 componente, int cantidad, decimal precioUnitario)
        {
            Componente = componente;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
        }
    }
}
