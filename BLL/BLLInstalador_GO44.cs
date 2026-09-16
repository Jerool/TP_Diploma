using DAL;
using Servicios.Instalacion;

namespace BLL
{
    public static class BLLInstalador_GO44
    {
        public static string NombreBD
        {
            get { return InstaladorBD_GO44.NOMBRE_BD; }
        }

        public static bool ExisteBaseDatos(string instancia)
        {
            return InstaladorBD_GO44.ExisteBaseDatos(instancia);
        }

        public static void InstalarBaseDatos(string instancia)
        {
            InstaladorBD_GO44.InstalarBaseDatos(instancia);
        }

        public static void ConfigurarConexion(string instancia)
        {
            Acceso.InstanciaActual = instancia;
            Acceso.ConnectionString = ConfiguracionBD_GO44.ArmarConnectionString(
                instancia, InstaladorBD_GO44.NOMBRE_BD);
        }
    }
}
