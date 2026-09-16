using System;
using System.Collections.Generic;
using System.Linq;

namespace BE
{
    public class BE_Carrito_GO44
    {
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

        private List<BE_LineaCarrito_GO44> _Lineas;
        public List<BE_LineaCarrito_GO44> Lineas
        {
            get { return _Lineas; }
            set { _Lineas = value; }
        }

        public decimal Total
        {
            get { return _Lineas != null ? _Lineas.Sum(l => l.Subtotal) : 0m; }
        }

        public int CantidadItems
        {
            get { return _Lineas != null ? _Lineas.Sum(l => l.Cantidad) : 0; }
        }

        public string ClienteDNI
        {
            get { return _Cliente != null ? _Cliente.DNI : string.Empty; }
        }

        public BE_Carrito_GO44()
        {
            _Lineas = new List<BE_LineaCarrito_GO44>();
            _FechaCreacion = DateTime.Now;
            _Estado = EstadoCarrito.Abierto;
        }

        public void AgregarLinea(BE_Componente_GO44 componente, int cantidad)
        {
            if (componente == null) return;
            if (cantidad <= 0) return;

            BE_LineaCarrito_GO44 existente = _Lineas.FirstOrDefault(l => l.Componente != null && l.Componente.Id == componente.Id);
            if (existente != null)
            {
                existente.Cantidad += cantidad;
            }
            else
            {
                _Lineas.Add(new BE_LineaCarrito_GO44(componente, cantidad));
            }
        }

        public bool QuitarLinea(int idComponente)
        {
            BE_LineaCarrito_GO44 linea = _Lineas.FirstOrDefault(l => l.Componente != null && l.Componente.Id == idComponente);
            if (linea == null) return false;
            _Lineas.Remove(linea);
            return true;
        }

        public void Vaciar()
        {
            _Lineas.Clear();
        }

        public bool EstaVacio()
        {
            return _Lineas == null || _Lineas.Count == 0;
        }
    }
}
