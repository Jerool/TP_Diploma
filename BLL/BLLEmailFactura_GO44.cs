using BE;
using Servicios;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace BLL
{
    /// <summary>
    /// Envío del comprobante de factura por email al cliente.
    /// Config SMTP: la lee de la sección system.net/mailSettings del App.config.
    /// El PDF adjunto se genera con GeneradorPdf_GO44 (pura C#, sin dependencias externas).
    /// Tolerante a fallos: nunca lanza excepciones, devuelve bool + mensaje descriptivo.
    /// </summary>
    public static class BLLEmailFactura_GO44
    {
        /// <summary>
        /// Indica si el envío está configurado (App.config tiene la sección mailSettings con host y from).
        /// </summary>
        public static bool EmailConfigurado()
        {
            try
            {
                SmtpSection seccion = LeerSeccionSmtp();
                return seccion != null
                    && !string.IsNullOrWhiteSpace(seccion.Network?.Host)
                    && !string.IsNullOrWhiteSpace(seccion.From);
            }
            catch { return false; }
        }

        private static SmtpSection LeerSeccionSmtp()
            => ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;

        /// <summary>
        /// Envía la factura por email al cliente con el PDF adjunto.
        /// </summary>
        /// <param name="factura">Factura ya generada</param>
        /// <param name="cliente">Cliente destinatario (usa cliente.Email)</param>
        /// <param name="alicuotaIVA">Alícuota de IVA para mostrar en el PDF (ej: 0.21)</param>
        /// <param name="mensaje">Mensaje descriptivo del resultado (éxito o error)</param>
        /// <returns>true si el email se entregó al servidor SMTP</returns>
        public static bool EnviarFacturaEmail(BE_Factura_GO44 factura, BE_Cliente_GO44 cliente,
                                              decimal alicuotaIVA, out string mensaje)
        {
            mensaje = null;

            if (factura == null || cliente == null)
            {
                mensaje = "Faltan datos para enviar el email (factura o cliente nulo)";
                return false;
            }

            if (string.IsNullOrWhiteSpace(cliente.Email))
            {
                mensaje = "El cliente no tiene email registrado";
                return false;
            }

            if (!EmailConfigurado())
            {
                mensaje = "El envío de emails no está configurado (revisá system.net/mailSettings en App.config)";
                return false;
            }

            try
            {
                // Generar PDF en memoria
                byte[] pdf = GenerarFacturaPdf(factura, alicuotaIVA);

                using (var smtp = new SmtpClient { Timeout = 30000 })
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(LeerSeccionSmtp().From);

                    string nombreCompleto = (cliente.Nombre + " " + cliente.Apellido).Trim();
                    mail.To.Add(new MailAddress(cliente.Email.Trim(), nombreCompleto));

                    mail.Subject         = $"Factura {factura.NumeroFactura} - TechFlow ElectroPoint";
                    mail.SubjectEncoding = Encoding.UTF8;
                    mail.Body            = ConstruirCuerpoHtml(factura, cliente);
                    mail.BodyEncoding    = Encoding.UTF8;
                    mail.IsBodyHtml      = true;

                    var attachment = new Attachment(new MemoryStream(pdf),
                                                    $"Factura_{factura.NumeroFactura}.pdf",
                                                    MediaTypeNames.Application.Pdf);
                    mail.Attachments.Add(attachment);

                    smtp.Send(mail);
                }

                mensaje = "Factura enviada por email a " + cliente.Email;
                return true;
            }
            catch (SmtpException ex)
            {
                mensaje = "Error SMTP: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                mensaje = "Error al enviar email: " + ex.Message;
                return false;
            }
        }

        /// <summary>Genera el PDF de la factura en memoria usando GeneradorPdf_GO44.</summary>
        private static byte[] GenerarFacturaPdf(BE_Factura_GO44 f, decimal alicuotaIVA)
        {
            string titulo = "TECHFLOW - ELECTROPOINT · FACTURA " + f.NumeroFactura;
            string subtitulo =
                "Fecha: " + f.FechaEmision.ToString("dd/MM/yyyy HH:mm") +
                "   |   Cajero: " + f.LoginCajero +
                "   |   Cliente: " + (f.Cliente != null ? (f.Cliente.NombreCompleto + " · DNI " + f.Cliente.DNI) : "") +
                "   |   Estado: " + f.Estado;

            string[] headers = { "Codigo", "Componente", "Cant", "Precio Unit.", "Subtotal" };
            float[] anchos = { 0.12f, 0.48f, 0.10f, 0.15f, 0.15f };

            var filas = new List<string[]>();
            if (f.Lineas != null)
            {
                foreach (var l in f.Lineas)
                {
                    filas.Add(new string[] {
                        l.ComponenteCodigo ?? "",
                        l.ComponenteNombre ?? "",
                        l.Cantidad.ToString(),
                        "$ " + l.PrecioUnitario.ToString("N2"),
                        "$ " + l.Subtotal.ToString("N2")
                    });
                }
            }

            // Filas de totales
            filas.Add(new string[] { "", "", "", "", "" });
            filas.Add(new string[] { "", "", "", "Subtotal:", "$ " + f.Subtotal.ToString("N2") });
            filas.Add(new string[] { "", "", "", "IVA (" + (alicuotaIVA * 100).ToString("0") + "%):", "$ " + f.IVA.ToString("N2") });
            filas.Add(new string[] { "", "", "", "TOTAL:", "$ " + f.Total.ToString("N2") });

            var gen = new GeneradorPdf_GO44();
            return gen.GenerarBytes(titulo, subtitulo, headers, anchos, filas);
        }

        /// <summary>Construye el cuerpo HTML del email con estilo básico.</summary>
        private static string ConstruirCuerpoHtml(BE_Factura_GO44 f, BE_Cliente_GO44 cliente)
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"></head>");
            sb.Append("<body style=\"margin:0;padding:0;background:#f0f4f8;font-family:'Segoe UI',Arial,sans-serif;\">");
            sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"padding:24px 0;\"><tr><td align=\"center\">");
            sb.Append("<table role=\"presentation\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:600px;width:100%;background:#ffffff;border:1px solid #d1d5db;color:#1f2937;\">");

            // Encabezado
            sb.Append("<tr><td style=\"background:#0d47a1;padding:24px 32px;\">");
            sb.Append("<div style=\"font-size:24px;font-weight:bold;color:#ffffff;\">TechFlow · ElectroPoint</div>");
            sb.Append("<div style=\"font-size:12px;letter-spacing:1.5px;color:#bbdefb;text-transform:uppercase;margin-top:6px;\">Factura Electrónica</div>");
            sb.Append("</td></tr>");

            // Saludo
            sb.Append("<tr><td style=\"padding:24px 32px 8px;\">");
            sb.Append("<p style=\"font-size:16px;margin:0 0 8px;\">Hola " + Html(cliente.Nombre) + ",</p>");
            sb.Append("<p style=\"font-size:14px;line-height:1.5;margin:0;color:#4b5563;\">");
            sb.Append("Gracias por tu compra. Adjuntamos el comprobante de la factura en formato PDF. ");
            sb.Append("También te dejamos abajo un resumen.");
            sb.Append("</p></td></tr>");

            // Card con datos
            sb.Append("<tr><td style=\"padding:16px 32px;\">");
            sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#f8fafc;border:1px solid #e5e7eb;\">");
            sb.Append("<tr><td colspan=\"2\" style=\"padding:14px 20px;border-bottom:1px dashed #d1d5db;\">");
            sb.Append("<div style=\"font-size:11px;letter-spacing:1px;color:#6b7280;\">FACTURA N°</div>");
            sb.Append("<div style=\"font-size:26px;font-weight:bold;color:#0d47a1;\">" + Html(f.NumeroFactura) + "</div>");
            sb.Append("</td></tr>");
            sb.Append(FilaHtml("Fecha", f.FechaEmision.ToString("dd/MM/yyyy HH:mm")));
            sb.Append(FilaHtml("Cliente", cliente.NombreCompleto));
            sb.Append(FilaHtml("DNI", cliente.DNI));
            sb.Append(FilaHtml("Cantidad de items", (f.Lineas != null ? f.Lineas.Count : 0).ToString()));
            sb.Append(FilaHtml("Subtotal", "$ " + f.Subtotal.ToString("N2")));
            sb.Append(FilaHtml("IVA", "$ " + f.IVA.ToString("N2")));
            sb.Append(FilaHtml("TOTAL", "$ " + f.Total.ToString("N2")));
            sb.Append("</table>");
            sb.Append("</td></tr>");

            // Firma
            sb.Append("<tr><td style=\"padding:8px 32px 24px;font-size:13px;color:#6b7280;line-height:1.5;\">");
            sb.Append("Si tenés alguna consulta sobre tu compra, respondé este email.<br><br>");
            sb.Append("<b style=\"color:#0d47a1;\">TechFlow · ElectroPoint</b>");
            sb.Append("</td></tr>");
            sb.Append("<tr><td style=\"background:#f9fafb;border-top:1px solid #e5e7eb;padding:14px 32px;font-size:11px;color:#9ca3af;text-align:center;\">");
            sb.Append("Este email fue enviado automáticamente. No responder si no es necesario.");
            sb.Append("</td></tr>");

            sb.Append("</table></td></tr></table></body></html>");
            return sb.ToString();
        }

        private static string Html(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");

        private static string FilaHtml(string clave, string valor)
        {
            return "<tr>" +
                "<td style=\"padding:9px 20px;font-size:11px;letter-spacing:1px;color:#6b7280;text-transform:uppercase;width:40%;\">" + Html(clave) + "</td>" +
                "<td style=\"padding:9px 20px;font-size:14px;font-weight:bold;color:#1f2937;\">" + Html(valor) + "</td>" +
                "</tr>";
        }
    }
}
