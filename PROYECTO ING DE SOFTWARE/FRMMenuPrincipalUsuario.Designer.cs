namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMMenuPrincipalUsuario
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.usuarioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cambiarClaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.maestrosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clientesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.productosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ventasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.carritoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.lblUsuarioActual = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlContenido = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip1 — barra de menú azul arriba
            //
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.menuStrip1.ForeColor = System.Drawing.Color.White;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.usuarioToolStripMenuItem,
            this.maestrosToolStripMenuItem,
            this.ventasToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 4, 0, 4);
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(800, 32);
            this.menuStrip1.TabIndex = 0;
            //
            // usuarioToolStripMenuItem — única opción del menú para el rol Usuario común
            //
            this.usuarioToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.usuarioToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cambiarClaveToolStripMenuItem,
            this.logOutToolStripMenuItem});
            this.usuarioToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.usuarioToolStripMenuItem.Name = "usuarioToolStripMenuItem";
            this.usuarioToolStripMenuItem.Size = new System.Drawing.Size(69, 24);
            this.usuarioToolStripMenuItem.Text = "Usuario";
            //
            // cambiarClaveToolStripMenuItem
            //
            this.cambiarClaveToolStripMenuItem.BackColor = System.Drawing.Color.White;
            this.cambiarClaveToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.cambiarClaveToolStripMenuItem.Name = "cambiarClaveToolStripMenuItem";
            this.cambiarClaveToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.cambiarClaveToolStripMenuItem.Text = "Cambiar Clave";
            this.cambiarClaveToolStripMenuItem.Click += new System.EventHandler(this.cambiarClaveToolStripMenuItem_Click);
            //
            // logOutToolStripMenuItem
            //
            this.logOutToolStripMenuItem.BackColor = System.Drawing.Color.White;
            this.logOutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.logOutToolStripMenuItem.Name = "logOutToolStripMenuItem";
            this.logOutToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.logOutToolStripMenuItem.Text = "LogOut";
            this.logOutToolStripMenuItem.Click += new System.EventHandler(this.logOutToolStripMenuItem_Click);
            //
            // maestrosToolStripMenuItem
            //
            this.maestrosToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.maestrosToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clientesToolStripMenuItem,
            this.productosToolStripMenuItem});
            this.maestrosToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.maestrosToolStripMenuItem.Name = "maestrosToolStripMenuItem";
            this.maestrosToolStripMenuItem.Size = new System.Drawing.Size(85, 24);
            this.maestrosToolStripMenuItem.Text = "Maestros";
            //
            // clientesToolStripMenuItem
            //
            this.clientesToolStripMenuItem.BackColor = System.Drawing.Color.White;
            this.clientesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.clientesToolStripMenuItem.Name = "clientesToolStripMenuItem";
            this.clientesToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.clientesToolStripMenuItem.Text = "Clientes";
            this.clientesToolStripMenuItem.Click += new System.EventHandler(this.clientesToolStripMenuItem_Click);
            //
            // productosToolStripMenuItem
            //
            this.productosToolStripMenuItem.BackColor = System.Drawing.Color.White;
            this.productosToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.productosToolStripMenuItem.Name = "productosToolStripMenuItem";
            this.productosToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.productosToolStripMenuItem.Text = "Productos";
            this.productosToolStripMenuItem.Click += new System.EventHandler(this.productosToolStripMenuItem_Click);
            //
            // ventasToolStripMenuItem
            //
            this.ventasToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.ventasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.carritoToolStripMenuItem});
            this.ventasToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.ventasToolStripMenuItem.Name = "ventasToolStripMenuItem";
            this.ventasToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            this.ventasToolStripMenuItem.Text = "Ventas";
            //
            // carritoToolStripMenuItem
            //
            this.carritoToolStripMenuItem.BackColor = System.Drawing.Color.White;
            this.carritoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.carritoToolStripMenuItem.Name = "carritoToolStripMenuItem";
            this.carritoToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.carritoToolStripMenuItem.Text = "Cargar Carrito";
            this.carritoToolStripMenuItem.Click += new System.EventHandler(this.carritoToolStripMenuItem_Click);
            //
            // pnlContenido — panel central donde se hostean los formularios hijos.
            // Igual que en MenuPrincipalAdmin, esto reemplaza el modelo MDI viejo
            // para que los hijos se vean integrados al form padre, sin chrome propio.
            //
            this.pnlContenido.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.pnlContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContenido.Location = new System.Drawing.Point(0, 32);
            this.pnlContenido.Name = "pnlContenido";
            this.pnlContenido.Size = new System.Drawing.Size(800, 396);
            this.pnlContenido.TabIndex = 2;
            //
            // statusBar
            //
            this.statusBar.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblUsuarioActual});
            this.statusBar.Location = new System.Drawing.Point(0, 428);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(800, 22);
            this.statusBar.TabIndex = 1;
            //
            // lblUsuarioActual
            //
            this.lblUsuarioActual.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblUsuarioActual.Name = "lblUsuarioActual";
            this.lblUsuarioActual.Size = new System.Drawing.Size(118, 17);
            this.lblUsuarioActual.Text = "Sesión: (no iniciada)";
            //
            // FRMMenuPrincipalUsuario — IsMdiContainer = false, usamos pnlContenido en su lugar.
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pnlContenido);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FRMMenuPrincipalUsuario";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Menú Principal — Usuario";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FRMMenuPrincipalUsuario_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem usuarioToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cambiarClaveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem maestrosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clientesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem productosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ventasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem carritoToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel lblUsuarioActual;
        private System.Windows.Forms.Panel pnlContenido;
    }
}
