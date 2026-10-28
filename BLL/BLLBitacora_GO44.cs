using DAL;
using Servicios;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// BLL de la BITÁCORA de auditoría. Es un Singleton (una sola instancia compartida por toda la app,
    /// expuesta en <see cref="Instancia"/>), para que cualquier BLL registre eventos con una sola línea.
    /// Delega el guardado y las consultas en la DAL; acá solo arma el registro y provee listados para filtros.
    /// </summary>
    public class BLLBitacora_GO44
    {
        private static BLLBitacora_GO44 _Instancia;
        private readonly DALBitacora_GO44 _DALBitacora;

        // Constructor privado: nadie instancia desde afuera (Singleton).
        private BLLBitacora_GO44()
        {
            _DALBitacora = new DALBitacora_GO44();
        }

        /// <summary>Punto de acceso al Singleton: crea la instancia la primera vez y la reutiliza.</summary>
        public static BLLBitacora_GO44 Instancia
        {
            get
            {
                if (_Instancia == null)
                    _Instancia = new BLLBitacora_GO44();
                return _Instancia;
            }
        }

        /// <summary>Arma un registro de evento con la fecha/hora actual y lo manda a guardar. Es el método que todas las BLL llaman desde su Auditar().</summary>
        public void RegistrarEvento(string login, string modulo, string tipoEvento, string detalle, string criticidad)
        {
            var registro = new Bitacora_GO44(login, modulo, tipoEvento, detalle, criticidad, DateTime.Now);
            _DALBitacora.Guardar(registro);
        }

        /// <summary>Lista todos los eventos (para la pantalla de bitácora).</summary>
        public List<Bitacora_GO44> Listar()
        {
            return _DALBitacora.Listar();
        }

        /// <summary>Lista eventos aplicando los filtros de la pantalla (login, módulo, tipo, criticidad y rango de fechas).</summary>
        public List<Bitacora_GO44> Filtrar(string login, string modulo, string tipoEvento, string criticidad, DateTime fechaInicio, DateTime fechaFin)
        {
            return _DALBitacora.Filtrar(login, modulo, tipoEvento, criticidad, fechaInicio, fechaFin);
        }

        // Listados para llenar los combos de filtro de la pantalla de bitácora.
        public List<string> ListarModulos() => _DALBitacora.ListarModulos();
        public List<string> ListarTiposEvento() => _DALBitacora.ListarTiposEvento();

        /// <summary>Las criticidades son un conjunto fijo del negocio (no se leen de la base).</summary>
        public List<string> ListarCriticidades()
        {
            return new List<string> { "Alta", "Media", "Baja" };
        }
    }
}
