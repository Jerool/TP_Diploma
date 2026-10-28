namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMMenuPrincipal_GO44
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();

            // ADMIN
            this.menuAdmin = new System.Windows.Forms.ToolStripMenuItem();
            this.miUsuarios = new System.Windows.Forms.ToolStripMenuItem();
            this.miPermisos = new System.Windows.Forms.ToolStripMenuItem();
            this.miBitacora = new System.Windows.Forms.ToolStripMenuItem();
            this.miBackup = new System.Windows.Forms.ToolStripMenuItem();

            // MAESTROS
            this.menuMaestros = new System.Windows.Forms.ToolStripMenuItem();
            this.miClientes = new System.Windows.Forms.ToolStripMenuItem();
            this.miProductos = new System.Windows.Forms.ToolStripMenuItem();
            this.miProductosCambios = new System.Windows.Forms.ToolStripMenuItem();
            this.miRegistrarProveedor = new System.Windows.Forms.ToolStripMenuItem();

            // VENTAS
            this.menuVentas = new System.Windows.Forms.ToolStripMenuItem();
            this.miCargarCarrito = new System.Windows.Forms.ToolStripMenuItem();
            this.miFacturar = new System.Windows.Forms.ToolStripMenuItem();

            // REPORTES
            this.menuReportes = new System.Windows.Forms.ToolStripMenuItem();
            this.miReporteFacturas = new System.Windows.Forms.ToolStripMenuItem();
            this.miReporteCompras = new System.Windows.Forms.ToolStripMenuItem();
            this.miReporteInteligente = new System.Windows.Forms.ToolStripMenuItem();

            // COMPRAS (RFN2 — Cotizaciones + Orden de Compra + Recepción)
            this.menuCompras = new System.Windows.Forms.ToolStripMenuItem();
            this.miGenerarSC = new System.Windows.Forms.ToolStripMenuItem();
            this.miGenerarOC = new System.Windows.Forms.ToolStripMenuItem();
            this.miRecibirMercaderia = new System.Windows.Forms.ToolStripMenuItem();

            // USUARIO
            this.menuUsuario = new System.Windows.Forms.ToolStripMenuItem();
            this.miCambiarClave = new System.Windows.Forms.ToolStripMenuItem();
            this.miLogout = new System.Windows.Forms.ToolStripMenuItem();

            // AYUDA (ayuda en línea — menú escalera + contextual)
            this.menuAyuda = new System.Windows.Forms.ToolStripMenuItem();
            this.miAyudaIndice = new System.Windows.Forms.ToolStripMenuItem();
            this.miAyudaMaestros = new System.Windows.Forms.ToolStripMenuItem();
            this.miAyudaVentas = new System.Windows.Forms.ToolStripMenuItem();
            this.miAyudaCompras = new System.Windows.Forms.ToolStripMenuItem();
            this.miAyudaVisita = new System.Windows.Forms.ToolStripMenuItem();

            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.lblUsuarioActual = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlContenido = new System.Windows.Forms.Panel();

            this.menuStrip1.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();

            // MenuStrip azul
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.menuStrip1.ForeColor = System.Drawing.Color.White;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuAdmin, this.menuMaestros, this.menuUsuario, this.menuVentas, this.menuCompras, this.menuReportes, this.menuAyuda});
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 4, 0, 4);
            // Tooltips contextuales (aparecen en la posición del mouse al pasar por los menús).
            this.menuStrip1.ShowItemToolTips = true;

            // ADMIN
            ConfigMenuPadre(this.menuAdmin, "Admin");
            this.menuAdmin.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miUsuarios, this.miPermisos, this.miBitacora, this.miBackup});
            ConfigMenuHijo(this.miUsuarios, "Usuarios",             this.miUsuarios_Click);
            ConfigMenuHijo(this.miPermisos, "Gestión de permisos",  this.miPermisos_Click);
            ConfigMenuHijo(this.miBitacora, "Bitácora de eventos",  this.miBitacora_Click);
            ConfigMenuHijo(this.miBackup,   "Backup y restore",     this.miBackup_Click);

            // MAESTROS
            ConfigMenuPadre(this.menuMaestros, "Maestros");
            this.menuMaestros.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miProductos, this.miClientes, this.miProductosCambios, this.miRegistrarProveedor});
            ConfigMenuHijo(this.miProductos, "Productos", this.miProductos_Click);
            ConfigMenuHijo(this.miClientes,  "Clientes",  this.miClientes_Click);
            ConfigMenuHijo(this.miProductosCambios, "Productos - C", this.miProductosCambios_Click);
            ConfigMenuHijo(this.miRegistrarProveedor, "Proveedores", this.miRegistrarProveedor_Click);

            // VENTAS
            ConfigMenuPadre(this.menuVentas, "Ventas");
            this.menuVentas.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miCargarCarrito, this.miFacturar});
            ConfigMenuHijo(this.miCargarCarrito, "Cargar carrito",   this.miCargarCarrito_Click);
            ConfigMenuHijo(this.miFacturar,      "Generar factura",  this.miFacturar_Click);

            // REPORTES
            ConfigMenuPadre(this.menuReportes, "Reportes");
            this.menuReportes.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miReporteFacturas, this.miReporteCompras, this.miReporteInteligente});
            ConfigMenuHijo(this.miReporteFacturas,   "Rep. 1 (RFN1)", this.miReporteFacturas_Click);
            ConfigMenuHijo(this.miReporteCompras,    "Rep. 2 (RFN2)", this.miReporteCompras_Click);
            ConfigMenuHijo(this.miReporteInteligente,"Rep. 3 (INT.)", this.miReporteInteligente_Click);

            // COMPRAS (RFN2 — Cotizaciones + Orden de Compra + Recepción)
            ConfigMenuPadre(this.menuCompras, "Compras");
            this.menuCompras.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miGenerarSC, this.miGenerarOC, this.miRecibirMercaderia});
            ConfigMenuHijo(this.miGenerarSC,         "Cotizaciones",            this.miGenerarSC_Click);
            ConfigMenuHijo(this.miGenerarOC,         "Orden de Compra",         this.miGenerarOC_Click);
            ConfigMenuHijo(this.miRecibirMercaderia, "Recepción de mercadería", this.miRecibirMercaderia_Click);

            // USUARIO
            ConfigMenuPadre(this.menuUsuario, "Usuario");
            this.menuUsuario.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miCambiarClave, this.miLogout});
            ConfigMenuHijo(this.miCambiarClave, "Cambiar clave", this.miCambiarClave_Click);
            ConfigMenuHijo(this.miLogout,       "Cerrar sesión", this.miLogout_Click);

            // AYUDA (menú escalera + contextual)
            ConfigMenuPadre(this.menuAyuda, "Ayuda");
            this.menuAyuda.ToolTipText = "Ayuda en línea (F1 para la ayuda de la pantalla abierta)";
            this.menuAyuda.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miAyudaIndice, this.miAyudaVisita, this.miAyudaMaestros, this.miAyudaVentas, this.miAyudaCompras});
            ConfigMenuHijo(this.miAyudaIndice,   "Índice de ayuda", this.miAyudaIndice_Click);
            ConfigMenuHijo(this.miAyudaVisita,   "Visita guiada",   this.miAyudaVisita_Click);
            ConfigMenuHijo(this.miAyudaMaestros, "Maestros",        this.miAyudaMaestros_Click);
            ConfigMenuHijo(this.miAyudaVentas,   "Ventas",          this.miAyudaVentas_Click);
            ConfigMenuHijo(this.miAyudaCompras,  "Compras",         this.miAyudaCompras_Click);

            // Panel contenido
            this.pnlContenido.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.pnlContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContenido.Location = new System.Drawing.Point(0, 32);
            this.pnlContenido.Name = "pnlContenido";

            // Status bar
            this.statusBar.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.lblUsuarioActual });
            this.lblUsuarioActual.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblUsuarioActual.Text = "Sesión: (no iniciada)";

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.pnlContenido);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FRMMenuPrincipal_GO44";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Menú Principal — TechFlow";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FRMMenuPrincipal_GO44_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ConfigMenuPadre(System.Windows.Forms.ToolStripMenuItem mi, string texto)
        {
            mi.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            mi.ForeColor = System.Drawing.Color.White;
            mi.Text = texto;
        }

        private void ConfigMenuHijo(System.Windows.Forms.ToolStripMenuItem mi, string texto, System.EventHandler onClick)
        {
            mi.BackColor = System.Drawing.Color.White;
            mi.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            mi.Text = texto;
            mi.Click += onClick;
        }
        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;

        private System.Windows.Forms.ToolStripMenuItem menuAdmin;
        private System.Windows.Forms.ToolStripMenuItem miUsuarios;
        private System.Windows.Forms.ToolStripMenuItem miPermisos;
        private System.Windows.Forms.ToolStripMenuItem miBitacora;
        private System.Windows.Forms.ToolStripMenuItem miBackup;

        private System.Windows.Forms.ToolStripMenuItem menuMaestros;
        private System.Windows.Forms.ToolStripMenuItem miClientes;
        private System.Windows.Forms.ToolStripMenuItem miProductos;
        private System.Windows.Forms.ToolStripMenuItem miProductosCambios;
        private System.Windows.Forms.ToolStripMenuItem miRegistrarProveedor;

        private System.Windows.Forms.ToolStripMenuItem menuVentas;
        private System.Windows.Forms.ToolStripMenuItem miCargarCarrito;
        private System.Windows.Forms.ToolStripMenuItem miFacturar;

        private System.Windows.Forms.ToolStripMenuItem menuReportes;
        private System.Windows.Forms.ToolStripMenuItem miReporteFacturas;
        private System.Windows.Forms.ToolStripMenuItem miReporteCompras;
        private System.Windows.Forms.ToolStripMenuItem miReporteInteligente;

        private System.Windows.Forms.ToolStripMenuItem menuCompras;
        private System.Windows.Forms.ToolStripMenuItem miGenerarSC;
        private System.Windows.Forms.ToolStripMenuItem miGenerarOC;
        private System.Windows.Forms.ToolStripMenuItem miRecibirMercaderia;

        private System.Windows.Forms.ToolStripMenuItem menuUsuario;
        private System.Windows.Forms.ToolStripMenuItem miCambiarClave;
        private System.Windows.Forms.ToolStripMenuItem miLogout;

        private System.Windows.Forms.ToolStripMenuItem menuAyuda;
        private System.Windows.Forms.ToolStripMenuItem miAyudaIndice;
        private System.Windows.Forms.ToolStripMenuItem miAyudaMaestros;
        private System.Windows.Forms.ToolStripMenuItem miAyudaVentas;
        private System.Windows.Forms.ToolStripMenuItem miAyudaCompras;
        private System.Windows.Forms.ToolStripMenuItem miAyudaVisita;

        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel lblUsuarioActual;
        private System.Windows.Forms.Panel pnlContenido;
    }
}
