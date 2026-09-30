namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMReporteFacturas_GO44
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
            this.grpFiltros = new System.Windows.Forms.GroupBox();
            this.chkDesde = new System.Windows.Forms.CheckBox();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.chkHasta = new System.Windows.Forms.CheckBox();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.lblDni = new System.Windows.Forms.Label();
            this.txtDni = new System.Windows.Forms.TextBox();
            this.lblEstado = new System.Windows.Forms.Label();
            this.cmbEstado = new System.Windows.Forms.ComboBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.dgvFacturas = new System.Windows.Forms.DataGridView();
            this.lblTotal = new System.Windows.Forms.Label();
            this.btnGenerarPDF = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components = new System.ComponentModel.Container());

            this.grpFiltros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFacturas)).BeginInit();
            this.SuspendLayout();

            // Título
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Text = "Reporte de Facturas";

            // Filtros
            this.grpFiltros.Location = new System.Drawing.Point(20, 45);
            this.grpFiltros.Size = new System.Drawing.Size(1100, 100);
            this.grpFiltros.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFiltros.Text = "Filtros";
            this.grpFiltros.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.grpFiltros.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpFiltros.Controls.Add(this.chkDesde);
            this.grpFiltros.Controls.Add(this.dtpDesde);
            this.grpFiltros.Controls.Add(this.chkHasta);
            this.grpFiltros.Controls.Add(this.dtpHasta);
            this.grpFiltros.Controls.Add(this.lblDni);
            this.grpFiltros.Controls.Add(this.txtDni);
            this.grpFiltros.Controls.Add(this.lblEstado);
            this.grpFiltros.Controls.Add(this.cmbEstado);
            this.grpFiltros.Controls.Add(this.btnBuscar);
            this.grpFiltros.Controls.Add(this.btnLimpiar);

            this.chkDesde.AutoSize = true; this.chkDesde.Location = new System.Drawing.Point(15, 30); this.chkDesde.Text = "Desde:";
            this.chkDesde.ForeColor = System.Drawing.Color.Black;
            this.dtpDesde.Location = new System.Drawing.Point(90, 27); this.dtpDesde.Size = new System.Drawing.Size(150, 22);
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;

            this.chkHasta.AutoSize = true; this.chkHasta.Location = new System.Drawing.Point(260, 30); this.chkHasta.Text = "Hasta:";
            this.chkHasta.ForeColor = System.Drawing.Color.Black;
            this.dtpHasta.Location = new System.Drawing.Point(330, 27); this.dtpHasta.Size = new System.Drawing.Size(150, 22);
            this.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;

            this.lblDni.AutoSize = true; this.lblDni.Location = new System.Drawing.Point(510, 30); this.lblDni.Text = "DNI:";
            this.lblDni.ForeColor = System.Drawing.Color.Black;
            this.txtDni.Location = new System.Drawing.Point(555, 27); this.txtDni.Size = new System.Drawing.Size(120, 22);

            this.lblEstado.AutoSize = true; this.lblEstado.Location = new System.Drawing.Point(690, 30); this.lblEstado.Text = "Estado:";
            this.lblEstado.ForeColor = System.Drawing.Color.Black;
            this.cmbEstado.Location = new System.Drawing.Point(750, 27); this.cmbEstado.Size = new System.Drawing.Size(140, 22);
            this.cmbEstado.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.btnBuscar.Location = new System.Drawing.Point(910, 25); this.btnBuscar.Size = new System.Drawing.Size(85, 30);
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnBuscar.ForeColor = System.Drawing.Color.White;
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);

            this.btnLimpiar.Location = new System.Drawing.Point(1000, 25); this.btnLimpiar.Size = new System.Drawing.Size(80, 30);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.BackColor = System.Drawing.Color.Silver;
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);

            // Grilla
            this.dgvFacturas.Location = new System.Drawing.Point(20, 160);
            this.dgvFacturas.Size = new System.Drawing.Size(1100, 400);
            this.dgvFacturas.BackgroundColor = System.Drawing.Color.White;
            this.dgvFacturas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            // Total y botones
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTotal.Location = new System.Drawing.Point(20, 575);
            this.lblTotal.Text = "Total: 0 facturas · 0.00";
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.btnGenerarPDF.Location = new System.Drawing.Point(880, 570); this.btnGenerarPDF.Size = new System.Drawing.Size(160, 35);
            this.btnGenerarPDF.Text = "🖨 Generar PDF";
            this.btnGenerarPDF.BackColor = System.Drawing.Color.SeaGreen;
            this.btnGenerarPDF.ForeColor = System.Drawing.Color.White;
            this.btnGenerarPDF.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnGenerarPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerarPDF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerarPDF.Click += new System.EventHandler(this.btnGenerarPDF_Click);

            this.btnSalir.Location = new System.Drawing.Point(1050, 570); this.btnSalir.Size = new System.Drawing.Size(70, 35);
            this.btnSalir.Text = "Salir";
            this.btnSalir.BackColor = System.Drawing.Color.IndianRed;
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // Tooltips descriptivos
            this.toolTip.SetToolTip(this.btnBuscar, "Ejecuta la búsqueda aplicando los filtros seleccionados");
            this.toolTip.SetToolTip(this.btnLimpiar, "Restablece todos los filtros y muestra el listado completo");
            this.toolTip.SetToolTip(this.btnGenerarPDF, "Genera un PDF de la factura seleccionada (usa Microsoft Print to PDF)");
            this.toolTip.SetToolTip(this.btnSalir, "Cierra la pantalla y vuelve al menú principal");
            this.toolTip.SetToolTip(this.dgvFacturas, "Lista de facturas. Seleccione una fila para poder generar su PDF.");

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1140, 620);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.grpFiltros);
            this.Controls.Add(this.dgvFacturas);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.btnGenerarPDF);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMReporteFacturas_GO44";
            this.Text = "Reporte de Facturas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FRMReporteFacturas_GO44_Load);
            this.grpFiltros.ResumeLayout(false); this.grpFiltros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFacturas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox grpFiltros;
        private System.Windows.Forms.CheckBox chkDesde;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.CheckBox chkHasta;
        private System.Windows.Forms.DateTimePicker dtpHasta;
        private System.Windows.Forms.Label lblDni;
        private System.Windows.Forms.TextBox txtDni;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.ComboBox cmbEstado;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.DataGridView dgvFacturas;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnGenerarPDF;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
