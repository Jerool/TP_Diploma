namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMReporteInteligente_GO44
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
            this.dgv = new System.Windows.Forms.DataGridView();
            this.lblResumen = new System.Windows.Forms.Label();
            this.btnPromos = new System.Windows.Forms.Button();
            this.btnExportar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.nudLeadTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Rep. 3 (INT.) — Estrategia de Ventas";

            this.lblLeadTime.AutoSize = true; this.lblLeadTime.Location = new System.Drawing.Point(20, 52);
            this.lblLeadTime.Text = "Tiempo de entrega del proveedor (días):"; this.lblLeadTime.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLeadTime.ForeColor = System.Drawing.Color.Black;

            this.nudLeadTime.Location = new System.Drawing.Point(280, 49); this.nudLeadTime.Size = new System.Drawing.Size(60, 22);
            this.nudLeadTime.Minimum = 1; this.nudLeadTime.Maximum = 120; this.nudLeadTime.Value = 7;

            this.btnGenerar.Location = new System.Drawing.Point(360, 46); this.btnGenerar.Size = new System.Drawing.Size(120, 30);
            this.btnGenerar.Text = "Generar"; this.btnGenerar.BackColor = azul; this.btnGenerar.ForeColor = System.Drawing.Color.White;
            this.btnGenerar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerar.Click += new System.EventHandler(this.btnGenerar_Click);

            this.dgv.Location = new System.Drawing.Point(20, 90);
            this.dgv.Size = new System.Drawing.Size(1120, 500);
            this.dgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblResumen.AutoSize = true; this.lblResumen.Location = new System.Drawing.Point(20, 598);
            this.lblResumen.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold); this.lblResumen.ForeColor = azul;
            this.lblResumen.Text = "Productos: 0";
            this.lblResumen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));


            this.btnPromos.Location = new System.Drawing.Point(640, 600); this.btnPromos.Size = new System.Drawing.Size(200, 38);
            this.btnPromos.Text = "Promociones y gráfico"; this.btnPromos.BackColor = System.Drawing.Color.FromArgb(13, 71, 161); this.btnPromos.ForeColor = System.Drawing.Color.White;
            this.btnPromos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPromos.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnPromos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPromos.Click += new System.EventHandler(this.btnPromos_Click);

            this.btnExportar.Location = new System.Drawing.Point(860, 600); this.btnExportar.Size = new System.Drawing.Size(180, 38);
            this.btnExportar.Text = "Exportar PDF"; this.btnExportar.BackColor = System.Drawing.Color.SeaGreen; this.btnExportar.ForeColor = System.Drawing.Color.White;
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
            this.Controls.Add(this.lblLeadTime);
            this.Controls.Add(this.nudLeadTime);
            this.Controls.Add(this.btnGenerar);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.lblResumen);
            this.Controls.Add(this.btnPromos);
            this.Controls.Add(this.btnExportar);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMReporteInteligente_GO44";
            this.Text = "Reporte Inteligente — Estrategia de Ventas";
            this.Load += new System.EventHandler(this.FRMReporteInteligente_GO44_Load);

            ((System.ComponentModel.ISupportInitialize)(this.nudLeadTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblLeadTime;
        private System.Windows.Forms.NumericUpDown nudLeadTime;
        private System.Windows.Forms.Button btnGenerar;
        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.Button btnPromos;
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.Button btnSalir;
    }
}
