using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace DAL
{

    public class Acceso
    {
        private static Acceso _instancia;

        protected SqlConnection conexion = null;

        public static string ConnectionString { get; set; }
        public static string InstanciaActual { get; set; }

        private Acceso()
        {
            conexion = new SqlConnection();
        }

        public static Acceso Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new Acceso();
                }
                return _instancia;
            }
        }

        public void conectar()
        {
            try
            {
                if (conexion.State == System.Data.ConnectionState.Closed)
                {
                    string cs = ConnectionString;
                    if (string.IsNullOrEmpty(cs))
                        cs = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=""Gestion Usuario"";Integrated Security=True";
                    conexion.ConnectionString = cs;
                    conexion.Open();
                    Console.WriteLine("Conexión exitosa");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error de conexión" + ex.Message);
            }
        }

        public void desconectar()
        {
            try
            {
                if (conexion.State == System.Data.ConnectionState.Open)
                {
                    conexion.Close();
                    Console.WriteLine("Desconexión exitosa.");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error al desconectar: " + ex.Message);
            }
        }

        public SqlTransaction IniciarTransaccion()
        {
            conectar();
            return conexion.BeginTransaction();
        }

        public void ConfirmarTransaccion(SqlTransaction tx)
        {
            try
            {
                if (tx != null)
                {
                    tx.Commit();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error al confirmar la transacción: " + ex.Message);
            }
            finally
            {
                desconectar();
            }
        }

        public void CancelarTransaccion(SqlTransaction tx)
        {
            try
            {
                if (tx != null)
                {
                    tx.Rollback();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error al cancelar la transacción: " + ex.Message);
            }
        }

        public int escribir(string query, SqlParameter[] parametro)
        {
            SqlTransaction tx = null;
            int filasAfectadas = 0;
            SqlCommand comando = new SqlCommand();
            try
            {
                comando.Parameters.Clear();
                tx = IniciarTransaccion();
                comando.Connection = tx.Connection;
                comando.Transaction = tx;
                comando.CommandText = query;
                if (parametro != null)
                {
                    foreach(SqlParameter param in parametro)
                    {
                        comando.Parameters.AddWithValue(param.ParameterName, param.Value);
                    }
                }
                filasAfectadas = comando.ExecuteNonQuery();
                ConfirmarTransaccion(tx);
                return filasAfectadas;
            }
            catch (Exception ex)
            {
                CancelarTransaccion(tx);
                throw new Exception("Error en Escribir: " + ex.Message, ex);
            }
        }

        public DataTable leer(string query, SqlParameter[] parametro)
        {
            SqlCommand comando = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adaptador = new SqlDataAdapter();
            try
            {
                conectar();
                comando.Connection = conexion;
                comando.CommandText = query;
                if (parametro != null)
                {
                    foreach (SqlParameter param in parametro)
                    {
                        comando.Parameters.AddWithValue(param.ParameterName, param.Value);
                    }
                }

                adaptador.SelectCommand = comando;
                adaptador.Fill(dt);
            }
            catch (SqlException ex)
            {
                throw new Exception("Error en Leer: " + ex.Message);
            }
            finally
            {
                desconectar();
            }
            return dt;
        }

        public object leerEscalar(string query, SqlParameter[] parametro)
        {
            SqlCommand comando = new SqlCommand();
            object resultado = null;
            try
            {
                conectar();
                comando.Connection = conexion;
                comando.CommandText = query;

                if (parametro != null)
                {
                    comando.Parameters.Clear();
                    comando.Parameters.AddRange(parametro);
                }
                resultado = comando.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error al ejecutar ExecuteScalar: " + ex.Message);
            }
            finally
            {
                comando.Parameters.Clear();
                desconectar();
            }
            return resultado;
        }

        // =====================================================================
        // MÉTODOS PARA STORED PROCEDURES (usados por la capa de negocio GO44)
        // =====================================================================

        /// <summary>
        /// Ejecuta un SP de lectura (SELECT) y retorna un DataTable.
        /// </summary>
        public DataTable leerSP(string nombreSP, SqlParameter[] parametros)
        {
            DataTable dt = new DataTable();
            SqlCommand comando = new SqlCommand();
            SqlDataAdapter adaptador = new SqlDataAdapter();
            try
            {
                conectar();
                comando.Connection = conexion;
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandText = nombreSP;
                if (parametros != null)
                {
                    foreach (SqlParameter p in parametros)
                        comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
                }
                adaptador.SelectCommand = comando;
                adaptador.Fill(dt);
            }
            catch (SqlException ex)
            {
                throw new Exception("Error en leerSP (" + nombreSP + "): " + ex.Message, ex);
            }
            finally
            {
                desconectar();
            }
            return dt;
        }

        /// <summary>
        /// Ejecuta un SP de escritura (INSERT/UPDATE/DELETE) con su propia transacción.
        /// Retorna filas afectadas.
        /// </summary>
        public int escribirSP(string nombreSP, SqlParameter[] parametros)
        {
            SqlTransaction tx = null;
            SqlCommand comando = new SqlCommand();
            int filas = 0;
            try
            {
                tx = IniciarTransaccion();
                comando.Connection = tx.Connection;
                comando.Transaction = tx;
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandText = nombreSP;
                if (parametros != null)
                {
                    foreach (SqlParameter p in parametros)
                        comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
                }
                filas = comando.ExecuteNonQuery();
                ConfirmarTransaccion(tx);
                return filas;
            }
            catch (Exception ex)
            {
                CancelarTransaccion(tx);
                throw new Exception("Error en escribirSP (" + nombreSP + "): " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Ejecuta un SP escalar (típicamente para SCOPE_IDENTITY o COUNT) con su propia conexión.
        /// </summary>
        public object leerEscalarSP(string nombreSP, SqlParameter[] parametros)
        {
            SqlCommand comando = new SqlCommand();
            object resultado = null;
            try
            {
                conectar();
                comando.Connection = conexion;
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandText = nombreSP;
                if (parametros != null)
                {
                    foreach (SqlParameter p in parametros)
                        comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
                }
                resultado = comando.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw new Exception("Error en leerEscalarSP (" + nombreSP + "): " + ex.Message, ex);
            }
            finally
            {
                desconectar();
            }
            return resultado;
        }

        // ----- Variantes que participan de una transacción externa (para CU01 Cargar Carrito) -----

        /// <summary>
        /// Ejecuta un SP de escritura dentro de una transacción ya iniciada por el llamador.
        /// El llamador es responsable de hacer Commit/Rollback y de cerrar la conexión.
        /// </summary>
        public int escribirSPEnTx(string nombreSP, SqlParameter[] parametros, SqlTransaction tx)
        {
            if (tx == null) throw new ArgumentNullException("tx");
            SqlCommand comando = new SqlCommand();
            comando.Connection = tx.Connection;
            comando.Transaction = tx;
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = nombreSP;
            if (parametros != null)
            {
                foreach (SqlParameter p in parametros)
                    comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
            }
            return comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Ejecuta un SP escalar dentro de una transacción ya iniciada. Útil para SCOPE_IDENTITY.
        /// </summary>
        public object leerEscalarSPEnTx(string nombreSP, SqlParameter[] parametros, SqlTransaction tx)
        {
            if (tx == null) throw new ArgumentNullException("tx");
            SqlCommand comando = new SqlCommand();
            comando.Connection = tx.Connection;
            comando.Transaction = tx;
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = nombreSP;
            if (parametros != null)
            {
                foreach (SqlParameter p in parametros)
                    comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
            }
            return comando.ExecuteScalar();
        }
    }
}
