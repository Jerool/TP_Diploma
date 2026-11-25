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
    /// Es un Singleton: existe una sola instancia compartida (<see cref="Instancia"/>). Pero —a
    /// diferencia de la versión anterior— NO mantiene una única SqlConnection viva para toda la app:
    /// cada operación abre su PROPIA conexión del pool de ADO.NET y la cierra al terminar
    /// (patrón "open late / close early"). El pool reutiliza conexiones físicas de forma eficiente
    /// y descarta solas las que el servidor dio de baja por inactividad, de modo que la primera
    /// operación tras un rato de inactividad ya no falla por una conexión "vieja".
    /// </summary>
    public class Acceso
    {
        // Única instancia del Singleton (se crea la primera vez que se pide Instancia).
        private static Acceso _instancia;

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
        private Acceso() { }

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

        // =====================================================================
        // Helper: abre una conexión NUEVA del pool y la devuelve ya abierta.
        // =====================================================================
        private static SqlConnection AbrirConexion()
        {
            string cs = ConnectionString;
            if (string.IsNullOrEmpty(cs))
                throw new InvalidOperationException(
                    "No hay una instancia SQL configurada. Reiniciá la aplicación " +
                    "y elegí tu instancia en la pantalla de selección.");

            var cn = new SqlConnection(cs);
            cn.Open();
            return cn;
        }

        /// <summary>Cierra y libera una conexión (y su transacción) sin propagar errores. Usado en el finally de las operaciones transaccionales.</summary>
        private static void CerrarConexionDeTx(SqlTransaction tx, SqlConnection cn)
        {
            try { tx?.Dispose(); } catch { }
            if (cn != null)
            {
                try { if (cn.State != ConnectionState.Closed) cn.Close(); } catch { }
                try { cn.Dispose(); } catch { }
            }
        }

        // =====================================================================
        // Transacciones (encabezado + líneas + stock atómicos)
        // Cada transacción vive sobre su PROPIA conexión; la conexión se cierra
        // al confirmar o cancelar (Confirmar/Cancelar liberan todo).
        // =====================================================================

        /// <summary>
        /// Abre una conexión nueva y arranca una transacción sobre ella, devolviéndola al llamador.
        /// La conexión queda "colgada" de la transacción: el llamador DEBE cerrarla con
        /// ConfirmarTransaccion o CancelarTransaccion (ambas la cierran y liberan).
        /// </summary>
        public SqlTransaction IniciarTransaccion()
        {
            SqlConnection cn = AbrirConexion();
            try
            {
                return cn.BeginTransaction();
            }
            catch
            {
                // Si falla el BeginTransaction, no dejamos la conexión colgada.
                CerrarConexionDeTx(null, cn);
                throw;
            }
        }

        /// <summary>Confirma (Commit) la transacción y cierra/libera su conexión. Si tx es null, no hace nada.</summary>
        public void ConfirmarTransaccion(SqlTransaction tx)
        {
            if (tx == null) return;
            SqlConnection cn = tx.Connection;
            try
            {
                tx.Commit();
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error al confirmar la transacción: " + ex.Message);
            }
            finally
            {
                CerrarConexionDeTx(tx, cn); // pase lo que pase, liberar la conexión
            }
        }

        /// <summary>Deshace (Rollback) la transacción ante un error y cierra/libera su conexión. Deja la base como estaba.</summary>
        public void CancelarTransaccion(SqlTransaction tx)
        {
            if (tx == null) return;
            SqlConnection cn = tx.Connection;
            try
            {
                tx.Rollback();
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error al cancelar la transacción: " + ex.Message);
            }
            finally
            {
                CerrarConexionDeTx(tx, cn); // liberar siempre la conexión, también en el rollback
            }
        }

        // =====================================================================
        // Operaciones con query de texto
        // =====================================================================

        /// <summary>
        /// Ejecuta una sentencia SQL de escritura (INSERT/UPDATE/DELETE) con query de texto,
        /// envuelta en su propia transacción. Ante error hace Rollback y relanza. Devuelve filas afectadas.
        /// </summary>
        public int escribir(string query, SqlParameter[] parametro)
        {
            using (SqlConnection cn = AbrirConexion())
            {
                SqlTransaction tx = cn.BeginTransaction();
                try
                {
                    SqlCommand comando = new SqlCommand
                    {
                        Connection = cn,
                        Transaction = tx,
                        CommandText = query
                    };
                    if (parametro != null)
                    {
                        foreach (SqlParameter param in parametro)
                            comando.Parameters.AddWithValue(param.ParameterName, param.Value);
                    }
                    int filasAfectadas = comando.ExecuteNonQuery();
                    tx.Commit();
                    return filasAfectadas;
                }
                catch (Exception ex)
                {
                    try { tx.Rollback(); } catch { }
                    throw new Exception("Error en Escribir: " + ex.Message, ex);
                }
            }
        }

        /// <summary>Ejecuta un SELECT de texto y devuelve el resultado como DataTable.</summary>
        public DataTable leer(string query, SqlParameter[] parametro)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection cn = AbrirConexion())
                {
                    SqlCommand comando = new SqlCommand { Connection = cn, CommandText = query };
                    if (parametro != null)
                    {
                        foreach (SqlParameter param in parametro)
                            comando.Parameters.AddWithValue(param.ParameterName, param.Value);
                    }
                    using (SqlDataAdapter adaptador = new SqlDataAdapter(comando))
                        adaptador.Fill(dt);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error en Leer: " + ex.Message);
            }
            return dt;
        }

        /// <summary>Ejecuta una consulta que devuelve un único valor (ej. un COUNT o un MAX). Retorna ese valor como object.</summary>
        public object leerEscalar(string query, SqlParameter[] parametro)
        {
            object resultado = null;
            try
            {
                using (SqlConnection cn = AbrirConexion())
                {
                    SqlCommand comando = new SqlCommand { Connection = cn, CommandText = query };
                    if (parametro != null)
                        comando.Parameters.AddRange(parametro);
                    resultado = comando.ExecuteScalar();
                    comando.Parameters.Clear();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error al ejecutar ExecuteScalar: " + ex.Message);
            }
            return resultado;
        }

        // =====================================================================
        // MÉTODOS PARA STORED PROCEDURES (usados por la capa de negocio GO44)
        // =====================================================================

        /// <summary>Ejecuta un SP de lectura (SELECT) y retorna un DataTable.</summary>
        public DataTable leerSP(string nombreSP, SqlParameter[] parametros)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection cn = AbrirConexion())
                {
                    SqlCommand comando = new SqlCommand
                    {
                        Connection = cn,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = nombreSP
                    };
                    if (parametros != null)
                    {
                        foreach (SqlParameter p in parametros)
                            comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
                    }
                    using (SqlDataAdapter adaptador = new SqlDataAdapter(comando))
                        adaptador.Fill(dt);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error en leerSP (" + nombreSP + "): " + ex.Message, ex);
            }
            return dt;
        }

        /// <summary>Ejecuta un SP de escritura (INSERT/UPDATE/DELETE) con su propia transacción. Retorna filas afectadas.</summary>
        public int escribirSP(string nombreSP, SqlParameter[] parametros)
        {
            using (SqlConnection cn = AbrirConexion())
            {
                SqlTransaction tx = cn.BeginTransaction();
                try
                {
                    SqlCommand comando = new SqlCommand
                    {
                        Connection = cn,
                        Transaction = tx,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = nombreSP
                    };
                    if (parametros != null)
                    {
                        foreach (SqlParameter p in parametros)
                            comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
                    }
                    int filas = comando.ExecuteNonQuery();
                    tx.Commit();
                    return filas;
                }
                catch (Exception ex)
                {
                    try { tx.Rollback(); } catch { }
                    throw new Exception("Error en escribirSP (" + nombreSP + "): " + ex.Message, ex);
                }
            }
        }

        /// <summary>Ejecuta un SP escalar (típicamente para SCOPE_IDENTITY o COUNT) con su propia conexión.</summary>
        public object leerEscalarSP(string nombreSP, SqlParameter[] parametros)
        {
            try
            {
                using (SqlConnection cn = AbrirConexion())
                {
                    SqlCommand comando = new SqlCommand
                    {
                        Connection = cn,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = nombreSP
                    };
                    if (parametros != null)
                    {
                        foreach (SqlParameter p in parametros)
                            comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
                    }
                    return comando.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error en leerEscalarSP (" + nombreSP + "): " + ex.Message, ex);
            }
        }

        // ----- Variantes que participan de una transacción externa (para CU01 Cargar Carrito) -----

        /// <summary>
        /// Ejecuta un SP de escritura dentro de una transacción ya iniciada por el llamador.
        /// El llamador es responsable de hacer Commit/Rollback y de cerrar la conexión
        /// (vía ConfirmarTransaccion / CancelarTransaccion).
        /// </summary>
        public int escribirSPEnTx(string nombreSP, SqlParameter[] parametros, SqlTransaction tx)
        {
            if (tx == null) throw new ArgumentNullException("tx");
            SqlCommand comando = new SqlCommand
            {
                Connection = tx.Connection,
                Transaction = tx,
                CommandType = CommandType.StoredProcedure,
                CommandText = nombreSP
            };
            if (parametros != null)
            {
                foreach (SqlParameter p in parametros)
                    comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
            }
            return comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Ejecuta un SP escalar dentro de una transacción ya iniciada. Útil para SCOPE_IDENTITY.
        /// El llamador cierra la conexión vía ConfirmarTransaccion / CancelarTransaccion.
        /// </summary>
        public object leerEscalarSPEnTx(string nombreSP, SqlParameter[] parametros, SqlTransaction tx)
        {
            if (tx == null) throw new ArgumentNullException("tx");
            SqlCommand comando = new SqlCommand
            {
                Connection = tx.Connection,
                Transaction = tx,
                CommandType = CommandType.StoredProcedure,
                CommandText = nombreSP
            };
            if (parametros != null)
            {
                foreach (SqlParameter p in parametros)
                    comando.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
            }
            return comando.ExecuteScalar();
        }
    }
}
