using Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DAL
{
    /// <summary>
    /// DAL del ROL (permisos). El esquema de permisos es un árbol tipo Composite:
    /// un Rol tiene hijos que pueden ser Patentes (permisos concretos) o Familias (grupos de permisos,
    /// que a su vez contienen patentes/familias). Esta DAL reconstruye ese árbol desde las tablas
    /// de relación (RolPatente, RolFamilia) y maneja el alta/baja/modificación de roles.
    /// </summary>
    public class DALRol_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALFamilia_GO44 _dalFamilia; // para expandir las familias del rol en sub-árboles
        private readonly DALPatente_GO44 _dalPatente;

        public DALRol_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalFamilia = new DALFamilia_GO44();
            _dalPatente = new DALPatente_GO44();
        }

        /// <summary>Lista los roles (solo Id y Nombre, sin su árbol de permisos).</summary>
        public List<Rol_GO44> ListarTodos()
        {
            string q = "SELECT Id, Nombre FROM Roles ORDER BY Nombre";
            DataTable dt = _acceso.leer(q, null);

            var lista = new List<Rol_GO44>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Rol_GO44
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString()
                });
            }
            return lista;
        }

        /// <summary>
        /// Reconstruye el árbol COMPLETO de permisos de un rol. Paso a paso:
        /// 1) trae el rol; 2) le cuelga como hijos sus patentes directas (RolPatente);
        /// 3) por cada familia asociada (RolFamilia) pide su sub-árbol a DALFamilia y lo cuelga también.
        /// Devuelve null si el rol no existe.
        /// </summary>
        public Rol_GO44 ObtenerArbol(int idRol)
        {
            string qRol = "SELECT Id, Nombre FROM Roles WHERE Id = @Id";
            DataTable dtRol = _acceso.leer(qRol, new[] { new SqlParameter("@Id", idRol) });
            if (dtRol.Rows.Count == 0) return null;

            var rol = new Rol_GO44
            {
                Id = Convert.ToInt32(dtRol.Rows[0]["Id"]),
                Nombre = dtRol.Rows[0]["Nombre"].ToString()
            };

            string qPat =
                "SELECT P.Id, P.Nombre, P.DataKey " +
                "FROM RolPatente RP " +
                "INNER JOIN Patente P ON P.Id = RP.IdPatente " +
                "WHERE RP.IdRol = @Id";
            DataTable dtPat = _acceso.leer(qPat, new[] { new SqlParameter("@Id", idRol) });
            foreach (DataRow row in dtPat.Rows)
            {
                rol.Hijos.Add(new Patente_GO44
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString(),
                    DataKey = row["DataKey"].ToString()
                });
            }

            string qFam = "SELECT IdFamilia FROM RolFamilia WHERE IdRol = @Id";
            DataTable dtFam = _acceso.leer(qFam, new[] { new SqlParameter("@Id", idRol) });
            foreach (DataRow row in dtFam.Rows)
            {
                int idFam = Convert.ToInt32(row["IdFamilia"]);
                Familia_GO44 fam = _dalFamilia.ObtenerArbol(idFam);
                if (fam != null) rol.Hijos.Add(fam);
            }

            return rol;
        }

        /// <summary>
        /// Crea un rol: inserta la fila y recupera su Id con SCOPE_IDENTITY, luego vincula sus patentes
        /// y familias insertando filas en RolPatente/RolFamilia. El operador ?? evita recorrer listas null.
        /// </summary>
        public int Crear(string nombre, List<int> idsPatentes, List<int> idsFamilias)
        {
            string qIns = "INSERT INTO Roles (Nombre) VALUES (@Nombre); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            object res = _acceso.leerEscalar(qIns, new[] { new SqlParameter("@Nombre", nombre) });

            if (res == null || res == DBNull.Value)
                throw new Exception(IdiomaManager_GO44.T("err.rolSinIdCreado"));

            int idRol = Convert.ToInt32(res);

            // Vincular cada patente directa elegida.
            foreach (int idPat in idsPatentes ?? new List<int>())
            {
                _acceso.escribir(
                    "INSERT INTO RolPatente (IdRol, IdPatente) VALUES (@R, @P)",
                    new[] { new SqlParameter("@R", idRol), new SqlParameter("@P", idPat) });
            }

            foreach (int idFam in idsFamilias ?? new List<int>())
            {
                _acceso.escribir(
                    "INSERT INTO RolFamilia (IdRol, IdFamilia) VALUES (@R, @F)",
                    new[] { new SqlParameter("@R", idRol), new SqlParameter("@F", idFam) });
            }

            return idRol;
        }

        /// <summary>Indica si algún usuario tiene este rol (no se puede borrar un rol en uso).</summary>
        public bool EstaEnUso(int idRol)
        {
            string q = "SELECT COUNT(1) FROM Usuario WHERE IdRol = @Id";
            object res = _acceso.leerEscalar(q, new[] { new SqlParameter("@Id", idRol) });
            return Convert.ToInt32(res) > 0;
        }

        /// <summary>Cantidad de usuarios que tienen este rol (para avisar al intentar eliminarlo).</summary>
        public int CantidadUsuariosConRol(int idRol)
        {
            string q = "SELECT COUNT(1) FROM Usuario WHERE IdRol = @Id";
            object res = _acceso.leerEscalar(q, new[] { new SqlParameter("@Id", idRol) });
            return Convert.ToInt32(res);
        }

        /// <summary>Elimina un rol y sus vínculos. Borra primero las relaciones (RolPatente/RolFamilia) y al final el rol, para no violar las FK.</summary>
        public void Eliminar(int idRol)
        {
            _acceso.escribir("DELETE FROM RolPatente WHERE IdRol = @Id",
                new[] { new SqlParameter("@Id", idRol) });
            _acceso.escribir("DELETE FROM RolFamilia WHERE IdRol = @Id",
                new[] { new SqlParameter("@Id", idRol) });
            _acceso.escribir("DELETE FROM Roles WHERE Id = @Id",
                new[] { new SqlParameter("@Id", idRol) });
        }

        /// <summary>
        /// Modifica un rol. Actualiza el nombre y, para los permisos, usa la estrategia "borrar y reinsertar":
        /// elimina todos los vínculos actuales y vuelve a crear los de las listas recibidas (más simple que
        /// calcular diferencias). Así el rol queda exactamente con las patentes/familias indicadas.
        /// </summary>
        public void Modificar(int idRol, string nombre, List<int> idsPatentes, List<int> idsFamilias)
        {
            _acceso.escribir(
                "UPDATE Roles SET Nombre = @Nombre WHERE Id = @Id",
                new[] { new SqlParameter("@Nombre", nombre), new SqlParameter("@Id", idRol) });

            _acceso.escribir("DELETE FROM RolPatente WHERE IdRol = @Id",
                new[] { new SqlParameter("@Id", idRol) });
            _acceso.escribir("DELETE FROM RolFamilia WHERE IdRol = @Id",
                new[] { new SqlParameter("@Id", idRol) });

            foreach (int idPat in idsPatentes ?? new List<int>())
            {
                _acceso.escribir(
                    "INSERT INTO RolPatente (IdRol, IdPatente) VALUES (@R, @P)",
                    new[] { new SqlParameter("@R", idRol), new SqlParameter("@P", idPat) });
            }

            foreach (int idFam in idsFamilias ?? new List<int>())
            {
                _acceso.escribir(
                    "INSERT INTO RolFamilia (IdRol, IdFamilia) VALUES (@R, @F)",
                    new[] { new SqlParameter("@R", idRol), new SqlParameter("@F", idFam) });
            }
        }

        /// <summary>Devuelve los Ids de las patentes asociadas DIRECTAMENTE al rol (sin expandir familias). Usa LINQ para proyectar la columna a una lista de int.</summary>
        public List<int> IdsPatentesDirectas(int idRol)
        {
            string q = "SELECT IdPatente FROM RolPatente WHERE IdRol = @Id ORDER BY IdPatente";
            DataTable dt = _acceso.leer(q, new[] { new SqlParameter("@Id", idRol) });
            return dt.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["IdPatente"])).ToList();
        }

        /// <summary>Devuelve los Ids de las familias asociadas directamente al rol (para precargar la pantalla de edición).</summary>
        public List<int> IdsFamiliasDirectas(int idRol)
        {
            string q = "SELECT IdFamilia FROM RolFamilia WHERE IdRol = @Id ORDER BY IdFamilia";
            DataTable dt = _acceso.leer(q, new[] { new SqlParameter("@Id", idRol) });
            return dt.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["IdFamilia"])).ToList();
        }
    }
}
