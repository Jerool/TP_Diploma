using BE;
using DAL;
using Servicios;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// BLL de Proveedor — CU07 "Registrar proveedor".
    /// Reutiliza Validaciones_GO44 y BLLBitacora_GO44.
    /// </summary>
    public class BLLProveedor_GO44
    {
        private readonly DALProveedor_GO44 _dal;

        public BLLProveedor_GO44()
        {
            _dal = new DALProveedor_GO44();
        }

        /// <summary>Resultados posibles del alta de proveedor: uno por cada campo que puede fallar, más éxito y error genérico.</summary>
        public enum ResultadoRegistro
        {
            Exitoso,
            CuitInvalido,
            CuitDuplicado,
            NombreInvalido,
            ApellidoInvalido,
            EmpresaVacia,
            TelefonoInvalido,
            CvuInvalido,
            BancoVacio,
            TitularInvalido,
            Error
        }

        /// <summary>Resultado de Registrar: el estado, el proveedor creado y un mensaje para la UI.</summary>
        public class RegistroVO
        {
            public ResultadoRegistro Resultado { get; set; }
            public BE_Proveedor_GO44 Proveedor { get; set; }
            public string Mensaje { get; set; }
        }

        /// <summary>Registra un evento de auditoría del módulo Proveedor (best-effort).</summary>
        private void Auditar(string tipoEvento, string detalle, string criticidad)
        {
            try
            {
                var usuario = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
                string login = usuario != null ? usuario.Login : "SISTEMA";
                BLLBitacora_GO44.Instancia.RegistrarEvento(login, "Proveedor", tipoEvento, detalle, criticidad);
            }
            catch { /* best-effort */ }
        }

        /// <summary>
        /// Da de alta un proveedor (CU07). Primero valida campo por campo (reutilizando Validaciones_GO44)
        /// devolviendo el enum correspondiente al primero que falle; luego controla CUIT duplicado e inserta.
        /// </summary>
        public RegistroVO Registrar(BE_Proveedor_GO44 prov)
        {
            var vo = new RegistroVO();

            // Validaciones delegadas a Servicios.Validaciones_GO44 (reutilización).
            // Cada línea corta y devuelve apenas un campo no cumple (patrón "guard clauses").
            if (!Validaciones_GO44.EsCuitValido(prov.CUIT))      { vo.Resultado = ResultadoRegistro.CuitInvalido;     return vo; }
            if (!Validaciones_GO44.EsNombreValido(prov.Nombre))  { vo.Resultado = ResultadoRegistro.NombreInvalido;   return vo; }
            if (!Validaciones_GO44.EsApellidoValido(prov.Apellido)) { vo.Resultado = ResultadoRegistro.ApellidoInvalido; return vo; }
            if (string.IsNullOrWhiteSpace(prov.Empresa))         { vo.Resultado = ResultadoRegistro.EmpresaVacia;     return vo; }
            if (!Validaciones_GO44.EsTelefonoValido(prov.Telefono)) { vo.Resultado = ResultadoRegistro.TelefonoInvalido; return vo; }
            if (!Validaciones_GO44.EsCvuValido(prov.CVU))        { vo.Resultado = ResultadoRegistro.CvuInvalido;      return vo; }
            if (string.IsNullOrWhiteSpace(prov.Banco))           { vo.Resultado = ResultadoRegistro.BancoVacio;       return vo; }
            if (!Validaciones_GO44.EsNombreValido(prov.Titular)) { vo.Resultado = ResultadoRegistro.TitularInvalido;  return vo; }

            try
            {
                if (_dal.ExisteCUIT(prov.CUIT))
                {
                    Auditar("CUIT duplicado", "Intento de alta con CUIT existente: " + prov.CUIT, "Media");
                    vo.Resultado = ResultadoRegistro.CuitDuplicado;
                    return vo;
                }

                int filas = _dal.Insertar(prov);
                if (filas == 0)
                {
                    vo.Resultado = ResultadoRegistro.Error;
                    vo.Mensaje = "No se pudo registrar el proveedor";
                    return vo;
                }

                Auditar("Proveedor registrado",
                        $"CUIT {prov.CUIT} · {prov.Empresa} ({prov.NombreCompleto})",
                        "Baja");

                vo.Resultado = ResultadoRegistro.Exitoso;
                vo.Proveedor = prov;
                vo.Mensaje = "Proveedor registrado correctamente";
                return vo;
            }
            catch (Exception ex)
            {
                vo.Resultado = ResultadoRegistro.Error;
                vo.Mensaje = ex.Message;
                return vo;
            }
        }

        /// <summary>Lista los proveedores activos (delegación directa a la DAL).</summary>
        public List<BE_Proveedor_GO44> ListarActivos() => _dal.ListarActivos();

        /// <summary>Busca un proveedor por CUIT (lo usa Pagar OC para traer sus datos bancarios).</summary>
        public BE_Proveedor_GO44 BuscarPorCUIT(string cuit) => _dal.BuscarPorCUIT(cuit);
    }
}
