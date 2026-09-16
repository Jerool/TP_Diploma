using System;

namespace Servicios
{

    public class TipoEvento_GO44
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public TipoEvento_GO44() { }
        public TipoEvento_GO44(int id, string nombre) { Id = id; Nombre = nombre; }

        public override string ToString() => Nombre ?? string.Empty;
    }
}
