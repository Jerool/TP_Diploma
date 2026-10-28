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
    /// Además maneja el ABM de productos. Nunca toca la base directo: todo pasa por su DAL.
    /// </summary>
    public class BLLComponente_GO44
    {
        private readonly DALComponente_GO44 _dalComponente;

        public BLLComponente_GO44()
        {
            _dalComponente = new DALComponente_GO44();
        }

        /// <summary>Posibles resultados de seleccionar un componente para el carrito (CU03).</summary>
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

        /// <summary>
        /// Registra un evento en la bitácora tomando el usuario de la sesión (o "SISTEMA" si no hay).
        /// Va en try/catch porque la auditoría es best-effort: si falla, no debe tumbar la operación real.
        /// </summary>
        private void Auditar(string modulo, string tipoEvento, string detalle, string criticidad)
        {
            try
            {
                Usuario_GO44 usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
                string login = usuario != null ? usuario.Login : "SISTEMA";
                BLLBitacora_GO44.Instancia.RegistrarEvento(login, modulo, tipoEvento, detalle, criticidad);
            }
            catch { /* la bitácora es best-effort; no debe romper la operación principal */ }
        }

        // ============ CU03 Seleccionar Componente (por Id) ============

        /// <summary>
        /// Valida y selecciona un componente para agregar al carrito. Aplica, en orden, las reglas de
        /// negocio del CU03: cantidad &gt; 0, que el componente exista, que esté activo y que haya stock.
        /// Devuelve un VO con el resultado y, si fue exitoso, el componente listo para usar.
        /// </summary>
        public SeleccionComponenteVO SeleccionarPorId(int idComponente, int cantidad)
        {
            SeleccionComponenteVO vo = new SeleccionComponenteVO();
            try
            {
                if (cantidad <= 0)
                {
                    vo.Resultado = ResultadoSeleccionComponente.CantidadInvalida;
                    vo.Mensaje = IdiomaManager_GO44.T("bll.componente.cantidadInvalida");
                    return vo;
                }

                BE_Componente_GO44 comp = _dalComponente.BuscarPorId(idComponente);

                if (comp == null)
                {
                    vo.Resultado = ResultadoSeleccionComponente.ComponenteInexistente;
                    vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.componente.noExiste"), idComponente);
                    return vo;
                }

                if (!comp.Activo)
                {
                    vo.Resultado = ResultadoSeleccionComponente.ComponenteInactivo;
                    vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.componente.dadoBaja"), comp.Codigo);
                    return vo;
                }

                // Última validación: ¿alcanza el stock? Si no, se audita (criticidad media) y se corta.
                if (!comp.StockDisponible(cantidad))
                {
                    Auditar("Componente", "Stock insuficiente",
                            "Se pidió " + cantidad + " de " + comp.Codigo + " y hay " + comp.StockActual, "Media");
                    vo.Resultado = ResultadoSeleccionComponente.StockInsuficiente;
                    vo.Componente = comp;
                    vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.componente.stockInsuficiente"), comp.StockActual);
                    return vo;
                }

                vo.Resultado = ResultadoSeleccionComponente.Exitoso;
                vo.Componente = comp;
                vo.Mensaje = IdiomaManager_GO44.T("bll.componente.ok");
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

        /// <summary>
        /// Igual que SeleccionarPorId pero buscando por código. Si lo encuentra, delega en SeleccionarPorId
        /// para no repetir las validaciones de stock/estado (reutiliza la misma lógica).
        /// </summary>
        public SeleccionComponenteVO SeleccionarPorCodigo(string codigo, int cantidad)
        {
            SeleccionComponenteVO vo = new SeleccionComponenteVO();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                vo.Resultado = ResultadoSeleccionComponente.ComponenteInexistente;
                vo.Mensaje = IdiomaManager_GO44.T("bll.componente.debeIngresarCodigo");
                return vo;
            }

            BE_Componente_GO44 comp = _dalComponente.BuscarPorCodigo(codigo);
            if (comp == null)
            {
                vo.Resultado = ResultadoSeleccionComponente.ComponenteInexistente;
                vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.componente.codigoNoEncontrado"), codigo);
                return vo;
            }

            return SeleccionarPorId(comp.Id, cantidad);
        }

        // ============ Consultas para grillas (delegan directo en la DAL) ============

        /// <summary>Lista los componentes activos (para venta).</summary>
        public List<BE_Componente_GO44> ListarActivos()
        {
            return _dalComponente.ListarActivos();
        }

        /// <summary>Lista todos los componentes, incluidos los dados de baja (para el ABM).</summary>
        public List<BE_Componente_GO44> ListarTodos()
        {
            return _dalComponente.ListarTodos();
        }

        /// <summary>Busca un componente por Id.</summary>
        public BE_Componente_GO44 BuscarPorId(int id)
        {
            return _dalComponente.BuscarPorId(id);
        }

        // ============ ABM Productos ============

        /// <summary>Posibles resultados del alta de un producto.</summary>
        public enum ResultadoAltaProducto
        {
            Exitoso,
            CodigoDuplicado,
            DatosIncompletos,
            PrecioInvalido,
            StockInvalido,
            Error
        }

        /// <summary>Sobrecarga de Auditar fija al módulo "Componente" (azúcar para el ABM, que siempre audita ahí).</summary>
        private void Auditar(string tipoEvento, string detalle, string criticidad)
        {
            try
            {
                Usuario_GO44 usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
                string login = usuario != null ? usuario.Login : "SISTEMA";
                BLLBitacora_GO44.Instancia.RegistrarEvento(login, "Componente", tipoEvento, detalle, criticidad);
            }
            catch { /* la bitácora es best-effort; no debe romper la operación principal */ }
        }

        /// <summary>
        /// Da de alta un producto validando los datos (código y nombre obligatorios, precio y stock no
        /// negativos, código no duplicado). Devuelve un enum que indica qué pasó. Audita el alta o el rechazo.
        /// </summary>
        public ResultadoAltaProducto RegistrarProducto(string codigo, string nombre, string categoria,
                                                        string descripcion, decimal precio, int stockActual, int stockMinimo)
        {
            try
            {
                // Validaciones de negocio antes de tocar la base.
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

        /// <summary>Cambia el precio de un producto (rechaza negativos e inexistentes). Audita el cambio con precio viejo → nuevo.</summary>
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

        /// <summary>Da de baja (lógica) un producto si existe. Audita la baja.</summary>
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

        /// <summary>Reactiva un producto dado de baja si existe. Audita la reactivación.</summary>
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
