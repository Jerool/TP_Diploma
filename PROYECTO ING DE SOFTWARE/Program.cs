using BLL;
using Servicios;
using Servicios.Instalacion;
using System;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    internal static class Program
    {
        // NOTA: NO hardcodear ninguna instancia por default. La app siempre
        // debe preguntar al usuario cuál es su instancia SQL en la primera
        // ejecución (FRMSeleccionInstancia). Esto garantiza portabilidad
        // entre PCs con distintos SQL Server (LocalDB, SQLEXPRESS, etc.).

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
            // Solo intentamos auto-conectar si el usuario YA eligió su instancia alguna vez
            // (guardada en %LocalAppData%\GestionUsuarios\conexion.cfg).
            // No hay defaults hardcodeados: garantiza que la app funcione en cualquier PC
            // con cualquier instancia SQL (LocalDB, SQLEXPRESS, servidor remoto, etc.).
            string instancia = ConfiguracionBD_GO44.LeerInstanciaGuardada();

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
                    // Si la BD no existe en la instancia guardada, caemos al FRMSeleccionInstancia
                    // para que el usuario confirme la instancia y la instalemos ahí.
                }
                catch
                {
                    // Si falla la conexión a la instancia guardada (ej: cambió el nombre de la PC,
                    // se desinstaló SQL Server, etc.), también caemos al selector.
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
