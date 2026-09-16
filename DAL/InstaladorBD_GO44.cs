using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace DAL
{
    public static class InstaladorBD_GO44
    {
        public const string NOMBRE_BD = "Gestion Usuario";
        private const string NOMBRE_SCRIPT = "EsquemaCompleto.sql";

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

        public static void InstalarBaseDatos(string instancia)
        {
            string rutaScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NOMBRE_SCRIPT);
            if (!File.Exists(rutaScript))
                throw new Exception($"No se encontró el script de instalación: {rutaScript}");

            string script = File.ReadAllText(rutaScript);
            string connMaster = $"Data Source={instancia};Initial Catalog=master;Integrated Security=True";

            using (var conn = new SqlConnection(connMaster))
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
