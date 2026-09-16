using BE;
using Servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALCliente_GO44
    {
        private readonly Acceso _acceso;
        private readonly DALIntegridad_GO44 _dalIntegridad;

        public DALCliente_GO44()
        {
            _acceso = Acceso.Instancia;
            _dalIntegridad = new DALIntegridad_GO44();
        }

        private void RecalcularIntegridadCliente()
        {
            if (DALIntegridad_GO44.IntegridadConocidamenteRota) return;
            try { _dalIntegridad.RecalcularTabla("Clientes"); } catch { }
        }

        public bool ExisteDNI(string dni)
        {
            SqlParameter[] p = { new SqlParameter("@DNI", dni) };
            object resultado = _acceso.leerEscalarSP("sp_Cliente_ExisteDNI_GO44", p);
            return resultado != null && Convert.ToInt32(resultado) > 0;
        }

        public BE_Cliente_GO44 BuscarPorDNI(string dni)
        {
            SqlParameter[] p = { new SqlParameter("@DNI", dni) };
            DataTable dt = _acceso.leerSP("sp_Cliente_BuscarPorDNI_GO44", p);
            if (dt.Rows.Count == 0) return null;
            return MapearFila(dt.Rows[0]);
        }

        public List<BE_Cliente_GO44> Listar()
        {
            DataTable dt = _acceso.leerSP("sp_Cliente_Listar_GO44", null);
            return MapearLista(dt);
        }

        public int Insertar(BE_Cliente_GO44 cliente)
        {
            string emailCifrado = EncriptadorReversible_GO44.Instancia.Encriptar(cliente.Email);

            SqlParameter[] p = {
                new SqlParameter("@DNI",      cliente.DNI),
                new SqlParameter("@Apellido", cliente.Apellido),
                new SqlParameter("@Nombre",   cliente.Nombre),
                new SqlParameter("@Email",    emailCifrado),
                new SqlParameter("@Telefono", (object)cliente.Telefono ?? DBNull.Value)
            };
            int filas = Convert.ToInt32(_acceso.leerEscalarSP("sp_Cliente_Insertar_GO44", p));
            RecalcularIntegridadCliente();
            return filas;
        }

        /// <summary>
        /// Variante que participa de una transacción externa (usada por CU01 Cargar Carrito
        /// cuando se registra un cliente nuevo dentro del mismo commit).
        /// </summary>
        public int InsertarEnTx(BE_Cliente_GO44 cliente, SqlTransaction tx)
        {
            string emailCifrado = EncriptadorReversible_GO44.Instancia.Encriptar(cliente.Email);

            SqlParameter[] p = {
                new SqlParameter("@DNI",      cliente.DNI),
                new SqlParameter("@Apellido", cliente.Apellido),
                new SqlParameter("@Nombre",   cliente.Nombre),
                new SqlParameter("@Email",    emailCifrado),
                new SqlParameter("@Telefono", (object)cliente.Telefono ?? DBNull.Value)
            };
            // El SP devuelve filas como SELECT — usamos ExecuteScalar dentro de la tx
            object r = _acceso.leerEscalarSPEnTx("sp_Cliente_Insertar_GO44", p, tx);
            return r == null ? 0 : Convert.ToInt32(r);
        }

        public int ModificarEmail(string dni, string email)
        {
            string emailCifrado = EncriptadorReversible_GO44.Instancia.Encriptar(email);
            SqlParameter[] p = {
                new SqlParameter("@DNI",   dni),
                new SqlParameter("@Email", emailCifrado)
            };
            int filas = Convert.ToInt32(_acceso.leerEscalarSP("sp_Cliente_ModificarEmail_GO44", p));
            RecalcularIntegridadCliente();
            return filas;
        }

        public int ActivarDesactivar(string dni, bool activo)
        {
            SqlParameter[] p = {
                new SqlParameter("@DNI",    dni),
                new SqlParameter("@Activo", activo)
            };
            int filas = Convert.ToInt32(_acceso.leerEscalarSP("sp_Cliente_ActivarDesactivar_GO44", p));
            RecalcularIntegridadCliente();
            return filas;
        }

        private List<BE_Cliente_GO44> MapearLista(DataTable dt)
        {
            List<BE_Cliente_GO44> lista = new List<BE_Cliente_GO44>();
            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));
            return lista;
        }

        private BE_Cliente_GO44 MapearFila(DataRow row)
        {
            return new BE_Cliente_GO44
            {
                DNI       = row["DNI"].ToString(),
                Apellido  = row["Apellido"].ToString(),
                Nombre    = row["Nombre"].ToString(),
                Email     = EncriptadorReversible_GO44.Instancia.Desencriptar(row["Email"].ToString()),
                Telefono  = row["Telefono"] == DBNull.Value ? null : row["Telefono"].ToString(),
                FechaAlta = Convert.ToDateTime(row["FechaAlta"]),
                Activo    = Convert.ToBoolean(row["Activo"])
            };
        }
    }
}
