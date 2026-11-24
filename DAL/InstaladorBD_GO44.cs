using System;
using System.Data.SqlClient;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace DAL
{
    /// <summary>
    /// Instalador automático de la base de datos. En modo Debug, si la app arranca y la BD no existe,
    /// corre el script de creación (EsquemaCompleto.sql) contra la instancia SQL elegida para dejar
    /// todo listo (tablas + SP + datos de prueba) sin tener que abrir SSMS. Es una clase estática
    /// (sin estado): son utilidades sueltas que operan sobre la instancia que se les pasa.
    /// </summary>
    public static class InstaladorBD_GO44
    {
        public const string NOMBRE_BD = "Gestion Usuario";

        // Scripts en orden de ejecución.
        //   1) EsquemaCompleto.sql              — SNAPSHOT COMPLETO generado con SSMS.
        //                                          Contiene: CREATE DATABASE + TODAS las tablas (seguridad, negocio, facturación)
        //                                          + TODOS los SPs (28) + datos iniciales (patentes, familias, roles,
        //                                          tipos de evento, usuarios, productos y clientes de prueba).
        //   2) EsquemaNegocio_GO44.sql          — Backup de seguridad. Sus tablas usan IF NOT EXISTS y sus SPs
        //                                          DROP-CREATE, así que es idempotente. Sirve por si en el futuro
        //                                          el snapshot queda desactualizado.
        //   3) EsquemaNegocio_Facturacion_GO44.sql — Idem, tablas y SPs de Facturación/Cobro. Idempotente.
        // Nota: si en la PC nueva alguien clonó y le falta algún SP, con volver a ejecutar EsquemaCompleto.sql
        //       en SSMS sobre una BD limpia queda todo. Alternativamente estos scripts se corren automáticamente
        //       cuando la app arranca en modo Debug y la BD no existe.
        // Solo el snapshot completo — ya contiene todas las tablas, SPs y datos
        // (seguridad + negocio + facturación + compras).
        private static readonly string[] SCRIPTS = {
            "EsquemaCompleto.sql"
        };

        /// <summary>Indica si la base ya existe, consultando el catálogo del sistema (sys.databases) a través de master.</summary>
        public static bool ExisteBaseDatos(string instancia)
        {
            string connMaster = $"Data Source={instancia};Initial Catalog=master;Integrated Security=True;Connect Timeout=3";
            using (var conn = new SqlConnection(connMaster))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM sys.databases WHERE name = @n", conn))
                {
                    cmd.CommandTimeout = 5;
                    cmd.Parameters.AddWithValue("@n", NOMBRE_BD);
                    int cant = Convert.ToInt32(cmd.ExecuteScalar());
                    return cant > 0;
                }
            }
        }

        /// <summary>
        /// Todas las tablas que tiene que tener una base COMPLETA (seguridad + negocio + facturación
        /// + compras + integridad). Si falta aunque sea una, la base se considera incompleta y hay que
        /// repararla volviendo a correr EsquemaCompleto.sql. Antes se chequeaban solo 4 tablas viejas,
        /// por eso una base a la que le faltaba 'Proveedores' (o las de compras) pasaba como "completa"
        /// y después el control de integridad explotaba con "Invalid object name 'Proveedores'".
        /// </summary>
        private static readonly string[] TABLAS_ESPERADAS = {
            "Carrito", "Clientes", "CobrosFactura", "Componentes", "Componentes_C",
            "EVENTOS", "Facturas", "Familia", "FamiliaIntegrada", "FamiliaPatente",
            "IntegridadDVH", "IntegridadDVV", "LineaCarrito", "LineaFactura",
            "LineasOrdenCompra", "LineasRecepcion", "LineasSolicitudCotizacion",
            "Modulo", "OrdenesCompra", "PagosOrdenCompra", "Patente", "Proveedores",
            "RecepcionesMercaderia", "RolFamilia", "RolPatente", "Roles",
            "SolicitudesCotizacion", "TipoEvento", "Usuario"
        };

        /// <summary>
        /// Devuelve true solo si la BD tiene TODAS las tablas esperadas (esquema completo). Si falta
        /// alguna, devuelve false para que el instalador/arranque vuelva a correr EsquemaCompleto.sql
        /// (idempotente: crea lo que falta y saltea lo que ya está) y así reparar bases viejas/parciales.
        /// </summary>
        public static bool ExisteEsquemaNegocio(string instancia)
        {
            // Ojo: en connection string el nombre de BD NO va entre corchetes ([]).
            // Los [] son sintaxis SQL para identificadores, pero en la CS SqlConnection
            // acepta el nombre tal cual (con espacios y todo).
            string cs = $"Data Source={instancia};Initial Catalog={NOMBRE_BD};Integrated Security=True;Connect Timeout=3";
            string inLista = string.Join(",", Array.ConvertAll(TABLAS_ESPERADAS, t => "'" + t + "'"));
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    $"SELECT COUNT(1) FROM sys.tables WHERE name IN ({inLista})", conn))
                {
                    cmd.CommandTimeout = 5;
                    // Está completa solo si están TODAS las tablas esperadas.
                    return Convert.ToInt32(cmd.ExecuteScalar()) >= TABLAS_ESPERADAS.Length;
                }
            }
        }

        /// <summary>
        /// Crea (o repara) la base corriendo EsquemaCompleto.sql.
        ///
        /// IMPORTANTE: el snapshot generado por SSMS trae un CREATE DATABASE con rutas de archivo
        /// HARDCODEADAS (C:\Users\Administrator\Gestion Usuario.mdf). En otra PC ese usuario de Windows
        /// no existe y el CREATE DATABASE falla, dejando la base sin crear. Por eso NO se ejecuta el
        /// CREATE DATABASE del script: primero se crea la base con un CREATE DATABASE limpio (sin rutas,
        /// SQL la ubica en la carpeta por defecto del servidor, lo que funciona en cualquier PC/instancia)
        /// y recién después se corre el CUERPO del script (tablas, SPs, datos) ya conectados a esa base,
        /// salteando los lotes a nivel servidor/base (CREATE/ALTER DATABASE, USE, fulltext) que no aplican.
        ///
        /// Como ADO.NET no entiende "GO" (es de SSMS), el script se parte en lotes usando GO como separador.
        /// Los lotes que fallan por features no soportados o por objetos ya existentes (en una reparación)
        /// se registran en instalador_db.log y se saltean, para no perder el resto.
        /// </summary>
        public static void InstalarBaseDatos(string instancia)
        {
            string rutaScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EsquemaCompleto.sql");
            if (!File.Exists(rutaScript))
                throw new Exception($"No se encontró el script de instalación: {rutaScript}");

            string script = File.ReadAllText(rutaScript);

            // 1) Crear la base SIN rutas hardcodeadas (si todavía no existe). Funciona en cualquier PC.
            string csMaster = $"Data Source={instancia};Initial Catalog=master;Integrated Security=True";
            using (var conn = new SqlConnection(csMaster))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    $"IF DB_ID(N'{NOMBRE_BD}') IS NULL CREATE DATABASE [{NOMBRE_BD}]", conn))
                {
                    cmd.CommandTimeout = 120;
                    cmd.ExecuteNonQuery();
                }
            }

            // 2) Correr el cuerpo del script DENTRO de la base ya creada.
            string csBase = $"Data Source={instancia};Initial Catalog={NOMBRE_BD};Integrated Security=True";
            using (var conn = new SqlConnection(csBase))
            {
                conn.Open();
                var batches = Regex.Split(script, @"^\s*GO\s*$",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);

                foreach (var batch in batches)
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    string lote = batch.Trim();

                    // Quitar comentarios para decidir si es un lote a nivel servidor/base que hay que saltear.
                    string sinComentarios = Regex.Replace(lote, @"/\*.*?\*/|--[^\r\n]*", "",
                        RegexOptions.Singleline).Trim();
                    if (sinComentarios.Length == 0) continue;
                    if (Regex.IsMatch(sinComentarios, @"^(CREATE|ALTER)\s+DATABASE\b", RegexOptions.IgnoreCase)) continue;
                    if (Regex.IsMatch(sinComentarios, @"^USE\b", RegexOptions.IgnoreCase)) continue;
                    if (sinComentarios.IndexOf("FULLTEXTSERVICEPROPERTY", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                    try
                    {
                        using (var cmd = new SqlCommand(lote, conn))
                        {
                            cmd.CommandTimeout = 120;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        // Lotes no críticos: features no soportados en LocalDB/Express, u objetos que ya
                        // existen cuando se repara una base. Se ignoran y se sigue con el resto.
                        try
                        {
                            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "instalador_db.log");
                            File.AppendAllText(logPath,
                                $"[{DateTime.Now}] Lote ignorado: {ex.Message}{Environment.NewLine}");
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Aplica solo el esquema de negocio (útil si la BD ya existe de una instalación
        /// vieja y hay que sumarle las tablas/SPs GO44 nuevas).
        /// </summary>
        public static void InstalarEsquemaNegocio(string instancia)
        {
            string rutaScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EsquemaNegocio_GO44.sql");
            if (!File.Exists(rutaScript))
                throw new Exception($"No se encontró el script: {rutaScript}");

            string script = File.ReadAllText(rutaScript);
            string cs = $"Data Source={instancia};Initial Catalog={NOMBRE_BD};Integrated Security=True";

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var batches = Regex.Split(script, @"^\s*GO\s*$",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);

                foreach (var batch in batches)
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    using (var cmd = new SqlCommand(batch, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Migración idempotente de la patente "Maestros.ProductosCambios" (pantalla Control de
        /// cambios) y su asignación a los roles supervisores (Supervisor de Ventas / de Compras),
        /// usando el script Migracion_PatenteProductosCambios_GO44.sql que viaja junto al .exe.
        /// Solo escribe si falta algo; en ese caso recalcula los dígitos verificadores de las tablas
        /// tocadas (Patente, Roles, RolPatente). Best-effort: cualquier fallo se traga y queda en el log.
        /// </summary>
        public static void AplicarMigracionProductosCambios(string instancia)
        {
            try
            {
                string cs = $"Data Source={instancia};Initial Catalog={NOMBRE_BD};Integrated Security=True";
                string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           "Migracion_PatenteProductosCambios_GO44.sql");
                if (!File.Exists(ruta)) return;   // si el script no viajó con la app, no frenar el arranque

                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();

                    // Si ya está todo aplicado no se escribe ni se recalcula: así no se re-sella la
                    // integridad en cada arranque (eso ocultaría una manipulación de estas tablas).
                    if (MigracionProductosCambiosYaAplicada(conn)) return;

                    string script = File.ReadAllText(ruta);
                    var batches = Regex.Split(script, @"^\s*GO\s*$",
                        RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    foreach (var batch in batches)
                    {
                        if (string.IsNullOrWhiteSpace(batch)) continue;
                        string lote = batch.Trim();
                        string sinComentarios = Regex.Replace(lote, @"/\*.*?\*/|--[^\r\n]*", "",
                            RegexOptions.Singleline).Trim();
                        if (sinComentarios.Length == 0) continue;
                        if (Regex.IsMatch(sinComentarios, @"^USE\b", RegexOptions.IgnoreCase)) continue;

                        using (var cmd = new SqlCommand(lote, conn))
                        {
                            cmd.CommandTimeout = 120;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Se tocaron Patente / Roles / RolPatente: recalcular sus dígitos verificadores,
                // igual que en el resto del sistema. No recalcular si la integridad ya está rota.
                if (!DALIntegridad_GO44.IntegridadConocidamenteRota)
                {
                    var dalIntegridad = new DALIntegridad_GO44();
                    try { dalIntegridad.RecalcularTabla("Patente"); } catch { }
                    try { dalIntegridad.RecalcularTabla("Roles"); } catch { }
                    try { dalIntegridad.RecalcularTabla("RolPatente"); } catch { }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "instalador_db.log");
                    File.AppendAllText(logPath,
                        $"[{DateTime.Now}] AVISO migración ProductosCambios: {ex.Message}{Environment.NewLine}");
                }
                catch { }
            }
        }

        /// <summary>true si la patente, los dos roles supervisores y sus dos asignaciones ya existen.</summary>
        private static bool MigracionProductosCambiosYaAplicada(SqlConnection conn)
        {
            const string q = @"
SELECT CASE WHEN
      EXISTS (SELECT 1 FROM dbo.Patente WHERE DataKey = N'Maestros.ProductosCambios')
  AND EXISTS (SELECT 1 FROM dbo.Roles   WHERE Nombre  = N'Supervisor de Ventas')
  AND EXISTS (SELECT 1 FROM dbo.Roles   WHERE Nombre  = N'Supervisor de Compras')
  AND EXISTS (SELECT 1 FROM dbo.RolPatente rp
               JOIN dbo.Roles   r ON r.Id = rp.IdRol
               JOIN dbo.Patente p ON p.Id = rp.IdPatente
              WHERE p.DataKey = N'Maestros.ProductosCambios' AND r.Nombre = N'Supervisor de Ventas')
  AND EXISTS (SELECT 1 FROM dbo.RolPatente rp
               JOIN dbo.Roles   r ON r.Id = rp.IdRol
               JOIN dbo.Patente p ON p.Id = rp.IdPatente
              WHERE p.DataKey = N'Maestros.ProductosCambios' AND r.Nombre = N'Supervisor de Compras')
  THEN 1 ELSE 0 END";
            using (var cmd = new SqlCommand(q, conn))
            {
                cmd.CommandTimeout = 15;
                object r = cmd.ExecuteScalar();
                return r != null && r != DBNull.Value && Convert.ToInt32(r) == 1;
            }
        }

        // ====================================================================
        //  BACKUP / RESTORE  ("Restore sobre reinstalación")
        // --------------------------------------------------------------------
        //  Al reinstalar sobre una base ya existente, el instalador hace un
        //  backup automático ANTES de tocar nada, y permite RESTAURAR el último
        //  backup para volver la base a un estado anterior.
        //
        //  OJO: el archivo .bak lo escribe/lee el PROCESO de SQL Server (no la
        //  app). Por eso se usa una carpeta fija y accesible
        //  (C:\ProgramData\TechFlow\Backups). Con LocalDB (corre como el usuario)
        //  funciona directo; para un SQL Server como servicio, esa carpeta tiene
        //  que ser escribible por la cuenta del servicio.
        // ====================================================================

        private static string RutaCarpetaBackups()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "TechFlow", "Backups");
        }

        /// <summary>Carpeta donde se guardan los backups (.bak); la crea si no existe.</summary>
        public static string CarpetaBackups()
        {
            string carpeta = RutaCarpetaBackups();
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
                // Best-effort: permiso de escritura a los usuarios locales, para que tanto
                // LocalDB (usuario) como un SQL Express (servicio) puedan escribir el .bak.
                try
                {
                    var di = new DirectoryInfo(carpeta);
                    var sec = di.GetAccessControl();
                    sec.AddAccessRule(new FileSystemAccessRule(
                        new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                        FileSystemRights.Modify | FileSystemRights.Synchronize,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None, AccessControlType.Allow));
                    di.SetAccessControl(sec);
                }
                catch { }
            }
            return carpeta;
        }

        /// <summary>Devuelve el .bak más reciente de la carpeta de backups, o null si no hay.</summary>
        public static string UltimoBackup()
        {
            string carpeta = RutaCarpetaBackups();
            if (!Directory.Exists(carpeta)) return null;
            string[] files = Directory.GetFiles(carpeta, "GestionUsuario_*.bak");
            if (files.Length == 0) return null;
            Array.Sort(files);               // el timestamp en el nombre ordena cronológicamente
            return files[files.Length - 1];
        }

        /// <summary>
        /// Hace un backup de la base en la carpeta de backups. Devuelve la ruta del .bak
        /// generado, o null si la base no existía (nada para respaldar).
        /// </summary>
        public static string BackupBaseDatos(string instancia)
        {
            if (!ExisteBaseDatos(instancia)) return null;

            string archivo = Path.Combine(CarpetaBackups(),
                "GestionUsuario_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak");

            string csMaster = $"Data Source={instancia};Initial Catalog=master;Integrated Security=True";
            using (var conn = new SqlConnection(csMaster))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "BACKUP DATABASE [" + NOMBRE_BD + "] TO DISK = @ruta " +
                    "WITH INIT, FORMAT, COPY_ONLY, NAME = N'TechFlow - backup automatico'", conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@ruta", archivo);
                    cmd.ExecuteNonQuery();
                }
            }
            return archivo;
        }

        /// <summary>
        /// Restaura la base desde un archivo .bak, pisando la actual. Cierra las conexiones
        /// abiertas (SINGLE_USER), restaura con REPLACE y vuelve a MULTI_USER. Pensado para
        /// restaurar sobre la MISMA instancia que generó el backup (reinstalación).
        /// </summary>
        public static void RestaurarBaseDatos(string instancia, string rutaBak)
        {
            if (string.IsNullOrWhiteSpace(rutaBak) || !File.Exists(rutaBak))
                throw new Exception("No se encontró el archivo de backup: " + rutaBak);

            string csMaster = $"Data Source={instancia};Initial Catalog=master;Integrated Security=True";
            using (var conn = new SqlConnection(csMaster))
            {
                conn.Open();

                // Si la base está en uso, forzar el cierre de conexiones para poder restaurar.
                if (ExisteEnConexion(conn))
                {
                    using (var cmd = new SqlCommand(
                        "ALTER DATABASE [" + NOMBRE_BD + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                }

                try
                {
                    using (var cmd = new SqlCommand(
                        "RESTORE DATABASE [" + NOMBRE_BD + "] FROM DISK = @ruta WITH REPLACE", conn))
                    {
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.AddWithValue("@ruta", rutaBak);
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    // Por las dudas (si el restore falló y la base quedó en SINGLE_USER), reabrirla.
                    try
                    {
                        using (var cmd = new SqlCommand(
                            "IF DB_ID(N'" + NOMBRE_BD + "') IS NOT NULL " +
                            "ALTER DATABASE [" + NOMBRE_BD + "] SET MULTI_USER", conn))
                        {
                            cmd.CommandTimeout = 60;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch { }
                }
            }
        }

        private static bool ExisteEnConexion(SqlConnection conn)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM sys.databases WHERE name = @n", conn))
            {
                cmd.Parameters.AddWithValue("@n", NOMBRE_BD);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
