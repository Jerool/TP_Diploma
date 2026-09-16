using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Sql;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Servicios.Instalacion
{
    public static class DetectorInstancias_GO44
    {
        private const int TIMEOUT_SEG_ENUMERATOR = 3;

        public static List<string> DetectarInstancias()
        {
            var lista = new List<string>();

            foreach (var inst in DetectarLocalDB())
                if (!lista.Contains(inst)) lista.Add(inst);

            foreach (var inst in DetectarSqlServerLocalesConTimeout(TIMEOUT_SEG_ENUMERATOR))
                if (!lista.Contains(inst)) lista.Add(inst);

            if (lista.Count == 0)
                lista.Add(@"(localdb)\MSSQLLocalDB");

            return lista;
        }

        private static List<string> DetectarSqlServerLocalesConTimeout(int segundos)
        {
            var lista = new List<string>();

            var task = Task.Run(() =>
            {
                try
                {
                    var instancias = SqlDataSourceEnumerator.Instance.GetDataSources();
                    foreach (System.Data.DataRow r in instancias.Rows)
                    {
                        string server = r["ServerName"]?.ToString();
                        string inst = r["InstanceName"]?.ToString();
                        if (string.IsNullOrEmpty(server)) continue;
                        string nombre = string.IsNullOrEmpty(inst) ? server : $"{server}\\{inst}";
                        lock (lista) { lista.Add(nombre); }
                    }
                }
                catch { }
            });

            try { task.Wait(TimeSpan.FromSeconds(segundos)); }
            catch { }

            lock (lista) { return new List<string>(lista); }
        }

        private static List<string> DetectarLocalDB()
        {
            var lista = new List<string>();

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions"))
                {
                    if (key != null)
                    {
                        foreach (var version in key.GetSubKeyNames())
                        {
                            lista.Add(@"(localdb)\MSSQLLocalDB");
                            break;
                        }
                    }
                }
            }
            catch { }

            if (lista.Count == 0)
            {
                try
                {
                    var psi = new ProcessStartInfo("sqllocaldb", "info")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    var p = Process.Start(psi);
                    string salida = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(3000);
                    if (!string.IsNullOrWhiteSpace(salida))
                    {
                        foreach (var linea in salida.Split('\n'))
                        {
                            var nombre = linea.Trim();
                            if (!string.IsNullOrEmpty(nombre))
                                lista.Add($"(localdb)\\{nombre}");
                        }
                    }
                }
                catch { }
            }

            return lista;
        }
    }
}
