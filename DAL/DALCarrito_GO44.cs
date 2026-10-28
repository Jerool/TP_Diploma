using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL (mapper) del CARRITO. Como el carrito se guarda en dos tablas (Carrito + LineaCarrito)
    /// y debe persistirse de forma atómica, esta DAL expone envoltorios de transacción
    /// (IniciarTransaccion/Confirmar/Cancelar) para que la BLL encadene encabezado + líneas en un
    /// solo commit sin tocar <see cref="Acceso"/> directamente. También recalcula la integridad de ambas tablas.
    /// </summary>
    public class DALCarrito_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALIntegridad_GO44 _dalIntegridad;

        public DALCarrito_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalIntegridad = new DALIntegridad_GO44();
        }

        /// <summary>Recalcula el dígito verificador de las dos tablas del carrito tras confirmar (best-effort).</summary>
        private void RecalcularIntegridadCarrito()
        {
            if (DALIntegridad_GO44.IntegridadConocidamenteRota) return;
            try { _dalIntegridad.RecalcularTabla("Carrito"); } catch { }
            try { _dalIntegridad.RecalcularTabla("LineaCarrito"); } catch { }
        }

        // ============ Transacción (encapsula Acceso para no romper las capas) ============
        // La BLL usa estos tres para que varias escrituras entren o no entren juntas.

        public SqlTransaction IniciarTransaccion()          => _acceso.IniciarTransaccion();
        public void ConfirmarTransaccion(SqlTransaction tx) => _acceso.ConfirmarTransaccion(tx);
        public void CancelarTransaccion(SqlTransaction tx)  => _acceso.CancelarTransaccion(tx);

        /// <summary>
        /// Inserta el encabezado del carrito y devuelve el Id generado. Usa transacción externa.
        /// </summary>
        public int InsertarEncabezadoEnTx(BE_Carrito_GO44 carrito, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@DniCliente",    carrito.Cliente.DNI),
                new SqlParameter("@LoginVendedor", carrito.LoginVendedor),
                new SqlParameter("@Total",         carrito.Total),
                new SqlParameter("@Estado",        carrito.Estado.ToString())
            };
            object r = _acceso.leerEscalarSPEnTx("sp_Carrito_Insertar_GO44", p, tx);
            return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
        }

        /// <summary>
        /// Inserta una línea del carrito dentro de la transacción externa.
        /// </summary>
        public int InsertarLineaEnTx(int idCarrito, BE_LineaCarrito_GO44 linea, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdCarrito",      idCarrito),
                new SqlParameter("@IdComponente",   linea.Componente.Id),
                new SqlParameter("@Cantidad",       linea.Cantidad),
                new SqlParameter("@PrecioUnitario", linea.PrecioUnitario)
            };
            return _acceso.escribirSPEnTx("sp_LineaCarrito_Insertar_GO44", p, tx);
        }

        /// <summary>
        /// Marca las tablas Carrito/LineaCarrito para recálculo de integridad después del commit.
        /// </summary>
        public void RecalcularIntegridadPostConfirmacion()
        {
            RecalcularIntegridadCarrito();
        }

        // ------- Consultas -------

        /// <summary>
        /// Trae el último carrito Confirmado del cliente que aún no tiene factura asociada.
        /// Usado por CU04 Generar Factura.
        /// </summary>
        public BE_Carrito_GO44 BuscarPendientePorDNI(string dni)
        {
            SqlParameter[] p = { new SqlParameter("@DNI", dni) };
            DataTable dtCab = _acceso.leerSP("sp_Carrito_BuscarPendientePorDNI_GO44", p);
            if (dtCab.Rows.Count == 0) return null;

            BE_Carrito_GO44 carrito = MapearEncabezado(dtCab.Rows[0]);

            SqlParameter[] p2 = { new SqlParameter("@IdCarrito", carrito.Id) };
            DataTable dtLin = _acceso.leerSP("sp_LineaCarrito_ListarPorCarrito_GO44", p2);
            carrito.Lineas = MapearLineas(dtLin);
            return carrito;
        }

        public BE_Carrito_GO44 BuscarPorId(int id)
        {
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            DataTable dtCab = _acceso.leerSP("sp_Carrito_ObtenerPorId_GO44", p);
            if (dtCab.Rows.Count == 0) return null;

            BE_Carrito_GO44 carrito = MapearEncabezado(dtCab.Rows[0]);

            SqlParameter[] p2 = { new SqlParameter("@IdCarrito", id) };
            DataTable dtLin = _acceso.leerSP("sp_LineaCarrito_ListarPorCarrito_GO44", p2);
            carrito.Lineas = MapearLineas(dtLin);

            return carrito;
        }

        /// <summary>
        /// Mapea la fila de cabecera a un BE_Carrito. El estado viene como texto en la base, así que se
        /// convierte al enum con Enum.TryParse (si no coincide, queda el valor por defecto sin romper).
        /// El cliente se crea "liviano" (solo DNI); el resto de sus datos no hace falta acá.
        /// </summary>
        private BE_Carrito_GO44 MapearEncabezado(DataRow row)
        {
            BE_Carrito_GO44.EstadoCarrito estado;
            Enum.TryParse<BE_Carrito_GO44.EstadoCarrito>(row["Estado"].ToString(), out estado);

            return new BE_Carrito_GO44
            {
                Id             = Convert.ToInt32(row["Id"]),
                LoginVendedor  = row["LoginVendedor"].ToString(),
                FechaCreacion  = Convert.ToDateTime(row["FechaCreacion"]),
                Estado         = estado,
                Cliente        = new BE_Cliente_GO44 { DNI = row["DniCliente"].ToString() }
            };
        }

        /// <summary>Mapea las filas de líneas a objetos, armando en cada una un componente liviano (Id/Código/Nombre).</summary>
        private List<BE_LineaCarrito_GO44> MapearLineas(DataTable dt)
        {
            List<BE_LineaCarrito_GO44> lista = new List<BE_LineaCarrito_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new BE_LineaCarrito_GO44
                {
                    Id             = Convert.ToInt32(row["Id"]),
                    IdCarrito      = Convert.ToInt32(row["IdCarrito"]),
                    Cantidad       = Convert.ToInt32(row["Cantidad"]),
                    PrecioUnitario = Convert.ToDecimal(row["PrecioUnitario"]),
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
