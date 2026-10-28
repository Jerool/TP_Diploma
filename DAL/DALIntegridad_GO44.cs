using Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// DAL del control de INTEGRIDAD (dígitos verificadores). Protege tablas sensibles contra
    /// modificaciones hechas por fuera del sistema:
    ///  - DVH (Dígito Verificador Horizontal): un hash POR FILA, calculado con sus campos.
    ///  - DVV (Dígito Verificador Vertical): un hash de toda la tabla, calculado a partir de sus DVH.
    /// En cada arranque se recalculan y se comparan con los guardados (tablas IntegridadDVH/DVV):
    /// si no coinciden, alguien tocó la base por afuera. También hace backup/restore de la BD.
    /// </summary>
    public class DALIntegridad_GO44
    {
        private readonly Acceso _acceso;

        /// <summary>
        /// Lista de tablas protegidas por dígito verificador. Está hardcodeada (no se descubre sola):
        /// para proteger una tabla nueva hay que agregarla acá Y escribir su método DVHs correspondiente.
        /// </summary>
        public static readonly string[] TABLAS_PROTEGIDAS = {
            "Usuario", "Roles", "Familia", "Patente",
            "FamiliaPatente", "FamiliaIntegrada",
            "RolPatente", "RolFamilia",
            "Modulo", "TipoEvento",
            "Proveedores"
        };

        /// <summary>Bandera global: si la verificación detectó integridad rota, se prende y se evita seguir recalculando.</summary>
        public static bool IntegridadConocidamenteRota { get; set; }

        public DALIntegridad_GO44()
        {
            _acceso = Acceso.Instancia;
        }

        /// <summary>
        /// Despacha al método que calcula los DVH de la tabla pedida. El switch es el "índice" de las
        /// tablas protegidas; cada case arma un diccionario {claveDeLaFila → hash DVH}.
        /// </summary>
        public Dictionary<string, string> CalcularDVHsTabla(string nombreTabla)
        {
            switch (nombreTabla)
            {
                case "Usuario":          return DVHsUsuario();
                case "Roles":            return DVHsRoles();
                case "Familia":          return DVHsFamilia();
                case "Patente":          return DVHsPatente();
                case "FamiliaPatente":   return DVHsFamiliaPatente();
                case "FamiliaIntegrada": return DVHsFamiliaIntegrada();
                case "RolPatente":       return DVHsRolPatente();
                case "RolFamilia":       return DVHsRolFamilia();
                case "Modulo":           return DVHsModulo();
                case "TipoEvento":       return DVHsTipoEvento();
                case "Proveedores":      return DVHsProveedores();
                default: throw new Exception("Tabla protegida desconocida: " + nombreTabla);
            }
        }

        /// <summary>
        /// Ejemplo del patrón que siguen todos los DVHs*: lee las columnas protegidas de la tabla y,
        /// por cada fila, calcula su DVH (hash de todos esos campos) indexado por la clave de la fila.
        /// Si alguien cambia un solo campo por afuera, el DVH recalculado dejará de coincidir con el guardado.
        /// </summary>
        private Dictionary<string, string> DVHsProveedores()
        {
            // Datos bancarios sensibles (CVU, Banco, Titular) → se protegen con DVH por fila.
            string q = "SELECT CUIT, Nombre, Apellido, Empresa, Telefono, CVU, Banco, Titular, Activo FROM Proveedores";
            DataTable dt = _acceso.leer(q, null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = Convert.ToString(r["CUIT"]); // clave = CUIT (PK)
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(
                    r["CUIT"], r["Nombre"], r["Apellido"], r["Empresa"],
                    r["Telefono"], r["CVU"], r["Banco"], r["Titular"], r["Activo"]);
            }
            return dict;
        }

        private Dictionary<string, string> DVHsModulo()
        {
            DataTable dt = _acceso.leer("SELECT Id, Nombre FROM Modulo", null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = Convert.ToString(r["Id"]);
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(r["Id"], r["Nombre"]);
            }
            return dict;
        }

        private Dictionary<string, string> DVHsTipoEvento()
        {
            DataTable dt = _acceso.leer("SELECT Id, Nombre FROM TipoEvento", null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = Convert.ToString(r["Id"]);
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(r["Id"], r["Nombre"]);
            }
            return dict;
        }

        private Dictionary<string, string> DVHsUsuario()
        {
            string q = "SELECT DNI, Apellido, Nombre, UserName, Email, " +
                       "       IdRol, Activo, Bloqueo, IntentosFallidos, DebeCambiarContrasena " +
                       "FROM Usuario";
            DataTable dt = _acceso.leer(q, null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = Convert.ToString(r["DNI"]);
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(
                    r["DNI"], r["Apellido"], r["Nombre"], r["UserName"], r["Email"],
                    r["IdRol"], r["Activo"], r["Bloqueo"], r["IntentosFallidos"], r["DebeCambiarContrasena"]);
            }
            return dict;
        }

        private Dictionary<string, string> DVHsRoles()
        {
            DataTable dt = _acceso.leer("SELECT Id, Nombre FROM Roles", null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = Convert.ToString(r["Id"]);
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(r["Id"], r["Nombre"]);
            }
            return dict;
        }

        private Dictionary<string, string> DVHsFamilia()
        {
            DataTable dt = _acceso.leer("SELECT Id, Nombre FROM Familia", null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = Convert.ToString(r["Id"]);
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(r["Id"], r["Nombre"]);
            }
            return dict;
        }

        private Dictionary<string, string> DVHsPatente()
        {
            DataTable dt = _acceso.leer("SELECT Id, Nombre, DataKey FROM Patente", null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = Convert.ToString(r["Id"]);
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(r["Id"], r["Nombre"], r["DataKey"]);
            }
            return dict;
        }

        // En las tablas de relación (sin PK simple) la clave del diccionario se arma uniendo
        // las dos columnas con "_", ej. "3_7", para identificar la fila de forma única.
        private Dictionary<string, string> DVHsFamiliaPatente()
        {
            DataTable dt = _acceso.leer("SELECT IdFamilia, IdPatente FROM FamiliaPatente", null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = $"{r["IdFamilia"]}_{r["IdPatente"]}";
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(r["IdFamilia"], r["IdPatente"]);
            }
            return dict;
        }

        private Dictionary<string, string> DVHsFamiliaIntegrada()
        {
            DataTable dt = _acceso.leer("SELECT IdFamiliaPadre, IdFamiliaHija FROM FamiliaIntegrada", null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = $"{r["IdFamiliaPadre"]}_{r["IdFamiliaHija"]}";
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(r["IdFamiliaPadre"], r["IdFamiliaHija"]);
            }
            return dict;
        }

        private Dictionary<string, string> DVHsRolPatente()
        {
            DataTable dt = _acceso.leer("SELECT IdRol, IdPatente FROM RolPatente", null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = $"{r["IdRol"]}_{r["IdPatente"]}";
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(r["IdRol"], r["IdPatente"]);
            }
            return dict;
        }

        private Dictionary<string, string> DVHsRolFamilia()
        {
            DataTable dt = _acceso.leer("SELECT IdRol, IdFamilia FROM RolFamilia", null);
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
            {
                string id = $"{r["IdRol"]}_{r["IdFamilia"]}";
                dict[id] = CalculadorIntegridad_GO44.CalcularDVH(r["IdRol"], r["IdFamilia"]);
            }
            return dict;
        }

        /// <summary>Lee los DVH que están GUARDADOS para una tabla (los "esperados"), para comparar contra los recalculados.</summary>
        public Dictionary<string, string> ObtenerDVHsAlmacenados(string nombreTabla)
        {
            string q = "SELECT IdRegistro, DVH FROM IntegridadDVH WHERE NombreTabla = @T";
            DataTable dt = _acceso.leer(q, new[] { new SqlParameter("@T", nombreTabla) });
            var dict = new Dictionary<string, string>();
            foreach (DataRow r in dt.Rows)
                dict[r["IdRegistro"].ToString()] = r["DVH"].ToString();
            return dict;
        }

        /// <summary>Lee el DVV guardado de una tabla (null si nunca se calculó). Es el "esperado" de toda la tabla.</summary>
        public string ObtenerDVVAlmacenado(string nombreTabla)
        {
            string q = "SELECT DVV FROM IntegridadDVV WHERE NombreTabla = @T";
            object res = _acceso.leerEscalar(q, new[] { new SqlParameter("@T", nombreTabla) });
            return (res == null || res == DBNull.Value) ? null : res.ToString();
        }

        /// <summary>
        /// Recalcula y REESCRIBE los dígitos de una tabla (se llama después de cada cambio legítimo
        /// hecho por el sistema): recalcula los DVH de cada fila, los guarda, y a partir de ellos el DVV.
        /// </summary>
        public void RecalcularTabla(string nombreTabla)
        {
            Dictionary<string, string> dvhs = CalcularDVHsTabla(nombreTabla);
            GuardarDVHs(nombreTabla, dvhs);
            string dvv = CalculadorIntegridad_GO44.CalcularDVV(dvhs.Values);
            GuardarDVV(nombreTabla, dvv);
        }

        /// <summary>Guarda los DVH de una tabla: borra los anteriores y reinserta los nuevos, uno por fila.</summary>
        public void GuardarDVHs(string nombreTabla, Dictionary<string, string> dvhs)
        {
            _acceso.escribir(
                "DELETE FROM IntegridadDVH WHERE NombreTabla = @T",
                new[] { new SqlParameter("@T", nombreTabla) });
            foreach (var kv in dvhs)
            {
                _acceso.escribir(
                    "INSERT INTO IntegridadDVH (NombreTabla, IdRegistro, DVH) VALUES (@T, @I, @D)",
                    new[] {
                        new SqlParameter("@T", nombreTabla),
                        new SqlParameter("@I", kv.Key),
                        new SqlParameter("@D", kv.Value)
                    });
            }
        }

        /// <summary>Guarda el DVV de una tabla con un "upsert": si ya existía lo actualiza, si no lo inserta.</summary>
        public void GuardarDVV(string nombreTabla, string dvv)
        {
            string existeQ = "SELECT COUNT(1) FROM IntegridadDVV WHERE NombreTabla = @T";
            object res = _acceso.leerEscalar(existeQ, new[] { new SqlParameter("@T", nombreTabla) });
            int existe = Convert.ToInt32(res);
            if (existe > 0)
            {
                _acceso.escribir(
                    "UPDATE IntegridadDVV SET DVV = @D, FechaCalculo = GETDATE() WHERE NombreTabla = @T",
                    new[] { new SqlParameter("@D", dvv), new SqlParameter("@T", nombreTabla) });
            }
            else
            {
                _acceso.escribir(
                    "INSERT INTO IntegridadDVV (NombreTabla, DVV) VALUES (@T, @D)",
                    new[] { new SqlParameter("@T", nombreTabla), new SqlParameter("@D", dvv) });
            }
        }

        /// <summary>Indica si ya existe al menos un DVV calculado (si la base nunca se "selló", no tiene sentido verificar).</summary>
        public bool ExisteAlgunDVV()
        {
            object res = _acceso.leerEscalar("SELECT COUNT(1) FROM IntegridadDVV", null);
            return Convert.ToInt32(res) > 0;
        }

        /// <summary>
        /// Hace un BACKUP completo de la base a un archivo .bak. Se conecta a master y usa BACKUP DATABASE.
        /// Se ofrece al usuario cuando se detecta integridad rota, antes de intentar restaurar.
        /// </summary>
        public void HacerBackupSQL(string connStringMaster, string nombreBd, string rutaArchivoBak)
        {
            using (var conn = new SqlConnection(connStringMaster))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        $"BACKUP DATABASE [{nombreBd}] TO DISK = @ruta WITH INIT, FORMAT, NAME = N'Backup automatico';";
                    cmd.Parameters.AddWithValue("@ruta", rutaArchivoBak);
                    cmd.CommandTimeout = 120;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// RESTAURA la base desde un .bak. Para poder pisarla, primero la pone en modo un-solo-usuario
        /// (echando a los demás con ROLLBACK IMMEDIATE), restaura con REPLACE y vuelve a multi-usuario.
        /// ClearAllPools() libera las conexiones abiertas para que el RESTORE no quede bloqueado.
        /// </summary>
        public void RestaurarBackupSQL(string connStringMaster, string nombreBd, string rutaArchivoBak)
        {
            SqlConnection.ClearAllPools();
            using (var conn = new SqlConnection(connStringMaster))
            {
                conn.Open();
                string sql =
                    $"ALTER DATABASE [{nombreBd}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                    $"RESTORE DATABASE [{nombreBd}] FROM DISK = @ruta WITH REPLACE; " +
                    $"ALTER DATABASE [{nombreBd}] SET MULTI_USER;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ruta", rutaArchivoBak);
                    cmd.CommandTimeout = 120;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
