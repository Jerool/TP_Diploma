using System;
using System.Collections.Generic;
using System.Linq;

namespace BE
{
    public class BE_Factura_GO44
    {
        public enum EstadoFactura
        {
            Pendiente,
            Cobrada,
            Anulada
        }

        private int _Id;
        public int Id { get { return _Id; } set { _Id = value; } }

        private string _NumeroFactura;
        public string NumeroFactura { get { return _NumeroFactura; } set { _NumeroFactura = value; } }

        private int _IdCarrito;
        public int IdCarrito { get { return _IdCarrito; } set { _IdCarrito = value; } }

        private BE_Cliente_GO44 _Cliente;
        public BE_Cliente_GO44 Cliente { get { return _Cliente; } set { _Cliente = value; } }

        private string _LoginCajero;
        public string LoginCajero { get { return _LoginCajero; } set { _LoginCajero = value; } }

        private DateTime _FechaEmision;
        public DateTime FechaEmision { get { return _FechaEmision; } set { _FechaEmision = value; } }

        private decimal _Subtotal;
        public decimal Subtotal { get { return _Subtotal; } set { _Subtotal = value; } }

        private decimal _IVA;
        public decimal IVA { get { return _IVA; } set { _IVA = value; } }

        private decimal _Total;
        public decimal Total { get { return _Total; } set { _Total = value; } }

        private EstadoFactura _Estado;
        public EstadoFactura Estado { get { return _Estado; } set { _Estado = value; } }

        private List<BE_LineaFactura_GO44> _Lineas;
        public List<BE_LineaFactura_GO44> Lineas { get { return _Lineas; } set { _Lineas = value; } }

        public string DniCliente { get { return _Cliente != null ? _Cliente.DNI : string.Empty; } }
        public int CantidadItems { get { return _Lineas != null ? _Lineas.Sum(l => l.Cantidad) : 0; } }

        public BE_Factura_GO44()
        {
            _Lineas = new List<BE_LineaFactura_GO44>();
            _FechaEmision = DateTime.Now;
            _Estado = EstadoFactura.Pendiente;
        }
    }
}
