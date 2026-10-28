using Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL de la PATENTE: el permiso concreto (hoja del árbol de permisos). El campo DataKey es la
    /// clave que la UI usa para habilitar/deshabilitar una opción de menú según los permisos del rol.
    /// Solo lectura (las patentes son un catálogo fijo que no se crea desde la app).
    /// </summary>
    public class DALPatente_GO44
    {
        private readonly Acceso _acceso;

        public DALPatente_GO44()
        {
            _acceso = Acceso.Instancia;
        }

        /// <summary>Lista todas las patentes del catálogo (para armar las pantallas de permisos de rol/familia).</summary>
        public List<Patente_GO44> ListarTodas()
        {
            string query = "SELECT Id, Nombre, DataKey FROM Patente ORDER BY Nombre";
            DataTable dt = _acceso.leer(query, null);

            var lista = new List<Patente_GO44>();
            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));
            return lista;
        }

        /// <summary>Busca una patente por Id. Devuelve null si no existe.</summary>
        public Patente_GO44 BuscarPorId(int id)
        {
            string query = "SELECT Id, Nombre, DataKey FROM Patente WHERE Id = @Id";
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            DataTable dt = _acceso.leer(query, p);
            return dt.Rows.Count == 0 ? null : MapearFila(dt.Rows[0]);
        }

        /// <summary>Mapea una fila a entidad Patente (Id, Nombre y DataKey).</summary>
        private Patente_GO44 MapearFila(DataRow row)
        {
            return new Patente_GO44
            {
                Id = Convert.ToInt32(row["Id"]),
                Nombre = row["Nombre"].ToString(),
                DataKey = row["DataKey"].ToString()
            };
        }
    }
}
