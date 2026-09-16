using System.Collections.Generic;

namespace Servicios
{

    public interface IComponentePermiso_GO44
    {
        int Id { get; }
        string Nombre { get; }

        IEnumerable<Patente_GO44> ObtenerPatentes();
    }
}
