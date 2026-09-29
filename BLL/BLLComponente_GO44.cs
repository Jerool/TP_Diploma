using BE;
using DAL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<BE_Componente_GO44> ListarTodos()
        {
            return _dalComponente.ListarTodos();
        }

        public BE_Componente_GO44 BuscarPorId(int id)
        {
            return _dalComponente.BuscarPorId(id);
        }

        // ============ ABM Productos ============

        public enum ResultadoAltaProducto
        {
            Exitoso,
            CodigoDuplicado,
            DatosIncompletos,
            PrecioInvalido,
            StockInvalido,
            Error
        }

        private void Auditar(string tipoEvento, string detalle, string criticidad)
        {
            Usuario_GO44 usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            string login = usuario != null ? usuario.Login : "SISTEMA";
            BLLBitacora_GO44.Instancia.RegistrarEvento(login, "Componente", tipoEvento, detalle, criticidad);
        }

        public ResultadoAltaProducto RegistrarProducto(string codigo, string nombre, string categoria,
                                                        string descripcion, decimal precio, int stockActual, int stockMinimo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
                    return ResultadoAltaProducto.DatosIncompletos;

                if (precio < 0) return ResultadoAltaProducto.PrecioInvalido;
                if (stockActual < 0 || stockMinimo < 0) return ResultadoAltaProducto.StockInvalido;

                // Chequeo de duplicado por Código
                if (_dalComponente.BuscarPorCodigo(codigo) != null)
                {
                    Auditar("Componente rechazado", "Código duplicado: " + codigo, "Media");
                    return ResultadoAltaProducto.CodigoDuplicado;
                }

                BE_Componente_GO44 nuevo = new BE_Componente_GO44
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    Categoria = categoria,
                    Descripcion = descripcion,
                    Precio = precio,
                    StockActual = stockActual,
                    StockMinimo = stockMinimo,
                    Activo = true
                };

                int id = _dalComponente.Insertar(nuevo);
                if (id <= 0)
                    return ResultadoAltaProducto.Error;

                Auditar("Componente registrado", "Código " + codigo + " · " + nombre + " · Stock " + stockActual, "Baja");
                return ResultadoAltaProducto.Exitoso;
            }
            catch (Exception ex)
            {
                Auditar("Componente registrado", "Error: " + ex.Message, "Alta");
                return ResultadoAltaProducto.Error;
            }
        }

        public bool ActualizarPrecio(int id, decimal nuevoPrecio)
        {
            if (nuevoPrecio < 0) return false;
            BE_Componente_GO44 actual = _dalComponente.BuscarPorId(id);
            if (actual == null) return false;

            int filas = _dalComponente.ActualizarPrecio(id, nuevoPrecio);
            if (filas > 0)
            {
                Auditar("Componente modificado",
                    "Precio " + actual.Codigo + ": $" + actual.Precio.ToString("N2") + " → $" + nuevoPrecio.ToString("N2"),
                    "Media");
                return true;
            }
            return false;
        }

        public bool DarDeBaja(int id)
        {
            BE_Componente_GO44 c = _dalComponente.BuscarPorId(id);
            if (c == null) return false;

            int filas = _dalComponente.DarDeBaja(id);
            if (filas > 0)
            {
                Auditar("Componente dado de baja", "Código " + c.Codigo + " · " + c.Nombre, "Media");
                return true;
            }
            return false;
        }

        public bool Reactivar(int id)
        {
            BE_Componente_GO44 c = _dalComponente.BuscarPorId(id);
            if (c == null) return false;

            int filas = _dalComponente.Reactivar(id);
            if (filas > 0)
            {
                Auditar("Componente reactivado", "Código " + c.Codigo + " · " + c.Nombre, "Media");
                return true;
            }
            return false;
        }
    }
}
