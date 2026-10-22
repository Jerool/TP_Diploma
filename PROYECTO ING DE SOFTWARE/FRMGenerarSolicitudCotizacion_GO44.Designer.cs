namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMGenerarSolicitudCotizacion_GO44
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
            this.grpProveedor = new System.Windows.Forms.GroupBox();
            this.lblProveedor = new System.Windows.Forms.Label();
            this.cmbProveedor = new System.Windows.Forms.ComboBox();
            this.btnRegistrarProveedor = new System.Windows.Forms.Button();
            this.lblAvisoSinProveedores = new System.Windows.Forms.Label();

            this.grpComponentes = new System.Windows.Forms.GroupBox();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.chkBajoStock = new System.Windows.Forms.CheckBox();
            this.dgvComponentes = new System.Windows.Forms.DataGridView();
            this.lblCantidad = new System.Windows.Forms.Label();
            this.txtCantidad = new System.Windows.Forms.TextBox();
            this.btnAgregar = new System.Windows.Forms.Button();

            this.grpCotizacion = new System.Windows.Forms.GroupBox();
            this.dgvLineas = new System.Windows.Forms.DataGridView();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.lblContadorLineas = new System.Windows.Forms.Label();

            this.btnFinalizar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();

            this.grpProveedor.SuspendLayout();
            this.grpComponentes.SuspendLayout();
            this.grpCotizacion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComponentes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).BeginInit();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            // Título
            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Generar Solicitud de Cotización";

            // Grupo Proveedor
            this.grpProveedor.Location = new System.Drawing.Point(20, 45);
            this.grpProveedor.Size = new System.Drawing.Size(1120, 65);
            this.grpProveedor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpProveedor.Text = "Proveedor"; this.grpProveedor.ForeColor = azul;
            this.grpProveedor.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpProveedor.Controls.Add(this.lblProveedor);
            this.grpProveedor.Controls.Add(this.cmbProveedor);
            this.grpProveedor.Controls.Add(this.btnRegistrarProveedor);
            this.grpProveedor.Controls.Add(this.lblAvisoSinProveedores);

            this.lblProveedor.AutoSize = true; this.lblProveedor.Location = new System.Drawing.Point(15, 28); this.lblProveedor.Text = "Proveedor:";
            this.lblProveedor.ForeColor = System.Drawing.Color.Black; this.lblProveedor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbProveedor.Location = new System.Drawing.Point(100, 25); this.cmbProveedor.Size = new System.Drawing.Size(400, 23);
            this.cmbProveedor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.btnRegistrarProveedor.Location = new System.Drawing.Point(510, 23); this.btnRegistrarProveedor.Size = new System.Drawing.Size(180, 28);
            this.btnRegistrarProveedor.Text = "+ Registrar proveedor"; this.btnRegistrarProveedor.BackColor = azul;
            this.btnRegistrarProveedor.ForeColor = System.Drawing.Color.White;
            this.btnRegistrarProveedor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegistrarProveedor.Click += new System.EventHandler(this.btnRegistrarProveedor_Click);

            this.lblAvisoSinProveedores.AutoSize = true; this.lblAvisoSinProveedores.Location = new System.Drawing.Point(700, 28);
            this.lblAvisoSinProveedores.ForeColor = System.Drawing.Color.Firebrick;
            this.lblAvisoSinProveedores.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.lblAvisoSinProveedores.Text = "⚠ No hay proveedores registrados. Registre uno con el botón de la izquierda.";
            this.lblAvisoSinProveedores.Visible = false;

            // Grupo Componentes (izquierda)
            this.grpComponentes.Location = new System.Drawing.Point(20, 125);
            this.grpComponentes.Size = new System.Drawing.Size(680, 500);
            this.grpComponentes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left))));
            this.grpComponentes.Text = "Componentes disponibles"; this.grpComponentes.ForeColor = azul;
            this.grpComponentes.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpComponentes.Controls.Add(this.lblBuscar);
            this.grpComponentes.Controls.Add(this.txtBuscar);
            this.grpComponentes.Controls.Add(this.chkBajoStock);
            this.grpComponentes.Controls.Add(this.dgvComponentes);
            this.grpComponentes.Controls.Add(this.lblCantidad);
            this.grpComponentes.Controls.Add(this.txtCantidad);
            this.grpComponentes.Controls.Add(this.btnAgregar);

            this.lblBuscar.AutoSize = true; this.lblBuscar.Location = new System.Drawing.Point(15, 30); this.lblBuscar.Text = "Buscar:";
            this.lblBuscar.ForeColor = System.Drawing.Color.Black; this.lblBuscar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBuscar.Location = new System.Drawing.Point(70, 27); this.txtBuscar.Size = new System.Drawing.Size(280, 22);
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);

            this.chkBajoStock.AutoSize = true; this.chkBajoStock.Location = new System.Drawing.Point(370, 29); this.chkBajoStock.Text = "Solo bajo stock";
            this.chkBajoStock.ForeColor = System.Drawing.Color.Black; this.chkBajoStock.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkBajoStock.CheckedChanged += new System.EventHandler(this.chkBajoStock_CheckedChanged);

            this.dgvComponentes.Location = new System.Drawing.Point(15, 60);
            this.dgvComponentes.Size = new System.Drawing.Size(650, 390);
            this.dgvComponentes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblCantidad.AutoSize = true; this.lblCantidad.Location = new System.Drawing.Point(15, 465); this.lblCantidad.Text = "Cantidad:";
            this.lblCantidad.ForeColor = System.Drawing.Color.Black; this.lblCantidad.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCantidad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtCantidad.Location = new System.Drawing.Point(80, 462); this.txtCantidad.Size = new System.Drawing.Size(60, 22);
            this.txtCantidad.Text = "1"; this.txtCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtCantidad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.btnAgregar.Location = new System.Drawing.Point(160, 457); this.btnAgregar.Size = new System.Drawing.Size(220, 32);
            this.btnAgregar.Text = "► Agregar a cotización"; this.btnAgregar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnAgregar.ForeColor = System.Drawing.Color.White;
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnAgregar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);

            // Grupo Cotización (derecha)
            this.grpCotizacion.Location = new System.Drawing.Point(720, 125);
            this.grpCotizacion.Size = new System.Drawing.Size(420, 500);
            this.grpCotizacion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right))));
            this.grpCotizacion.Text = "Cotización en curso"; this.grpCotizacion.ForeColor = azul;
            this.grpCotizacion.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpCotizacion.Controls.Add(this.dgvLineas);
            this.grpCotizacion.Controls.Add(this.btnQuitar);
            this.grpCotizacion.Controls.Add(this.lblContadorLineas);

            this.dgvLineas.Location = new System.Drawing.Point(15, 30);
            this.dgvLineas.Size = new System.Drawing.Size(390, 390);
            this.dgvLineas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.btnQuitar.Location = new System.Drawing.Point(15, 435); this.btnQuitar.Size = new System.Drawing.Size(150, 32);
            this.btnQuitar.Text = "Quitar línea"; this.btnQuitar.BackColor = System.Drawing.Color.IndianRed;
            this.btnQuitar.ForeColor = System.Drawing.Color.White;
            this.btnQuitar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuitar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnQuitar.Click += new System.EventHandler(this.btnQuitar_Click);

            this.lblContadorLineas.AutoSize = true; this.lblContadorLineas.Location = new System.Drawing.Point(180, 442);
            this.lblContadorLineas.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblContadorLineas.ForeColor = System.Drawing.Color.DimGray; this.lblContadorLineas.Text = "Items: 0  ·  Unidades totales: 0";
            this.lblContadorLineas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            // Botones inferiores
            this.btnFinalizar.Location = new System.Drawing.Point(760, 640); this.btnFinalizar.Size = new System.Drawing.Size(170, 40);
            this.btnFinalizar.Text = "✓ Finalizar"; this.btnFinalizar.BackColor = azul;
            this.btnFinalizar.ForeColor = System.Drawing.Color.White;
            this.btnFinalizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFinalizar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnFinalizar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFinalizar.Click += new System.EventHandler(this.btnFinalizar_Click);

            this.btnCancelar.Location = new System.Drawing.Point(940, 640); this.btnCancelar.Size = new System.Drawing.Size(110, 40);
            this.btnCancelar.Text = "Cancelar"; this.btnCancelar.BackColor = System.Drawing.Color.Gray;
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            this.btnSalir.Location = new System.Drawing.Point(1060, 640); this.btnSalir.Size = new System.Drawing.Size(80, 40);
            this.btnSalir.Text = "Salir"; this.btnSalir.BackColor = System.Drawing.Color.IndianRed;
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1160, 700);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.grpProveedor);
            this.Controls.Add(this.grpComponentes);
            this.Controls.Add(this.grpCotizacion);
            this.Controls.Add(this.btnFinalizar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMGenerarSolicitudCotizacion_GO44";
            this.Text = "Generar Solicitud de Cotización";
            this.Load += new System.EventHandler(this.FRMGenerarSolicitudCotizacion_GO44_Load);

            this.grpProveedor.ResumeLayout(false); this.grpProveedor.PerformLayout();
            this.grpComponentes.ResumeLayout(false); this.grpComponentes.PerformLayout();
            this.grpCotizacion.ResumeLayout(false); this.grpCotizacion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComponentes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox grpProveedor;
        private System.Windows.Forms.Label lblProveedor;
        private System.Windows.Forms.ComboBox cmbProveedor;
        private System.Windows.Forms.Button btnRegistrarProveedor;
        private System.Windows.Forms.Label lblAvisoSinProveedores;

        private System.Windows.Forms.GroupBox grpComponentes;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.CheckBox chkBajoStock;
        private System.Windows.Forms.DataGridView dgvComponentes;
        private System.Windows.Forms.Label lblCantidad;
        private System.Windows.Forms.TextBox txtCantidad;
        private System.Windows.Forms.Button btnAgregar;

        private System.Windows.Forms.GroupBox grpCotizacion;
        private System.Windows.Forms.DataGridView dgvLineas;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Label lblContadorLineas;

        private System.Windows.Forms.Button btnFinalizar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnSalir;
    }
}
