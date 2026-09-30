using BE;
using Servicios;
using System;
using System.Text.RegularExpressions;

namespace BLL
{
    /// <summary>
    /// BLL de Cobro — CU05 Cobrar Venta.
    /// Se encarga de las validaciones de método de pago (efectivo o tarjeta),
    /// enmascaramiento seguro del número de tarjeta y simulación de comunicación bancaria.
    /// La persistencia definitiva del cobro la hace BLLFactura_GO44.GenerarFacturaYCobrar
    /// (todo dentro de la misma transacción atómica).
    /// </summary>
    public class BLLCobro_GO44
    {
        public enum ResultadoValidacion
        {
            Ok,
            MontoInvalido,
            NroTarjetaInvalido,
            VencimientoInvalido,
            CvvInvalido,
            TitularIncompleto,
            BancoRequerido,
            AutorizacionRechazada,
            Error
        }

        public class ValidacionCobroVO
        {
            public ResultadoValidacion Resultado { get; set; }
            public BE_Cobro_GO44 Cobro { get; set; }
            public string Mensaje { get; set; }
        }

        // ============ Cobro EFECTIVO ============

        public ValidacionCobroVO PrepararCobroEfectivo(decimal montoAPagar, decimal montoEntregado)
        {
            ValidacionCobroVO vo = new ValidacionCobroVO();
            if (montoEntregado < montoAPagar)
            {
                vo.Resultado = ResultadoValidacion.MontoInvalido;
                vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.cobro.montoMenor"),
                    montoEntregado.ToString("N2"), montoAPagar.ToString("N2"));
                return vo;
            }

            vo.Resultado = ResultadoValidacion.Ok;
            vo.Cobro = new BE_Cobro_GO44
            {
                Metodo = BE_Cobro_GO44.MetodoPago.Efectivo,
                Monto  = montoAPagar
            };
            vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.cobro.efectivoOk"), (montoEntregado - montoAPagar).ToString("N2"));
            return vo;
        }

        // ============ Cobro TARJETA ============

        public ValidacionCobroVO PrepararCobroTarjeta(decimal montoAPagar,
            string nroTarjeta, string mesVenc, string anioVenc, string cvv,
            string titularNombre, string titularApellido, string banco)
        {
            ValidacionCobroVO vo = new ValidacionCobroVO();

            // Sacar espacios y guiones
            string nro = (nroTarjeta ?? "").Replace(" ", "").Replace("-", "");

            if (!Regex.IsMatch(nro, @"^\d{13,19}$"))
            {
                vo.Resultado = ResultadoValidacion.NroTarjetaInvalido;
                vo.Mensaje = IdiomaManager_GO44.T("bll.cobro.nroTarjetaInvalido");
                return vo;
            }
            if (!PasaValidacionLuhn(nro))
            {
                vo.Resultado = ResultadoValidacion.NroTarjetaInvalido;
                vo.Mensaje = IdiomaManager_GO44.T("bll.cobro.nroTarjetaLuhn");
                return vo;
            }

            int mes, anio;
            if (!int.TryParse(mesVenc, out mes) || mes < 1 || mes > 12)
            {
                vo.Resultado = ResultadoValidacion.VencimientoInvalido;
                vo.Mensaje = IdiomaManager_GO44.T("bll.cobro.mesInvalido");
                return vo;
            }
            if (!int.TryParse(anioVenc, out anio) || anio < 2020 || anio > 2100)
            {
                vo.Resultado = ResultadoValidacion.VencimientoInvalido;
                vo.Mensaje = IdiomaManager_GO44.T("bll.cobro.anioInvalido");
                return vo;
            }
            DateTime venc = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));
            if (venc < DateTime.Today)
            {
                vo.Resultado = ResultadoValidacion.VencimientoInvalido;
                vo.Mensaje = IdiomaManager_GO44.T("bll.cobro.tarjetaVencida");
                return vo;
            }

            if (!Regex.IsMatch(cvv ?? "", @"^\d{3,4}$"))
            {
                vo.Resultado = ResultadoValidacion.CvvInvalido;
                vo.Mensaje = IdiomaManager_GO44.T("bll.cobro.cvvInvalido");
                return vo;
            }

            if (string.IsNullOrWhiteSpace(titularNombre) || string.IsNullOrWhiteSpace(titularApellido))
            {
                vo.Resultado = ResultadoValidacion.TitularIncompleto;
                vo.Mensaje = IdiomaManager_GO44.T("bll.cobro.titularIncompleto");
                return vo;
            }

            if (string.IsNullOrWhiteSpace(banco))
            {
                vo.Resultado = ResultadoValidacion.BancoRequerido;
                vo.Mensaje = IdiomaManager_GO44.T("bll.cobro.bancoRequerido");
                return vo;
            }

            // Simulación de comunicación con el banco (siempre autoriza; en producción va a la pasarela real)
            string codigoAutorizacion = SimularAutorizacionBancaria(nro, montoAPagar);
            if (string.IsNullOrEmpty(codigoAutorizacion))
            {
                vo.Resultado = ResultadoValidacion.AutorizacionRechazada;
                vo.Mensaje = IdiomaManager_GO44.T("bll.cobro.rechazado");
                return vo;
            }

            vo.Resultado = ResultadoValidacion.Ok;
            vo.Cobro = new BE_Cobro_GO44
            {
                Metodo           = BE_Cobro_GO44.MetodoPago.Tarjeta,
                Monto            = montoAPagar,
                NroTarjetaEnmasc = Enmascarar(nro),
                Banco            = banco,
                TitularNombre    = titularNombre,
                TitularApellido  = titularApellido,
                CodigoAutoriz    = codigoAutorizacion
            };
            vo.Mensaje = string.Format(IdiomaManager_GO44.T("bll.cobro.tarjetaOk"), codigoAutorizacion);
            return vo;
        }

        // ============ Utilidades privadas ============

        /// <summary>
        /// Algoritmo de Luhn para validar checksum de números de tarjeta.
        /// </summary>
        private static bool PasaValidacionLuhn(string nro)
        {
            int sum = 0;
            bool alt = false;
            for (int i = nro.Length - 1; i >= 0; i--)
            {
                int n = nro[i] - '0';
                if (alt)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alt = !alt;
            }
            return sum % 10 == 0;
        }

        private static string Enmascarar(string nro)
        {
            if (string.IsNullOrEmpty(nro) || nro.Length < 4) return "****";
            string ult4 = nro.Substring(nro.Length - 4);
            return "****-****-****-" + ult4;
        }

        private static string SimularAutorizacionBancaria(string nro, decimal monto)
        {
            // Placeholder — en producción se llama a pasarela real (Prisma, Mercado Pago, etc.)
            // Rechaza los que terminan en "0000" solo para probar el flujo alternativo.
            if (nro.EndsWith("0000")) return null;
            return "AUTH-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}
