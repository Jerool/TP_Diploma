using Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{

    public class DALPatente_GO44
    {
        private readonly Acceso _acceso;

        public DALPatente_GO44()
        {
            _acceso = Acceso.Instancia;
        }

        public List<Patente_GO44> ListarTodas()
        {
            string query = "SELECT Id, Nombre, DataKey FROM Patente ORDER BY Nombre";
            DataTable dt = _acceso.leer(query, null);

            var lista = new List<Patente_GO44>();
            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));
            return lista;
        }

        public Patente_GO44 BuscarPorId(int id)
        {
            string query = "SELECT Id, Nombre, DataKey FROM Patente WHERE Id = @Id";
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            DataTable dt = _acceso.leer(query, p);
            return dt.Rows.Count == 0 ? null : MapearFila(dt.Rows[0]);
        }

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
