namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMGenerarOrdenCompra_GO44
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
            this.grpOrigen = new System.Windows.Forms.GroupBox();
            this.lblSolicitud = new System.Windows.Forms.Label();
            this.cmbSolicitud = new System.Windows.Forms.ComboBox();
            this.btnCargarSC = new System.Windows.Forms.Button();
            this.lblAvisoSinSC = new System.Windows.Forms.Label();
            this.lblProveedor = new System.Windows.Forms.Label();
            this.lblSCInfo = new System.Windows.Forms.Label();
            this.lblEntrega = new System.Windows.Forms.Label();
            this.dtpEntrega = new System.Windows.Forms.DateTimePicker();

            this.grpLineas = new System.Windows.Forms.GroupBox();
            this.dgvLineas = new System.Windows.Forms.DataGridView();
            this.lblTotal = new System.Windows.Forms.Label();

            this.btnGenerar = new System.Windows.Forms.Button();
            this.btnPagar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();

            this.grpOrigen.SuspendLayout();
            this.grpLineas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).BeginInit();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            // Título
            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Generar Orden de Compra";

            // Grupo Origen (SC)
            this.grpOrigen.Location = new System.Drawing.Point(20, 45);
            this.grpOrigen.Size = new System.Drawing.Size(1120, 115);
            this.grpOrigen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOrigen.Text = "Solicitud de cotización de origen"; this.grpOrigen.ForeColor = azul;
            this.grpOrigen.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpOrigen.Controls.Add(this.lblSolicitud);
            this.grpOrigen.Controls.Add(this.cmbSolicitud);
            this.grpOrigen.Controls.Add(this.btnCargarSC);
            this.grpOrigen.Controls.Add(this.lblAvisoSinSC);
            this.grpOrigen.Controls.Add(this.lblProveedor);
            this.grpOrigen.Controls.Add(this.lblSCInfo);
            this.grpOrigen.Controls.Add(this.lblEntrega);
            this.grpOrigen.Controls.Add(this.dtpEntrega);

            this.lblSolicitud.AutoSize = true; this.lblSolicitud.Location = new System.Drawing.Point(15, 30); this.lblSolicitud.Text = "Solicitud (SC):";
            this.lblSolicitud.ForeColor = System.Drawing.Color.Black; this.lblSolicitud.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbSolicitud.Location = new System.Drawing.Point(110, 27); this.cmbSolicitud.Size = new System.Drawing.Size(220, 23);
            this.cmbSolicitud.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.btnCargarSC.Location = new System.Drawing.Point(345, 25); this.btnCargarSC.Size = new System.Drawing.Size(150, 28);
            this.btnCargarSC.Text = "Cargar SC"; this.btnCargarSC.BackColor = azul;
            this.btnCargarSC.ForeColor = System.Drawing.Color.White;
            this.btnCargarSC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargarSC.Click += new System.EventHandler(this.btnCargarSC_Click);

            this.lblAvisoSinSC.AutoSize = true; this.lblAvisoSinSC.Location = new System.Drawing.Point(510, 31);
            this.lblAvisoSinSC.ForeColor = System.Drawing.Color.Firebrick;
            this.lblAvisoSinSC.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.lblAvisoSinSC.Text = "⚠ No hay solicitudes de cotización disponibles.";
            this.lblAvisoSinSC.Visible = false;

            this.lblProveedor.AutoSize = true; this.lblProveedor.Location = new System.Drawing.Point(15, 62);
            this.lblProveedor.ForeColor = System.Drawing.Color.Black; this.lblProveedor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProveedor.Text = "Proveedor: —";

            this.lblSCInfo.AutoSize = true; this.lblSCInfo.Location = new System.Drawing.Point(15, 85);
            this.lblSCInfo.ForeColor = System.Drawing.Color.DimGray; this.lblSCInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblSCInfo.Text = "SC de origen: —";

            this.lblEntrega.AutoSize = true; this.lblEntrega.Location = new System.Drawing.Point(620, 62); this.lblEntrega.Text = "Fecha de entrega:";
            this.lblEntrega.ForeColor = System.Drawing.Color.Black; this.lblEntrega.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpEntrega.Location = new System.Drawing.Point(735, 58); this.dtpEntrega.Size = new System.Drawing.Size(150, 23);
            this.dtpEntrega.Format = System.Windows.Forms.DateTimePickerFormat.Short;

            // Grupo Líneas
            this.grpLineas.Location = new System.Drawing.Point(20, 170);
            this.grpLineas.Size = new System.Drawing.Size(1120, 420);
            this.grpLineas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLineas.Text = "Componentes (completá el precio unitario cotizado)"; this.grpLineas.ForeColor = azul;
            this.grpLineas.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpLineas.Controls.Add(this.dgvLineas);
            this.grpLineas.Controls.Add(this.lblTotal);

            this.dgvLineas.Location = new System.Drawing.Point(15, 30);
            this.dgvLineas.Size = new System.Drawing.Size(1090, 330);
            this.dgvLineas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblTotal.AutoSize = true; this.lblTotal.Location = new System.Drawing.Point(930, 385);
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = azul; this.lblTotal.Text = "Total: $0.00";
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            // Botones inferiores
            this.btnGenerar.Location = new System.Drawing.Point(760, 605); this.btnGenerar.Size = new System.Drawing.Size(180, 40);
            this.btnGenerar.Text = "✓ Generar OC"; this.btnGenerar.BackColor = azul;
            this.btnGenerar.ForeColor = System.Drawing.Color.White;
            this.btnGenerar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnGenerar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerar.Click += new System.EventHandler(this.btnGenerar_Click);

            this.btnPagar.Location = new System.Drawing.Point(20, 605); this.btnPagar.Size = new System.Drawing.Size(160, 40);
            this.btnPagar.Text = "$ Pagar orden"; this.btnPagar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnPagar.ForeColor = System.Drawing.Color.White;
            this.btnPagar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPagar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnPagar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPagar.Click += new System.EventHandler(this.btnPagar_Click);

            this.btnCancelar.Location = new System.Drawing.Point(950, 605); this.btnCancelar.Size = new System.Drawing.Size(110, 40);
            this.btnCancelar.Text = "Cancelar"; this.btnCancelar.BackColor = System.Drawing.Color.Gray;
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            this.btnSalir.Location = new System.Drawing.Point(1070, 605); this.btnSalir.Size = new System.Drawing.Size(70, 40);
            this.btnSalir.Text = "Salir"; this.btnSalir.BackColor = System.Drawing.Color.IndianRed;
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1160, 665);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.grpOrigen);
            this.Controls.Add(this.grpLineas);
            this.Controls.Add(this.btnGenerar);
            this.Controls.Add(this.btnPagar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMGenerarOrdenCompra_GO44";
            this.Text = "Generar Orden de Compra";
            this.Load += new System.EventHandler(this.FRMGenerarOrdenCompra_GO44_Load);

            this.grpOrigen.ResumeLayout(false); this.grpOrigen.PerformLayout();
            this.grpLineas.ResumeLayout(false); this.grpLineas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox grpOrigen;
        private System.Windows.Forms.Label lblSolicitud;
        private System.Windows.Forms.ComboBox cmbSolicitud;
        private System.Windows.Forms.Button btnCargarSC;
        private System.Windows.Forms.Label lblAvisoSinSC;
        private System.Windows.Forms.Label lblProveedor;
        private System.Windows.Forms.Label lblSCInfo;
        private System.Windows.Forms.Label lblEntrega;
        private System.Windows.Forms.DateTimePicker dtpEntrega;

        private System.Windows.Forms.GroupBox grpLineas;
        private System.Windows.Forms.DataGridView dgvLineas;
        private System.Windows.Forms.Label lblTotal;

        private System.Windows.Forms.Button btnGenerar;
        private System.Windows.Forms.Button btnPagar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnSalir;
    }
}
