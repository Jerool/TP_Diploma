using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BLL
{
    public class BLLIntegridad_GO44
    {
        private readonly DALIntegridad_GO44 _dal;

        private const string NOMBRE_BD = "Gestion Usuario";

        private static string CONN_MASTER
        {
            get
            {
                string instancia = DAL.Acceso.InstanciaActual;
                if (string.IsNullOrEmpty(instancia))
                    instancia = Servicios.Instalacion.ConfiguracionBD_GO44.LeerInstanciaGuardada();
                if (string.IsNullOrEmpty(instancia))
                    instancia = @"(localdb)\MSSQLLocalDB";
                return $"Data Source={instancia};Initial Catalog=master;Integrated Security=True";
            }
        }

        private const string CARPETA_BACKUPS = @"C:\Backups\GestionUsuario";
        private const int CANTIDAD_BACKUPS_A_CONSERVAR = 5;
        public const int INTERVALO_BACKUP_HORAS = 3;

        private static Timer _timerBackup;
        private static readonly object _lockTimer = new object();

        public BLLIntegridad_GO44()
        {
            _dal = new DALIntegridad_GO44();
        }

        public static bool IntegridadConocidamenteRota
        {
            get { return DALIntegridad_GO44.IntegridadConocidamenteRota; }
            set { DALIntegridad_GO44.IntegridadConocidamenteRota = value; }
        }

        public ResultadoIntegridad Verificar()
        {
            var resultado = new ResultadoIntegridad();

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

        public void Recalcular()
        {
            foreach (var tabla in DALIntegridad_GO44.TABLAS_PROTEGIDAS)
                RecalcularTabla(tabla);
            IntegridadConocidamenteRota = false;
        }

        public void RecalcularTabla(string nombreTabla)
        {
            Dictionary<string, string> dvhs = _dal.CalcularDVHsTabla(nombreTabla);
            _dal.GuardarDVHs(nombreTabla, dvhs);
            string dvv = CalculadorIntegridad_GO44.CalcularDVV(dvhs.Values);
            _dal.GuardarDVV(nombreTabla, dvv);
        }

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

        private void LimpiarBackupsViejos()
        {
            if (!Directory.Exists(CARPETA_BACKUPS)) return;

            var archivos = Directory.GetFiles(CARPETA_BACKUPS, "*.bak")
                .Select(f => new FileInfo(f))
                .OrderByDescending(fi => fi.CreationTime)
                .ToList();

            for (int i = CANTIDAD_BACKUPS_A_CONSERVAR; i < archivos.Count; i++)
            {
                try { archivos[i].Delete(); } catch { }
            }
        }

        public void RestaurarUltimoBackup()
        {
            string ruta = ObtenerUltimoBackup();
            if (string.IsNullOrEmpty(ruta))
                throw new Exception(IdiomaManager_GO44.T("err.backupSinDisponibles"));
            RestaurarBackupDesdeRuta(ruta);
        }

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

        public void RestaurarBackupDesdeRuta(string rutaArchivoBak)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivoBak))
                throw new Exception(IdiomaManager_GO44.T("err.backupRutaObligatoria"));
            if (!File.Exists(rutaArchivoBak))
                throw new Exception(string.Format(IdiomaManager_GO44.T("err.backupArchivoNoExiste"), rutaArchivoBak));

            _dal.RestaurarBackupSQL(CONN_MASTER, NOMBRE_BD, rutaArchivoBak);
        }

        public void IniciarBackupsProgramados()
        {
            lock (_lockTimer)
            {
                if (DebeHacerBackupAhora())
                    EjecutarBackupProgramado(null);

                if (_timerBackup != null) return;

                TimeSpan intervalo = TimeSpan.FromHours(INTERVALO_BACKUP_HORAS);
                _timerBackup = new Timer(EjecutarBackupProgramado, null, intervalo, intervalo);
            }
        }

        public void DetenerBackupsProgramados()
        {
            lock (_lockTimer)
            {
                _timerBackup?.Dispose();
                _timerBackup = null;
            }
        }

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

    public class ResultadoIntegridad
    {
        public bool EsIntegra { get; set; }
        public bool EsBootstrap { get; set; }
        public List<string> TablasComprometidas { get; set; } = new List<string>();
        public List<DetalleTampering> Detalles { get; set; } = new List<DetalleTampering>();
    }

    public enum TipoTampering
    {
        Insertado,
        Modificado,
        Eliminado
    }

    public class DetalleTampering
    {
        public string Tabla { get; set; }
        public string IdRegistro { get; set; }
        public TipoTampering Tipo { get; set; }
    }
}
