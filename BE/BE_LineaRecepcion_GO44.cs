namespace BE
{
    /// <summary>
    /// Línea de una Recepción de mercadería (RFN2 — CU10).
    /// Registra, por componente, cuánto se había pedido en la OC y cuánto se recibió.
    /// </summary>
    public class BE_LineaRecepcion_GO44
    {
        public int Id { get; set; }
        public int IdRecepcion { get; set; }
        public BE_Componente_GO44 Componente { get; set; }
        public int CantidadPedida { get; set; }
        public int CantidadRecibida { get; set; }

        /// <summary>Diferencia pedida - recibida (positivo = faltante).</summary>
        public int Discrepancia { get { return CantidadPedida - CantidadRecibida; } }

        public string ComponenteCodigo { get { return Componente != null ? Componente.Codigo : ""; } }
        public string ComponenteNombre { get { return Componente != null ? Componente.Nombre : ""; } }
        public string ComponenteCategoria { get { return Componente != null ? Componente.Categoria : ""; } }

        public BE_LineaRecepcion_GO44() { }

        public BE_LineaRecepcion_GO44(BE_Componente_GO44 componente, int cantidadPedida, int cantidadRecibida)
        {
            Componente = componente;
            CantidadPedida = cantidadPedida;
            CantidadRecibida = cantidadRecibida;
        }
    }
}
