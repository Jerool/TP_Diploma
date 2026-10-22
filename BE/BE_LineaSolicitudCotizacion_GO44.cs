namespace BE
{
    /// <summary>
    /// Línea de una Solicitud de Cotización (RFN2 — PN2 paso 4).
    /// Reutiliza el patrón de BE_LineaCarrito_GO44 y BE_LineaFactura_GO44.
    /// </summary>
    public class BE_LineaSolicitudCotizacion_GO44
    {
        public int Id { get; set; }
        public int IdSolicitud { get; set; }
        public BE_Componente_GO44 Componente { get; set; }
        public int Cantidad { get; set; }

        // Snapshot para la grilla (evita null si el componente se da de baja después)
        public string ComponenteCodigo { get { return Componente != null ? Componente.Codigo : ""; } }
        public string ComponenteNombre { get { return Componente != null ? Componente.Nombre : ""; } }
        public string ComponenteCategoria { get { return Componente != null ? Componente.Categoria : ""; } }

        public BE_LineaSolicitudCotizacion_GO44() { }

        public BE_LineaSolicitudCotizacion_GO44(BE_Componente_GO44 componente, int cantidad)
        {
            Componente = componente;
            Cantidad = cantidad;
        }
    }
}
