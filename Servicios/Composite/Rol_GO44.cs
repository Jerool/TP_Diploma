using System.Collections.Generic;
using System.Linq;

namespace Servicios
{

    public class Rol_GO44 : IComponentePermiso_GO44
    {
        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        private string _Nombre;
        public string Nombre
        {
            get { return _Nombre; }
            set { _Nombre = value; }
        }

        public List<IComponentePermiso_GO44> Hijos { get; set; } = new List<IComponentePermiso_GO44>();

        public Rol_GO44() { }

        public Rol_GO44(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

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

        public bool TienePermiso(string dataKey)
        {
            if (string.IsNullOrEmpty(dataKey)) return false;
            return ObtenerPatentes().Any(p => p.DataKey == dataKey);
        }
        public override string ToString()
        {
            return Nombre ?? string.Empty;
        }
    }
}
