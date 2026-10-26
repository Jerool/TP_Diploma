using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    /// <summary>
    /// DAL del Reporte Inteligente (Rep. 3). Solo lectura: trae las métricas crudas por componente.
    /// </summary>
    public class DALReporteInteligente_GO44
    {
        private readonly Acceso _acceso;

        public DALReporteInteligente_GO44()
        {
            _acceso = Acceso.Instancia;
        }

        public List<BE_ReporteInteligente_GO44> Listar()
        {
            DataTable dt = _acceso.leerSP("sp_ReporteInteligente_GO44", null);
            var lista = new List<BE_ReporteInteligente_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new BE_ReporteInteligente_GO44
                {
                    Id          = Convert.ToInt32(row["Id"]),
                    Codigo      = row["Codigo"].ToString(),
                    Nombre      = row["Nombre"].ToString(),
                    Categoria   = row["Categoria"] == DBNull.Value ? null : row["Categoria"].ToString(),
                    StockActual = Convert.ToInt32(row["StockActual"]),
                    StockMinimo = Convert.ToInt32(row["StockMinimo"]),
                    Precio      = Convert.ToDecimal(row["Precio"]),
                    Vendidas30  = Convert.ToInt32(row["Vendidas30"]),
                    UltimaVenta = row["UltimaVenta"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["UltimaVenta"])
                });
            }
            return lista;
        }
    }
}
