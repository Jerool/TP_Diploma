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
            AsegurarBaseDatosEn(instancia);
        }

        /// <summary>
        /// Asegura que la base exista y esté completa en la instancia indicada:
        ///  - si no existe, la crea con EsquemaCompleto.sql;
        ///  - si existe pero le faltan las tablas de negocio (base parcial o de otra versión),
        ///    vuelve a correr EsquemaCompleto.sql a modo de REPARACIÓN (los lotes que ya están
        ///    se ignoran solos).
        /// NO depende de EsquemaNegocio_GO44.sql (ese script no se distribuye con la app), así que
        /// nunca falla con "no se encontró el script" sobre una instancia que ya tenía una base.
        /// </summary>
        public static void AsegurarBaseDatosEn(string instancia)
        {
            if (!ExisteBaseDatos(instancia))
                InstalarBaseDatos(instancia);
            else if (!ExisteEsquemaNegocio(instancia))
                InstalarBaseDatos(instancia);   // reparación idempotente con el snapshot completo
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

        // --- Backup / Restore ("Restore sobre reinstalación") ------------------------------

        /// <summary>Hace un backup de la base. Devuelve la ruta del .bak, o null si la base no existía.</summary>
        public static string BackupBaseDatos(string instancia)
        {
            return InstaladorBD_GO44.BackupBaseDatos(instancia);
        }

        /// <summary>Restaura la base desde un archivo .bak, pisando la actual.</summary>
        public static void RestaurarBaseDatos(string instancia, string rutaBak)
        {
            InstaladorBD_GO44.RestaurarBaseDatos(instancia, rutaBak);
        }

        /// <summary>Ruta del .bak más reciente de la carpeta de backups, o null si no hay.</summary>
        public static string UltimoBackup()
        {
            return InstaladorBD_GO44.UltimoBackup();
        }

        /// <summary>Fija la cadena de conexión global de Acceso para la instancia elegida por el usuario.</summary>
        public static void ConfigurarConexion(string instancia)
        {
            Acceso.ConnectionString = ConfiguracionBD_GO44.ArmarConnectionString(
                instancia, InstaladorBD_GO44.NOMBRE_BD);
        }
    }
}
