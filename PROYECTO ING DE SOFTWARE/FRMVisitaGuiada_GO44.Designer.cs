namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMVisitaGuiada_GO44
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
            this.lblContador = new System.Windows.Forms.Label();
            this.lblTexto = new System.Windows.Forms.Label();
            this.btnSaltar = new System.Windows.Forms.Button();
            this.btnAnterior = new System.Windows.Forms.Button();
            this.btnSiguiente = new System.Windows.Forms.Button();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(16, 14);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul; this.lblTitulo.Text = "Título";

            this.lblContador.AutoSize = true; this.lblContador.Location = new System.Drawing.Point(255, 16);
            this.lblContador.Font = new System.Drawing.Font("Segoe UI", 9F); this.lblContador.ForeColor = System.Drawing.Color.Gray;
            this.lblContador.Text = "1 de 1"; this.lblContador.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblContador.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));

            this.lblTexto.Location = new System.Drawing.Point(16, 42);
            this.lblTexto.Size = new System.Drawing.Size(300, 55);
            this.lblTexto.Font = new System.Drawing.Font("Segoe UI", 9.5F); this.lblTexto.ForeColor = System.Drawing.Color.Black;
            this.lblTexto.Text = "Descripción del paso.";

            this.btnSaltar.Location = new System.Drawing.Point(16, 108); this.btnSaltar.Size = new System.Drawing.Size(90, 30);
            this.btnSaltar.Text = "Saltar visita"; this.btnSaltar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaltar.BackColor = System.Drawing.Color.White; this.btnSaltar.ForeColor = System.Drawing.Color.Gray;
            this.btnSaltar.FlatAppearance.BorderSize = 0;
            this.btnSaltar.Click += new System.EventHandler(this.btnSaltar_Click);

            this.btnAnterior.Location = new System.Drawing.Point(140, 108); this.btnAnterior.Size = new System.Drawing.Size(85, 30);
            this.btnAnterior.Text = "Anterior"; this.btnAnterior.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnterior.BackColor = System.Drawing.Color.White; this.btnAnterior.ForeColor = azul;
            this.btnAnterior.Click += new System.EventHandler(this.btnAnterior_Click);

            this.btnSiguiente.Location = new System.Drawing.Point(230, 108); this.btnSiguiente.Size = new System.Drawing.Size(90, 30);
            this.btnSiguiente.Text = "Siguiente"; this.btnSiguiente.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSiguiente.BackColor = azul; this.btnSiguiente.ForeColor = System.Drawing.Color.White;
            this.btnSiguiente.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnSiguiente.Click += new System.EventHandler(this.btnSiguiente_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(336, 152);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblContador);
            this.Controls.Add(this.lblTexto);
            this.Controls.Add(this.btnSaltar);
            this.Controls.Add(this.btnAnterior);
            this.Controls.Add(this.btnSiguiente);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Name = "FRMVisitaGuiada_GO44";
            this.Text = "Visita guiada";
            this.Load += new System.EventHandler(this.FRMVisitaGuiada_GO44_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblContador;
        private System.Windows.Forms.Label lblTexto;
        private System.Windows.Forms.Button btnSaltar;
        private System.Windows.Forms.Button btnAnterior;
        private System.Windows.Forms.Button btnSiguiente;
    }
}
