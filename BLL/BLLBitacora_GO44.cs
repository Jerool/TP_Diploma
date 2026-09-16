using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{

    public class BLLBitacora_GO44
    {
        private static BLLBitacora_GO44 _Instancia;
        private readonly DALBitacora_GO44 _DALBitacora;

        private BLLBitacora_GO44()
        {
            _DALBitacora = new DALBitacora_GO44();
        }

        public static BLLBitacora_GO44 Instancia
        {
            get
            {
                if (_Instancia == null)
                    _Instancia = new BLLBitacora_GO44();
                return _Instancia;
            }
        }

        public void RegistrarEvento(string login, string modulo, string tipoEvento, string detalle, string criticidad)
        {
            var registro = new Bitacora_GO44(login, modulo, tipoEvento, detalle, criticidad, DateTime.Now);
            _DALBitacora.Guardar(registro);
        }

        public List<Bitacora_GO44> Listar()
        {
            return _DALBitacora.Listar();
        }
        public List<Bitacora_GO44> Filtrar(string login, string modulo, string tipoEvento, string criticidad, DateTime fechaInicio, DateTime fechaFin)
        {
            return _DALBitacora.Filtrar(login, modulo, tipoEvento, criticidad, fechaInicio, fechaFin);
        }

        public List<string> ListarModulos() => _DALBitacora.ListarModulos();
        public List<string> ListarTiposEvento() => _DALBitacora.ListarTiposEvento();
        public List<string> ListarCriticidades()
        {
            return new List<string> { "Alta", "Media", "Baja" };
        }
    }
}
