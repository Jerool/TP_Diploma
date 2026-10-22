using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL de Solicitud de Cotización — RFN2 CU06.
    /// Reutiliza DALProveedor y DALComponente para completar las entidades relacionadas.
    /// </summary>
    public class DALSolicitudCotizacion_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALProveedor_GO44  _dalProveedor;
        private readonly DALComponente_GO44 _dalComponente;

        public DALSolicitudCotizacion_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalProveedor  = new DALProveedor_GO44();
            _dalComponente = new DALComponente_GO44();
        }

        // ============ Transacción (encapsula Acceso para no romper las capas) ============

        public SqlTransaction IniciarTransaccion()      => _acceso.IniciarTransaccion();
        public void ConfirmarTransaccion(SqlTransaction tx) => _acceso.ConfirmarTransaccion(tx);
        public void CancelarTransaccion(SqlTransaction tx)  => _acceso.CancelarTransaccion(tx);

        /// <summary>Inserta el encabezado y devuelve el Id autogenerado.</summary>
        public int InsertarEncabezado(BE_SolicitudCotizacion_GO44 sc)
        {
            SqlParameter[] p = {
                new SqlParameter("@CuitProveedor",  sc.Proveedor.CUIT),
                new SqlParameter("@LoginEncargado", sc.LoginEncargado)
            };
            object r = _acceso.leerEscalarSP("sp_SolicitudCotizacion_Insertar_GO44", p);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        /// <summary>Variante transaccional del encabezado.</summary>
        public int InsertarEncabezadoEnTx(BE_SolicitudCotizacion_GO44 sc, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@CuitProveedor",  sc.Proveedor.CUIT),
                new SqlParameter("@LoginEncargado", sc.LoginEncargado)
            };
            object r = _acceso.leerEscalarSPEnTx("sp_SolicitudCotizacion_Insertar_GO44", p, tx);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        public int InsertarLinea(int idSolicitud, BE_LineaSolicitudCotizacion_GO44 linea)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdSolicitud",  idSolicitud),
                new SqlParameter("@IdComponente", linea.Componente.Id),
                new SqlParameter("@Cantidad",     linea.Cantidad)
            };
            return Convert.ToInt32(_acceso.leerEscalarSP("sp_LineaSolicitudCotizacion_Insertar_GO44", p));
        }

        /// <summary>Variante transaccional de la línea.</summary>
        public int InsertarLineaEnTx(int idSolicitud, BE_LineaSolicitudCotizacion_GO44 linea, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdSolicitud",  idSolicitud),
                new SqlParameter("@IdComponente", linea.Componente.Id),
                new SqlParameter("@Cantidad",     linea.Cantidad)
            };
            object r = _acceso.leerEscalarSPEnTx("sp_LineaSolicitudCotizacion_Insertar_GO44", p, tx);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        public List<BE_SolicitudCotizacion_GO44> Listar()
        {
            DataTable dt = _acceso.leerSP("sp_SolicitudCotizacion_Listar_GO44", null);
            var lista = new List<BE_SolicitudCotizacion_GO44>();
            foreach (DataRow row in dt.Rows) lista.Add(MapearEncabezado(row));
            return lista;
        }

        public BE_SolicitudCotizacion_GO44 BuscarPorId(int id)
        {
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            DataTable dt = _acceso.leerSP("sp_SolicitudCotizacion_BuscarPorId_GO44", p);
            if (dt.Rows.Count == 0) return null;
            var sc = MapearEncabezado(dt.Rows[0]);
            sc.Lineas = ListarLineas(id);
            return sc;
        }

        public List<BE_LineaSolicitudCotizacion_GO44> ListarLineas(int idSolicitud)
        {
            SqlParameter[] p = { new SqlParameter("@IdSolicitud", idSolicitud) };
            DataTable dt = _acceso.leerSP("sp_LineaSolicitudCotizacion_ListarPorSolicitud_GO44", p);
            var lista = new List<BE_LineaSolicitudCotizacion_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                var comp = _dalComponente.BuscarPorId(Convert.ToInt32(row["IdComponente"]));
                lista.Add(new BE_LineaSolicitudCotizacion_GO44
                {
                    Id          = Convert.ToInt32(row["Id"]),
                    IdSolicitud = Convert.ToInt32(row["IdSolicitud"]),
                    Componente  = comp,
                    Cantidad    = Convert.ToInt32(row["Cantidad"])
                });
            }
            return lista;
        }

        private BE_SolicitudCotizacion_GO44 MapearEncabezado(DataRow row)
        {
            var sc = new BE_SolicitudCotizacion_GO44
            {
                Id             = Convert.ToInt32(row["Id"]),
                NumeroSC       = row["NumeroSC"].ToString(),
                FechaEmision   = Convert.ToDateTime(row["FechaEmision"]),
                LoginEncargado = row["LoginEncargado"].ToString(),
                Proveedor      = new BE_Proveedor_GO44 {
                    CUIT    = row["CuitProveedor"].ToString(),
                    Empresa = row["EmpresaProveedor"].ToString()
                },
                Estado = (BE_SolicitudCotizacion_GO44.EstadoSolicitud)
                         Enum.Parse(typeof(BE_SolicitudCotizacion_GO44.EstadoSolicitud),
                                    row["Estado"].ToString())
            };
            return sc;
        }
    }
}
