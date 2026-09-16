using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Servicios
{
    public static class CalculadorIntegridad_GO44
    {
        public static string CalcularDVH(params object[] campos)
        {
            var sb = new StringBuilder();
            foreach (var c in campos)
            {
                sb.Append(NormalizarCampo(c));
                sb.Append('|');
            }
            return Sha256Hex(sb.ToString());
        }

        public static string CalcularDVV(IEnumerable<string> dvhs)
        {
            var sb = new StringBuilder();
            foreach (var d in dvhs.OrderBy(x => x, StringComparer.Ordinal))
                sb.Append(d);
            return Sha256Hex(sb.ToString());
        }

        private static string Sha256Hex(string input)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static string NormalizarCampo(object v)
        {
            if (v == null || v == DBNull.Value) return "";
            if (v is DateTime dt) return dt.ToString("o", CultureInfo.InvariantCulture);
            if (v is bool b)      return b ? "1" : "0";
            return Convert.ToString(v, CultureInfo.InvariantCulture);
        }
    }
}
