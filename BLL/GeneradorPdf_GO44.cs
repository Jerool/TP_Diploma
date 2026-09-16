using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BLL
{

    public class GeneradorPdf_GO44
    {
        private const int ANCHO_PAGINA = 792;
        private const int ALTO_PAGINA = 612;
        private const int MARGEN = 50;
        private const int ALTO_FILA = 18;
        private const int FILAS_POR_PAGINA = 22;

        private static readonly CultureInfo INV = CultureInfo.InvariantCulture;

        private MemoryStream _buffer;
        private List<long> _offsetsObjetos;

        public void Generar(string ruta, string titulo, string subtitulo,
                            string[] headers, float[] anchosProporcionales,
                            List<string[]> filas)
        {
            _buffer = new MemoryStream();
            _offsetsObjetos = new List<long>();

            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)filas.Count / FILAS_POR_PAGINA));

            int idCatalog = 1;
            int idPages = 2;
            int primerIdPagina = 3;
            int idFont = primerIdPagina + totalPaginas;
            int idFontBold = idFont + 1;
            int primerIdContents = idFontBold + 1;

            EscribirHeader();
            EscribirObjeto(idCatalog, $"<</Type/Catalog/Pages {idPages} 0 R>>");

            string kidsStr = string.Join(" ",
                Enumerable.Range(0, totalPaginas).Select(i => $"{primerIdPagina + i} 0 R"));
            EscribirObjeto(idPages, $"<</Type/Pages/Kids[{kidsStr}]/Count {totalPaginas}>>");

            for (int p = 0; p < totalPaginas; p++)
            {
                int idPage = primerIdPagina + p;
                int idContents = primerIdContents + p;
                EscribirObjeto(idPage,
                    $"<</Type/Page/Parent {idPages} 0 R" +
                    $"/MediaBox[0 0 {ANCHO_PAGINA} {ALTO_PAGINA}]" +
                    $"/Resources<</Font<</F1 {idFont} 0 R/F2 {idFontBold} 0 R>>>>" +
                    $"/Contents {idContents} 0 R>>");
            }

            EscribirObjeto(idFont,
                "<</Type/Font/Subtype/Type1/BaseFont/Helvetica/Encoding/WinAnsiEncoding>>");
            EscribirObjeto(idFontBold,
                "<</Type/Font/Subtype/Type1/BaseFont/Helvetica-Bold/Encoding/WinAnsiEncoding>>");

            float anchoUtil = ANCHO_PAGINA - 2 * MARGEN;
            float[] anchos = anchosProporcionales.Select(pct => pct * anchoUtil).ToArray();

            for (int p = 0; p < totalPaginas; p++)
            {
                int idContents = primerIdContents + p;
                int filaInicio = p * FILAS_POR_PAGINA;
                int filaFin = Math.Min(filaInicio + FILAS_POR_PAGINA, filas.Count);

                StringBuilder content = new StringBuilder();
                int yActual = ALTO_PAGINA - MARGEN;

                content.Append("BT\n");
                content.Append("0 0 0 rg\n");
                content.Append("/F2 16 Tf\n");
                content.AppendFormat(INV, "1 0 0 1 {0} {1} Tm\n", MARGEN, yActual - 16);
                content.AppendFormat(INV, "({0}) Tj\n", EscaparTexto(titulo));
                content.Append("ET\n");
                yActual -= 24;

                content.Append("BT\n");
                content.Append("0.45 0.45 0.45 rg\n");
                content.Append("/F1 9 Tf\n");
                content.AppendFormat(INV, "1 0 0 1 {0} {1} Tm\n", MARGEN, yActual);
                string subtitFinal = subtitulo + (totalPaginas > 1 ? $"   |   Pagina {p + 1} de {totalPaginas}" : "");
                content.AppendFormat(INV, "({0}) Tj\n", EscaparTexto(subtitFinal));
                content.Append("ET\n");
                yActual -= 20;

                content.Append("0.7 0.7 0.7 RG\n");
                content.Append("0.6 w\n");
                content.AppendFormat(INV, "{0} {1} m\n", MARGEN, yActual);
                content.AppendFormat(INV, "{0} {1} l\n", MARGEN + anchoUtil, yActual);
                content.Append("S\n");
                yActual -= 14;

                int yHeaderBaseline = yActual;
                content.Append("BT\n");
                content.Append("0 0 0 rg\n");
                content.Append("/F2 10 Tf\n");
                float xCol = MARGEN;
                for (int h = 0; h < headers.Length; h++)
                {
                    content.AppendFormat(INV, "1 0 0 1 {0} {1} Tm\n", xCol, yHeaderBaseline);
                    content.AppendFormat(INV, "({0}) Tj\n", EscaparTexto(headers[h]));
                    xCol += anchos[h];
                }
                content.Append("ET\n");
                yActual -= 8;

                content.Append("0.5 0.5 0.5 RG\n");
                content.Append("0.7 w\n");
                content.AppendFormat(INV, "{0} {1} m\n", MARGEN, yActual);
                content.AppendFormat(INV, "{0} {1} l\n", MARGEN + anchoUtil, yActual);
                content.Append("S\n");
                yActual -= 12;

                for (int f = filaInicio; f < filaFin; f++)
                {
                    string[] valores = filas[f];

                    content.Append("BT\n");
                    content.Append("0 0 0 rg\n");
                    content.Append("/F1 9 Tf\n");
                    xCol = MARGEN;
                    for (int c = 0; c < headers.Length; c++)
                    {
                        string val = c < valores.Length ? (valores[c] ?? "") : "";
                        int maxChars = (int)(anchos[c] / 5);
                        if (val.Length > maxChars && maxChars > 1)
                            val = val.Substring(0, maxChars - 1) + ".";

                        content.AppendFormat(INV, "1 0 0 1 {0} {1} Tm\n", xCol, yActual);
                        content.AppendFormat(INV, "({0}) Tj\n", EscaparTexto(val));
                        xCol += anchos[c];
                    }
                    content.Append("ET\n");

                    yActual -= ALTO_FILA;

                    content.Append("0.88 0.88 0.88 RG\n");
                    content.Append("0.3 w\n");
                    content.AppendFormat(INV, "{0} {1} m\n", MARGEN, yActual + 4);
                    content.AppendFormat(INV, "{0} {1} l\n", MARGEN + anchoUtil, yActual + 4);
                    content.Append("S\n");
                }

                EscribirStreamObjeto(idContents, content.ToString());
            }

            long xrefOffset = _buffer.Position;
            EscribirXref();
            EscribirTrailer(idCatalog, xrefOffset);

            File.WriteAllBytes(ruta, _buffer.ToArray());
        }

        private void EscribirHeader()
        {
            EscribirBytes("%PDF-1.4\n");
            byte[] binario = { (byte)'%', 0xE2, 0xE3, 0xCF, 0xD3, (byte)'\n' };
            _buffer.Write(binario, 0, binario.Length);
        }

        private void EscribirObjeto(int id, string contenido)
        {
            while (_offsetsObjetos.Count < id) _offsetsObjetos.Add(0);
            _offsetsObjetos[id - 1] = _buffer.Position;
            EscribirBytes($"{id} 0 obj\n{contenido}\nendobj\n");
        }

        private void EscribirStreamObjeto(int id, string contenidoStream)
        {
            byte[] streamBytes = Encoding.GetEncoding(1252).GetBytes(contenidoStream);
            while (_offsetsObjetos.Count < id) _offsetsObjetos.Add(0);
            _offsetsObjetos[id - 1] = _buffer.Position;

            EscribirBytes($"{id} 0 obj\n");
            EscribirBytes($"<</Length {streamBytes.Length}>>\nstream\n");
            _buffer.Write(streamBytes, 0, streamBytes.Length);
            EscribirBytes("\nendstream\nendobj\n");
        }

        private void EscribirXref()
        {
            int total = _offsetsObjetos.Count + 1;
            EscribirBytes("xref\n");
            EscribirBytes($"0 {total}\n");
            EscribirBytes("0000000000 65535 f \n");
            foreach (var off in _offsetsObjetos)
            {
                EscribirBytes($"{off.ToString("D10")} 00000 n \n");
            }
        }

        private void EscribirTrailer(int idCatalog, long xrefOffset)
        {
            int total = _offsetsObjetos.Count + 1;
            EscribirBytes($"trailer\n<</Size {total}/Root {idCatalog} 0 R>>\n");
            EscribirBytes($"startxref\n{xrefOffset}\n");
            EscribirBytes("%%EOF\n");
        }

        private void EscribirBytes(string s)
        {
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(s);
            _buffer.Write(bytes, 0, bytes.Length);
        }

        private string EscaparTexto(string s)
        {
            if (s == null) return "";
            return s
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }
    }
}
