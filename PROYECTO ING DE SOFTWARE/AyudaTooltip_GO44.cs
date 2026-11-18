using Servicios;
using System;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// Ayuda contextual "con posición de mouse": recorre los controles de una pantalla y les pone
    /// un tooltip (globo de ayuda que aparece al lado del cursor al pasar por encima). El texto sale
    /// de pistas por nombre de control; si no hay pista específica, el control no recibe tooltip
    /// (para no generar ruido). Se aplica de forma centralizada al abrir cada formulario hijo, así
    /// cubre TODAS las pantallas sin tocarlas una por una. Complementa a la ayuda F1 (AyudaManager).
    /// </summary>
    public static class AyudaTooltip_GO44
    {
        // Pistas por NOMBRE de control (subcadena en minúsculas) -> CLAVE de traducción.
        // Gana la primera que coincide, así que van de lo más específico a lo más genérico.
        // El texto final se resuelve con IdiomaManager (es/en) al abrir la pantalla.
        private static readonly string[][] PISTAS =
        {
            new[] { "guardar",     "tip.ctl.guardar" },
            new[] { "aceptar",     "tip.ctl.aceptar" },
            new[] { "confirmar",   "tip.ctl.confirmar" },
            new[] { "cancelar",    "tip.ctl.cancelar" },
            new[] { "salir",       "tip.ctl.cerrar" },
            new[] { "cerrar",      "tip.ctl.cerrar" },
            new[] { "volver",      "tip.ctl.volver" },
            new[] { "buscar",      "tip.ctl.buscar" },
            new[] { "filtrar",     "tip.ctl.filtrar" },
            new[] { "limpiar",     "tip.ctl.limpiar" },
            new[] { "exportar",    "tip.ctl.exportar" },
            new[] { "imprimir",    "tip.ctl.imprimir" },
            new[] { "pdf",         "tip.ctl.exportar" },
            new[] { "importar",    "tip.ctl.importar" },
            new[] { "agregar",     "tip.ctl.agregar" },
            new[] { "añadir",      "tip.ctl.agregar" },
            new[] { "anadir",      "tip.ctl.agregar" },
            new[] { "quitar",      "tip.ctl.quitar" },
            new[] { "eliminar",    "tip.ctl.eliminar" },
            new[] { "baja",        "tip.ctl.baja" },
            new[] { "modificar",   "tip.ctl.editar" },
            new[] { "editar",      "tip.ctl.editar" },
            new[] { "nuevo",       "tip.ctl.nuevo" },
            new[] { "alta",        "tip.ctl.nuevo" },
            new[] { "facturar",    "tip.ctl.facturar" },
            new[] { "cobrar",      "tip.ctl.cobrar" },
            new[] { "pagar",       "tip.ctl.pagar" },
            new[] { "recibir",     "tip.ctl.recibir" },
            new[] { "recepcion",   "tip.ctl.recibir" },
            new[] { "cotiz",       "tip.ctl.cotizar" },
            new[] { "orden",       "tip.ctl.orden" },
            new[] { "bloquear",    "tip.ctl.bloquear" },
            new[] { "desbloquear", "tip.ctl.bloquear" },
            new[] { "activar",     "tip.ctl.activar" },
            new[] { "idioma",      "tip.ctl.idioma" },
            // Campos de entrada
            new[] { "usuario",     "tip.ctl.usuario" },
            new[] { "login",       "tip.ctl.usuario" },
            new[] { "contrase",    "tip.ctl.password" },
            new[] { "password",    "tip.ctl.password" },
            new[] { "dni",         "tip.ctl.dni" },
            new[] { "email",       "tip.ctl.email" },
            new[] { "correo",      "tip.ctl.email" },
            new[] { "telefono",    "tip.ctl.telefono" },
            new[] { "cuit",        "tip.ctl.cuit" },
            new[] { "cvu",         "tip.ctl.cvu" },
            new[] { "banco",       "tip.ctl.banco" },
            new[] { "titular",     "tip.ctl.titular" },
            new[] { "tarjeta",     "tip.ctl.tarjeta" },
            new[] { "cvv",         "tip.ctl.cvv" },
            new[] { "precio",      "tip.ctl.precio" },
            new[] { "cantidad",    "tip.ctl.cantidad" },
            new[] { "stock",       "tip.ctl.stock" },
            new[] { "existencia",  "tip.ctl.stock" },
            new[] { "fecha",       "tip.ctl.fecha" },
            new[] { "codigo",      "tip.ctl.codigo" },
            new[] { "nombre",      "tip.ctl.nombre" },
            new[] { "categoria",   "tip.ctl.categoria" },
            new[] { "cliente",     "tip.ctl.cliente" },
            new[] { "proveedor",   "tip.ctl.proveedor" },
        };

        /// <summary>
        /// Aplica tooltips de ayuda a todos los controles accionables / de entrada de la pantalla.
        /// Agrega abajo de todo un recordatorio de que F1 abre la ayuda completa de la pantalla.
        /// </summary>
        public static void Aplicar(Form form)
        {
            if (form == null) return;

            var tip = new ToolTip
            {
                IsBalloon = true,        // globo al lado del mouse
                ShowAlways = true,
                InitialDelay = 400,
                ReshowDelay = 150,
                AutoPopDelay = 9000,
                ToolTipTitle = "Ayuda"
            };

            // Mantener vivo el ToolTip mientras viva el form y liberarlo al cerrarlo.
            form.Disposed += (s, e) => tip.Dispose();

            AplicarEnControles(form.Controls, tip);
        }

        private static void AplicarEnControles(Control.ControlCollection controles, ToolTip tip)
        {
            foreach (Control c in controles)
            {
                string texto = Resolver(c);
                if (texto != null)
                    tip.SetToolTip(c, texto);

                if (c.HasChildren)
                    AplicarEnControles(c.Controls, tip);
            }
        }

        private static string Resolver(Control c)
        {
            // Solo controles con los que el usuario interactúa.
            bool accionable =
                c is Button || c is TextBox || c is ComboBox || c is CheckBox ||
                c is RadioButton || c is DateTimePicker || c is NumericUpDown ||
                c is DataGridView || c is ListBox || c is MaskedTextBox;
            if (!accionable) return null;

            string nombre = (c.Name ?? string.Empty).ToLowerInvariant();
            foreach (var par in PISTAS)
                if (nombre.IndexOf(par[0], StringComparison.Ordinal) >= 0)
                    return IdiomaManager_GO44.T(par[1]);

            if (c is DataGridView)
                return IdiomaManager_GO44.T("tip.ctl.grilla");

            // Fallback: TODO botón / casilla sin pista específica igual recibe tooltip, usando su
            // propio texto visible (ya traducido por el form). Así ningún botón queda sin ayuda.
            if (c is Button || c is CheckBox || c is RadioButton)
            {
                string txt = LimpiarTexto(c.Text);
                return txt.Length > 0 ? txt : IdiomaManager_GO44.T("tip.ctl.accion");
            }

            return null; // otros controles sin pista -> sin tooltip (para no hacer ruido)
        }

        /// <summary>Quita mnemónicos (&amp;) y símbolos/íconos iniciales del texto de un control.</summary>
        private static string LimpiarTexto(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            s = s.Replace("&", string.Empty);
            int i = 0;
            while (i < s.Length && !char.IsLetterOrDigit(s[i])) i++;
            return s.Substring(i).Trim();
        }
    }
}
