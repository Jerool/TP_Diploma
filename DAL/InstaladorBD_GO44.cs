using System;
using System.Data.SqlClient;
using System.IO;
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
        /// Devuelve true si en la BD ya existe alguna tabla del módulo de negocio
        /// (Clientes, Componentes, Carrito, LineaCarrito). Se usa para decidir
        /// si hay que correr EsquemaNegocio_GO44.sql sobre una BD ya instalada.
        /// </summary>
        public static bool ExisteEsquemaNegocio(string instancia)
        {
            // Ojo: en connection string el nombre de BD NO va entre corchetes ([]).
            // Los [] son sintaxis SQL para identificadores, pero en la CS SqlConnection
            // acepta el nombre tal cual (con espacios y todo).
            string cs = $"Data Source={instancia};Initial Catalog={NOMBRE_BD};Integrated Security=True;Connect Timeout=3";
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM sys.tables WHERE name IN ('Clientes','Componentes','Carrito','LineaCarrito')", conn))
                {
                    cmd.CommandTimeout = 5;
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
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
    }
}
