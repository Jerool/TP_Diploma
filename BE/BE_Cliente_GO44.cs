using System;
using System.Xml.Serialization;

namespace BE
{
    /// <summary>
    /// Entidad de negocio (BE) del CLIENTE. POCO con los datos de la persona que compra.
    /// Está marcada [Serializable]/[XmlRoot] porque en algún punto puede exportarse/serializarse a XML;
    /// por eso el campo calculado NombreCompleto se excluye con [XmlIgnore] (no es un dato propio, se deriva).
    /// </summary>
    [Serializable]
    [XmlRoot("Cliente")]
    public class BE_Cliente_GO44
    {
        private string _DNI;
        public string DNI
        {
            get { return _DNI; }
            set { _DNI = value; }
        }

        private string _Apellido;
        public string Apellido
        {
            get { return _Apellido; }
            set { _Apellido = value; }
        }

        private string _Nombre;
        public string Nombre
        {
            get { return _Nombre; }
            set { _Nombre = value; }
        }

        private string _Email;
        public string Email
        {
            get { return _Email; }
            set { _Email = value; }
        }

        private string _Telefono;
        public string Telefono
        {
            get { return _Telefono; }
            set { _Telefono = value; }
        }

        private DateTime _FechaAlta;
        public DateTime FechaAlta
        {
            get { return _FechaAlta; }
            set { _FechaAlta = value; }
        }

        private bool _Activo;
        public bool Activo
        {
            get { return _Activo; }
            set { _Activo = value; }
        }

        /// <summary>Campo calculado "Apellido, Nombre". [XmlIgnore] porque se deriva y no debe serializarse.</summary>
        [XmlIgnore]
        public string NombreCompleto
        {
            get { return (_Apellido ?? string.Empty) + ", " + (_Nombre ?? string.Empty); }
        }

        /// <summary>Constructor vacío requerido por la serialización XML y por las DAL.</summary>
        public BE_Cliente_GO44() { }

        /// <summary>Crea un cliente nuevo: marca la fecha de alta de hoy y lo deja activo.</summary>
        public BE_Cliente_GO44(string dni, string apellido, string nombre, string email, string telefono)
        {
            DNI = dni;
            Apellido = apellido;
            Nombre = nombre;
            Email = email;
            Telefono = telefono;
            FechaAlta = DateTime.Now;
            Activo = true;
        }

        public override string ToString()
        {
            return NombreCompleto;
        }
    }
}
