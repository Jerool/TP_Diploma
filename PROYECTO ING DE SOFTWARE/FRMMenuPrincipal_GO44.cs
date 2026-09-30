using BLL;
using Servicios;
using System;
using System.Linq;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// Menú Principal UNIFICADO — reemplaza a FRMMenuPrincipalAdmin y FRMMenuPrincipalUsuario.
    /// Cada item del menú se muestra/oculta según las patentes (DataKeys) del rol logueado,
    /// usando el Composite pre-existente: Rol → Familia → Patente + rol.TienePermiso("clave").
    /// </summary>
    public partial class FRMMenuPrincipal_GO44 : Form, IObservadorIdioma_GO44
    {
        private Form _formularioActual = null;
        private readonly BLLUsuario_GO44 _bllUsuario;
        private Rol_GO44 _rolActualCompleto;

        private ToolStripMenuItem _menuIdioma;
        private ToolStripMenuItem _itemEspanol;
        private ToolStripMenuItem _itemIngles;

        public FRMMenuPrincipal_GO44()
        {
            InitializeComponent();
            _bllUsuario = new BLLUsuario_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);

            ConstruirMenuIdioma();
        }

        private void FRMMenuPrincipal_GO44_Load(object sender, EventArgs e)
        {
            Usuario_GO44 actual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (actual != null)
            {
                lblUsuarioActual.Text = "Sesión: " + actual.Nombre + " " + actual.Apellido +
                                        " (" + actual.Login + ")  ·  Rol: " +
                                        (actual.Rol != null ? actual.Rol.Nombre : "s/rol");

                // Cargar el árbol completo del rol para poder consultar patentes
                if (actual.Rol != null)
                {
                    var bllPermisos = new BLLPermisos_GO44();
                    _rolActualCompleto = bllPermisos.ObtenerArbolRol(actual.Rol.Id);
                }
            }

            ActualizarIdioma();
            AplicarPermisos();
        }

        /// <summary>
        /// Setea la visibilidad de cada item del menú según las patentes del rol.
        /// También setea directamente la visibilidad de los menús padre (más robusto
        /// que "ocultar si sin hijos" porque WinForms tiene issues con Visible dinámico).
        /// </summary>
        private void AplicarPermisos()
        {
            // Fallback para admins: si el nombre del rol CONTIENE "admin" ve TODO.
            bool esAdmin = false;
            Usuario_GO44 u = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (u != null && u.Rol != null && !string.IsNullOrEmpty(u.Rol.Nombre))
            {
                string n = u.Rol.Nombre.Trim().ToLower();
                esAdmin = n.Contains("admin");
            }

            // ---------- ADMIN ----------
            bool puedeUsuarios  = esAdmin || TienePermiso("Admin.Usuarios");
            bool puedePermisos  = esAdmin || TienePermiso("Admin.Permisos");
            bool puedeBitacora  = esAdmin || TienePermiso("Admin.Bitacora");
            bool puedeBackup    = esAdmin || TienePermiso("Admin.Backup");
            miUsuarios.Visible  = puedeUsuarios;
            miPermisos.Visible  = puedePermisos;
            miBitacora.Visible  = puedeBitacora;
            miBackup.Visible    = puedeBackup;
            menuAdmin.Visible   = puedeUsuarios || puedePermisos || puedeBitacora || puedeBackup;

            // ---------- MAESTROS ----------
            bool puedeClientes  = esAdmin || TienePermiso("Maestros.Clientes");
            bool puedeProductos = esAdmin || TienePermiso("Maestros.Productos");
            miClientes.Visible  = puedeClientes;
            miProductos.Visible = puedeProductos;
            menuMaestros.Visible = puedeClientes || puedeProductos;

            // ---------- VENTAS ----------
            bool puedeCarrito  = esAdmin || TienePermiso("Ventas.CargarCarrito");
            bool puedeFacturar = esAdmin || TienePermiso("Ventas.Facturar");
            miCargarCarrito.Visible = puedeCarrito;
            miFacturar.Visible      = puedeFacturar;
            menuVentas.Visible = puedeCarrito || puedeFacturar;

            // ---------- REPORTES ----------
            bool puedeRepFactura = esAdmin || TienePermiso("Reportes.Facturas") || TienePermiso("Ventas.Facturar");
            miReporteFacturas.Visible = puedeRepFactura;
            menuReportes.Visible = puedeRepFactura;

            // ---------- USUARIO (siempre visible) ----------
            miCambiarClave.Visible = true;
            miLogout.Visible       = true;
            menuUsuario.Visible    = true;

            // Forzar refresh del layout — sino WinForms a veces no re-dibuja al cambiar Visible
            menuStrip1.PerformLayout();
            menuStrip1.Refresh();
        }

        private bool TienePermiso(string dataKey)
        {
            if (_rolActualCompleto == null) return false;
            return _rolActualCompleto.TienePermiso(dataKey);
        }

        private static void OcultarSiSinHijos(ToolStripMenuItem menuPadre)
        {
            if (menuPadre == null) return;
            bool algunoVisible = false;
            foreach (ToolStripItem child in menuPadre.DropDownItems)
            {
                if (child.Visible) { algunoVisible = true; break; }
            }
            menuPadre.Visible = algunoVisible;
        }

        // ============ IDIOMA ============

        private void ConstruirMenuIdioma()
        {
            MenuStrip menu = this.Controls.OfType<MenuStrip>().FirstOrDefault();
            if (menu == null) return;

            _itemEspanol = new ToolStripMenuItem("Español");
            _itemEspanol.Click += (s, e) => _bllUsuario.CambiarIdioma(IdiomaManager_GO44.ES);

            _itemIngles = new ToolStripMenuItem("English");
            _itemIngles.Click += (s, e) => _bllUsuario.CambiarIdioma(IdiomaManager_GO44.EN);

            _menuIdioma = new ToolStripMenuItem("Idioma");
            _menuIdioma.DropDownItems.Add(_itemEspanol);
            _menuIdioma.DropDownItems.Add(_itemIngles);
            _menuIdioma.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            _menuIdioma.ForeColor = System.Drawing.Color.White;

            menu.Items.Add(_menuIdioma);
        }

        public void ActualizarIdioma()
        {
            if (menuAdmin != null)    menuAdmin.Text    = IdiomaManager_GO44.T("menu.admin");
            if (menuMaestros != null) menuMaestros.Text = "Maestros";
            if (menuVentas != null)   menuVentas.Text   = "Ventas";
            if (menuUsuario != null)  menuUsuario.Text  = IdiomaManager_GO44.T("menu.usuario");

            if (_menuIdioma != null)  _menuIdioma.Text  = IdiomaManager_GO44.T("menu.idioma");
            if (_itemEspanol != null) _itemEspanol.Text = IdiomaManager_GO44.T("general.espanol");
            if (_itemIngles != null)  _itemIngles.Text  = IdiomaManager_GO44.T("general.ingles");
        }

        // ============ HOSTING DE FORMS HIJOS ============

        private void AbrirFormularioHijo(Form f)
        {
            if (_formularioActual != null && _formularioActual.GetType() == f.GetType())
            {
                _formularioActual.Close();
                _formularioActual = null;
                return;
            }
            if (_formularioActual != null) { _formularioActual.Close(); _formularioActual = null; }

            f.TopLevel = false;
            f.FormBorderStyle = FormBorderStyle.None;
            f.Dock = DockStyle.Fill;
            pnlContenido.Controls.Clear();
            pnlContenido.Controls.Add(f);
            f.Show();
            _formularioActual = f;
        }

        // ============ HANDLERS ADMIN ============

        private void miUsuarios_Click(object sender, EventArgs e)   { AbrirFormularioHijo(new FRMGestionUsuariosAdmin()); }
        private void miPermisos_Click(object sender, EventArgs e)   { AbrirFormularioHijo(new FRMGestionPermisos()); }
        private void miBitacora_Click(object sender, EventArgs e)   { AbrirFormularioHijo(new FRMBitacoraDeEventos()); }
        private void miBackup_Click(object sender, EventArgs e)     { AbrirFormularioHijo(new FRMBackupManual()); }

        // ============ HANDLERS MAESTROS ============

        private void miClientes_Click(object sender, EventArgs e)   { AbrirFormularioHijo(new FRMGestionClientes_GO44()); }
        private void miProductos_Click(object sender, EventArgs e)  { AbrirFormularioHijo(new FRMGestionProductos_GO44()); }

        // ============ HANDLERS VENTAS ============

        private void miCargarCarrito_Click(object sender, EventArgs e) { AbrirFormularioHijo(new FRMCargarCarrito_GO44()); }
        private void miFacturar_Click(object sender, EventArgs e)      { AbrirFormularioHijo(new FRMFacturar_GO44()); }

        // ============ HANDLERS REPORTES ============
        private void miReporteFacturas_Click(object sender, EventArgs e) { AbrirFormularioHijo(new FRMReporteFacturas_GO44()); }

        // ============ HANDLERS USUARIO ============

        private void miCambiarClave_Click(object sender, EventArgs e)
        {
            AbrirFormularioHijo(new FRMCambiarContrasenia());
        }

        private void miLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                IdiomaManager_GO44.T("menu.confirmarLogout"),
                IdiomaManager_GO44.T("menu.tituloLogout"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                BLLUsuario_GO44.CerrarSesión();
                FRMIniciarSesion frm = new FRMIniciarSesion();
                frm.Show();
                this.Close();
            }
        }
    }
}
