namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMAyuda_GO44
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
            this.lblTitulo = new System.Windows.Forms.Label();
            this.tvTemas = new System.Windows.Forms.TreeView();
            this.grpContenido = new System.Windows.Forms.GroupBox();
            this.lblTema = new System.Windows.Forms.Label();
            this.txtContenido = new System.Windows.Forms.TextBox();
            this.btnExportar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();

            this.grpContenido.SuspendLayout();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Ayuda en línea";

            // Árbol de temas (menú escalera)
            this.tvTemas.Location = new System.Drawing.Point(20, 50);
            this.tvTemas.Size = new System.Drawing.Size(340, 540);
            this.tvTemas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.tvTemas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tvTemas.HideSelection = false;
            this.tvTemas.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvTemas_AfterSelect);

            // Contenido
            this.grpContenido.Location = new System.Drawing.Point(375, 50);
            this.grpContenido.Size = new System.Drawing.Size(765, 540);
            this.grpContenido.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpContenido.Text = "Tema"; this.grpContenido.ForeColor = azul;
            this.grpContenido.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpContenido.Controls.Add(this.lblTema);
            this.grpContenido.Controls.Add(this.txtContenido);

            this.lblTema.AutoSize = true; this.lblTema.Location = new System.Drawing.Point(15, 28);
            this.lblTema.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold); this.lblTema.ForeColor = azul;
            this.lblTema.Text = "—";

            this.txtContenido.Location = new System.Drawing.Point(15, 55);
            this.txtContenido.Size = new System.Drawing.Size(735, 470);
            this.txtContenido.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContenido.Multiline = true; this.txtContenido.ReadOnly = true;
            this.txtContenido.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtContenido.BackColor = System.Drawing.Color.White;
            this.txtContenido.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtContenido.BorderStyle = System.Windows.Forms.BorderStyle.None;

            // Botones
            this.btnExportar.Location = new System.Drawing.Point(840, 600); this.btnExportar.Size = new System.Drawing.Size(200, 38);
            this.btnExportar.Text = "Exportar PDF (árbol)"; this.btnExportar.BackColor = System.Drawing.Color.SeaGreen; this.btnExportar.ForeColor = System.Drawing.Color.White;
            this.btnExportar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnExportar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportar.Click += new System.EventHandler(this.btnExportar_Click);

            this.btnSalir.Location = new System.Drawing.Point(1050, 600); this.btnSalir.Size = new System.Drawing.Size(90, 38);
            this.btnSalir.Text = "Salir"; this.btnSalir.BackColor = System.Drawing.Color.IndianRed; this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1160, 655);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.tvTemas);
            this.Controls.Add(this.grpContenido);
            this.Controls.Add(this.btnExportar);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMAyuda_GO44";
            this.Text = "Ayuda en línea";
            this.Load += new System.EventHandler(this.FRMAyuda_GO44_Load);

            this.grpContenido.ResumeLayout(false); this.grpContenido.PerformLayout();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.TreeView tvTemas;
        private System.Windows.Forms.GroupBox grpContenido;
        private System.Windows.Forms.Label lblTema;
        private System.Windows.Forms.TextBox txtContenido;
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.Button btnSalir;
    }
}
