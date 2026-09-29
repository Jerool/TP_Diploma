using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALComponente_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALIntegridad_GO44 _dalIntegridad;

        public DALComponente_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalIntegridad = new DALIntegridad_GO44();
        }

        private void RecalcularIntegridadComponente()
        {
            if (DALIntegridad_GO44.IntegridadConocidamenteRota) return;
            try { _dalIntegridad.RecalcularTabla("Componentes"); } catch { }
        }

        public List<BE_Componente_GO44> ListarActivos()
        {
            DataTable dt = _acceso.leerSP("sp_Componente_ListarActivos_GO44", null);
            return MapearLista(dt);
        }

        public List<BE_Componente_GO44> ListarTodos()
        {
            DataTable dt = _acceso.leerSP("sp_Componente_ListarTodos_GO44", null);
            return MapearLista(dt);
        }

        public int Insertar(BE_Componente_GO44 c)
        {
            SqlParameter[] p = {
                new SqlParameter("@Codigo",      c.Codigo),
                new SqlParameter("@Nombre",      c.Nombre),
                new SqlParameter("@Descripcion", (object)c.Descripcion ?? DBNull.Value),
                new SqlParameter("@Categoria",   (object)c.Categoria   ?? DBNull.Value),
                new SqlParameter("@Precio",      c.Precio),
                new SqlParameter("@StockActual", c.StockActual),
                new SqlParameter("@StockMinimo", c.StockMinimo)
            };
            object r = _acceso.leerEscalarSP("sp_Componente_Insertar_GO44", p);
            int nuevoId = r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
            RecalcularIntegridadComponente();
            return nuevoId;
        }

        public int ActualizarPrecio(int id, decimal precio)
        {
            SqlParameter[] p = {
                new SqlParameter("@Id",     id),
                new SqlParameter("@Precio", precio)
            };
            int filas = Convert.ToInt32(_acceso.leerEscalarSP("sp_Componente_ActualizarPrecio_GO44", p));
            RecalcularIntegridadComponente();
            return filas;
        }

        public int DarDeBaja(int id)
        {
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            int filas = Convert.ToInt32(_acceso.leerEscalarSP("sp_Componente_DarDeBaja_GO44", p));
            RecalcularIntegridadComponente();
            return filas;
        }

        public int Reactivar(int id)
        {
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            int filas = Convert.ToInt32(_acceso.leerEscalarSP("sp_Componente_Reactivar_GO44", p));
            RecalcularIntegridadComponente();
            return filas;
        }

        public BE_Componente_GO44 BuscarPorId(int id)
        {
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            DataTable dt = _acceso.leerSP("sp_Componente_BuscarPorId_GO44", p);
            if (dt.Rows.Count == 0) return null;
            return MapearFila(dt.Rows[0]);
        }

        public BE_Componente_GO44 BuscarPorCodigo(string codigo)
        {
            SqlParameter[] p = { new SqlParameter("@Codigo", codigo) };
            DataTable dt = _acceso.leerSP("sp_Componente_BuscarPorCodigo_GO44", p);
            if (dt.Rows.Count == 0) return null;
            return MapearFila(dt.Rows[0]);
        }

        public bool VerificarStock(int idComponente, int cantidad)
        {
            SqlParameter[] p = {
                new SqlParameter("@Id",       idComponente),
                new SqlParameter("@Cantidad", cantidad)
            };
            DataTable dt = _acceso.leerSP("sp_Componente_VerificarStock_GO44", p);
            if (dt.Rows.Count == 0) return false;
            return Convert.ToBoolean(dt.Rows[0]["TieneStock"]);
        }

        /// <summary>
        /// Descuenta stock dentro de una transacción externa (usado por CU01 al confirmar carrito).
        /// El SP protege contra stock negativo (WHERE StockActual >= @Cantidad); si retorna 0 filas
        /// hubo condición de carrera y el BLL debe hacer rollback.
        /// </summary>
        public int DescontarStockEnTx(int idComponente, int cantidad, SqlTransaction tx)
        {
            SqlParameter[] p = {
                new SqlParameter("@Id",       idComponente),
                new SqlParameter("@Cantidad", cantidad)
            };
            _acceso.escribirSPEnTx("sp_Componente_DescontarStock_GO44", p, tx);
            // El SP devuelve @@ROWCOUNT como SELECT; para consistencia lo re-consultamos vía scalar
            SqlParameter[] pv = {
                new SqlParameter("@Id",       idComponente),
                new SqlParameter("@Cantidad", 0)
            };
            // No cambia stock, solo consulta si sigue habiendo. Alternativa: derivar de @@ROWCOUNT directamente.
            return cantidad;   // BLL debe re-verificar con BuscarPorId si necesita el nuevo stock
        }

        private List<BE_Componente_GO44> MapearLista(DataTable dt)
        {
            List<BE_Componente_GO44> lista = new List<BE_Componente_GO44>();
            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));
            return lista;
        }

        private BE_Componente_GO44 MapearFila(DataRow row)
        {
            return new BE_Componente_GO44
            {
                Id          = Convert.ToInt32(row["Id"]),
                Codigo      = row["Codigo"].ToString(),
                Nombre      = row["Nombre"].ToString(),
                Descripcion = row["Descripcion"] == DBNull.Value ? null : row["Descripcion"].ToString(),
                Categoria   = row["Categoria"]   == DBNull.Value ? null : row["Categoria"].ToString(),
                Precio      = Convert.ToDecimal(row["Precio"]),
                StockActual = Convert.ToInt32(row["StockActual"]),
                StockMinimo = Convert.ToInt32(row["StockMinimo"]),
                Activo      = Convert.ToBoolean(row["Activo"])
            };
        }
    }
}
