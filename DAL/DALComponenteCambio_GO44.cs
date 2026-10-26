using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL de la bitácora de cambios de productos (tabla Componentes_C).
    /// Solo lectura con filtros + marcar versión vigente; las altas de historial las generan
    /// los triggers de la BD (no se insertan desde la app).
    /// </summary>
    public class DALComponenteCambio_GO44
    {
        private readonly Acceso _acceso;

        public DALComponenteCambio_GO44()
        {
            _acceso = Acceso.Instancia;
        }

        public List<BE_ComponenteCambio_GO44> ListarConFiltros(string codigo, string nombre, DateTime? fechaIni, DateTime? fechaFin)
        {
            SqlParameter[] p = {
                new SqlParameter("@Codigo",   string.IsNullOrWhiteSpace(codigo) ? (object)DBNull.Value : codigo),
                new SqlParameter("@Nombre",   string.IsNullOrWhiteSpace(nombre) ? (object)DBNull.Value : nombre),
                new SqlParameter("@FechaIni", (object)fechaIni ?? DBNull.Value),
                new SqlParameter("@FechaFin", (object)fechaFin ?? DBNull.Value)
            };
            DataTable dt = _acceso.leerSP("sp_ComponenteCambio_ListarConFiltros_GO44", p);
            var lista = new List<BE_ComponenteCambio_GO44>();
            foreach (DataRow row in dt.Rows) lista.Add(Mapear(row));
            return lista;
        }

        /// <summary>Marca una versión como la vigente (Act = 1) y las demás del producto como históricas.</summary>
        public int MarcarActivo(int idCambio)
        {
            SqlParameter[] p = { new SqlParameter("@Id", idCambio) };
            object r = _acceso.leerEscalarSP("sp_ComponenteCambio_MarcarActivo_GO44", p);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        private BE_ComponenteCambio_GO44 Mapear(DataRow row)
        {
            return new BE_ComponenteCambio_GO44
            {
                Id             = Convert.ToInt32(row["Id"]),
                IdComponente   = Convert.ToInt32(row["IdComponente"]),
                Codigo         = row["Codigo"].ToString(),
                Nombre         = row["Nombre"].ToString(),
                Descripcion    = row["Descripcion"] == DBNull.Value ? null : row["Descripcion"].ToString(),
                Categoria      = row["Categoria"]   == DBNull.Value ? null : row["Categoria"].ToString(),
                Precio         = Convert.ToDecimal(row["Precio"]),
                StockActual    = Convert.ToInt32(row["StockActual"]),
                StockMinimo    = Convert.ToInt32(row["StockMinimo"]),
                ActivoProducto = Convert.ToBoolean(row["ActivoProducto"]),
                TipoCambio     = row["TipoCambio"].ToString(),
                FechaInicio    = Convert.ToDateTime(row["FechaInicio"]),
                FechaFin       = row["FechaFin"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["FechaFin"]),
                Act            = Convert.ToBoolean(row["Act"])
            };
        }
    }
}
