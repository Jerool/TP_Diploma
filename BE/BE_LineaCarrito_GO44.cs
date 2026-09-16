using System;

namespace BE
{
    public class BE_LineaCarrito_GO44
    {
        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        private int _IdCarrito;
        public int IdCarrito
        {
            get { return _IdCarrito; }
            set { _IdCarrito = value; }
        }

        private BE_Componente_GO44 _Componente;
        public BE_Componente_GO44 Componente
        {
            get { return _Componente; }
            set { _Componente = value; }
        }

        private int _Cantidad;
        public int Cantidad
        {
            get { return _Cantidad; }
            set { _Cantidad = value; }
        }

        private decimal _PrecioUnitario;
        public decimal PrecioUnitario
        {
            get { return _PrecioUnitario; }
            set { _PrecioUnitario = value; }
        }

        public decimal Subtotal
        {
            get { return _PrecioUnitario * _Cantidad; }
        }

        public string ComponenteCodigo
        {
            get { return _Componente != null ? _Componente.Codigo : string.Empty; }
        }

        public string ComponenteNombre
        {
            get { return _Componente != null ? _Componente.Nombre : string.Empty; }
        }

        public BE_LineaCarrito_GO44() { }

        public BE_LineaCarrito_GO44(BE_Componente_GO44 componente, int cantidad)
        {
            Componente = componente;
            Cantidad = cantidad;
            PrecioUnitario = componente != null ? componente.Precio : 0m;
        }
    }
}
