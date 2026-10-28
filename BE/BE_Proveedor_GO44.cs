using System;

namespace BE
{
    /// <summary>
    /// Entidad Proveedor (RFN2 — PN2 paso 2 y 3).
    /// Representa a un proveedor de componentes de hardware al que se le pueden
    /// generar solicitudes de cotización y órdenes de compra.
    /// </summary>
    public class BE_Proveedor_GO44
    {
        /// <summary>CUIT del proveedor (11 dígitos, PK).</summary>
        public string CUIT { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Empresa { get; set; }
        /// <summary>Número de teléfono (10 dígitos).</summary>
        public string Telefono { get; set; }
        /// <summary>CVU para transferencias (22 dígitos).</summary>
        public string CVU { get; set; }
        public string Banco { get; set; }
        public string Titular { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }

        /// <summary>Constructor: alta de hoy y proveedor activo por defecto (la baja es lógica, nunca física).</summary>
        public BE_Proveedor_GO44()
        {
            Activo = true;
            FechaAlta = DateTime.Now;
        }

        /// <summary>Nombre y apellido del contacto unidos; Trim() limpia el espacio sobrante si falta alguno.</summary>
        public string NombreCompleto
        {
            get { return (Nombre + " " + Apellido).Trim(); }
        }

        public override string ToString()
        {
            // Para que el ComboBox muestre "Empresa (CUIT)"
            return string.Format("{0} ({1})", Empresa, CUIT);
        }
    }
}
