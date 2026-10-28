using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BLL
{
    /// <summary>
    /// BLL de INTEGRIDAD. Orquesta dos cosas:
    ///  1) La verificación de dígitos verificadores (DVH por fila, DVV por tabla): compara lo que hay
    ///     hoy en la base contra lo guardado para detectar si alguien modificó/insertó/borró filas por afuera.
    ///  2) Los backups de la base (manuales y automáticos cada N horas con un Timer), con retención de los últimos.
    /// Se apoya en DALIntegridad para los cálculos y en CalculadorIntegridad (Servicios) para los hashes.
    /// </summary>
    public class BLLIntegridad_GO44
    {
        private readonly DALIntegridad_GO44 _dal;

        private const string NOMBRE_BD = "Gestion Usuario";

        private static string CONN_MASTER
        {
            get
            {
                // La instancia sale de:
                //   1) Acceso.InstanciaActual (seteada en runtime al configurar la conexión)
                //   2) conexion.cfg (elegida por el usuario en FRMSeleccionInstancia)
                // NO hay default hardcodeado: si no hay instancia, es un error de config.
                string instancia = DAL.Acceso.InstanciaActual;
                if (string.IsNullOrEmpty(instancia))
                    instancia = Servicios.Instalacion.ConfiguracionBD_GO44.LeerInstanciaGuardada();
                if (string.IsNullOrEmpty(instancia))
                    throw new InvalidOperationException(
                        "No hay una instancia SQL configurada para el backup. " +
                        "Reiniciá la aplicación y elegí tu instancia.");
                return $"Data Source={instancia};Initial Catalog=master;Integrated Security=True";
            }
        }

        private const string CARPETA_BACKUPS = @"C:\Backups\GestionUsuario";
        private const int CANTIDAD_BACKUPS_A_CONSERVAR = 5;   // retención: se conservan los 5 más nuevos
        public const int INTERVALO_BACKUP_HORAS = 3;          // frecuencia del backup automático

        // Timer compartido (static) para que el backup programado sea único en toda la app; _lockTimer lo protege.
        private static Timer _timerBackup;
        private static readonly object _lockTimer = new object();

        public BLLIntegridad_GO44()
        {
            _dal = new DALIntegridad_GO44();
        }

        /// <summary>Atajo a la bandera global de integridad rota (vive en la DAL). Si está en true, las DAL dejan de recalcular dígitos.</summary>
        public static bool IntegridadConocidamenteRota
        {
            get { return DALIntegridad_GO44.IntegridadConocidamenteRota; }
            set { DALIntegridad_GO44.IntegridadConocidamenteRota = value; }
        }

        /// <summary>
        /// Verifica la integridad de todas las tablas protegidas. Para cada una compara los DVH actuales
        /// contra los guardados y detecta tres tipos de alteración: filas Insertadas (están ahora y no en
        /// el guardado), Modificadas (el hash cambió) y Eliminadas (estaban guardadas y ya no están). Si
        /// ninguna fila cambió, compara además el DVV (hash de toda la tabla) como red de seguridad.
        /// La primera vez (sin ningún DVV guardado) hace "bootstrap": calcula la línea base y la da por íntegra.
        /// </summary>
        public ResultadoIntegridad Verificar()
        {
            var resultado = new ResultadoIntegridad();

            // Bootstrap global: si la base nunca se "selló", se calcula la línea base y listo.
            if (!_dal.ExisteAlgunDVV())
            {
                Recalcular();
                resultado.EsBootstrap = true;
                resultado.EsIntegra = true;
                IntegridadConocidamenteRota = false;
                return resultado;
            }

            foreach (var tabla in DALIntegridad_GO44.TABLAS_PROTEGIDAS)
            {
                // Tabla protegida sin baseline todavía (ej: recién agregada a la lista, o una BD
                // instalada antes de protegerla): se calcula su línea base y se considera íntegra,
                // en vez de marcarla como alterada. Mismo criterio que el bootstrap global.
                if (_dal.ObtenerDVVAlmacenado(tabla) == null)
                {
                    RecalcularTabla(tabla);
                    continue;
                }

                Dictionary<string, string> dvhsAhora = _dal.CalcularDVHsTabla(tabla);
                Dictionary<string, string> dvhsBd    = _dal.ObtenerDVHsAlmacenados(tabla);

                bool comprometida = false;

                foreach (var kv in dvhsAhora)
                {
                    if (!dvhsBd.TryGetValue(kv.Key, out string bd))
                    {
                        resultado.Detalles.Add(new DetalleTampering
                        {
                            Tabla = tabla, IdRegistro = kv.Key, Tipo = TipoTampering.Insertado
                        });
                        comprometida = true;
                    }
                    else if (bd != kv.Value)
                    {
                        resultado.Detalles.Add(new DetalleTampering
                        {
                            Tabla = tabla, IdRegistro = kv.Key, Tipo = TipoTampering.Modificado
                        });
                        comprometida = true;
                    }
                }

                foreach (var kv in dvhsBd)
                {
                    if (!dvhsAhora.ContainsKey(kv.Key))
                    {
                        resultado.Detalles.Add(new DetalleTampering
                        {
                            Tabla = tabla, IdRegistro = kv.Key, Tipo = TipoTampering.Eliminado
                        });
                        comprometida = true;
                    }
                }

                if (!comprometida)
                {
                    string dvvAhora = CalculadorIntegridad_GO44.CalcularDVV(dvhsAhora.Values);
                    string dvvBd    = _dal.ObtenerDVVAlmacenado(tabla);
                    if (dvvAhora != dvvBd) comprometida = true;
                }

                if (comprometida && !resultado.TablasComprometidas.Contains(tabla))
                    resultado.TablasComprometidas.Add(tabla);
            }

            resultado.EsIntegra = resultado.TablasComprometidas.Count == 0;
            IntegridadConocidamenteRota = !resultado.EsIntegra;
            return resultado;
        }

        /// <summary>Recalcula la línea base de TODAS las tablas protegidas (las "sella" como íntegras ahora).</summary>
        public void Recalcular()
        {
            foreach (var tabla in DALIntegridad_GO44.TABLAS_PROTEGIDAS)
                RecalcularTabla(tabla);
            IntegridadConocidamenteRota = false;
        }

        /// <summary>Recalcula y guarda los DVH de una tabla y, a partir de ellos, su DVV. Lo llaman las DAL tras cada cambio legítimo.</summary>
        public void RecalcularTabla(string nombreTabla)
        {
            Dictionary<string, string> dvhs = _dal.CalcularDVHsTabla(nombreTabla);
            _dal.GuardarDVHs(nombreTabla, dvhs);
            string dvv = CalculadorIntegridad_GO44.CalcularDVV(dvhs.Values);
            _dal.GuardarDVV(nombreTabla, dvv);
        }

        /// <summary>Genera un backup con nombre por fecha/hora en la carpeta de backups y purga los viejos.</summary>
        public string HacerBackupAutomatico()
        {
            if (!Directory.Exists(CARPETA_BACKUPS))
                Directory.CreateDirectory(CARPETA_BACKUPS);

            string nombreArchivo = $"GestionUsuario_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            string rutaCompleta = Path.Combine(CARPETA_BACKUPS, nombreArchivo);

            _dal.HacerBackupSQL(CONN_MASTER, NOMBRE_BD, rutaCompleta);

            LimpiarBackupsViejos();
            return rutaCompleta;
        }

        /// <summary>Devuelve la ruta del backup más reciente (null si no hay ninguno).</summary>
        public string ObtenerUltimoBackup()
        {
            if (!Directory.Exists(CARPETA_BACKUPS)) return null;

            var archivos = Directory.GetFiles(CARPETA_BACKUPS, "*.bak");
            if (archivos.Length == 0) return null;

            return archivos
                .Select(f => new FileInfo(f))
                .OrderByDescending(fi => fi.CreationTime)
                .First()
                .FullName;
        }

        /// <summary>Aplica la retención: ordena los .bak del más nuevo al más viejo y borra todo lo que sobra del tope a conservar.</summary>
        private void LimpiarBackupsViejos()
        {
            if (!Directory.Exists(CARPETA_BACKUPS)) return;

            var archivos = Directory.GetFiles(CARPETA_BACKUPS, "*.bak")
                .Select(f => new FileInfo(f))
                .OrderByDescending(fi => fi.CreationTime)
                .ToList();

            // Desde el índice "a conservar" en adelante ya son los sobrantes → borrar.
            for (int i = CANTIDAD_BACKUPS_A_CONSERVAR; i < archivos.Count; i++)
            {
                try { archivos[i].Delete(); } catch { }
            }
        }

        /// <summary>Restaura el backup más reciente (atajo usado cuando se detecta integridad rota). Falla si no hay backups.</summary>
        public void RestaurarUltimoBackup()
        {
            string ruta = ObtenerUltimoBackup();
            if (string.IsNullOrEmpty(ruta))
                throw new Exception(IdiomaManager_GO44.T("err.backupSinDisponibles"));
            RestaurarBackupDesdeRuta(ruta);
        }

        /// <summary>Hace un backup manual en la ruta elegida por el admin (crea la carpeta si falta) y lo audita.</summary>
        public void HacerBackupEnRuta(string rutaArchivoBak)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivoBak))
                throw new Exception(IdiomaManager_GO44.T("err.backupRutaObligatoria"));

            string carpeta = Path.GetDirectoryName(rutaArchivoBak);
            if (!string.IsNullOrEmpty(carpeta) && !Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            _dal.HacerBackupSQL(CONN_MASTER, NOMBRE_BD, rutaArchivoBak);

            BLLBitacora_GO44.Instancia.RegistrarEvento(
                SessionManager_GO44.Instancia.ObtenerUsuarioActual()?.Login ?? "SISTEMA",
                "Admin", "Backup manual generado",
                $"Ruta: {rutaArchivoBak}", "Media");
        }

        /// <summary>Restaura la base desde un .bak concreto (valida que la ruta exista antes de intentarlo).</summary>
        public void RestaurarBackupDesdeRuta(string rutaArchivoBak)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivoBak))
                throw new Exception(IdiomaManager_GO44.T("err.backupRutaObligatoria"));
            if (!File.Exists(rutaArchivoBak))
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.backupArchivoNoExiste"), rutaArchivoBak));

            _dal.RestaurarBackupSQL(CONN_MASTER, NOMBRE_BD, rutaArchivoBak);
        }

        /// <summary>
        /// Arranca los backups automáticos: si ya toca uno (pasó el intervalo desde el último) lo hace al
        /// instante, y programa un Timer que se repite cada INTERVALO_BACKUP_HORAS. El lock + chequeo de null
        /// evitan crear dos timers si se llama más de una vez.
        /// </summary>
        public void IniciarBackupsProgramados()
        {
            lock (_lockTimer)
            {
                if (DebeHacerBackupAhora())
                    EjecutarBackupProgramado(null);

                if (_timerBackup != null) return; // ya estaba programado

                TimeSpan intervalo = TimeSpan.FromHours(INTERVALO_BACKUP_HORAS);
                _timerBackup = new Timer(EjecutarBackupProgramado, null, intervalo, intervalo);
            }
        }

        /// <summary>Frena los backups automáticos (libera el Timer). Se llama al cerrar la app.</summary>
        public void DetenerBackupsProgramados()
        {
            lock (_lockTimer)
            {
                _timerBackup?.Dispose();
                _timerBackup = null;
            }
        }

        /// <summary>Decide si ya toca un backup: true si no hay ninguno o si el último es más viejo que el intervalo.</summary>
        private bool DebeHacerBackupAhora()
        {
            string ultimo = ObtenerUltimoBackup();
            if (string.IsNullOrEmpty(ultimo)) return true;

            try
            {
                FileInfo fi = new FileInfo(ultimo);
                TimeSpan transcurrido = DateTime.Now - fi.CreationTime;
                return transcurrido.TotalHours >= INTERVALO_BACKUP_HORAS;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Callback del Timer. Antes de hacer el backup, VERIFICA la integridad: si la base está
        /// comprometida no respalda (para no "congelar" datos ya adulterados). Solo backupea si está íntegra.
        /// </summary>
        private void EjecutarBackupProgramado(object state)
        {
            try
            {
                ResultadoIntegridad res = Verificar();
                if (!res.EsIntegra) return;

                string ruta = HacerBackupAutomatico();

                BLLBitacora_GO44.Instancia.RegistrarEvento(
                    "SISTEMA", "Admin", "Backup automatico generado",
                    $"Ruta: {ruta}", "Baja");
            }
            catch { }
        }
    }

    /// <summary>Resultado de una verificación: si está íntegra, si fue bootstrap, qué tablas se comprometieron y el detalle fila por fila.</summary>
    public class ResultadoIntegridad
    {
        public bool EsIntegra { get; set; }
        public bool EsBootstrap { get; set; }
        public List<string> TablasComprometidas { get; set; } = new List<string>();
        public List<DetalleTampering> Detalles { get; set; } = new List<DetalleTampering>();
    }

    /// <summary>Tipo de alteración detectada en una fila protegida.</summary>
    public enum TipoTampering
    {
        Insertado,
        Modificado,
        Eliminado
    }

    /// <summary>Detalle de una fila alterada: en qué tabla, qué registro y de qué tipo fue la alteración.</summary>
    public class DetalleTampering
    {
        public string Tabla { get; set; }
        public string IdRegistro { get; set; }
        public TipoTampering Tipo { get; set; }
    }
}
