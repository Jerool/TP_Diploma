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
                        // La BD existe → verificar si tiene el esquema de negocio GO44 (Clientes, Componentes...)
                        // Si NO lo tiene (instalación vieja o migración), correr solo el script de negocio.
                        try
                        {
                            if (!BLLInstalador_GO44.ExisteEsquemaNegocio(instancia))
                                BLLInstalador_GO44.InstalarEsquemaNegocio(instancia);
                        }
                        catch (Exception exNeg)
                        {
                            MessageBox.Show("No se pudo instalar el esquema de negocio:\n" + exNeg.Message,
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        BLLInstalador_GO44.ConfigurarConexion(instancia);
                        return true;
                    }

#if DEBUG
                    try
                    {
                        BLLInstalador_GO44.InstalarBaseDatos(instancia);   // Corre EsquemaCompleto + EsquemaNegocio_GO44
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

                // Si la BD no existe en la instancia elegida, la instala completa (base + negocio)
                try
                {
                    if (!BLLInstalador_GO44.ExisteBaseDatos(frm.InstanciaElegida))
                        BLLInstalador_GO44.InstalarBaseDatos(frm.InstanciaElegida);
                    else if (!BLLInstalador_GO44.ExisteEsquemaNegocio(frm.InstanciaElegida))
                        BLLInstalador_GO44.InstalarEsquemaNegocio(frm.InstanciaElegida);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error instalando la BD:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                BLLInstalador_GO44.ConfigurarConexion(frm.InstanciaElegida);
                return true;
            }
        }
    }
}
