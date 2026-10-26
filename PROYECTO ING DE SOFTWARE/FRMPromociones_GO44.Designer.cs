namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMPromociones_GO44
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
            this.lblLeadTime = new System.Windows.Forms.Label();
            this.nudLeadTime = new System.Windows.Forms.NumericUpDown();
            this.btnGenerar = new System.Windows.Forms.Button();
            this.pnlGrafico = new System.Windows.Forms.Panel();
            this.grpPromos = new System.Windows.Forms.GroupBox();
            this.dgvPromos = new System.Windows.Forms.DataGridView();
            this.lblResumen = new System.Windows.Forms.Label();
            this.lblLeyenda = new System.Windows.Forms.Label();
            this.btnSalir = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.nudLeadTime)).BeginInit();
            this.grpPromos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPromos)).BeginInit();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Plan de Promociones y Más Vendidos";

            this.lblLeadTime.AutoSize = true; this.lblLeadTime.Location = new System.Drawing.Point(20, 52);
            this.lblLeadTime.Text = "Tiempo de entrega (días):"; this.lblLeadTime.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLeadTime.ForeColor = System.Drawing.Color.Black;
            this.nudLeadTime.Location = new System.Drawing.Point(180, 49); this.nudLeadTime.Size = new System.Drawing.Size(60, 22);
            this.nudLeadTime.Minimum = 1; this.nudLeadTime.Maximum = 120; this.nudLeadTime.Value = 7;

            this.btnGenerar.Location = new System.Drawing.Point(260, 46); this.btnGenerar.Size = new System.Drawing.Size(120, 30);
            this.btnGenerar.Text = "Generar"; this.btnGenerar.BackColor = azul; this.btnGenerar.ForeColor = System.Drawing.Color.White;
            this.btnGenerar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerar.Click += new System.EventHandler(this.btnGenerar_Click);

            // Gráfico
            this.pnlGrafico.Location = new System.Drawing.Point(20, 90);
            this.pnlGrafico.Size = new System.Drawing.Size(1120, 260);
            this.pnlGrafico.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlGrafico.BackColor = System.Drawing.Color.White;
            this.pnlGrafico.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // Promociones
            this.grpPromos.Location = new System.Drawing.Point(20, 360);
            this.grpPromos.Size = new System.Drawing.Size(1120, 270);
            this.grpPromos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPromos.Text = "Plan de promociones sugeridas"; this.grpPromos.ForeColor = azul;
            this.grpPromos.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpPromos.Controls.Add(this.dgvPromos);

            this.dgvPromos.Location = new System.Drawing.Point(15, 28);
            this.dgvPromos.Size = new System.Drawing.Size(1090, 230);
            this.dgvPromos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblResumen.AutoSize = true; this.lblResumen.Location = new System.Drawing.Point(20, 640);
            this.lblResumen.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold); this.lblResumen.ForeColor = azul;
            this.lblResumen.Text = "Propuestas: 0";
            this.lblResumen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.lblLeyenda.AutoSize = true; this.lblLeyenda.Location = new System.Drawing.Point(150, 640);
            this.lblLeyenda.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic); this.lblLeyenda.ForeColor = System.Drawing.Color.DimGray;
            this.lblLeyenda.Text = "Celeste = combo   ·   Gris = liquidación   ·   Rosa = reposición";
            this.lblLeyenda.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.btnSalir.Location = new System.Drawing.Point(1050, 638); this.btnSalir.Size = new System.Drawing.Size(90, 36);
            this.btnSalir.Text = "Salir"; this.btnSalir.BackColor = System.Drawing.Color.IndianRed; this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1160, 690);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblLeadTime);
            this.Controls.Add(this.nudLeadTime);
            this.Controls.Add(this.btnGenerar);
            this.Controls.Add(this.pnlGrafico);
            this.Controls.Add(this.grpPromos);
            this.Controls.Add(this.lblResumen);
            this.Controls.Add(this.lblLeyenda);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMPromociones_GO44";
            this.Text = "Plan de Promociones — Reporte Inteligente";
            this.Load += new System.EventHandler(this.FRMPromociones_GO44_Load);

            ((System.ComponentModel.ISupportInitialize)(this.nudLeadTime)).EndInit();
            this.grpPromos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPromos)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblLeadTime;
        private System.Windows.Forms.NumericUpDown nudLeadTime;
        private System.Windows.Forms.Button btnGenerar;
        private System.Windows.Forms.Panel pnlGrafico;
        private System.Windows.Forms.GroupBox grpPromos;
        private System.Windows.Forms.DataGridView dgvPromos;
        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.Label lblLeyenda;
        private System.Windows.Forms.Button btnSalir;
    }
}
