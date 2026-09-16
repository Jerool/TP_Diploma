using BLL;
using Servicios;
using Servicios.Instalacion;
using System;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    internal static class Program
    {
        private const string INSTANCIA_DEBUG_DEFAULT = @"(localdb)\MSSQLLocalDB";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Captura global de excepciones no manejadas — evita que la app crashee
            // y muestra el mensaje + stack trace en un MessageBox.
            Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, ex) =>
            {
                MessageBox.Show(
                    "Error no manejado:\n\n" + ex.Exception.Message + "\n\n" + ex.Exception.StackTrace,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                Exception e = ex.ExceptionObject as Exception;
                MessageBox.Show(
                    "Error crítico:\n\n" + (e != null ? e.Message + "\n\n" + e.StackTrace : ex.ExceptionObject.ToString()),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            IdiomaManager_GO44.Instancia.CambiarIdioma(IdiomaManager_GO44.IDIOMA_POR_DEFECTO);

            if (!ConfigurarConexionBD()) return;

            Application.Run(new FRMIniciarSesion());
        }

        private static bool ConfigurarConexionBD()
        {
            string instancia = ConfiguracionBD_GO44.LeerInstanciaGuardada();

#if DEBUG
            if (string.IsNullOrEmpty(instancia))
                instancia = INSTANCIA_DEBUG_DEFAULT;
#endif

            if (!string.IsNullOrEmpty(instancia))
            {
                try
                {
                    if (BLLInstalador_GO44.ExisteBaseDatos(instancia))
                    {
                        BLLInstalador_GO44.ConfigurarConexion(instancia);
                        return true;
                    }

#if DEBUG
                    try
                    {
                        BLLInstalador_GO44.InstalarBaseDatos(instancia);
                        BLLInstalador_GO44.ConfigurarConexion(instancia);
                        return true;
                    }
                    catch { }
#endif
                }
                catch
                {
                }
            }

            using (var frm = new FRMSeleccionInstancia())
            {
                DialogResult r = frm.ShowDialog();
                if (r != DialogResult.OK || string.IsNullOrEmpty(frm.InstanciaElegida))
                    return false;

                BLLInstalador_GO44.ConfigurarConexion(frm.InstanciaElegida);
                return true;
            }
        }
    }
}
