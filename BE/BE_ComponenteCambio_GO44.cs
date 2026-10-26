using System;

namespace BE
{
    /// <summary>
    /// Registro histórico de un componente (tabla Componentes_C) — bitácora de cambios SCD2.
    /// Cada alta/modificación/baja de un producto genera una fila nueva; la vigente tiene Act = 1.
    /// </summary>
    public class BE_ComponenteCambio_GO44
    {
        public int Id { get; set; }
        public int IdComponente { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        /// <summary>Estado Activo del producto en ese momento (baja lógica).</summary>
        public bool ActivoProducto { get; set; }
        /// <summary>ALTA · MODIF · BAJA · REACTIV.</summary>
        public string TipoCambio { get; set; }
        public DateTime FechaInicio { get; set; }
        /// <summary>Null si es la versión vigente.</summary>
        public DateTime? FechaFin { get; set; }
        /// <summary>1 = versión vigente del producto; 0 = histórica.</summary>
        public bool Act { get; set; }

        // Columnas de presentación para la grilla (Fecha y Hora separadas, como la bitácora).
        public string FechaCambio { get { return FechaInicio.ToString("dd/MM/yy"); } }
        public string HoraCambio { get { return FechaInicio.ToString("HH:mm"); } }
        /// <summary>Act como 0/1 para mostrar en la grilla.</summary>
        public int ActInt { get { return Act ? 1 : 0; } }
    }
}
