using BE;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL de Pago de Orden de Compra — RFN2 CU09.
    /// Expone wrappers de transacción (el pago + el cambio de estado de la OC van juntos).
    /// </summary>
    public class DALPagoOrdenCompra_GO44
    {
        private readonly Acceso _acceso;

        public DALPagoOrdenCompra_GO44()
        {
            _acceso = Acceso.Instancia;
        }

        public SqlTransaction IniciarTransaccion()          => _acceso.IniciarTransaccion();
        public void ConfirmarTransaccion(SqlTransaction tx) => _acceso.ConfirmarTransaccion(tx);
        public void CancelarTransaccion(SqlTransaction tx)  => _acceso.CancelarTransaccion(tx);

        public int InsertarEnTx(BE_PagoOrdenCompra_GO44 pago, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdOrdenCompra",  pago.IdOrdenCompra),
                new SqlParameter("@CVU",            pago.CVU),
                new SqlParameter("@Banco",          pago.Banco),
                new SqlParameter("@Titular",        pago.Titular),
                new SqlParameter("@MontoAbonado",   pago.MontoAbonado),
                new SqlParameter("@NroComprobante", pago.NroComprobante),
                new SqlParameter("@LoginEncargado", pago.LoginEncargado)
            };
            object r = _acceso.leerEscalarSPEnTx("sp_PagoOrdenCompra_Insertar_GO44", p, tx);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        public BE_PagoOrdenCompra_GO44 BuscarPorOrden(int idOrdenCompra)
        {
            SqlParameter[] p = { new SqlParameter("@IdOrdenCompra", idOrdenCompra) };
            DataTable dt = _acceso.leerSP("sp_PagoOrdenCompra_BuscarPorOrden_GO44", p);
            if (dt.Rows.Count == 0) return null;
            DataRow row = dt.Rows[0];
            return new BE_PagoOrdenCompra_GO44
            {
                Id             = Convert.ToInt32(row["Id"]),
                IdOrdenCompra  = Convert.ToInt32(row["IdOrdenCompra"]),
                CVU            = row["CVU"].ToString(),
                Banco          = row["Banco"].ToString(),
                Titular        = row["Titular"].ToString(),
                MontoAbonado   = Convert.ToDecimal(row["MontoAbonado"]),
                FechaOperacion = Convert.ToDateTime(row["FechaOperacion"]),
                NroComprobante = row["NroComprobante"].ToString(),
                Estado         = row["Estado"].ToString(),
                LoginEncargado = row["LoginEncargado"].ToString()
            };
        }
    }
}
