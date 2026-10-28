using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BLL
{
    /// <summary>
    /// Generador de PDF hecho a mano, sin librerías externas: escribe el formato PDF 1.4 byte a byte.
    /// Produce un reporte tabular genérico (título, subtítulo y una tabla con encabezados y filas) con
    /// paginación automática. Un PDF es una lista de "objetos" numerados (catálogo, páginas, fuentes y
    /// los "contents" con las instrucciones de dibujo), más una tabla de posiciones (xref) al final que
    /// dice en qué byte arranca cada objeto. Esta clase arma todo eso.
    /// </summary>
    public class GeneradorPdf_GO44
    {
        // Dimensiones en "puntos" (1/72 pulgada). 792x612 = carta apaisado (landscape).
        private const int ANCHO_PAGINA = 792;
        private const int ALTO_PAGINA = 612;
        private const int MARGEN = 50;
        private const int ALTO_FILA = 18;
        private const int FILAS_POR_PAGINA = 22;   // cuántas filas entran antes de saltar de página

        // Cultura invariante: fuerza el punto como separador decimal (el PDF no entiende comas).
        private static readonly CultureInfo INV = CultureInfo.InvariantCulture;

        private MemoryStream _buffer;          // acá se va armando el PDF en memoria
        private List<long> _offsetsObjetos;    // posición (byte) donde arranca cada objeto, para la xref

        /// <summary>
        /// Genera el PDF y lo devuelve como byte[] (útil para adjuntar en emails
        /// sin escribir a disco). Misma firma que Generar() salvo que no toma ruta.
        /// </summary>
        public byte[] GenerarBytes(string titulo, string subtitulo,
                                   string[] headers, float[] anchosProporcionales,
                                   List<string[]> filas)
        {
            GenerarInterno(titulo, subtitulo, headers, anchosProporcionales, filas);
            return _buffer.ToArray();
        }

        /// <summary>Igual que GenerarBytes pero guarda el PDF directo en un archivo (para "exportar a PDF").</summary>
        public void Generar(string ruta, string titulo, string subtitulo,
                            string[] headers, float[] anchosProporcionales,
                            List<string[]> filas)
        {
            GenerarInterno(titulo, subtitulo, headers, anchosProporcionales, filas);
            File.WriteAllBytes(ruta, _buffer.ToArray());
        }

        /// <summary>
        /// Arma el PDF completo en el buffer. Primero calcula cuántas páginas hacen falta y reserva un
        /// número de objeto (Id) para cada parte: catálogo, lista de páginas, una página por hoja, las dos
        /// fuentes y un "contents" por hoja. Después escribe esos objetos, dibuja cada página y cierra con
        /// la tabla xref y el trailer. El orden de los Id es fijo para poder referenciarlos entre objetos.
        /// </summary>
        /// <param name="anchosProporcionales">Ancho de cada columna como fracción (0..1) del ancho útil; deben sumar ~1.</param>
        private void GenerarInterno(string titulo, string subtitulo,
                                    string[] headers, float[] anchosProporcionales,
                                    List<string[]> filas)
        {
            _buffer = new MemoryStream();
            _offsetsObjetos = new List<long>();

            // Techo de la división: si hay 23 filas y entran 22 por página → 2 páginas.
            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)filas.Count / FILAS_POR_PAGINA));

            // Reserva de Ids (numeración fija): catálogo, páginas, N páginas, 2 fuentes, N contents.
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

            // Convertir los anchos proporcionales (fracciones) a puntos reales según el ancho útil.
            float anchoUtil = ANCHO_PAGINA - 2 * MARGEN;
            float[] anchos = anchosProporcionales.Select(pct => pct * anchoUtil).ToArray();

            // Dibujar cada página: su porción de filas. 'content' acumula las instrucciones de dibujo PDF.
            for (int p = 0; p < totalPaginas; p++)
            {
                int idContents = primerIdContents + p;
                int filaInicio = p * FILAS_POR_PAGINA;
                int filaFin = Math.Min(filaInicio + FILAS_POR_PAGINA, filas.Count);

                StringBuilder content = new StringBuilder();
                // En PDF el origen (0,0) está abajo-izquierda, así que se dibuja de arriba hacia abajo
                // bajando 'yActual'. Arranca pegado al margen superior.
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
                        // Recorte simple para que el texto no se desborde de su columna: estima cuántos
                        // caracteres entran (~5 pt c/u) y si se pasa lo corta y le pone un punto.
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
        }

        /// <summary>Escribe el encabezado del PDF: la versión y una línea de bytes binarios que marca que el archivo no es solo texto.</summary>
        private void EscribirHeader()
        {
            EscribirBytes("%PDF-1.4\n");
            byte[] binario = { (byte)'%', 0xE2, 0xE3, 0xCF, 0xD3, (byte)'\n' };
            _buffer.Write(binario, 0, binario.Length);
        }

        /// <summary>
        /// Escribe un objeto PDF "N 0 obj ... endobj", anotando antes en qué byte arranca (lo necesita la xref).
        /// El while asegura que la lista de offsets tenga lugar para este Id aunque se escriban fuera de orden.
        /// </summary>
        private void EscribirObjeto(int id, string contenido)
        {
            while (_offsetsObjetos.Count < id) _offsetsObjetos.Add(0);
            _offsetsObjetos[id - 1] = _buffer.Position;
            EscribirBytes($"{id} 0 obj\n{contenido}\nendobj\n");
        }

        /// <summary>
        /// Como EscribirObjeto, pero para un objeto con "stream" (el contenido de dibujo de una página).
        /// Debe declarar /Length con la cantidad exacta de bytes del stream, por eso se calcula primero.
        /// </summary>
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

        /// <summary>
        /// Escribe la tabla de referencias cruzadas (xref): una línea por objeto con su posición en bytes
        /// (10 dígitos). Es lo que permite al lector de PDF saltar directo a cualquier objeto. La primera
        /// entrada (objeto 0) es siempre la "libre" especial.
        /// </summary>
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

        /// <summary>Escribe el cierre del PDF: el trailer (apunta al catálogo), dónde empieza la xref (startxref) y el marcador %%EOF.</summary>
        private void EscribirTrailer(int idCatalog, long xrefOffset)
        {
            int total = _offsetsObjetos.Count + 1;
            EscribirBytes($"trailer\n<</Size {total}/Root {idCatalog} 0 R>>\n");
            EscribirBytes($"startxref\n{xrefOffset}\n");
            EscribirBytes("%%EOF\n");
        }

        /// <summary>Escribe un string al buffer usando el codepage 1252 (Windows-ANSI), que es el encoding que declaran las fuentes.</summary>
        private void EscribirBytes(string s)
        {
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(s);
            _buffer.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Escapa los caracteres especiales del texto para el formato PDF: la barra invertida y los
        /// paréntesis (que delimitan las cadenas). Sin esto, un "(" en un nombre rompería el archivo.
        /// </summary>
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
