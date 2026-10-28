using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    /// <summary>
    /// BLL del Reporte Inteligente "Estrategia de Ventas" (Rep. 3).
    /// Calcula las alertas (reposición urgente / stock muerto) y el descuento sugerido a partir
    /// de las métricas crudas y del tiempo de entrega estimado del proveedor.
    /// </summary>
    public class BLLReporteInteligente_GO44
    {
        private readonly DALReporteInteligente_GO44 _dal;

        /// <summary>Días de entrega del proveedor por defecto (no hay dato por proveedor en el sistema).</summary>
        public const int LEAD_TIME_DEFAULT = 7;

        // A partir de cuántos días sin vender se considera que un producto es "stock muerto".
        private const int DIAS_STOCK_MUERTO = 60;

        public BLLReporteInteligente_GO44()
        {
            _dal = new DALReporteInteligente_GO44();
        }

        /// <summary>
        /// Genera el reporte: trae las métricas crudas de la DAL y, por cada producto, decide una alerta
        /// ("Reponer urgente", "Stock muerto" u "OK") y un descuento sugerido. La inteligencia está acá,
        /// en la BLL, no en la base. El lead time (días de entrega) ajusta el umbral de reposición.
        /// </summary>
        public List<BE_ReporteInteligente_GO44> Generar(int leadTimeDias = LEAD_TIME_DEFAULT)
        {
            if (leadTimeDias < 1) leadTimeDias = LEAD_TIME_DEFAULT; // valor inválido → default

            var lista = _dal.Listar();
            foreach (var r in lista)
            {
                // Reposición urgente: Existencia < Venta diaria promedio * Tiempo de entrega.
                bool urgente = r.VentaDiaria > 0 && r.StockActual < r.VentaDiaria * leadTimeDias;

                // Stock muerto: nunca vendido (con stock) o sin ventas hace más de 60 días.
                bool muerto = (!r.UltimaVenta.HasValue && r.StockActual > 0)
                              || (r.DiasSinVenta > DIAS_STOCK_MUERTO);

                if (urgente)
                {
                    r.Alerta = "Reponer urgente";
                    r.DescuentoSugerido = 0m;
                }
                else if (muerto)
                {
                    r.Alerta = "Stock muerto";
                    // Descuento escalonado: arranca en 10% y sube 1% por cada 10 días extra sin vender,
                    // con tope entre 10% y 30%. Si nunca se vendió (DiasSinVenta < 0) se asume 90 días.
                    int dias = r.DiasSinVenta < 0 ? 90 : r.DiasSinVenta;
                    decimal desc = 10m + (dias - DIAS_STOCK_MUERTO) / 10m;
                    if (desc < 10m) desc = 10m;
                    if (desc > 30m) desc = 30m;
                    r.DescuentoSugerido = desc;
                }
                else
                {
                    r.Alerta = "OK";
                    r.DescuentoSugerido = 0m;
                }
            }
            return lista;
        }

        /// <summary>
        /// Los N componentes más vendidos (últimos 30 días), para el gráfico.
        /// </summary>
        public List<BE_ReporteInteligente_GO44> MasVendidos(List<BE_ReporteInteligente_GO44> datos, int top = 8)
        {
            return datos.Where(x => x.Vendidas30 > 0)
                        .OrderByDescending(x => x.Vendidas30)
                        .Take(top)
                        .ToList();
        }

        /// <summary>
        /// Arma el plan de promociones a partir de los datos ya calculados (sin ir a la BD):
        /// liquidaciones de stock muerto, combos (top-seller + producto parado) y reposiciones.
        /// </summary>
        public List<BE_Promocion_GO44> ArmarPromociones(List<BE_ReporteInteligente_GO44> datos)
        {
            var promos = new List<BE_Promocion_GO44>();
            if (datos == null) return promos;

            var topSellers = datos.Where(x => x.Vendidas30 > 0).OrderByDescending(x => x.Vendidas30).ToList();
            var muertos    = datos.Where(x => x.Alerta == "Stock muerto").OrderByDescending(x => x.DiasSinVenta).ToList();
            var urgentes   = datos.Where(x => x.Alerta == "Reponer urgente").ToList();

            // 1) Liquidaciones de stock muerto
            foreach (var m in muertos)
            {
                promos.Add(new BE_Promocion_GO44
                {
                    Tipo = "Liquidación",
                    Titulo = "Liquidar " + m.Nombre,
                    Detalle = (m.DiasSinVenta < 0 ? "Sin ventas registradas" : m.DiasSinVenta + " días sin venta") +
                              " · stock " + m.StockActual + " u.",
                    Descuento = m.DescuentoSugerido
                });
            }

            // 2) Combos: emparejar cada top-seller con un producto parado para "empujarlo".
            // Se prefiere un parado de la MISMA categoría; si no hay, se toma cualquiera. El pool evita
            // usar dos veces el mismo producto muerto.
            var pool = new List<BE_ReporteInteligente_GO44>(muertos);
            foreach (var top in topSellers.Take(5))
            {
                if (pool.Count == 0) break;
                var dead = pool.FirstOrDefault(d => d.Categoria == top.Categoria) ?? pool[0];
                pool.Remove(dead);
                decimal desc = Math.Max(10m, dead.DescuentoSugerido);
                promos.Add(new BE_Promocion_GO44
                {
                    Tipo = "Combo",
                    Titulo = "Combo: " + top.Nombre + " + " + dead.Nombre,
                    Detalle = "Llevando el más vendido (" + top.Nombre + ") + " + dead.Nombre +
                              " con " + desc.ToString("N0") + "% en el 2do producto",
                    Descuento = desc
                });
            }

            // 3) Reposiciones urgentes
            foreach (var u in urgentes)
            {
                promos.Add(new BE_Promocion_GO44
                {
                    Tipo = "Reposición",
                    Titulo = "Reponer " + u.Nombre,
                    Detalle = "Stock " + u.StockActual + " u. insuficiente para la demanda (vende " + u.VentaDiariaTxt + "/día)",
                    Descuento = 0m
                });
            }

            return promos;
        }
    }
}
