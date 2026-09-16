using BE;
using DAL;
using Servicios;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// BLL de Componente — implementa el CU03 Seleccionar Componente y provee búsquedas
    /// que consume el CU01 Cargar Carrito para verificar stock antes de agregar líneas.
    /// </summary>
    public class BLLComponente_GO44
    {
        private readonly DALComponente_GO44 _dalComponente;

        public BLLComponente_GO44()
        {
            _dalComponente = new DALComponente_GO44();
        }

        public enum ResultadoSeleccionComponente
        {
            Exitoso,
            ComponenteInexistente,
            ComponenteInactivo,
            StockInsuficiente,
            CantidadInvalida,
            Error
        }

        /// <summary>
        /// Contenedor de resultado del CU03. Si Resultado==Exitoso, Componente trae la instancia
        /// con precio y stock actualizados listos para ser agregados al carrito.
        /// </summary>
        public class SeleccionComponenteVO
        {
            public ResultadoSeleccionComponente Resultado { get; set; }
            public BE_Componente_GO44 Componente { get; set; }
            public string Mensaje { get; set; }
        }

        private void Auditar(string modulo, string tipoEvento, string detalle, string criticidad)
        {
            Usuario_GO44 usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            string login = usuario != null ? usuario.Login : "SISTEMA";
            BLLBitacora_GO44.Instancia.RegistrarEvento(login, modulo, tipoEvento, detalle, criticidad);
        }

        // ============ CU03 Seleccionar Componente (por Id) ============

        public SeleccionComponenteVO SeleccionarPorId(int idComponente, int cantidad)
        {
            SeleccionComponenteVO vo = new SeleccionComponenteVO();
            try
            {
                if (cantidad <= 0)
                {
                    vo.Resultado = ResultadoSeleccionComponente.CantidadInvalida;
                    vo.Mensaje = "La cantidad debe ser mayor a 0";
                    return vo;
                }

                BE_Componente_GO44 comp = _dalComponente.BuscarPorId(idComponente);

                if (comp == null)
                {
                    vo.Resultado = ResultadoSeleccionComponente.ComponenteInexistente;
                    vo.Mensaje = "Componente ID " + idComponente + " no existe";
                    return vo;
                }

                if (!comp.Activo)
                {
                    vo.Resultado = ResultadoSeleccionComponente.ComponenteInactivo;
                    vo.Mensaje = "Componente " + comp.Codigo + " está dado de baja";
                    return vo;
                }

                if (!comp.StockDisponible(cantidad))
                {
                    Auditar("Componente", "Stock insuficiente",
                            "Se pidió " + cantidad + " de " + comp.Codigo + " y hay " + comp.StockActual, "Media");
                    vo.Resultado = ResultadoSeleccionComponente.StockInsuficiente;
                    vo.Componente = comp;
                    vo.Mensaje = "Stock insuficiente. Disponible: " + comp.StockActual;
                    return vo;
                }

                vo.Resultado = ResultadoSeleccionComponente.Exitoso;
                vo.Componente = comp;
                vo.Mensaje = "OK";
                return vo;
            }
            catch (Exception ex)
            {
                vo.Resultado = ResultadoSeleccionComponente.Error;
                vo.Mensaje = ex.Message;
                return vo;
            }
        }

        // ============ CU03 Seleccionar Componente (por Código / SKU) ============

        public SeleccionComponenteVO SeleccionarPorCodigo(string codigo, int cantidad)
        {
            SeleccionComponenteVO vo = new SeleccionComponenteVO();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                vo.Resultado = ResultadoSeleccionComponente.ComponenteInexistente;
                vo.Mensaje = "Debe ingresar un código";
                return vo;
            }

            BE_Componente_GO44 comp = _dalComponente.BuscarPorCodigo(codigo);
            if (comp == null)
            {
                vo.Resultado = ResultadoSeleccionComponente.ComponenteInexistente;
                vo.Mensaje = "Código " + codigo + " no encontrado";
                return vo;
            }

            return SeleccionarPorId(comp.Id, cantidad);
        }

        // ============ Consultas para grillas ============

        public List<BE_Componente_GO44> ListarActivos()
        {
            return _dalComponente.ListarActivos();
        }

        public BE_Componente_GO44 BuscarPorId(int id)
        {
            return _dalComponente.BuscarPorId(id);
        }
    }
}
