using System;

namespace BE
{
    public class BE_Cobro_GO44
    {
        public enum MetodoPago
        {
            Efectivo,
            Tarjeta
        }

        private int _Id;
        public int Id { get { return _Id; } set { _Id = value; } }

        private int _IdFactura;
        public int IdFactura { get { return _IdFactura; } set { _IdFactura = value; } }

        private MetodoPago _Metodo;
        public MetodoPago Metodo { get { return _Metodo; } set { _Metodo = value; } }

        private decimal _Monto;
        public decimal Monto { get { return _Monto; } set { _Monto = value; } }

        private DateTime _FechaCobro;
        public DateTime FechaCobro { get { return _FechaCobro; } set { _FechaCobro = value; } }

        // Datos tarjeta (opcionales — solo si Metodo == Tarjeta)
        private string _NroTarjetaEnmasc;
        public string NroTarjetaEnmasc { get { return _NroTarjetaEnmasc; } set { _NroTarjetaEnmasc = value; } }

        private string _Banco;
        public string Banco { get { return _Banco; } set { _Banco = value; } }

        private string _TitularNombre;
        public string TitularNombre { get { return _TitularNombre; } set { _TitularNombre = value; } }

        private string _TitularApellido;
        public string TitularApellido { get { return _TitularApellido; } set { _TitularApellido = value; } }

        private string _CodigoAutoriz;
        public string CodigoAutoriz { get { return _CodigoAutoriz; } set { _CodigoAutoriz = value; } }

        public BE_Cobro_GO44()
        {
            _FechaCobro = DateTime.Now;
            _Metodo = MetodoPago.Efectivo;
        }
    }
}
