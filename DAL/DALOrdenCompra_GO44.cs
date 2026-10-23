using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL de Orden de Compra — RFN2 CU08.
    /// Expone wrappers de transacción (encapsula Acceso) para que la BLL nunca lo toque directo.
    /// Reutiliza DALComponente para mapear las líneas.
    /// </summary>
    public class DALOrdenCompra_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALComponente_GO44 _dalComponente;

        public DALOrdenCompra_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalComponente = new DALComponente_GO44();
        }

        // ============ Transacción (encapsula Acceso para no romper las capas) ============

        public SqlTransaction IniciarTransaccion()          => _acceso.IniciarTransaccion();
        public void ConfirmarTransaccion(SqlTransaction tx) => _acceso.ConfirmarTransaccion(tx);
        public void CancelarTransaccion(SqlTransaction tx)  => _acceso.CancelarTransaccion(tx);

        // ============ Alta dentro de tx ============

        /// <summary>Inserta el encabezado de la OC y devuelve el Id autogenerado.</summary>
        public int InsertarEncabezadoEnTx(BE_OrdenCompra_GO44 oc, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdSolicitud",    oc.IdSolicitud > 0 ? (object)oc.IdSolicitud : DBNull.Value),
                new SqlParameter("@CuitProveedor",  oc.Proveedor.CUIT),
                new SqlParameter("@LoginEncargado", oc.LoginEncargado),
                new SqlParameter("@FechaEntrega",   oc.FechaEntrega),
                new SqlParameter("@Total",          oc.Total)
            };
            object r = _acceso.leerEscalarSPEnTx("sp_OrdenCompra_Insertar_GO44", p, tx);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        public int InsertarLineaEnTx(int idOrdenCompra, BE_LineaOrdenCompra_GO44 linea, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdOrdenCompra",  idOrdenCompra),
                new SqlParameter("@IdComponente",   linea.Componente.Id),
                new SqlParameter("@Cantidad",       linea.Cantidad),
                new SqlParameter("@PrecioUnitario", linea.PrecioUnitario),
                new SqlParameter("@Subtotal",       linea.Subtotal)
            };
            return _acceso.escribirSPEnTx("sp_LineaOrdenCompra_Insertar_GO44", p, tx);
        }

        /// <summary>Cambia el estado de la OC (fuera o dentro de tx según el caso de uso).</summary>
        public int CambiarEstado(int idOrdenCompra, string estado)
        {
            SqlParameter[] p = {
                new SqlParameter("@Id",     idOrdenCompra),
                new SqlParameter("@Estado", estado)
            };
            return _acceso.escribirSP("sp_OrdenCompra_CambiarEstado_GO44", p);
        }

        public int CambiarEstadoEnTx(int idOrdenCompra, string estado, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@Id",     idOrdenCompra),
                new SqlParameter("@Estado", estado)
            };
            return _acceso.escribirSPEnTx("sp_OrdenCompra_CambiarEstado_GO44", p, tx);
        }

        // ============ Consultas ============

        public List<BE_OrdenCompra_GO44> Listar()
        {
            DataTable dt = _acceso.leerSP("sp_OrdenCompra_Listar_GO44", null);
            return MapearLista(dt);
        }

        public List<BE_OrdenCompra_GO44> ListarPorEstado(string estado)
        {
            SqlParameter[] p = { new SqlParameter("@Estado", estado) };
            DataTable dt = _acceso.leerSP("sp_OrdenCompra_ListarPorEstado_GO44", p);
            return MapearLista(dt);
        }

        public BE_OrdenCompra_GO44 BuscarPorId(int id)
        {
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            DataTable dt = _acceso.leerSP("sp_OrdenCompra_BuscarPorId_GO44", p);
            if (dt.Rows.Count == 0) return null;
            var oc = MapearEncabezado(dt.Rows[0]);
            oc.Lineas = ListarLineas(id);
            return oc;
        }

        public List<BE_LineaOrdenCompra_GO44> ListarLineas(int idOrdenCompra)
        {
            SqlParameter[] p = { new SqlParameter("@IdOrdenCompra", idOrdenCompra) };
            DataTable dt = _acceso.leerSP("sp_LineaOrdenCompra_ListarPorOrden_GO44", p);
            var lista = new List<BE_LineaOrdenCompra_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                var comp = _dalComponente.BuscarPorId(Convert.ToInt32(row["IdComponente"]));
                lista.Add(new BE_LineaOrdenCompra_GO44
                {
                    Id             = Convert.ToInt32(row["Id"]),
                    IdOrdenCompra  = Convert.ToInt32(row["IdOrdenCompra"]),
                    Componente     = comp,
                    Cantidad       = Convert.ToInt32(row["Cantidad"]),
                    PrecioUnitario = Convert.ToDecimal(row["PrecioUnitario"])
                });
            }
            return lista;
        }

        private List<BE_OrdenCompra_GO44> MapearLista(DataTable dt)
        {
            var lista = new List<BE_OrdenCompra_GO44>();
            foreach (DataRow row in dt.Rows) lista.Add(MapearEncabezado(row));
            return lista;
        }

        private BE_OrdenCompra_GO44 MapearEncabezado(DataRow row)
        {
            BE_OrdenCompra_GO44.EstadoOrden estado;
            Enum.TryParse(row["Estado"].ToString(), out estado);

            return new BE_OrdenCompra_GO44
            {
                Id             = Convert.ToInt32(row["Id"]),
                NumeroOC       = row["NumeroOC"].ToString(),
                IdSolicitud    = row["IdSolicitud"] == DBNull.Value ? 0 : Convert.ToInt32(row["IdSolicitud"]),
                NumeroSC       = row.Table.Columns.Contains("NumeroSC") && row["NumeroSC"] != DBNull.Value ? row["NumeroSC"].ToString() : "",
                LoginEncargado = row["LoginEncargado"].ToString(),
                FechaEmision   = Convert.ToDateTime(row["FechaEmision"]),
                FechaEntrega   = Convert.ToDateTime(row["FechaEntrega"]),
                Total          = Convert.ToDecimal(row["Total"]),
                Estado         = estado,
                Proveedor      = new BE_Proveedor_GO44
                {
                    CUIT    = row["CuitProveedor"].ToString(),
                    Empresa = row["EmpresaProveedor"].ToString()
                }
            };
        }
    }
}
