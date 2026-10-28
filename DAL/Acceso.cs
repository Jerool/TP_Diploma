using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DAL
{
    /// <summary>
    /// Clase de acceso a datos de más bajo nivel ("la última DAL"): es la ÚNICA que habla
    /// directamente con SQL Server. Todas las DAL de entidades (DALComponente, DALFactura, etc.)
    /// delegan en ella para leer/escribir; así el detalle de ADO.NET (conexión, comandos,
    /// transacciones) queda encapsulado en un solo lugar.
    ///
    /// Es un Singleton: existe una sola instancia compartida (<see cref="Instancia"/>), que
    /// reutiliza una única conexión SqlConnection.
    /// </summary>
    public class Acceso
    {
        // Única instancia del Singleton (se crea la primera vez que se pide Instancia).
        private static Acceso _instancia;

        // Conexión física a SQL Server que comparten todas las operaciones.
        protected SqlConnection conexion = null;

        // ============================================================
        // CONNECTION STRING - CADA COMPAÑERO CAMBIA LA INSTANCIA AQUÍ
        // ============================================================
        // Lee de App.config (clave "GestionUsuario"). Si no existe, usa el default.
        // Cada compañero, cuando clone o descomprima el proyecto, cambia UNA sola
        // línea en App.config (o acá) para poner su instancia de SQL:
        //    - LocalDB:       (localdb)\MSSQLLocalDB
        //    - SQL Express:   .\SQLEXPRESS
        //    - Servidor:      NOMBRE-PC\SQLEXPRESS
        // ============================================================
        private const string CONNECTION_STRING_DEFAULT =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Gestion Usuario;Integrated Security=True";

        // Cadena de conexión seteada en caliente (si alguien la asigna, tiene prioridad).
        private static string _connectionString;

        /// <summary>
        /// Cadena de conexión que se usa para abrir la base. Orden de prioridad:
        /// 1) un valor asignado por código (set); 2) la clave "GestionUsuario" de App.config;
        /// 3) el default hardcodeado (LocalDB). Así cada compañero cambia su instancia en un solo lugar.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                // 1) Si ya se fijó una cadena por código, esa manda.
                if (!string.IsNullOrWhiteSpace(_connectionString))
                    return _connectionString;

                // 2) Intentar leerla del App.config (clave "GestionUsuario").
                try
                {
                    string desdeConfig = ConfigurationManager
                        .ConnectionStrings["GestionUsuario"]?.ConnectionString;
                    if (!string.IsNullOrWhiteSpace(desdeConfig))
                        return desdeConfig;
                }
                catch { } // si App.config falla, caemos al default sin romper

                // 3) Último recurso: la cadena por defecto.
                return CONNECTION_STRING_DEFAULT;
            }
            set { _connectionString = value; }
        }

        /// <summary>Nombre de la instancia SQL que surge de la cadena actual (para mostrarla en pantalla). El setter es legado y se ignora.</summary>
        public static string InstanciaActual
        {
            get { return new SqlConnectionStringBuilder(ConnectionString).DataSource; }
            set { /* legacy, ignorado */ }
        }

        // Constructor privado: nadie puede hacer "new Acceso()" desde afuera (patrón Singleton).
        private Acceso()
        {
            conexion = new SqlConnection();
        }

        /// <summary>Punto de acceso al Singleton: crea la instancia la primera vez y la reutiliza siempre.</summary>
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

        /// <summary>Abre la conexión si está cerrada. Si no hay cadena configurada, avisa con una excepción clara.</summary>
        public void conectar()
        {
            try
            {
                // Solo abrir si está cerrada (evita "la conexión ya está abierta").
                if (conexion.State == System.Data.ConnectionState.Closed)
                {
                    string cs = ConnectionString;
                    if (string.IsNullOrEmpty(cs))
                        throw new InvalidOperationException(
                            "No hay una instancia SQL configurada. Reiniciá la aplicación " +
                            "y elegí tu instancia en la pantalla de selección.");
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

        /// <summary>Cierra la conexión si está abierta. Se llama en el finally de cada operación.</summary>
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

        /// <summary>
        /// Abre la conexión y arranca una transacción, devolviéndola al llamador.
        /// Permite que varias escrituras (encabezado + líneas + stock) sean atómicas: o entran todas o ninguna.
        /// </summary>
        public SqlTransaction IniciarTransaccion()
        {
            conectar();
            return conexion.BeginTransaction();
        }

        /// <summary>Confirma (Commit) la transacción y cierra la conexión. Si tx es null, no hace nada.</summary>
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
                desconectar(); // pase lo que pase, liberar la conexión
            }
        }

        /// <summary>Deshace (Rollback) la transacción ante un error, dejando la base como estaba.</summary>
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

        /// <summary>
        /// Ejecuta una sentencia SQL de escritura (INSERT/UPDATE/DELETE) con query de texto,
        /// envuelta en su propia transacción. Ante error hace Rollback y relanza. Devuelve filas afectadas.
        /// </summary>
        public int escribir(string query, SqlParameter[] parametro)
        {
            SqlTransaction tx = null;
            int filasAfectadas = 0;
            SqlCommand comando = new SqlCommand();
            try
            {
                comando.Parameters.Clear();
                tx = IniciarTransaccion();          // abre conexión + transacción
                comando.Connection = tx.Connection;
                comando.Transaction = tx;
                comando.CommandText = query;
                if (parametro != null)
                {
                    // Volcar cada parámetro recibido al comando.
                    foreach(SqlParameter param in parametro)
                    {
                        comando.Parameters.AddWithValue(param.ParameterName, param.Value);
                    }
                }
                filasAfectadas = comando.ExecuteNonQuery();
                ConfirmarTransaccion(tx);           // todo OK: Commit
                return filasAfectadas;
            }
            catch (Exception ex)
            {
                CancelarTransaccion(tx);            // error: Rollback y relanzar con contexto
                throw new Exception("Error en Escribir: " + ex.Message, ex);
            }
        }

        /// <summary>Ejecuta un SELECT de texto y devuelve el resultado como DataTable. Cierra la conexión al terminar.</summary>
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

                // El adaptador ejecuta el SELECT y llena la tabla en memoria.
                adaptador.SelectCommand = comando;
                adaptador.Fill(dt);
            }
            catch (SqlException ex)
            {
                throw new Exception("Error en Leer: " + ex.Message);
            }
            finally
            {
                desconectar(); // liberar siempre la conexión
            }
            return dt;
        }

        /// <summary>Ejecuta una consulta que devuelve un único valor (ej. un COUNT o un MAX). Retorna ese valor como object.</summary>
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
                resultado = comando.ExecuteScalar(); // devuelve solo la 1ª columna de la 1ª fila
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
