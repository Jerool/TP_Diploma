using System;

namespace BE
{
    public class BE_Componente_GO44
    {
        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        private string _Codigo;
        public string Codigo
        {
            get { return _Codigo; }
            set { _Codigo = value; }
        }

        private string _Nombre;
        public string Nombre
        {
            get { return _Nombre; }
            set { _Nombre = value; }
        }

        private string _Descripcion;
        public string Descripcion
        {
            get { return _Descripcion; }
            set { _Descripcion = value; }
        }

        private string _Categoria;
        public string Categoria
        {
            get { return _Categoria; }
            set { _Categoria = value; }
        }

        private decimal _Precio;
        public decimal Precio
        {
            get { return _Precio; }
            set { _Precio = value; }
        }

        private int _StockActual;
        public int StockActual
        {
            get { return _StockActual; }
            set { _StockActual = value; }
        }

        private int _StockMinimo;
        public int StockMinimo
        {
            get { return _StockMinimo; }
            set { _StockMinimo = value; }
        }

        private bool _Activo;
        public bool Activo
        {
            get { return _Activo; }
            set { _Activo = value; }
        }

        public bool StockDisponible(int cantidadRequerida)
        {
            return _StockActual >= cantidadRequerida;
        }

        public BE_Componente_GO44() { }

        public BE_Componente_GO44(int id, string codigo, string nombre, decimal precio, int stockActual)
        {
            Id = id;
            Codigo = codigo;
            Nombre = nombre;
            Precio = precio;
            StockActual = stockActual;
            Activo = true;
        }

        public override string ToString()
        {
            return (_Codigo ?? string.Empty) + " - " + (_Nombre ?? string.Empty);
        }
    }
}
