using System.Collections.Generic;

namespace Servicios.Ayuda
{
    /// <summary>
    /// Nodo del árbol de ayuda (menú escalera). Un tema puede tener subtemas (hijos).
    /// </summary>
    public class NodoAyuda_GO44
    {
        public string Titulo { get; set; }
        public string Contenido { get; set; }
        public List<NodoAyuda_GO44> Hijos { get; set; }

        public NodoAyuda_GO44(string titulo, string contenido)
        {
            Titulo = titulo;
            Contenido = contenido;
            Hijos = new List<NodoAyuda_GO44>();
        }

        public NodoAyuda_GO44 Agregar(string titulo, string contenido)
        {
            var n = new NodoAyuda_GO44(titulo, contenido);
            Hijos.Add(n);
            return n;
        }
    }

    /// <summary>
    /// Ayuda en línea del sistema (TechFlow / ElectroPoint).
    /// Define el árbol de temas que consume la pantalla de Ayuda (menú escalera + export PDF)
    /// y la ayuda contextual (F1) por formulario.
    /// </summary>
    public static class AyudaManager_GO44
    {
        private static string T(string clave) { return IdiomaManager_GO44.T(clave); }

        /// <summary>Árbol completo de temas de ayuda (títulos y contenidos traducidos al idioma activo).</summary>
        public static List<NodoAyuda_GO44> ObtenerArbol()
        {
            var raiz = new List<NodoAyuda_GO44>();

            var general = new NodoAyuda_GO44(T("ayuda.general.tit"), T("ayuda.general.cont"));
            general.Agregar(T("ayuda.login.tit"), T("ayuda.login.cont"));
            general.Agregar(T("ayuda.roles.tit"), T("ayuda.roles.cont"));
            raiz.Add(general);

            var maestros = new NodoAyuda_GO44(T("ayuda.maestros.tit"), T("ayuda.maestros.cont"));
            maestros.Agregar(T("ayuda.productos.tit"), T("ayuda.productos.cont"));
            maestros.Agregar(T("ayuda.clientes.tit"), T("ayuda.clientes.cont"));
            maestros.Agregar(T("ayuda.compC.tit"), T("ayuda.compC.cont"));
            maestros.Agregar(T("ayuda.proveedores.tit"), T("ayuda.proveedores.cont"));
            raiz.Add(maestros);

            var ventas = new NodoAyuda_GO44(T("ayuda.ventas.tit"), T("ayuda.ventas.cont"));
            ventas.Agregar(T("ayuda.carrito.tit"), T("ayuda.carrito.cont"));
            ventas.Agregar(T("ayuda.facturar.tit"), T("ayuda.facturar.cont"));
            raiz.Add(ventas);

            var compras = new NodoAyuda_GO44(T("ayuda.compras.tit"), T("ayuda.compras.cont"));
            compras.Agregar(T("ayuda.cotizaciones.tit"), T("ayuda.cotizaciones.cont"));
            compras.Agregar(T("ayuda.oc.tit"), T("ayuda.oc.cont"));
            compras.Agregar(T("ayuda.pago.tit"), T("ayuda.pago.cont"));
            compras.Agregar(T("ayuda.recepcion.tit"), T("ayuda.recepcion.cont"));
            raiz.Add(compras);

            var reportes = new NodoAyuda_GO44(T("ayuda.reportes.tit"), T("ayuda.reportes.cont"));
            reportes.Agregar(T("ayuda.rep1.tit"), T("ayuda.rep1.cont"));
            reportes.Agregar(T("ayuda.rep2.tit"), T("ayuda.rep2.cont"));
            reportes.Agregar(T("ayuda.rep3.tit"), T("ayuda.rep3.cont"));
            raiz.Add(reportes);

            var admin = new NodoAyuda_GO44(T("ayuda.admin.tit"), T("ayuda.admin.cont"));
            admin.Agregar(T("ayuda.usuarios.tit"), T("ayuda.usuarios.cont"));
            admin.Agregar(T("ayuda.perfiles.tit"), T("ayuda.perfiles.cont"));
            admin.Agregar(T("ayuda.eventos.tit"), T("ayuda.eventos.cont"));
            admin.Agregar(T("ayuda.respaldos.tit"), T("ayuda.respaldos.cont"));
            raiz.Add(admin);

            return raiz;
        }

        /// <summary>
        /// Ayuda contextual de un formulario (para F1). Devuelve el título del tema que mejor aplica,
        /// traducido al idioma activo (coincide con el título del nodo en el árbol).
        /// </summary>
        public static string TemaDeFormulario(string nombreFormulario)
        {
            if (string.IsNullOrEmpty(nombreFormulario)) return T("ayuda.general.tit");
            string n = nombreFormulario.ToLower();
            if (n.Contains("productoscambios"))  return T("ayuda.compC.tit");
            if (n.Contains("gestionproductos"))  return T("ayuda.productos.tit");
            if (n.Contains("gestionclientes"))   return T("ayuda.clientes.tit");
            if (n.Contains("registrarproveedor"))return T("ayuda.proveedores.tit");
            if (n.Contains("cargarcarrito"))     return T("ayuda.carrito.tit");
            if (n.Contains("facturar"))          return T("ayuda.facturar.tit");
            if (n.Contains("solicitudcotizacion")) return T("ayuda.cotizaciones.tit");
            if (n.Contains("generarordencompra"))return T("ayuda.oc.tit");
            if (n.Contains("pagarordencompra"))  return T("ayuda.pago.tit");
            if (n.Contains("recepcion"))         return T("ayuda.recepcion.tit");
            if (n.Contains("reportefacturas"))   return T("ayuda.rep1.tit");
            if (n.Contains("reportecompras"))    return T("ayuda.rep2.tit");
            if (n.Contains("reporteinteligente") || n.Contains("promociones")) return T("ayuda.rep3.tit");
            if (n.Contains("gestionusuarios"))   return T("ayuda.usuarios.tit");
            if (n.Contains("gestionpermisos"))   return T("ayuda.perfiles.tit");
            if (n.Contains("bitacora"))          return T("ayuda.eventos.tit");
            if (n.Contains("backup"))            return T("ayuda.respaldos.tit");
            return T("ayuda.general.tit");
        }

        /// <summary>
        /// Filas para exportar el árbol de ayuda a PDF (menú escalera): columna 1 = tema con sangría por nivel,
        /// columna 2 = descripción resumida.
        /// </summary>
        public static List<string[]> FilasParaPdf()
        {
            var filas = new List<string[]>();
            foreach (var raiz in ObtenerArbol())
                AgregarFila(filas, raiz, 0);
            return filas;
        }

        private static void AgregarFila(List<string[]> filas, NodoAyuda_GO44 nodo, int nivel)
        {
            string sangria = new string(' ', nivel * 4);
            string prefijo = nivel == 0 ? "■ " : "– ";
            string resumen = (nodo.Contenido ?? "").Replace("\r\n", " ").Replace("\n", " ");
            if (resumen.Length > 110) resumen = resumen.Substring(0, 107) + "...";
            filas.Add(new string[] { sangria + prefijo + nodo.Titulo, resumen });
            foreach (var hijo in nodo.Hijos)
                AgregarFila(filas, hijo, nivel + 1);
        }
    }
}
