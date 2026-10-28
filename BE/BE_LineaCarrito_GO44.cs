using System;

namespace BE
{
    /// <summary>
    /// Línea (renglón) de un <see cref="BE_Carrito_GO44"/>: un componente con su cantidad y el precio
    /// unitario al que se vende. Guarda el precio en la propia línea (y no lo lee del componente cada vez)
    /// para "congelar" el precio del momento de la venta aunque luego cambie el del catálogo.
    /// </summary>
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

        /// <summary>Importe de la línea: precio unitario × cantidad. Derivado (no se almacena).</summary>
        public decimal Subtotal
        {
            get { return _PrecioUnitario * _Cantidad; }
        }

        // Atajos de solo lectura para que las grillas muestren datos del componente sin navegar el objeto.
        public string ComponenteCodigo
        {
            get { return _Componente != null ? _Componente.Codigo : string.Empty; }
        }

        public string ComponenteNombre
        {
            get { return _Componente != null ? _Componente.Nombre : string.Empty; }
        }

        /// <summary>Constructor vacío requerido por las DAL para rellenar por propiedades.</summary>
        public BE_LineaCarrito_GO44() { }

        /// <summary>Crea la línea tomando el precio unitario del componente en ese instante (precio congelado).</summary>
        public BE_LineaCarrito_GO44(BE_Componente_GO44 componente, int cantidad)
        {
            Componente = componente;
            Cantidad = cantidad;
            PrecioUnitario = componente != null ? componente.Precio : 0m;
        }
    }
}
