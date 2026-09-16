using Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DAL
{

    public class DALFamilia_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALPatente_GO44 _dalPatente;

        public DALFamilia_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalPatente = new DALPatente_GO44();
        }

        public List<Familia_GO44> ListarTodasPlanas()
        {
            string query = "SELECT Id, Nombre FROM Familia ORDER BY Nombre";
            DataTable dt = _acceso.leer(query, null);

            var lista = new List<Familia_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Familia_GO44
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString()
                });
            }
            return lista;
        }

        public Familia_GO44 ObtenerArbol(int idFamilia)
        {
            return ObtenerArbolRec(idFamilia, new HashSet<int>());
        }

        private Familia_GO44 ObtenerArbolRec(int idFamilia, HashSet<int> visitadas)
        {
            if (!visitadas.Add(idFamilia)) return null;

            string qFam = "SELECT Id, Nombre FROM Familia WHERE Id = @Id";
            SqlParameter[] pFam = { new SqlParameter("@Id", idFamilia) };
            DataTable dtFam = _acceso.leer(qFam, pFam);
            if (dtFam.Rows.Count == 0) return null;

            var familia = new Familia_GO44
            {
                Id = Convert.ToInt32(dtFam.Rows[0]["Id"]),
                Nombre = dtFam.Rows[0]["Nombre"].ToString()
            };

            string qPat =
                "SELECT P.Id, P.Nombre, P.DataKey " +
                "FROM FamiliaPatente FP " +
                "INNER JOIN Patente P ON P.Id = FP.IdPatente " +
                "WHERE FP.IdFamilia = @Id";
            DataTable dtPat = _acceso.leer(qPat, new[] { new SqlParameter("@Id", idFamilia) });
            foreach (DataRow row in dtPat.Rows)
            {
                familia.Hijos.Add(new Patente_GO44
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString(),
                    DataKey = row["DataKey"].ToString()
                });
            }

            string qSub = "SELECT IdFamiliaHija FROM FamiliaIntegrada WHERE IdFamiliaPadre = @Id";
            DataTable dtSub = _acceso.leer(qSub, new[] { new SqlParameter("@Id", idFamilia) });
            foreach (DataRow row in dtSub.Rows)
            {
                int idHija = Convert.ToInt32(row["IdFamiliaHija"]);
                Familia_GO44 hija = ObtenerArbolRec(idHija, visitadas);
                if (hija != null) familia.Hijos.Add(hija);
            }

            return familia;
        }

        public int Crear(string nombre, List<int> idsPatentes, List<int> idsSubfamilias)
        {

            string qIns = "INSERT INTO Familia (Nombre) VALUES (@Nombre); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var p = new[] { new SqlParameter("@Nombre", nombre) };
            object res = _acceso.leerEscalar(qIns, p);

            if (res == null || res == DBNull.Value)
                throw new Exception(IdiomaManager_GO44.T("err.familiaSinIdCreada"));

            int idFamilia = Convert.ToInt32(res);

            foreach (int idPat in idsPatentes ?? new List<int>())
            {
                _acceso.escribir(
                    "INSERT INTO FamiliaPatente (IdFamilia, IdPatente) VALUES (@F, @P)",
                    new[] { new SqlParameter("@F", idFamilia), new SqlParameter("@P", idPat) });
            }

            foreach (int idSub in idsSubfamilias ?? new List<int>())
            {
                _acceso.escribir(
                    "INSERT INTO FamiliaIntegrada (IdFamiliaPadre, IdFamiliaHija) VALUES (@P, @H)",
                    new[] { new SqlParameter("@P", idFamilia), new SqlParameter("@H", idSub) });
            }

            return idFamilia;
        }

        public void Eliminar(int idFamilia)
        {
            _acceso.escribir(
                "DELETE FROM FamiliaPatente WHERE IdFamilia = @Id",
                new[] { new SqlParameter("@Id", idFamilia) });
            _acceso.escribir(
                "DELETE FROM FamiliaIntegrada WHERE IdFamiliaPadre = @Id OR IdFamiliaHija = @Id",
                new[] { new SqlParameter("@Id", idFamilia) });
            _acceso.escribir(
                "DELETE FROM Familia WHERE Id = @Id",
                new[] { new SqlParameter("@Id", idFamilia) });
        }

        public void Modificar(int idFamilia, string nombre, List<int> idsPatentes, List<int> idsSubfamilias)
        {
            _acceso.escribir(
                "UPDATE Familia SET Nombre = @Nombre WHERE Id = @Id",
                new[] { new SqlParameter("@Nombre", nombre), new SqlParameter("@Id", idFamilia) });

            _acceso.escribir(
                "DELETE FROM FamiliaPatente WHERE IdFamilia = @Id",
                new[] { new SqlParameter("@Id", idFamilia) });
            _acceso.escribir(
                "DELETE FROM FamiliaIntegrada WHERE IdFamiliaPadre = @Id",
                new[] { new SqlParameter("@Id", idFamilia) });

            foreach (int idPat in idsPatentes ?? new List<int>())
            {
                _acceso.escribir(
                    "INSERT INTO FamiliaPatente (IdFamilia, IdPatente) VALUES (@F, @P)",
                    new[] { new SqlParameter("@F", idFamilia), new SqlParameter("@P", idPat) });
            }

            foreach (int idSub in idsSubfamilias ?? new List<int>())
            {
                _acceso.escribir(
                    "INSERT INTO FamiliaIntegrada (IdFamiliaPadre, IdFamiliaHija) VALUES (@P, @H)",
                    new[] { new SqlParameter("@P", idFamilia), new SqlParameter("@H", idSub) });
            }
        }

        public int CantidadRolesQueUsan(int idFamilia)
        {
            string q = "SELECT COUNT(*) FROM RolFamilia WHERE IdFamilia = @Id";
            object res = _acceso.leerEscalar(q, new[] { new SqlParameter("@Id", idFamilia) });
            return res == null || res == DBNull.Value ? 0 : Convert.ToInt32(res);
        }

        public int CantidadFamiliasQueLaContienen(int idFamilia)
        {
            string q = "SELECT COUNT(*) FROM FamiliaIntegrada WHERE IdFamiliaHija = @Id";
            object res = _acceso.leerEscalar(q, new[] { new SqlParameter("@Id", idFamilia) });
            return res == null || res == DBNull.Value ? 0 : Convert.ToInt32(res);
        }

        public List<string> NombresRolesQueUsan(int idFamilia)
        {
            string q = "SELECT R.Nombre FROM RolFamilia RF " +
                       "INNER JOIN Roles R ON R.Id = RF.IdRol " +
                       "WHERE RF.IdFamilia = @Id";
            DataTable dt = _acceso.leer(q, new[] { new SqlParameter("@Id", idFamilia) });
            return dt.Rows.Cast<DataRow>().Select(r => r["Nombre"].ToString()).ToList();
        }

        public List<string> NombresFamiliasQueLaContienen(int idFamilia)
        {
            string q = "SELECT F.Nombre FROM FamiliaIntegrada FI " +
                       "INNER JOIN Familia F ON F.Id = FI.IdFamiliaPadre " +
                       "WHERE FI.IdFamiliaHija = @Id";
            DataTable dt = _acceso.leer(q, new[] { new SqlParameter("@Id", idFamilia) });
            return dt.Rows.Cast<DataRow>().Select(r => r["Nombre"].ToString()).ToList();
        }

        public List<int> IdsPatentesDirectas(int idFamilia)
        {
            string q = "SELECT IdPatente FROM FamiliaPatente WHERE IdFamilia = @Id ORDER BY IdPatente";
            DataTable dt = _acceso.leer(q, new[] { new SqlParameter("@Id", idFamilia) });
            return dt.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["IdPatente"])).ToList();
        }

        public List<int> IdsSubfamiliasDirectas(int idFamilia)
        {
            string q = "SELECT IdFamiliaHija FROM FamiliaIntegrada WHERE IdFamiliaPadre = @Id ORDER BY IdFamiliaHija";
            DataTable dt = _acceso.leer(q, new[] { new SqlParameter("@Id", idFamilia) });
            return dt.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["IdFamiliaHija"])).ToList();
        }
    }
}
