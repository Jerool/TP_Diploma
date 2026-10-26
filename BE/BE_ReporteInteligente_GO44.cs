using System;

namespace BE
{
    /// <summary>
    /// Fila del Reporte Inteligente "Estrategia de Ventas" (Rep. 3).
    /// Trae datos crudos de ventas/stock por componente; las métricas derivadas se calculan
    /// como propiedades, y la alerta + descuento sugerido las completa la BLL (dependen del
    /// tiempo de entrega del proveedor).
    /// </summary>
    public class BE_ReporteInteligente_GO44
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public decimal Precio { get; set; }
        /// <summary>Unidades vendidas en los últimos 30 días.</summary>
        public int Vendidas30 { get; set; }
        public DateTime? UltimaVenta { get; set; }

        // ===== Métricas derivadas =====

        /// <summary>Promedio de unidades vendidas por día (base 30 días).</summary>
        public double VentaDiaria { get { return Vendidas30 / 30.0; } }

        /// <summary>Índice de rotación: ventas del mes / stock actual (proxy de stock promedio).</summary>
        public double Rotacion { get { return StockActual > 0 ? (double)Vendidas30 / StockActual : 0; } }

        /// <summary>Días estimados hasta agotar el stock. -1 = sin ventas (no proyectable).</summary>
        public int DiasInventario { get { return VentaDiaria > 0 ? (int)Math.Floor(StockActual / VentaDiaria) : -1; } }

        /// <summary>Días desde la última venta. -1 = nunca se vendió.</summary>
        public int DiasSinVenta { get { return UltimaVenta.HasValue ? (int)(DateTime.Now - UltimaVenta.Value).TotalDays : -1; } }

        // ===== Completados por la BLL =====

        /// <summary>OK · Reponer urgente · Stock muerto.</summary>
        public string Alerta { get; set; }
        /// <summary>% de descuento sugerido para liquidar stock muerto (0 si no aplica).</summary>
        public decimal DescuentoSugerido { get; set; }

        // ===== Presentación para la grilla =====
        public string VentaDiariaTxt { get { return VentaDiaria.ToString("N2"); } }
        public string RotacionTxt { get { return Rotacion.ToString("N2"); } }
        public string DiasInventarioTxt { get { return DiasInventario < 0 ? "—" : DiasInventario.ToString(); } }
        public string DiasSinVentaTxt { get { return DiasSinVenta < 0 ? "nunca" : DiasSinVenta.ToString(); } }
        public string DescuentoTxt { get { return DescuentoSugerido > 0 ? DescuentoSugerido.ToString("N0") + "%" : "—"; } }
    }
}
