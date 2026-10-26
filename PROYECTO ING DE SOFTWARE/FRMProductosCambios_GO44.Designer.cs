namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMProductosCambios_GO44
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
            this.dgv = new System.Windows.Forms.DataGridView();

            this.lblCodigo = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblFechaIni = new System.Windows.Forms.Label();
            this.dtpFechaIni = new System.Windows.Forms.DateTimePicker();
            this.lblFechaFin = new System.Windows.Forms.Label();
            this.dtpFechaFin = new System.Windows.Forms.DateTimePicker();

            this.btnBuscar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnMarcar = new System.Windows.Forms.Button();
            this.lblContador = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);
            System.Drawing.Color verde = System.Drawing.Color.FromArgb(40, 167, 69);

            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Bitácora de Cambios — Tabla Productos_C";

            // Grilla (arriba)
            this.dgv.Location = new System.Drawing.Point(20, 45);
            this.dgv.Size = new System.Drawing.Size(950, 400);
            this.dgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblContador.AutoSize = true; this.lblContador.Location = new System.Drawing.Point(20, 450);
            this.lblContador.ForeColor = System.Drawing.Color.DimGray; this.lblContador.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblContador.Text = "Registros: 0";
            this.lblContador.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            // Filtros (abajo, izquierda)
            this.lblCodigo.AutoSize = true; this.lblCodigo.Location = new System.Drawing.Point(20, 486); this.lblCodigo.Text = "Cod_Prod:";
            this.lblCodigo.Font = new System.Drawing.Font("Segoe UI", 9F); this.lblCodigo.ForeColor = System.Drawing.Color.Black;
            this.lblCodigo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtCodigo.Location = new System.Drawing.Point(110, 483); this.txtCodigo.Size = new System.Drawing.Size(150, 22);
            this.txtCodigo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.lblNombre.AutoSize = true; this.lblNombre.Location = new System.Drawing.Point(290, 486); this.lblNombre.Text = "Nombre:";
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI", 9F); this.lblNombre.ForeColor = System.Drawing.Color.Black;
            this.lblNombre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtNombre.Location = new System.Drawing.Point(350, 483); this.txtNombre.Size = new System.Drawing.Size(200, 22);
            this.txtNombre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.lblFechaIni.AutoSize = true; this.lblFechaIni.Location = new System.Drawing.Point(20, 521); this.lblFechaIni.Text = "Fecha Ini:";
            this.lblFechaIni.Font = new System.Drawing.Font("Segoe UI", 9F); this.lblFechaIni.ForeColor = System.Drawing.Color.Black;
            this.lblFechaIni.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dtpFechaIni.Location = new System.Drawing.Point(110, 518); this.dtpFechaIni.Size = new System.Drawing.Size(150, 22);
            this.dtpFechaIni.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaIni.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.lblFechaFin.AutoSize = true; this.lblFechaFin.Location = new System.Drawing.Point(20, 556); this.lblFechaFin.Text = "Fecha Fin:";
            this.lblFechaFin.Font = new System.Drawing.Font("Segoe UI", 9F); this.lblFechaFin.ForeColor = System.Drawing.Color.Black;
            this.lblFechaFin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dtpFechaFin.Location = new System.Drawing.Point(110, 553); this.dtpFechaFin.Size = new System.Drawing.Size(150, 22);
            this.dtpFechaFin.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaFin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            // Botones (abajo, verdes) — APLICAR / SALIR / LIMPAR / ACTIVAR
            this.btnBuscar.Location = new System.Drawing.Point(300, 513); this.btnBuscar.Size = new System.Drawing.Size(150, 36);
            this.btnBuscar.Text = "APLICAR"; this.btnBuscar.BackColor = verde; this.btnBuscar.ForeColor = System.Drawing.Color.White;
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);

            this.btnSalir.Location = new System.Drawing.Point(465, 513); this.btnSalir.Size = new System.Drawing.Size(150, 36);
            this.btnSalir.Text = "SALIR"; this.btnSalir.BackColor = verde; this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            this.btnLimpiar.Location = new System.Drawing.Point(300, 553); this.btnLimpiar.Size = new System.Drawing.Size(150, 36);
            this.btnLimpiar.Text = "LIMPAR"; this.btnLimpiar.BackColor = verde; this.btnLimpiar.ForeColor = System.Drawing.Color.White;
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnLimpiar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);

            this.btnMarcar.Location = new System.Drawing.Point(465, 553); this.btnMarcar.Size = new System.Drawing.Size(150, 36);
            this.btnMarcar.Text = "ACTIVAR"; this.btnMarcar.BackColor = verde; this.btnMarcar.ForeColor = System.Drawing.Color.White;
            this.btnMarcar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMarcar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnMarcar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMarcar.Click += new System.EventHandler(this.btnMarcar_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(990, 610);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.lblContador);
            this.Controls.Add(this.lblCodigo);
            this.Controls.Add(this.txtCodigo);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblFechaIni);
            this.Controls.Add(this.dtpFechaIni);
            this.Controls.Add(this.lblFechaFin);
            this.Controls.Add(this.dtpFechaFin);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.btnMarcar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMProductosCambios_GO44";
            this.Text = "Bitácora de Cambios — Tabla Productos_C";
            this.Load += new System.EventHandler(this.FRMProductosCambios_GO44_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblFechaIni;
        private System.Windows.Forms.DateTimePicker dtpFechaIni;
        private System.Windows.Forms.Label lblFechaFin;
        private System.Windows.Forms.DateTimePicker dtpFechaFin;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnMarcar;
        private System.Windows.Forms.Label lblContador;
    }
}
