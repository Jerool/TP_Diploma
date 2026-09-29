using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALFactura_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALIntegridad_GO44 _dalIntegridad;

        public DALFactura_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalIntegridad = new DALIntegridad_GO44();
        }

        private void RecalcularIntegridadFactura()
        {
            if (DALIntegridad_GO44.IntegridadConocidamenteRota) return;
            try { _dalIntegridad.RecalcularTabla("Facturas"); } catch { }
            try { _dalIntegridad.RecalcularTabla("LineaFactura"); } catch { }
            try { _dalIntegridad.RecalcularTabla("CobrosFactura"); } catch { }
        }

        public void RecalcularIntegridadPostCobro()
        {
            RecalcularIntegridadFactura();
        }

        // ---------- Insert encabezado + líneas dentro de tx ----------

        public int InsertarEncabezadoEnTx(BE_Factura_GO44 factura, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@NumeroFactura", factura.NumeroFactura),
                new SqlParameter("@IdCarrito",     factura.IdCarrito),
                new SqlParameter("@DniCliente",    factura.Cliente.DNI),
                new SqlParameter("@LoginCajero",   factura.LoginCajero),
                new SqlParameter("@Subtotal",      factura.Subtotal),
                new SqlParameter("@IVA",           factura.IVA),
                new SqlParameter("@Total",         factura.Total)
            };
            object r = _acceso.leerEscalarSPEnTx("sp_Factura_Insertar_GO44", p, tx);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        public int InsertarLineaEnTx(int idFactura, BE_LineaFactura_GO44 linea, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdFactura",      idFactura),
                new SqlParameter("@IdComponente",   linea.Componente.Id),
                new SqlParameter("@Cantidad",       linea.Cantidad),
                new SqlParameter("@PrecioUnitario", linea.PrecioUnitario),
                new SqlParameter("@Subtotal",       linea.Subtotal)
            };
            return _acceso.escribirSPEnTx("sp_LineaFactura_Insertar_GO44", p, tx);
        }

        public int MarcarCobradaEnTx(int idFactura, SqlTransaction tx)
        {
            SqlParameter[] p = { new SqlParameter("@Id", idFactura) };
            _acceso.escribirSPEnTx("sp_Factura_MarcarCobrada_GO44", p, tx);
            return 1;
        }

        // ---------- Registrar cobro dentro de tx ----------

        public int RegistrarCobroEnTx(BE_Cobro_GO44 cobro, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdFactura",        cobro.IdFactura),
                new SqlParameter("@MetodoPago",       cobro.Metodo.ToString()),
                new SqlParameter("@Monto",            cobro.Monto),
                new SqlParameter("@NroTarjetaEnmasc", (object)cobro.NroTarjetaEnmasc ?? DBNull.Value),
                new SqlParameter("@Banco",            (object)cobro.Banco ?? DBNull.Value),
                new SqlParameter("@TitularNombre",    (object)cobro.TitularNombre ?? DBNull.Value),
                new SqlParameter("@TitularApellido",  (object)cobro.TitularApellido ?? DBNull.Value),
                new SqlParameter("@CodigoAutoriz",    (object)cobro.CodigoAutoriz ?? DBNull.Value)
            };
            object r = _acceso.leerEscalarSPEnTx("sp_Cobro_Registrar_GO44", p, tx);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        // ---------- Consultas ----------

        public BE_Factura_GO44 BuscarPorId(int id)
        {
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            DataTable dtCab = _acceso.leerSP("sp_Factura_BuscarPorId_GO44", p);
            if (dtCab.Rows.Count == 0) return null;

            BE_Factura_GO44 f = MapearEncabezado(dtCab.Rows[0]);

            SqlParameter[] p2 = { new SqlParameter("@IdFactura", id) };
            DataTable dtLin = _acceso.leerSP("sp_LineaFactura_ListarPorFactura_GO44", p2);
            f.Lineas = MapearLineas(dtLin);
            return f;
        }

        private BE_Factura_GO44 MapearEncabezado(DataRow row)
        {
            BE_Factura_GO44.EstadoFactura estado;
            Enum.TryParse<BE_Factura_GO44.EstadoFactura>(row["Estado"].ToString(), out estado);

            return new BE_Factura_GO44
            {
                Id             = Convert.ToInt32(row["Id"]),
                NumeroFactura  = row["NumeroFactura"].ToString(),
                IdCarrito      = Convert.ToInt32(row["IdCarrito"]),
                LoginCajero    = row["LoginCajero"].ToString(),
                FechaEmision   = Convert.ToDateTime(row["FechaEmision"]),
                Subtotal       = Convert.ToDecimal(row["Subtotal"]),
                IVA            = Convert.ToDecimal(row["IVA"]),
                Total          = Convert.ToDecimal(row["Total"]),
                Estado         = estado,
                Cliente        = new BE_Cliente_GO44 { DNI = row["DniCliente"].ToString() }
            };
        }

        private List<BE_LineaFactura_GO44> MapearLineas(DataTable dt)
        {
            List<BE_LineaFactura_GO44> lista = new List<BE_LineaFactura_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new BE_LineaFactura_GO44
                {
                    Id             = Convert.ToInt32(row["Id"]),
                    IdFactura      = Convert.ToInt32(row["IdFactura"]),
                    Cantidad       = Convert.ToInt32(row["Cantidad"]),
                    PrecioUnitario = Convert.ToDecimal(row["PrecioUnitario"]),
                    Subtotal       = Convert.ToDecimal(row["Subtotal"]),
                    Componente     = new BE_Componente_GO44
                    {
                        Id     = Convert.ToInt32(row["IdComponente"]),
                        Codigo = row["Codigo"].ToString(),
                        Nombre = row["ComponenteNombre"].ToString()
                    }
                });
            }
            return lista;
        }
    }
}
