using System.Collections.Generic;

namespace Servicios
{

    public class Patente_GO44 : IComponentePermiso_GO44
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public string DataKey { get; set; }

        public Patente_GO44() { }

        public Patente_GO44(int id, string nombre, string dataKey)
        {
            Id = id;
            Nombre = nombre;
            DataKey = dataKey;
        }

        public IEnumerable<Patente_GO44> ObtenerPatentes()
        {
            yield return this;
        }

        public override string ToString() => Nombre ?? string.Empty;
    }
}
