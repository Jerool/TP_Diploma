using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL (mapper) de la FACTURA y su COBRO. La facturación afecta varias tablas
    /// (Facturas, LineaFactura, CobrosFactura) y debe ser atómica, por eso expone envoltorios de
    /// transacción y métodos *EnTx que la BLL encadena: insertar encabezado, líneas, registrar el cobro
    /// y marcar la factura como cobrada, todo en un único commit. Recalcula la integridad de las 3 tablas.
    /// </summary>
    public class DALFactura_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALIntegridad_GO44 _dalIntegridad;

        public DALFactura_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalIntegridad = new DALIntegridad_GO44();
        }

        /// <summary>Recalcula el dígito verificador de las 3 tablas de facturación tras confirmar (best-effort).</summary>
        private void RecalcularIntegridadFactura()
        {
            if (DALIntegridad_GO44.IntegridadConocidamenteRota) return;
            try { _dalIntegridad.RecalcularTabla("Facturas"); } catch { }
            try { _dalIntegridad.RecalcularTabla("LineaFactura"); } catch { }
            try { _dalIntegridad.RecalcularTabla("CobrosFactura"); } catch { }
        }

        /// <summary>La BLL la llama después del commit del cobro para reordenar los dígitos verificadores.</summary>
        public void RecalcularIntegridadPostCobro()
        {
            RecalcularIntegridadFactura();
        }

        // ============ Transacción (encapsula Acceso para no romper las capas) ============

        public SqlTransaction IniciarTransaccion()          => _acceso.IniciarTransaccion();
        public void ConfirmarTransaccion(SqlTransaction tx) => _acceso.ConfirmarTransaccion(tx);
        public void CancelarTransaccion(SqlTransaction tx)  => _acceso.CancelarTransaccion(tx);

        // ---------- Insert encabezado + líneas dentro de tx ----------

        /// <summary>Inserta la cabecera de la factura dentro de la tx y devuelve el Id generado (0 si falló).</summary>
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

        /// <summary>Inserta una línea de la factura dentro de la tx. Devuelve filas afectadas.</summary>
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

        /// <summary>Marca la factura como Cobrada dentro de la tx (cambia su estado).</summary>
        public int MarcarCobradaEnTx(int idFactura, SqlTransaction tx)
        {
            SqlParameter[] p = { new SqlParameter("@Id", idFactura) };
            _acceso.escribirSPEnTx("sp_Factura_MarcarCobrada_GO44", p, tx);
            return 1;
        }

        // ---------- Registrar cobro dentro de tx ----------

        /// <summary>
        /// Registra el cobro de la factura dentro de la tx. Los datos de tarjeta son opcionales
        /// (van como DBNull si el pago fue en efectivo). Devuelve el Id del cobro generado.
        /// </summary>
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

        /// <summary>
        /// Lista facturas con filtros para el reporte (fecha desde/hasta, DNI cliente, estado).
        /// Cualquier parámetro null/vacío se ignora.
        /// </summary>
        public List<BE_Factura_GO44> ListarConFiltros(DateTime? desde, DateTime? hasta, string dni, string estado)
        {
            SqlParameter[] p = {
                new SqlParameter("@FechaDesde", (object)desde  ?? DBNull.Value),
                new SqlParameter("@FechaHasta", (object)hasta  ?? DBNull.Value),
                new SqlParameter("@DniCliente", (object)dni    ?? DBNull.Value),
                new SqlParameter("@Estado",     (object)estado ?? DBNull.Value)
            };
            DataTable dt = _acceso.leerSP("sp_Factura_ListarConFiltros_GO44", p);
            List<BE_Factura_GO44> lista = new List<BE_Factura_GO44>();
            foreach (DataRow row in dt.Rows)
                lista.Add(MapearEncabezado(row));
            return lista;
        }

        /// <summary>
        /// Trae una factura completa: primero la cabecera (1 SP) y luego sus líneas (otro SP),
        /// armando el objeto con ambas partes. Devuelve null si la factura no existe.
        /// </summary>
        public BE_Factura_GO44 BuscarPorId(int id)
        {
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            DataTable dtCab = _acceso.leerSP("sp_Factura_BuscarPorId_GO44", p);
            if (dtCab.Rows.Count == 0) return null;

            BE_Factura_GO44 f = MapearEncabezado(dtCab.Rows[0]);

            // Segunda consulta: las líneas de esa factura.
            SqlParameter[] p2 = { new SqlParameter("@IdFactura", id) };
            DataTable dtLin = _acceso.leerSP("sp_LineaFactura_ListarPorFactura_GO44", p2);
            f.Lineas = MapearLineas(dtLin);
            return f;
        }

        /// <summary>Mapea la cabecera de la factura; el estado (texto) se convierte al enum con Enum.TryParse.</summary>
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

        /// <summary>Mapea las filas de líneas a objetos, con un componente liviano (Id/Código/Nombre) en cada una.</summary>
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
