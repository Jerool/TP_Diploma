using BLL;
using DAL;
using Servicios;
using System;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Captura global de excepciones no manejadas
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
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

            // Instalación silenciosa de la BD (crea la BD si no existe con EsquemaCompleto.sql)
            try
            {
                BLLInstalador_GO44.AsegurarBaseDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo preparar la base de datos:\n\n" + ex.Message,
                    "Error de instalación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FRMIniciarSesion());
        }
    }
}
