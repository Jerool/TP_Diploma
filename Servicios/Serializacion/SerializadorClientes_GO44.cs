using BE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Servicios.Serializacion
{
    /// <summary>
    /// Serialización XML de clientes. Permite exportar/importar la lista
    /// completa a un archivo .xml para backup o migración entre sucursales.
    /// </summary>
    public static class SerializadorClientes_GO44
    {
        /// <summary>
        /// Wrapper para poder serializar una List&lt;BE_Cliente_GO44&gt; como un
        /// documento XML con raíz &lt;ClientesExport&gt;.
        /// </summary>
        [Serializable]
        [XmlRoot("ClientesExport")]
        public class ClientesExport_GO44
        {
            [XmlAttribute("fechaExportacion")]
            public DateTime FechaExportacion { get; set; }

            [XmlAttribute("cantidad")]
            public int Cantidad { get; set; }

            [XmlAttribute("origen")]
            public string Origen { get; set; }

            [XmlElement("Cliente")]
            public List<BE_Cliente_GO44> Clientes { get; set; }

            public ClientesExport_GO44()
            {
                Clientes = new List<BE_Cliente_GO44>();
            }
        }

        /// <summary>
        /// Serializa la lista de clientes a un archivo XML en la ruta indicada.
        /// </summary>
        public static void ExportarAXml(List<BE_Cliente_GO44> clientes, string rutaArchivo, string origen)
        {
            if (clientes == null) clientes = new List<BE_Cliente_GO44>();

            ClientesExport_GO44 export = new ClientesExport_GO44
            {
                FechaExportacion = DateTime.Now,
                Cantidad = clientes.Count,
                Origen = origen ?? "TechFlow GO44",
                Clientes = clientes
            };

            XmlSerializer ser = new XmlSerializer(typeof(ClientesExport_GO44));
            using (StreamWriter sw = new StreamWriter(rutaArchivo, false, System.Text.Encoding.UTF8))
            {
                ser.Serialize(sw, export);
            }
        }

        /// <summary>
        /// Deserializa un archivo XML y retorna la lista de clientes contenida.
        /// Lanza excepción si el archivo está mal formado o no coincide el esquema.
        /// </summary>
        public static ClientesExport_GO44 ImportarDeXml(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("El archivo no existe: " + rutaArchivo);

            XmlSerializer ser = new XmlSerializer(typeof(ClientesExport_GO44));
            using (StreamReader sr = new StreamReader(rutaArchivo))
            {
                ClientesExport_GO44 export = ser.Deserialize(sr) as ClientesExport_GO44;
                if (export == null)
                    throw new Exception("El archivo XML no tiene el formato esperado (ClientesExport)");
                if (export.Clientes == null) export.Clientes = new List<BE_Cliente_GO44>();
                return export;
            }
        }
    }
}
