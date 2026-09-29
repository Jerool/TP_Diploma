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

            // VENTAS
            this.menuVentas = new System.Windows.Forms.ToolStripMenuItem();
            this.miCargarCarrito = new System.Windows.Forms.ToolStripMenuItem();
            this.miFacturar = new System.Windows.Forms.ToolStripMenuItem();

            // USUARIO
            this.menuUsuario = new System.Windows.Forms.ToolStripMenuItem();
            this.miCambiarClave = new System.Windows.Forms.ToolStripMenuItem();
            this.miLogout = new System.Windows.Forms.ToolStripMenuItem();

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
                this.menuAdmin, this.menuMaestros, this.menuVentas, this.menuUsuario});
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 4, 0, 4);

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
                this.miClientes, this.miProductos});
            ConfigMenuHijo(this.miClientes,  "Clientes",  this.miClientes_Click);
            ConfigMenuHijo(this.miProductos, "Productos", this.miProductos_Click);

            // VENTAS
            ConfigMenuPadre(this.menuVentas, "Ventas");
            this.menuVentas.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miCargarCarrito, this.miFacturar});
            ConfigMenuHijo(this.miCargarCarrito, "Cargar carrito",   this.miCargarCarrito_Click);
            ConfigMenuHijo(this.miFacturar,      "Generar factura",  this.miFacturar_Click);

            // USUARIO
            ConfigMenuPadre(this.menuUsuario, "Usuario");
            this.menuUsuario.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miCambiarClave, this.miLogout});
            ConfigMenuHijo(this.miCambiarClave, "Cambiar clave", this.miCambiarClave_Click);
            ConfigMenuHijo(this.miLogout,       "Cerrar sesión", this.miLogout_Click);

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

        private System.Windows.Forms.ToolStripMenuItem menuVentas;
        private System.Windows.Forms.ToolStripMenuItem miCargarCarrito;
        private System.Windows.Forms.ToolStripMenuItem miFacturar;

        private System.Windows.Forms.ToolStripMenuItem menuUsuario;
        private System.Windows.Forms.ToolStripMenuItem miCambiarClave;
        private System.Windows.Forms.ToolStripMenuItem miLogout;

        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel lblUsuarioActual;
        private System.Windows.Forms.Panel pnlContenido;
    }
}
