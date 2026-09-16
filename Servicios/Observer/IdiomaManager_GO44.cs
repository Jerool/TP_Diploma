using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Servicios
{

    public class IdiomaManager_GO44
    {
        private static IdiomaManager_GO44 _instancia;

        private readonly List<IObservadorIdioma_GO44> _observadores;

        private Dictionary<string, string> _traducciones;
        private string _idiomaActual;

        public const string ES = "es";
        public const string EN = "en";

        public const string IDIOMA_POR_DEFECTO = ES;

        private IdiomaManager_GO44()
        {
            _observadores = new List<IObservadorIdioma_GO44>();
            _traducciones = new Dictionary<string, string>();
            _idiomaActual = IDIOMA_POR_DEFECTO;
        }

        public static IdiomaManager_GO44 Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new IdiomaManager_GO44();
                return _instancia;
            }
        }

        public string IdiomaActual => _idiomaActual;

        public void Suscribir(IObservadorIdioma_GO44 observador)
        {
            if (observador == null) return;
            if (!_observadores.Contains(observador))
                _observadores.Add(observador);
        }

        public void Desuscribir(IObservadorIdioma_GO44 observador)
        {
            if (observador == null) return;
            _observadores.Remove(observador);
        }

        private void Notificar()
        {
            foreach (IObservadorIdioma_GO44 obs in _observadores.ToArray())
            {
                try { obs.ActualizarIdioma(); }
                catch {  }
            }
        }

        public void CambiarIdioma(string codigoIdioma)
        {
            if (string.IsNullOrWhiteSpace(codigoIdioma)) return;
            codigoIdioma = codigoIdioma.ToLower();
            if (codigoIdioma != ES && codigoIdioma != EN) return;

            Dictionary<string, string> nuevas = CargarDesdeArchivo(codigoIdioma);
            if (nuevas == null) return;

            _traducciones = nuevas;
            _idiomaActual = codigoIdioma;
            Notificar();
        }

        public string Traducir(string clave)
        {
            if (string.IsNullOrEmpty(clave)) return clave;
            return _traducciones.TryGetValue(clave, out string valor) ? valor : clave;
        }

        public static string T(string clave) => Instancia.Traducir(clave);

        private Dictionary<string, string> CargarDesdeArchivo(string codigoIdioma)
        {
            try
            {
                string ruta = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Idiomas",
                    codigoIdioma + ".json");

                if (!File.Exists(ruta)) return null;

                string contenido = File.ReadAllText(ruta);
                return ParsearJsonPlano(contenido);
            }
            catch
            {
                return null;
            }
        }

        private Dictionary<string, string> ParsearJsonPlano(string json)
        {
            var dic = new Dictionary<string, string>();
            var rx = new Regex(@"""((?:[^""\\]|\\.)*)""\s*:\s*""((?:[^""\\]|\\.)*)""");
            foreach (Match m in rx.Matches(json))
            {
                string clave = Desescapar(m.Groups[1].Value);
                string valor = Desescapar(m.Groups[2].Value);
                dic[clave] = valor;
            }
            return dic;
        }

        private string Desescapar(string s)
        {
            return s
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t");
        }
    }
}
