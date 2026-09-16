using System;

namespace Servicios
{

    public class Modulo_GO44
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public Modulo_GO44() { }
        public Modulo_GO44(int id, string nombre) { Id = id; Nombre = nombre; }

        public override string ToString() => Nombre ?? string.Empty;
    }
}
