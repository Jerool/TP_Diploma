using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace DAL
{
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
        private static readonly string[] SCRIPTS = {
            "EsquemaCompleto.sql",
            "EsquemaNegocio_GO44.sql",
            "EsquemaNegocio_Facturacion_GO44.sql"
        };

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

        public static void InstalarBaseDatos(string instancia)
        {
            foreach (string nombreScript in SCRIPTS)
            {
                string rutaScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreScript);
                if (!File.Exists(rutaScript))
                    throw new Exception($"No se encontró el script de instalación: {rutaScript}");

                string script = File.ReadAllText(rutaScript);
                // El primer script se conecta a master (crea la BD); el segundo directamente a la BD ya creada.
                bool esBase = nombreScript.Equals("EsquemaCompleto.sql", StringComparison.OrdinalIgnoreCase);
                string cs = esBase
                    ? $"Data Source={instancia};Initial Catalog=master;Integrated Security=True"
                    : $"Data Source={instancia};Initial Catalog={NOMBRE_BD};Integrated Security=True";

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
