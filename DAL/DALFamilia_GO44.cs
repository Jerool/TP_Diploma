using Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DAL
{
    /// <summary>
    /// DAL de la FAMILIA de permisos (grupo de patentes y/o sub-familias). Es el nodo compuesto del
    /// árbol de permisos: una familia puede contener patentes y otras familias (FamiliaIntegrada),
    /// y esas a su vez más familias, etc. Reconstruye ese árbol de forma recursiva, cuidándose de
    /// los ciclos (una familia que termine conteniéndose a sí misma colgaría la app).
    /// </summary>
    public class DALFamilia_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALPatente_GO44 _dalPatente;

        public DALFamilia_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalPatente = new DALPatente_GO44();
        }

        /// <summary>Lista todas las familias en forma plana (solo Id y Nombre), sin armar el árbol.</summary>
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

        /// <summary>Arma el árbol completo de una familia. Arranca la recursión con el set de "ya visitadas" vacío.</summary>
        public Familia_GO44 ObtenerArbol(int idFamilia)
        {
            return ObtenerArbolRec(idFamilia, new HashSet<int>());
        }

        /// <summary>
        /// Versión recursiva: trae la familia, le cuelga sus patentes y luego, por cada sub-familia,
        /// se llama a sí misma. El HashSet 'visitadas' evita ciclos infinitos: si una familia ya se
        /// visitó en esta rama, Add devuelve false y se corta (return null). Así nunca se cuelga.
        /// </summary>
        private Familia_GO44 ObtenerArbolRec(int idFamilia, HashSet<int> visitadas)
        {
            if (!visitadas.Add(idFamilia)) return null; // ya estaba → ciclo → cortar

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

        /// <summary>Crea una familia: inserta la fila, recupera su Id y vincula sus patentes y sub-familias.</summary>
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

        /// <summary>Elimina una familia y sus vínculos (como padre o como hija), borrando las relaciones antes que la familia (FK).</summary>
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

        /// <summary>Modifica una familia (nombre + permisos) con la misma estrategia "borrar y reinsertar" que el rol.</summary>
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

        // Las cuatro consultas siguientes sirven para advertir, antes de borrar una familia, quién la usa
        // (roles o familias padre). Las dos "Cantidad*" dan el número; las dos "Nombres*" dan la lista.
        /// <summary>Cantidad de roles que usan esta familia directamente.</summary>
        public int CantidadRolesQueUsan(int idFamilia)
        {
            string q = "SELECT COUNT(*) FROM RolFamilia WHERE IdFamilia = @Id";
            object res = _acceso.leerEscalar(q, new[] { new SqlParameter("@Id", idFamilia) });
            return res == null || res == DBNull.Value ? 0 : Convert.ToInt32(res);
        }

        /// <summary>Cantidad de familias padre que contienen a esta familia.</summary>
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

        /// <summary>Ids de las patentes asociadas directamente a la familia (para precargar la pantalla de edición).</summary>
        public List<int> IdsPatentesDirectas(int idFamilia)
        {
            string q = "SELECT IdPatente FROM FamiliaPatente WHERE IdFamilia = @Id ORDER BY IdPatente";
            DataTable dt = _acceso.leer(q, new[] { new SqlParameter("@Id", idFamilia) });
            return dt.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["IdPatente"])).ToList();
        }

        /// <summary>Ids de las sub-familias directas de la familia.</summary>
        public List<int> IdsSubfamiliasDirectas(int idFamilia)
        {
            string q = "SELECT IdFamiliaHija FROM FamiliaIntegrada WHERE IdFamiliaPadre = @Id ORDER BY IdFamiliaHija";
            DataTable dt = _acceso.leer(q, new[] { new SqlParameter("@Id", idFamilia) });
            return dt.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["IdFamiliaHija"])).ToList();
        }
    }
}
