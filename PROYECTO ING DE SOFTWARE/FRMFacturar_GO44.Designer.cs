namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMFacturar_GO44
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
            this.lblTitulo   = new System.Windows.Forms.Label();
            this.grpCliente  = new System.Windows.Forms.GroupBox();
            this.lblDni      = new System.Windows.Forms.Label();
            this.txtDni      = new System.Windows.Forms.TextBox();
            this.btnBuscar   = new System.Windows.Forms.Button();
            this.lblCliente  = new System.Windows.Forms.Label();
            this.grpCarrito  = new System.Windows.Forms.GroupBox();
            this.dgvLineas   = new System.Windows.Forms.DataGridView();
            this.lblSubtotal = new System.Windows.Forms.Label();
            this.lblIVA      = new System.Windows.Forms.Label();
            this.lblTotal    = new System.Windows.Forms.Label();
            this.btnCobrar   = new System.Windows.Forms.Button();
            this.btnImprimir = new System.Windows.Forms.Button();
            this.btnSalir    = new System.Windows.Forms.Button();

            this.grpCliente.SuspendLayout();
            this.grpCarrito.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).BeginInit();
            this.SuspendLayout();

            // Título
            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTitulo.Text = "Generar Factura";

            // GrupoCliente
            this.grpCliente.Location = new System.Drawing.Point(20, 45);
            this.grpCliente.Size = new System.Drawing.Size(1120, 70);
            this.grpCliente.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCliente.Text = "Cliente";
            this.grpCliente.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.grpCliente.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpCliente.Controls.Add(this.lblDni);
            this.grpCliente.Controls.Add(this.txtDni);
            this.grpCliente.Controls.Add(this.btnBuscar);
            this.grpCliente.Controls.Add(this.lblCliente);

            this.lblDni.AutoSize = true; this.lblDni.Location = new System.Drawing.Point(15, 30); this.lblDni.Text = "DNI:";
            this.lblDni.ForeColor = System.Drawing.Color.Black;
            this.txtDni.Location = new System.Drawing.Point(60, 27); this.txtDni.Size = new System.Drawing.Size(160, 22);

            this.btnBuscar.Location = new System.Drawing.Point(230, 25); this.btnBuscar.Size = new System.Drawing.Size(100, 30);
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnBuscar.ForeColor = System.Drawing.Color.White;
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);

            this.lblCliente.AutoSize = true; this.lblCliente.Location = new System.Drawing.Point(350, 32);
            this.lblCliente.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblCliente.ForeColor = System.Drawing.Color.Firebrick;
            this.lblCliente.Text = "Cliente: (sin asignar)";

            // GrupoCarrito
            this.grpCarrito.Location = new System.Drawing.Point(20, 125);
            this.grpCarrito.Size = new System.Drawing.Size(1120, 400);
            this.grpCarrito.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCarrito.Text = "Detalle del carrito a facturar";
            this.grpCarrito.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.grpCarrito.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpCarrito.Controls.Add(this.dgvLineas);
            this.grpCarrito.Controls.Add(this.lblSubtotal);
            this.grpCarrito.Controls.Add(this.lblIVA);
            this.grpCarrito.Controls.Add(this.lblTotal);

            this.dgvLineas.Location = new System.Drawing.Point(15, 25);
            this.dgvLineas.Size = new System.Drawing.Size(1090, 300);
            this.dgvLineas.Name = "dgvLineas";
            this.dgvLineas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblSubtotal.AutoSize = true;
            this.lblSubtotal.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblSubtotal.ForeColor = System.Drawing.Color.Black;
            this.lblSubtotal.Location = new System.Drawing.Point(680, 340);
            this.lblSubtotal.Text = "Subtotal: $ 0,00";
            this.lblSubtotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            this.lblIVA.AutoSize = true;
            this.lblIVA.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblIVA.ForeColor = System.Drawing.Color.Black;
            this.lblIVA.Location = new System.Drawing.Point(680, 360);
            this.lblIVA.Text = "IVA: $ 0,00";
            this.lblIVA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.SeaGreen;
            this.lblTotal.Location = new System.Drawing.Point(830, 350);
            this.lblTotal.Text = "TOTAL: $ 0,00";
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            // Botones
            this.btnCobrar.Location = new System.Drawing.Point(740, 540); this.btnCobrar.Size = new System.Drawing.Size(180, 40);
            this.btnCobrar.Text = "$ Cobrar venta";
            this.btnCobrar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnCobrar.ForeColor = System.Drawing.Color.White;
            this.btnCobrar.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.btnCobrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCobrar.Enabled = false;
            this.btnCobrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCobrar.Click += new System.EventHandler(this.btnCobrar_Click);

            this.btnImprimir.Location = new System.Drawing.Point(935, 540); this.btnImprimir.Size = new System.Drawing.Size(140, 40);
            this.btnImprimir.Text = "🖨 Imprimir factura";
            this.btnImprimir.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnImprimir.ForeColor = System.Drawing.Color.White;
            this.btnImprimir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImprimir.Enabled = false;
            this.btnImprimir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImprimir.Click += new System.EventHandler(this.btnImprimir_Click);

            this.btnSalir.Location = new System.Drawing.Point(1085, 540); this.btnSalir.Size = new System.Drawing.Size(60, 40);
            this.btnSalir.Text = "Salir";
            this.btnSalir.BackColor = System.Drawing.Color.IndianRed;
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1160, 600);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.grpCliente);
            this.Controls.Add(this.grpCarrito);
            this.Controls.Add(this.btnCobrar);
            this.Controls.Add(this.btnImprimir);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMFacturar_GO44";
            this.Text = "Generar Factura";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FRMFacturar_GO44_Load);
            this.grpCliente.ResumeLayout(false); this.grpCliente.PerformLayout();
            this.grpCarrito.ResumeLayout(false); this.grpCarrito.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox grpCliente;
        private System.Windows.Forms.Label lblDni;
        private System.Windows.Forms.TextBox txtDni;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Label lblCliente;
        private System.Windows.Forms.GroupBox grpCarrito;
        private System.Windows.Forms.DataGridView dgvLineas;
        private System.Windows.Forms.Label lblSubtotal;
        private System.Windows.Forms.Label lblIVA;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnCobrar;
        private System.Windows.Forms.Button btnImprimir;
        private System.Windows.Forms.Button btnSalir;
    }
}
