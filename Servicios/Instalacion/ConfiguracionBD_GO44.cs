using System;
using System.IO;

namespace Servicios.Instalacion
{
    public static class ConfiguracionBD_GO44
    {
        private const string NOMBRE_ARCHIVO = "conexion.cfg";
        private const string CARPETA_APP    = "GestionUsuarios";

        private static string CarpetaConfig
        {
            get
            {
                string appData = Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appData, CARPETA_APP);
            }
        }

        private static string RutaArchivo
        {
            get { return Path.Combine(CarpetaConfig, NOMBRE_ARCHIVO); }
        }

        private static string RutaArchivoLegacy
        {
            get
            {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(dir, NOMBRE_ARCHIVO);
            }
        }

        public static string LeerInstanciaGuardada()
        {
            try
            {
                if (File.Exists(RutaArchivo))
                {
                    string contenido = File.ReadAllText(RutaArchivo).Trim();
                    if (!string.IsNullOrEmpty(contenido)) return contenido;
                }

                if (File.Exists(RutaArchivoLegacy))
                {
                    string contenido = File.ReadAllText(RutaArchivoLegacy).Trim();
                    if (!string.IsNullOrEmpty(contenido))
                    {
                        GuardarInstancia(contenido);
                        return contenido;
                    }
                }

                return null;
            }
            catch { return null; }
        }

        public static void GuardarInstancia(string instancia)
        {
            try
            {
                if (!Directory.Exists(CarpetaConfig))
                    Directory.CreateDirectory(CarpetaConfig);

                File.WriteAllText(RutaArchivo, instancia ?? string.Empty);
            }
            catch { }
        }

        public static string ArmarConnectionString(string instancia, string nombreBd)
        {
            return $"Data Source={instancia};Initial Catalog=\"{nombreBd}\";Integrated Security=True";
        }

        public static string ArmarConnectionStringMaster(string instancia)
        {
            return $"Data Source={instancia};Initial Catalog=master;Integrated Security=True";
        }
    }
}
