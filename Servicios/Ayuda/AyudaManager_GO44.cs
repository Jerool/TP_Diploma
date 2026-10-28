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
        /// <summary>Árbol completo de temas de ayuda.</summary>
        public static List<NodoAyuda_GO44> ObtenerArbol()
        {
            var raiz = new List<NodoAyuda_GO44>();

            var general = new NodoAyuda_GO44("General",
                "Sistema de compra y venta de componentes de hardware.\r\n\r\n" +
                "• Ingresá con tu usuario y contraseña. Según tu rol vas a ver distintos menús.\r\n" +
                "• Los menús se habilitan por permisos (patentes). Un administrador ve todo.\r\n" +
                "• Podés cambiar el idioma desde Usuario → Cambiar idioma.\r\n" +
                "• Presioná F1 en cualquier pantalla para ver su ayuda.");
            general.Agregar("Inicio de sesión",
                "Ingresá tu nombre de usuario y contraseña. Tras 3 intentos fallidos el usuario se bloquea " +
                "y debe desbloquearlo un administrador.");
            general.Agregar("Roles y permisos",
                "Cada usuario tiene un rol (Administrador, Cajero, Encargado de compras, Almacenista). " +
                "El rol define qué opciones de menú están visibles.");
            raiz.Add(general);

            var maestros = new NodoAyuda_GO44("Maestros",
                "Alta y mantenimiento de los datos base del sistema.");
            maestros.Agregar("Productos",
                "ABM de componentes (CPU, GPU, RAM, etc.). Alta, modificación de precio y baja lógica. " +
                "Cada cambio queda registrado en 'Productos - C'.");
            maestros.Agregar("Clientes",
                "ABM de clientes. Validaciones de DNI (8), email, teléfono (10). Export/Import XML.");
            maestros.Agregar("Productos - C",
                "Bitácora de cambios de productos. Muestra el historial de cada componente (versionado): " +
                "Cod_Prod, Fecha, Hora, Nombre, Existencia, Descripción y Act (vigente). " +
                "Filtrá por código, nombre y rango de fechas. 'ACTIVAR' marca una versión como vigente.");
            maestros.Agregar("Proveedores",
                "Registro de proveedores (CUIT, datos de contacto y bancarios: CVU, Banco, Titular). " +
                "Se puede abrir desde acá o como paso del armado de una solicitud de cotización.");
            raiz.Add(maestros);

            var ventas = new NodoAyuda_GO44("Ventas",
                "Proceso de venta en la tienda (RFN1).");
            ventas.Agregar("Cargar carrito",
                "Seleccioná el cliente y agregá componentes con su cantidad. El sistema valida el stock.");
            ventas.Agregar("Facturar",
                "Genera la factura del carrito y registra el cobro (efectivo o tarjeta con validación Luhn + CVV) " +
                "en una sola operación. Descuenta el stock al cobrar.");
            raiz.Add(ventas);

            var compras = new NodoAyuda_GO44("Compras",
                "Reposición de stock: cotizaciones, órdenes de compra, pago y recepción (RFN2).");
            compras.Agregar("Cotizaciones",
                "Generá una solicitud de cotización (SC) eligiendo componentes y un proveedor. " +
                "Si el proveedor no existe, registralo desde el mismo paso.");
            compras.Agregar("Orden de Compra",
                "Generá la orden de compra a partir de una SC: cargá el precio unitario cotizado y la fecha de entrega. " +
                "Desde acá también podés pagar la orden con el botón 'Pagar orden'.");
            compras.Agregar("Pagar orden",
                "Registra el pago al proveedor con sus datos bancarios. El N° de comprobante se genera solo. " +
                "La orden pasa a 'Pagada' y se puede exportar el comprobante en PDF.");
            compras.Agregar("Recepción",
                "El almacenista registra la cantidad recibida de cada componente. El stock sube y, si falta algo, " +
                "la orden queda 'Recibida con faltantes'.");
            raiz.Add(compras);

            var reportes = new NodoAyuda_GO44("Reportes",
                "Informes del sistema.");
            reportes.Agregar("Rep. 1 (RFN1) — Facturas",
                "Ventas por período con filtros de fecha, cliente y estado. Exporta a PDF.");
            reportes.Agregar("Rep. 2 (RFN2) — Compras",
                "Recomposición de componentes: órdenes de compra con pedido vs recibido, proveedor y pago. Exporta a PDF.");
            reportes.Agregar("Rep. 3 (INT.) — Inteligente",
                "Estrategia de ventas: rotación, días de inventario, reposición urgente y stock muerto. " +
                "Incluye gráfico de más vendidos y plan de combos/promociones.");
            raiz.Add(reportes);

            var admin = new NodoAyuda_GO44("Administración",
                "Funciones de administración del sistema.");
            admin.Agregar("Gestión de usuarios",
                "Alta, modificación, bloqueo/desbloqueo y activación de usuarios.");
            admin.Agregar("Gestión de perfiles",
                "Asignación de familias y patentes a los roles (permisos, patrón Composite).");
            admin.Agregar("Eventos (bitácora)",
                "Auditoría de las operaciones del sistema, con filtros y export a PDF.");
            admin.Agregar("Respaldos",
                "Backup y restore de la base de datos. Backups automáticos cada 3 horas.");
            raiz.Add(admin);

            return raiz;
        }

        /// <summary>
        /// Ayuda contextual de un formulario (para F1). Devuelve el título del tema que mejor aplica.
        /// </summary>
        public static string TemaDeFormulario(string nombreFormulario)
        {
            if (string.IsNullOrEmpty(nombreFormulario)) return "General";
            string n = nombreFormulario.ToLower();
            if (n.Contains("productoscambios"))  return "Productos - C";
            if (n.Contains("gestionproductos"))  return "Productos";
            if (n.Contains("gestionclientes"))   return "Clientes";
            if (n.Contains("registrarproveedor"))return "Proveedores";
            if (n.Contains("cargarcarrito"))     return "Cargar carrito";
            if (n.Contains("facturar"))          return "Facturar";
            if (n.Contains("solicitudcotizacion")) return "Cotizaciones";
            if (n.Contains("generarordencompra"))return "Orden de Compra";
            if (n.Contains("pagarordencompra"))  return "Pagar orden";
            if (n.Contains("recepcion"))         return "Recepción";
            if (n.Contains("reportefacturas"))   return "Rep. 1 (RFN1) — Facturas";
            if (n.Contains("reportecompras"))    return "Rep. 2 (RFN2) — Compras";
            if (n.Contains("reporteinteligente") || n.Contains("promociones")) return "Rep. 3 (INT.) — Inteligente";
            if (n.Contains("gestionusuarios"))   return "Gestión de usuarios";
            if (n.Contains("gestionpermisos"))   return "Gestión de perfiles";
            if (n.Contains("bitacora"))          return "Eventos (bitácora)";
            if (n.Contains("backup"))            return "Respaldos";
            return "General";
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
            string resumen = (nodo.Contenido ?? "").Replace("\r\n", " ");
            if (resumen.Length > 110) resumen = resumen.Substring(0, 107) + "...";
            filas.Add(new string[] { sangria + prefijo + nodo.Titulo, resumen });
            foreach (var hijo in nodo.Hijos)
                AgregarFila(filas, hijo, nivel + 1);
        }
    }
}
