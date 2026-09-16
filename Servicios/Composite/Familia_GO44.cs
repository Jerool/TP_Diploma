using System.Collections.Generic;
using System.Linq;

namespace Servicios
{

    public class Familia_GO44 : IComponentePermiso_GO44
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public List<IComponentePermiso_GO44> Hijos { get; set; } = new List<IComponentePermiso_GO44>();

        public Familia_GO44() { }
        public Familia_GO44(int id, string nombre) { Id = id; Nombre = nombre; }

        public IEnumerable<Patente_GO44> ObtenerPatentes()
        {
            var vistas = new HashSet<int>();
            foreach (var hijo in Hijos)
            {
                foreach (var p in hijo.ObtenerPatentes())
                {
                    if (vistas.Add(p.Id))
                        yield return p;
                }
            }
        }

        public override string ToString() => Nombre ?? string.Empty;
    }
}
