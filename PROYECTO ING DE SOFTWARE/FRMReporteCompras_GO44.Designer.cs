namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMReporteCompras_GO44
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
            this.lblDesde = new System.Windows.Forms.Label();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.lblHasta = new System.Windows.Forms.Label();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.lblEstado = new System.Windows.Forms.Label();
            this.cmbEstado = new System.Windows.Forms.ComboBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.dgvOrdenes = new System.Windows.Forms.DataGridView();
            this.lblContador = new System.Windows.Forms.Label();
            this.grpDetalle = new System.Windows.Forms.GroupBox();
            this.dgvDetalle = new System.Windows.Forms.DataGridView();
            this.lblProveedor = new System.Windows.Forms.Label();
            this.lblPago = new System.Windows.Forms.Label();
            this.btnExportar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvOrdenes)).BeginInit();
            this.grpDetalle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle)).BeginInit();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Rep. 2 (RFN2) — Recomposición de componentes";

            // Filtros
            this.lblDesde.AutoSize = true; this.lblDesde.Location = new System.Drawing.Point(20, 50); this.lblDesde.Text = "Desde:";
            this.lblDesde.Font = new System.Drawing.Font("Segoe UI", 9F); this.lblDesde.ForeColor = System.Drawing.Color.Black;
            this.dtpDesde.Location = new System.Drawing.Point(70, 47); this.dtpDesde.Size = new System.Drawing.Size(140, 22);
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;

            this.lblHasta.AutoSize = true; this.lblHasta.Location = new System.Drawing.Point(230, 50); this.lblHasta.Text = "Hasta:";
            this.lblHasta.Font = new System.Drawing.Font("Segoe UI", 9F); this.lblHasta.ForeColor = System.Drawing.Color.Black;
            this.dtpHasta.Location = new System.Drawing.Point(280, 47); this.dtpHasta.Size = new System.Drawing.Size(140, 22);
            this.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;

            this.lblEstado.AutoSize = true; this.lblEstado.Location = new System.Drawing.Point(440, 50); this.lblEstado.Text = "Estado:";
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 9F); this.lblEstado.ForeColor = System.Drawing.Color.Black;
            this.cmbEstado.Location = new System.Drawing.Point(495, 47); this.cmbEstado.Size = new System.Drawing.Size(180, 23);
            this.cmbEstado.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.btnBuscar.Location = new System.Drawing.Point(695, 44); this.btnBuscar.Size = new System.Drawing.Size(120, 30);
            this.btnBuscar.Text = "Buscar"; this.btnBuscar.BackColor = azul; this.btnBuscar.ForeColor = System.Drawing.Color.White;
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);

            // Grilla de órdenes
            this.dgvOrdenes.Location = new System.Drawing.Point(20, 85);
            this.dgvOrdenes.Size = new System.Drawing.Size(1120, 230);
            this.dgvOrdenes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblContador.AutoSize = true; this.lblContador.Location = new System.Drawing.Point(20, 320);
            this.lblContador.ForeColor = System.Drawing.Color.DimGray; this.lblContador.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblContador.Text = "Órdenes: 0";

            // Detalle
            this.grpDetalle.Location = new System.Drawing.Point(20, 345);
            this.grpDetalle.Size = new System.Drawing.Size(1120, 290);
            this.grpDetalle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDetalle.Text = "Detalle (pedido vs recibido)"; this.grpDetalle.ForeColor = azul;
            this.grpDetalle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpDetalle.Controls.Add(this.dgvDetalle);
            this.grpDetalle.Controls.Add(this.lblProveedor);
            this.grpDetalle.Controls.Add(this.lblPago);

            this.dgvDetalle.Location = new System.Drawing.Point(15, 30);
            this.dgvDetalle.Size = new System.Drawing.Size(1090, 200);
            this.dgvDetalle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblProveedor.AutoSize = true; this.lblProveedor.Location = new System.Drawing.Point(15, 240);
            this.lblProveedor.Font = new System.Drawing.Font("Segoe UI", 9F); this.lblProveedor.ForeColor = System.Drawing.Color.Black;
            this.lblProveedor.Text = "Proveedor: —";
            this.lblProveedor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.lblPago.AutoSize = true; this.lblPago.Location = new System.Drawing.Point(15, 262);
            this.lblPago.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold); this.lblPago.ForeColor = azul;
            this.lblPago.Text = "Pago: —";
            this.lblPago.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            // Botones
            this.btnExportar.Location = new System.Drawing.Point(860, 645); this.btnExportar.Size = new System.Drawing.Size(180, 38);
            this.btnExportar.Text = "Exportar PDF"; this.btnExportar.BackColor = System.Drawing.Color.SeaGreen; this.btnExportar.ForeColor = System.Drawing.Color.White;
            this.btnExportar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnExportar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportar.Click += new System.EventHandler(this.btnExportar_Click);

            this.btnSalir.Location = new System.Drawing.Point(1050, 645); this.btnSalir.Size = new System.Drawing.Size(90, 38);
            this.btnSalir.Text = "Salir"; this.btnSalir.BackColor = System.Drawing.Color.IndianRed; this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1160, 700);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblDesde);
            this.Controls.Add(this.dtpDesde);
            this.Controls.Add(this.lblHasta);
            this.Controls.Add(this.dtpHasta);
            this.Controls.Add(this.lblEstado);
            this.Controls.Add(this.cmbEstado);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.dgvOrdenes);
            this.Controls.Add(this.lblContador);
            this.Controls.Add(this.grpDetalle);
            this.Controls.Add(this.btnExportar);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMReporteCompras_GO44";
            this.Text = "Reporte de Compras (RFN2)";
            this.Load += new System.EventHandler(this.FRMReporteCompras_GO44_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgvOrdenes)).EndInit();
            this.grpDetalle.ResumeLayout(false); this.grpDetalle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDesde;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.Label lblHasta;
        private System.Windows.Forms.DateTimePicker dtpHasta;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.ComboBox cmbEstado;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.DataGridView dgvOrdenes;
        private System.Windows.Forms.Label lblContador;
        private System.Windows.Forms.GroupBox grpDetalle;
        private System.Windows.Forms.DataGridView dgvDetalle;
        private System.Windows.Forms.Label lblProveedor;
        private System.Windows.Forms.Label lblPago;
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.Button btnSalir;
    }
}
