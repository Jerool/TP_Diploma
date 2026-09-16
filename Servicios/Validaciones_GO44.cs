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

        public static bool EsDniValido(string dni) => !string.IsNullOrWhiteSpace(dni) && Regex.IsMatch(dni, REGEX_DNI);

        public static bool EsEmailValido(string email) => !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, REGEX_EMAIL);

        public static bool EsNombreValido(string nombre) => !string.IsNullOrWhiteSpace(nombre) && Regex.IsMatch(nombre, REGEX_SOLO_LETRAS);

        public static bool EsApellidoValido(string apellido) => !string.IsNullOrWhiteSpace(apellido) && Regex.IsMatch(apellido, REGEX_SOLO_LETRAS);

        public static bool EsContrasenaValida(string contrasena) => !string.IsNullOrWhiteSpace(contrasena) && Regex.IsMatch(contrasena, REGEX_CONTRASENA);

        public static bool EsLoginValido(string login) => !string.IsNullOrWhiteSpace(login) && Regex.IsMatch(login, REGEX_LOGIN);

        public const string MENSAJE_DNI = "El DNI debe tener exactamente 8 dígitos numéricos (sin puntos ni espacios). Ej: 46947544";

        public const string MENSAJE_EMAIL = "El email no tiene un formato válido. Debe contener @ y un dominio. Ej: jeremias@gmail.com";

        public const string MENSAJE_NOMBRE ="El nombre solo puede contener letras y espacios (mínimo 2 caracteres).";

        public const string MENSAJE_APELLIDO ="El apellido solo puede contener letras y espacios (mínimo 2 caracteres).";

        public const string MENSAJE_CONTRASENA ="La contraseña debe tener al menos 6 caracteres, una letra y un número.";

        public const string MENSAJE_LOGIN ="El usuario solo puede contener letras, números y puntos (mínimo 3 caracteres).";
    }
}
