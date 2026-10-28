using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL de Recepción de mercadería — RFN2 CU10.
    /// Expone wrappers de transacción: el encabezado + líneas + el aumento de stock
    /// + el cambio de estado de la OC se hacen de forma atómica desde la BLL.
    /// </summary>
    public class DALRecepcionMercaderia_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALComponente_GO44 _dalComponente;

        public DALRecepcionMercaderia_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalComponente = new DALComponente_GO44();
        }

        public SqlTransaction IniciarTransaccion()          => _acceso.IniciarTransaccion();
        public void ConfirmarTransaccion(SqlTransaction tx) => _acceso.ConfirmarTransaccion(tx);
        public void CancelarTransaccion(SqlTransaction tx)  => _acceso.CancelarTransaccion(tx);

        /// <summary>Inserta la cabecera de la recepción dentro de la tx y devuelve su Id generado.</summary>
        public int InsertarEncabezadoEnTx(BE_RecepcionMercaderia_GO44 recepcion, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdOrdenCompra",    recepcion.IdOrdenCompra),
                new SqlParameter("@LoginAlmacenista", recepcion.LoginAlmacenista),
                new SqlParameter("@ConFaltantes",     recepcion.ConFaltantes),
                new SqlParameter("@Observaciones",    (object)recepcion.Observaciones ?? DBNull.Value)
            };
            object r = _acceso.leerEscalarSPEnTx("sp_Recepcion_Insertar_GO44", p, tx);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        /// <summary>Inserta una línea de recepción (pedida + recibida) dentro de la tx. Devuelve filas afectadas.</summary>
        public int InsertarLineaEnTx(int idRecepcion, BE_LineaRecepcion_GO44 linea, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdRecepcion",      idRecepcion),
                new SqlParameter("@IdComponente",     linea.Componente.Id),
                new SqlParameter("@CantidadPedida",   linea.CantidadPedida),
                new SqlParameter("@CantidadRecibida", linea.CantidadRecibida)
            };
            return _acceso.escribirSPEnTx("sp_LineaRecepcion_Insertar_GO44", p, tx);
        }

        /// <summary>Trae la recepción registrada de una OC (null si no se recibió). Usado por el reporte RFN2. Incluye sus líneas.</summary>
        public BE_RecepcionMercaderia_GO44 BuscarPorOrden(int idOrdenCompra)
        {
            SqlParameter[] p = { new SqlParameter("@IdOrdenCompra", idOrdenCompra) };
            DataTable dt = _acceso.leerSP("sp_Recepcion_BuscarPorOrden_GO44", p);
            if (dt.Rows.Count == 0) return null;
            DataRow row = dt.Rows[0];
            var recepcion = new BE_RecepcionMercaderia_GO44
            {
                Id               = Convert.ToInt32(row["Id"]),
                IdOrdenCompra    = Convert.ToInt32(row["IdOrdenCompra"]),
                LoginAlmacenista = row["LoginAlmacenista"].ToString(),
                FechaRecepcion   = Convert.ToDateTime(row["FechaRecepcion"]),
                ConFaltantes     = Convert.ToBoolean(row["ConFaltantes"]),
                Observaciones    = row["Observaciones"] == DBNull.Value ? null : row["Observaciones"].ToString()
            };
            recepcion.Lineas = ListarLineas(recepcion.Id);
            return recepcion;
        }

        /// <summary>Lista las líneas de una recepción, trayendo el componente completo de cada una con DALComponente.</summary>
        public List<BE_LineaRecepcion_GO44> ListarLineas(int idRecepcion)
        {
            SqlParameter[] p = { new SqlParameter("@IdRecepcion", idRecepcion) };
            DataTable dt = _acceso.leerSP("sp_LineaRecepcion_ListarPorRecepcion_GO44", p);
            var lista = new List<BE_LineaRecepcion_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                var comp = _dalComponente.BuscarPorId(Convert.ToInt32(row["IdComponente"]));
                lista.Add(new BE_LineaRecepcion_GO44
                {
                    Id               = Convert.ToInt32(row["Id"]),
                    IdRecepcion      = Convert.ToInt32(row["IdRecepcion"]),
                    Componente       = comp,
                    CantidadPedida   = Convert.ToInt32(row["CantidadPedida"]),
                    CantidadRecibida = Convert.ToInt32(row["CantidadRecibida"])
                });
            }
            return lista;
        }
    }
}
