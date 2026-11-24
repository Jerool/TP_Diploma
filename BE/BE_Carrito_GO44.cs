using System;
using System.Collections.Generic;
using System.Linq;

namespace BE
{
    /// <summary>
    /// Entidad de negocio (BE) que representa el CARRITO de venta: la compra en curso de un cliente
    /// antes de facturar. Es un agregado: contiene sus propias líneas (una por componente) y expone
    /// cálculos derivados (total, cantidad de ítems) y operaciones sobre esas líneas. No toca la base.
    /// </summary>
    public class BE_Carrito_GO44
    {
        /// <summary>Ciclo de vida del carrito: nace Abierto, y termina Confirmado (facturado) o Cancelado.</summary>
        public enum EstadoCarrito
        {
            Abierto,
            Confirmado,
            Cancelado
        }

        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        private BE_Cliente_GO44 _Cliente;
        public BE_Cliente_GO44 Cliente
        {
            get { return _Cliente; }
            set { _Cliente = value; }
        }

        private string _LoginVendedor;
        public string LoginVendedor
        {
            get { return _LoginVendedor; }
            set { _LoginVendedor = value; }
        }

        private DateTime _FechaCreacion;
        public DateTime FechaCreacion
        {
            get { return _FechaCreacion; }
            set { _FechaCreacion = value; }
        }

        private EstadoCarrito _Estado;
        public EstadoCarrito Estado
        {
            get { return _Estado; }
            set { _Estado = value; }
        }

        // Líneas del carrito: una por cada componente distinto agregado. El carrito es dueño de ellas.
        private List<BE_LineaCarrito_GO44> _Lineas;
        public List<BE_LineaCarrito_GO44> Lineas
        {
            get { return _Lineas; }
            set { _Lineas = value; }
        }

        /// <summary>Total a pagar: suma de los subtotales de cada línea. Se recalcula al leerse (no se almacena).</summary>
        public decimal Total
        {
            // Si todavía no hay lista de líneas, el total es 0 (evita NullReference).
            get { return _Lineas != null ? _Lineas.Sum(l => l.Subtotal) : 0m; }
        }

        /// <summary>Cantidad total de unidades en el carrito (suma de cantidades de todas las líneas).</summary>
        public int CantidadItems
        {
            get { return _Lineas != null ? _Lineas.Sum(l => l.Cantidad) : 0; }
        }

        /// <summary>Atajo al DNI del cliente asociado (string vacío si todavía no se asignó cliente).</summary>
        public string ClienteDNI
        {
            get { return _Cliente != null ? _Cliente.DNI : string.Empty; }
        }

        /// <summary>Constructor: deja el carrito listo para usar — lista vacía, fecha de hoy y estado Abierto.</summary>
        public BE_Carrito_GO44()
        {
            _Lineas = new List<BE_LineaCarrito_GO44>();
            _FechaCreacion = DateTime.Now;
            _Estado = EstadoCarrito.Abierto;
        }
    }
}
