using BE;
using DAL;
using Servicios;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// BLL de la bitácora de cambios de productos (Componentes_C).
    /// Consulta con filtros y permite marcar manualmente la versión vigente de un producto.
    /// </summary>
    public class BLLComponenteCambio_GO44
    {
        private readonly DALComponenteCambio_GO44 _dal;

        public BLLComponenteCambio_GO44()
        {
            _dal = new DALComponenteCambio_GO44();
        }

        public List<BE_ComponenteCambio_GO44> ListarConFiltros(string codigo, string nombre, DateTime? fechaIni, DateTime? fechaFin)
        {
            return _dal.ListarConFiltros(codigo, nombre, fechaIni, fechaFin);
        }

        public bool MarcarActivo(int idCambio)
        {
            int filas = _dal.MarcarActivo(idCambio);
            if (filas > 0)
            {
                try
                {
                    var u = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
                    BLLBitacora_GO44.Instancia.RegistrarEvento(
                        u != null ? u.Login : "SISTEMA", "Maestros",
                        "Versión de producto marcada vigente", "IdCambio=" + idCambio, "Media");
                }
                catch { }
            }
            return filas > 0;
        }
    }
}
