using System;

namespace BE
{
    /// <summary>
    /// Entidad de negocio (capa BE) que representa un COMPONENTE / producto del catálogo
    /// (la tabla Componentes de la base). Es un objeto de transporte de datos (POCO): solo
    /// guarda el estado del producto y expone un par de ayudas de dominio; no accede a la base
    /// ni contiene reglas de negocio pesadas (eso vive en la capa BLL).
    /// </summary>
    public class BE_Componente_GO44
    {
        // Identificador interno (clave primaria en la base). 0 = componente todavía no persistido.
        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        // Código visible del producto (ej. "CPU-001"). Es el identificador "de negocio" que ve el usuario.
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

        // Unidades que hay hoy en depósito. Lo suben las recepciones (CU10) y lo bajan las ventas.
        private int _StockActual;
        public int StockActual
        {
            get { return _StockActual; }
            set { _StockActual = value; }
        }

        // Umbral de reposición: si StockActual cae por debajo, el producto se considera "a reponer"
        // (lo usa el reporte inteligente para sugerir compras).
        private int _StockMinimo;
        public int StockMinimo
        {
            get { return _StockMinimo; }
            set { _StockMinimo = value; }
        }

        // Baja lógica: false = producto dado de baja. Nunca se borra físicamente la fila.
        private bool _Activo;
        public bool Activo
        {
            get { return _Activo; }
            set { _Activo = value; }
        }

        /// <summary>
        /// Indica si hay stock suficiente para cubrir la cantidad pedida.
        /// Ayuda de dominio usada por el carrito/venta antes de descontar stock.
        /// </summary>
        /// <param name="cantidadRequerida">Unidades que se quieren vender/reservar.</param>
        /// <returns>true si StockActual alcanza o supera lo requerido.</returns>
        public bool StockDisponible(int cantidadRequerida)
        {
            return _StockActual >= cantidadRequerida;
        }

        /// <summary>Constructor vacío: necesario para que las DAL puedan instanciar y rellenar la entidad por propiedades.</summary>
        public BE_Componente_GO44() { }

        /// <summary>Constructor de conveniencia para crear un componente nuevo; lo deja activo por defecto.</summary>
        public BE_Componente_GO44(int id, string codigo, string nombre, decimal precio, int stockActual)
        {
            Id = id;
            Codigo = codigo;
            Nombre = nombre;
            Precio = precio;
            StockActual = stockActual;
            Activo = true; // todo componente recién creado arranca habilitado
        }

        /// <summary>Representación legible ("Código - Nombre"); se usa al mostrarlo en combos y grillas.</summary>
        public override string ToString()
        {
            // El operador ?? evita un NullReferenceException si alguno todavía no fue seteado.
            return (_Codigo ?? string.Empty) + " - " + (_Nombre ?? string.Empty);
        }
    }
}
