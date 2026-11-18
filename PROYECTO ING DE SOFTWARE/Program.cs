using BLL;
using Servicios;
using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // ── MODO INSTALADOR (desatendido / sin ventanas) ────────────────────────────
            // Lo invoca el instalador de Windows (Inno Setup) después de copiar los archivos:
            //     "PROYECTO ING DE SOFTWARE.exe" --setup-db "<instancia SQL>"
            // Fija la instancia elegida en el .exe.config y crea la base si no existe.
            // Devuelve 0 si salió todo bien y 1 si hubo error (queda en instalador_db.log).
            if (args != null && args.Length >= 1 &&
                string.Equals(args[0], "--setup-db", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(EjecutarSetupBd(args.Length >= 2 ? args[1] : null));
                return;
            }

            // ── MODO RESTORE (desatendido) ──────────────────────────────────────────────
            // Lo invoca el instalador cuando el usuario elige RESTAURAR un backup al reinstalar:
            //     "PROYECTO ING DE SOFTWARE.exe" --restore-db "<instancia SQL>" "<ruta .bak>"
            // Si no se pasa la ruta, restaura el último backup disponible.
            if (args != null && args.Length >= 1 &&
                string.Equals(args[0], "--restore-db", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(EjecutarRestoreBd(
                    args.Length >= 2 ? args[1] : null,
                    args.Length >= 3 ? args[2] : null));
                return;
            }

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

        /// <summary>
        /// Modo instalación desatendida (lo llama el instalador Inno Setup con --setup-db).
        /// 1) Graba la instancia SQL elegida en el .exe.config (clave "GestionUsuario"), de modo
        ///    que CUALQUIER usuario de la PC abra la app contra esa instancia.
        /// 2) Crea la base de datos con EsquemaCompleto.sql si todavía no existe.
        /// No muestra ventanas: devuelve 0 (OK) o 1 (error) y deja rastro en instalador_db.log,
        /// al lado del .exe, para que el instalador pueda avisar si algo falló.
        /// </summary>
        private static int EjecutarSetupBd(string instancia)
        {
            string log = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "instalador_db.log");
            try
            {
                if (string.IsNullOrWhiteSpace(instancia))
                    instancia = @"(localdb)\MSSQLLocalDB";
                instancia = instancia.Trim();

                string cs = "Data Source=" + instancia +
                            ";Initial Catalog=Gestion Usuario;Integrated Security=True";

                // 1) Persistir la instancia elegida en el .exe.config (machine-wide).
                try
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(
                        ConfigurationUserLevel.None);
                    ConnectionStringSettings seccion =
                        config.ConnectionStrings.ConnectionStrings["GestionUsuario"];
                    if (seccion != null)
                        seccion.ConnectionString = cs;
                    else
                        config.ConnectionStrings.ConnectionStrings.Add(
                            new ConnectionStringSettings("GestionUsuario", cs, "System.Data.SqlClient"));
                    config.Save(ConfigurationSaveMode.Modified);
                    File.AppendAllText(log,
                        "[" + DateTime.Now + "] Instancia configurada en el .config: " + instancia + Environment.NewLine);
                }
                catch (Exception exCfg)
                {
                    File.AppendAllText(log,
                        "[" + DateTime.Now + "] AVISO: no se pudo escribir el .exe.config: " + exCfg.Message + Environment.NewLine);
                }

                bool existe = BLLInstalador_GO44.ExisteBaseDatos(instancia);

                // 2) Si la base YA existe (reinstalación), backup automático ANTES de tocar nada.
                if (existe)
                {
                    try
                    {
                        string bak = BLLInstalador_GO44.BackupBaseDatos(instancia);
                        if (!string.IsNullOrEmpty(bak))
                            File.AppendAllText(log,
                                "[" + DateTime.Now + "] Backup automático de la base en: " + bak + Environment.NewLine);
                    }
                    catch (Exception exBk)
                    {
                        File.AppendAllText(log,
                            "[" + DateTime.Now + "] AVISO: no se pudo hacer el backup automático: " + exBk.Message + Environment.NewLine);
                    }
                }

                // 3) Crear (o reparar) la base. Reusa EsquemaCompleto.sql que viaja junto al .exe.
                //    - No existe -> se crea.
                //    - Existe pero incompleta (base vieja / de otra versión) -> se repara.
                //    - Existe y completa -> no se toca.
                if (!existe)
                {
                    BLLInstalador_GO44.InstalarBaseDatos(instancia);
                    File.AppendAllText(log,
                        "[" + DateTime.Now + "] Base 'Gestion Usuario' creada en " + instancia + "." + Environment.NewLine);
                }
                else if (!BLLInstalador_GO44.ExisteEsquemaNegocio(instancia))
                {
                    BLLInstalador_GO44.InstalarBaseDatos(instancia);
                    File.AppendAllText(log,
                        "[" + DateTime.Now + "] La base existía incompleta en " + instancia + "; se reparó con EsquemaCompleto.sql." + Environment.NewLine);
                }
                else
                {
                    File.AppendAllText(log,
                        "[" + DateTime.Now + "] La base ya existía completa en " + instancia + "; no se recreó." + Environment.NewLine);
                }

                return 0;
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(log,
                        "[" + DateTime.Now + "] ERROR: " + ex.Message + Environment.NewLine +
                        ex.StackTrace + Environment.NewLine);
                }
                catch { }
                return 1;
            }
        }

        /// <summary>
        /// Modo restauración desatendida (lo llama el instalador con --restore-db cuando el usuario
        /// elige restaurar un backup al reinstalar). Restaura la base desde el .bak indicado (o el
        /// último disponible si no se pasa ruta). Devuelve 0 (OK) o 1 (error) y deja rastro en el log.
        /// </summary>
        private static int EjecutarRestoreBd(string instancia, string rutaBak)
        {
            string log = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "instalador_db.log");
            try
            {
                if (string.IsNullOrWhiteSpace(instancia))
                    instancia = @"(localdb)\MSSQLLocalDB";
                instancia = instancia.Trim();

                if (string.IsNullOrWhiteSpace(rutaBak))
                    rutaBak = BLLInstalador_GO44.UltimoBackup();

                if (string.IsNullOrWhiteSpace(rutaBak))
                {
                    File.AppendAllText(log,
                        "[" + DateTime.Now + "] RESTORE: no hay ningún backup para restaurar." + Environment.NewLine);
                    return 1;
                }

                BLLInstalador_GO44.RestaurarBaseDatos(instancia, rutaBak);
                File.AppendAllText(log,
                    "[" + DateTime.Now + "] Base restaurada desde: " + rutaBak + Environment.NewLine);
                return 0;
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(log,
                        "[" + DateTime.Now + "] ERROR RESTORE: " + ex.Message + Environment.NewLine +
                        ex.StackTrace + Environment.NewLine);
                }
                catch { }
                return 1;
            }
        }
    }
}
