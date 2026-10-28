using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL de Proveedor — RFN2 CU07.
    /// Sigue el mismo patrón que DALCliente_GO44 (singleton Acceso, SPs para todo).
    /// </summary>
    public class DALProveedor_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALIntegridad_GO44 _dalIntegridad;

        public DALProveedor_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalIntegridad = new DALIntegridad_GO44();
        }

        /// <summary>Recalcula el dígito verificador de la tabla Proveedores tras una escritura (best-effort).</summary>
        private void RecalcularIntegridadProveedor()
        {
            if (DALIntegridad_GO44.IntegridadConocidamenteRota) return;
            try { _dalIntegridad.RecalcularTabla("Proveedores"); } catch { }
        }

        /// <summary>Indica si ya existe un proveedor con ese CUIT (el SP devuelve un COUNT).</summary>
        public bool ExisteCUIT(string cuit)
        {
            SqlParameter[] p = { new SqlParameter("@CUIT", cuit) };
            object r = _acceso.leerEscalarSP("sp_Proveedor_ExisteCUIT_GO44", p);
            return r != null && Convert.ToInt32(r) > 0;
        }

        /// <summary>Inserta un proveedor nuevo con sus datos de contacto y bancarios. Devuelve filas afectadas.</summary>
        public int Insertar(BE_Proveedor_GO44 prov)
        {
            SqlParameter[] p = {
                new SqlParameter("@CUIT",     prov.CUIT),
                new SqlParameter("@Nombre",   prov.Nombre),
                new SqlParameter("@Apellido", prov.Apellido),
                new SqlParameter("@Empresa",  prov.Empresa),
                new SqlParameter("@Telefono", prov.Telefono),
                new SqlParameter("@CVU",      prov.CVU),
                new SqlParameter("@Banco",    prov.Banco),
                new SqlParameter("@Titular",  prov.Titular)
            };
            int filas = Convert.ToInt32(_acceso.leerEscalarSP("sp_Proveedor_Insertar_GO44", p));
            RecalcularIntegridadProveedor();
            return filas;
        }

        /// <summary>Lista los proveedores activos (los dados de baja no se muestran para elegir).</summary>
        public List<BE_Proveedor_GO44> ListarActivos()
        {
            DataTable dt = _acceso.leerSP("sp_Proveedor_ListarActivos_GO44", null);
            var lista = new List<BE_Proveedor_GO44>();
            foreach (DataRow row in dt.Rows) lista.Add(Mapear(row));
            return lista;
        }

        /// <summary>Busca un proveedor por CUIT. Devuelve null si no existe (clave para traer sus datos bancarios al pagar).</summary>
        public BE_Proveedor_GO44 BuscarPorCUIT(string cuit)
        {
            SqlParameter[] p = { new SqlParameter("@CUIT", cuit) };
            DataTable dt = _acceso.leerSP("sp_Proveedor_BuscarPorCUIT_GO44", p);
            if (dt.Rows.Count == 0) return null;
            return Mapear(dt.Rows[0]);
        }

        /// <summary>Mapea una fila de la tabla Proveedores a la entidad BE_Proveedor.</summary>
        private BE_Proveedor_GO44 Mapear(DataRow row)
        {
            return new BE_Proveedor_GO44
            {
                CUIT      = row["CUIT"].ToString(),
                Nombre    = row["Nombre"].ToString(),
                Apellido  = row["Apellido"].ToString(),
                Empresa   = row["Empresa"].ToString(),
                Telefono  = row["Telefono"].ToString(),
                CVU       = row["CVU"].ToString(),
                Banco     = row["Banco"].ToString(),
                Titular   = row["Titular"].ToString(),
                Activo    = Convert.ToBoolean(row["Activo"]),
                FechaAlta = Convert.ToDateTime(row["FechaAlta"])
            };
        }
    }
}
