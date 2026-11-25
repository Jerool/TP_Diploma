using System.Text.RegularExpressions;

namespace Servicios
{

    public static class Validaciones_GO44
    {

        public const string REGEX_DNI = @"^\d{8}$";

        public const string REGEX_EMAIL = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        public const string REGEX_SOLO_LETRAS = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,}$";

        public const string REGEX_CONTRASENA = @"^(?=.*[A-Za-zÁÉÍÓÚáéíóúÑñ])(?=.*\d).{6,}$";

        public const string REGEX_LOGIN = @"^[a-zA-Z0-9.]{3,}$";

        public const string REGEX_TELEFONO = @"^\d{10}$";

        // CUIT: 11 dígitos (sin guiones). Ej: 20469475447
        public const string REGEX_CUIT = @"^\d{11}$";

        // CVU: 22 dígitos. Ej: 0000003100099999999999
        public const string REGEX_CVU = @"^\d{22}$";

        public static bool EsDniValido(string dni) => !string.IsNullOrWhiteSpace(dni) && Regex.IsMatch(dni, REGEX_DNI);

        public static bool EsTelefonoValido(string tel) => !string.IsNullOrWhiteSpace(tel) && Regex.IsMatch(tel, REGEX_TELEFONO);

        public static bool EsEmailValido(string email) => !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, REGEX_EMAIL);

        public static bool EsNombreValido(string nombre) => !string.IsNullOrWhiteSpace(nombre) && Regex.IsMatch(nombre, REGEX_SOLO_LETRAS);

        public static bool EsApellidoValido(string apellido) => !string.IsNullOrWhiteSpace(apellido) && Regex.IsMatch(apellido, REGEX_SOLO_LETRAS);

        public static bool EsContrasenaValida(string contrasena) => !string.IsNullOrWhiteSpace(contrasena) && Regex.IsMatch(contrasena, REGEX_CONTRASENA);

        public static bool EsLoginValido(string login) => !string.IsNullOrWhiteSpace(login) && Regex.IsMatch(login, REGEX_LOGIN);

        public static bool EsCuitValido(string cuit) => !string.IsNullOrWhiteSpace(cuit) && Regex.IsMatch(cuit, REGEX_CUIT);

        public static bool EsCvuValido(string cvu) => !string.IsNullOrWhiteSpace(cvu) && Regex.IsMatch(cvu, REGEX_CVU);

        // Mensajes de validación: salen traducidos del IdiomaManager según el idioma activo.
        // Se mantiene la misma sintaxis de uso que antes (Validaciones_GO44.MENSAJE_X).
        public static string MENSAJE_DNI        => IdiomaManager_GO44.T("valid.dni");
        public static string MENSAJE_EMAIL      => IdiomaManager_GO44.T("valid.email");
        public static string MENSAJE_NOMBRE     => IdiomaManager_GO44.T("valid.nombre");
        public static string MENSAJE_APELLIDO   => IdiomaManager_GO44.T("valid.apellido");
        public static string MENSAJE_CONTRASENA => IdiomaManager_GO44.T("valid.contrasena");
        public static string MENSAJE_LOGIN      => IdiomaManager_GO44.T("valid.login");
        public static string MENSAJE_TELEFONO   => IdiomaManager_GO44.T("valid.telefono");
        public static string MENSAJE_CUIT       => IdiomaManager_GO44.T("valid.cuit");
        public static string MENSAJE_CVU        => IdiomaManager_GO44.T("valid.cvu");
    }
}
