namespace BE
{
    public class BE_LineaFactura_GO44
    {
        private int _Id;
        public int Id { get { return _Id; } set { _Id = value; } }

        private int _IdFactura;
        public int IdFactura { get { return _IdFactura; } set { _IdFactura = value; } }

        private BE_Componente_GO44 _Componente;
        public BE_Componente_GO44 Componente { get { return _Componente; } set { _Componente = value; } }

        private int _Cantidad;
        public int Cantidad { get { return _Cantidad; } set { _Cantidad = value; } }

        private decimal _PrecioUnitario;
        public decimal PrecioUnitario { get { return _PrecioUnitario; } set { _PrecioUnitario = value; } }

        private decimal _Subtotal;
        public decimal Subtotal { get { return _Subtotal; } set { _Subtotal = value; } }

        public string ComponenteCodigo { get { return _Componente != null ? _Componente.Codigo : string.Empty; } }
        public string ComponenteNombre { get { return _Componente != null ? _Componente.Nombre : string.Empty; } }

        public BE_LineaFactura_GO44() { }

        public BE_LineaFactura_GO44(BE_Componente_GO44 componente, int cantidad, decimal precioUnitario)
        {
            Componente = componente;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Subtotal = precioUnitario * cantidad;
        }
    }
}
