using DAL;
using Servicios.Instalacion;
using System.Data.SqlClient;

namespace BLL
{
    /// <summary>
    /// BLL (fachada) del instalador de la base. Expone a la UI un punto de entrada simple para
    /// asegurarse de que la BD exista y configurar la conexión, delegando el trabajo pesado en la DAL
    /// (InstaladorBD_GO44). Es estática porque son utilidades sin estado, de arranque de la app.
    /// </summary>
    public static class BLLInstalador_GO44
    {
        /// <summary>Nombre de la base ("Gestion Usuario"), expuesto para la UI.</summary>
        public static string NombreBD
        {
            get { return InstaladorBD_GO44.NOMBRE_BD; }
        }

        /// <summary>
        /// Punto de entrada único: si la BD no existe en la instancia configurada
        /// en App.config (o el fallback hardcodeado), la crea corriendo EsquemaCompleto.sql
        /// que está al lado del .exe. Silencioso.
        /// </summary>
        public static void AsegurarBaseDatos()
        {
            // Sacar la instancia del CS configurado (App.config o hardcodeado)
            string instancia = new SqlConnectionStringBuilder(Acceso.ConnectionString).DataSource;

            if (!ExisteBaseDatos(instancia))
                InstalarBaseDatos(instancia);
            else if (!ExisteEsquemaNegocio(instancia))
                InstalarEsquemaNegocio(instancia);
        }

        public static bool ExisteBaseDatos(string instancia)
        {
            return InstaladorBD_GO44.ExisteBaseDatos(instancia);
        }

        public static void InstalarBaseDatos(string instancia)
        {
            InstaladorBD_GO44.InstalarBaseDatos(instancia);
        }

        public static bool ExisteEsquemaNegocio(string instancia)
        {
            return InstaladorBD_GO44.ExisteEsquemaNegocio(instancia);
        }

        public static void InstalarEsquemaNegocio(string instancia)
        {
            InstaladorBD_GO44.InstalarEsquemaNegocio(instancia);
        }

        /// <summary>Fija la cadena de conexión global de Acceso para la instancia elegida por el usuario.</summary>
        public static void ConfigurarConexion(string instancia)
        {
            Acceso.ConnectionString = ConfiguracionBD_GO44.ArmarConnectionString(
                instancia, InstaladorBD_GO44.NOMBRE_BD);
        }
    }
}
