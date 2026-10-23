namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMPagarOrdenCompra_GO44
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
            this.grpOrdenes = new System.Windows.Forms.GroupBox();
            this.dgvOrdenes = new System.Windows.Forms.DataGridView();
            this.lblAvisoSinOrdenes = new System.Windows.Forms.Label();
            this.btnActualizar = new System.Windows.Forms.Button();

            this.grpPago = new System.Windows.Forms.GroupBox();
            this.lblDetalleOC = new System.Windows.Forms.Label();
            this.lblDetalleTotal = new System.Windows.Forms.Label();
            this.lblCVU = new System.Windows.Forms.Label();
            this.txtCVU = new System.Windows.Forms.TextBox();
            this.lblBanco = new System.Windows.Forms.Label();
            this.txtBanco = new System.Windows.Forms.TextBox();
            this.lblTitular = new System.Windows.Forms.Label();
            this.txtTitular = new System.Windows.Forms.TextBox();
            this.lblMonto = new System.Windows.Forms.Label();
            this.txtMonto = new System.Windows.Forms.TextBox();
            this.lblComprobante = new System.Windows.Forms.Label();
            this.txtComprobante = new System.Windows.Forms.TextBox();
            this.btnPagar = new System.Windows.Forms.Button();

            this.btnSalir = new System.Windows.Forms.Button();

            this.grpOrdenes.SuspendLayout();
            this.grpPago.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrdenes)).BeginInit();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            // Título
            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Pagar Orden de Compra";

            // Grupo Órdenes (izquierda)
            this.grpOrdenes.Location = new System.Drawing.Point(20, 45);
            this.grpOrdenes.Size = new System.Drawing.Size(700, 580);
            this.grpOrdenes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOrdenes.Text = "Órdenes pendientes de pago"; this.grpOrdenes.ForeColor = azul;
            this.grpOrdenes.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpOrdenes.Controls.Add(this.dgvOrdenes);
            this.grpOrdenes.Controls.Add(this.lblAvisoSinOrdenes);
            this.grpOrdenes.Controls.Add(this.btnActualizar);

            this.dgvOrdenes.Location = new System.Drawing.Point(15, 30);
            this.dgvOrdenes.Size = new System.Drawing.Size(670, 500);
            this.dgvOrdenes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblAvisoSinOrdenes.AutoSize = true; this.lblAvisoSinOrdenes.Location = new System.Drawing.Point(15, 540);
            this.lblAvisoSinOrdenes.ForeColor = System.Drawing.Color.Firebrick;
            this.lblAvisoSinOrdenes.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.lblAvisoSinOrdenes.Text = "No hay órdenes de compra pendientes de pago.";
            this.lblAvisoSinOrdenes.Visible = false;
            this.lblAvisoSinOrdenes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.btnActualizar.Location = new System.Drawing.Point(560, 538); this.btnActualizar.Size = new System.Drawing.Size(120, 30);
            this.btnActualizar.Text = "↻ Actualizar"; this.btnActualizar.BackColor = System.Drawing.Color.Gray;
            this.btnActualizar.ForeColor = System.Drawing.Color.White;
            this.btnActualizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActualizar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);

            // Grupo Pago (derecha)
            this.grpPago.Location = new System.Drawing.Point(740, 45);
            this.grpPago.Size = new System.Drawing.Size(400, 580);
            this.grpPago.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPago.Text = "Datos del pago"; this.grpPago.ForeColor = azul;
            this.grpPago.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpPago.Controls.Add(this.lblDetalleOC);
            this.grpPago.Controls.Add(this.lblDetalleTotal);
            this.grpPago.Controls.Add(this.lblCVU);
            this.grpPago.Controls.Add(this.txtCVU);
            this.grpPago.Controls.Add(this.lblBanco);
            this.grpPago.Controls.Add(this.txtBanco);
            this.grpPago.Controls.Add(this.lblTitular);
            this.grpPago.Controls.Add(this.txtTitular);
            this.grpPago.Controls.Add(this.lblMonto);
            this.grpPago.Controls.Add(this.txtMonto);
            this.grpPago.Controls.Add(this.lblComprobante);
            this.grpPago.Controls.Add(this.txtComprobante);
            this.grpPago.Controls.Add(this.btnPagar);

            this.lblDetalleOC.AutoSize = true; this.lblDetalleOC.Location = new System.Drawing.Point(15, 35);
            this.lblDetalleOC.ForeColor = System.Drawing.Color.Black; this.lblDetalleOC.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblDetalleOC.Text = "Seleccione una orden de compra";

            this.lblDetalleTotal.AutoSize = true; this.lblDetalleTotal.Location = new System.Drawing.Point(15, 65);
            this.lblDetalleTotal.ForeColor = azul; this.lblDetalleTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblDetalleTotal.Text = "Total a pagar: —";

            this.lblCVU.AutoSize = true; this.lblCVU.Location = new System.Drawing.Point(15, 110); this.lblCVU.Text = "CVU:";
            this.lblCVU.ForeColor = System.Drawing.Color.Black; this.lblCVU.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCVU.Location = new System.Drawing.Point(130, 107); this.txtCVU.Size = new System.Drawing.Size(250, 22); this.txtCVU.ReadOnly = true;

            this.lblBanco.AutoSize = true; this.lblBanco.Location = new System.Drawing.Point(15, 145); this.lblBanco.Text = "Banco:";
            this.lblBanco.ForeColor = System.Drawing.Color.Black; this.lblBanco.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBanco.Location = new System.Drawing.Point(130, 142); this.txtBanco.Size = new System.Drawing.Size(250, 22); this.txtBanco.ReadOnly = true;

            this.lblTitular.AutoSize = true; this.lblTitular.Location = new System.Drawing.Point(15, 180); this.lblTitular.Text = "Titular:";
            this.lblTitular.ForeColor = System.Drawing.Color.Black; this.lblTitular.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTitular.Location = new System.Drawing.Point(130, 177); this.txtTitular.Size = new System.Drawing.Size(250, 22); this.txtTitular.ReadOnly = true;

            this.lblMonto.AutoSize = true; this.lblMonto.Location = new System.Drawing.Point(15, 225); this.lblMonto.Text = "Monto abonado:";
            this.lblMonto.ForeColor = System.Drawing.Color.Black; this.lblMonto.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.txtMonto.Location = new System.Drawing.Point(130, 222); this.txtMonto.Size = new System.Drawing.Size(150, 22);
            this.txtMonto.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

            this.lblComprobante.AutoSize = true; this.lblComprobante.Location = new System.Drawing.Point(15, 260); this.lblComprobante.Text = "N° comprobante:";
            this.lblComprobante.ForeColor = System.Drawing.Color.Black; this.lblComprobante.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.txtComprobante.Location = new System.Drawing.Point(130, 257); this.txtComprobante.Size = new System.Drawing.Size(250, 22); this.txtComprobante.MaxLength = 50;

            this.btnPagar.Location = new System.Drawing.Point(130, 310); this.btnPagar.Size = new System.Drawing.Size(250, 42);
            this.btnPagar.Text = "✓ Registrar pago"; this.btnPagar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnPagar.ForeColor = System.Drawing.Color.White;
            this.btnPagar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPagar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnPagar.Click += new System.EventHandler(this.btnPagar_Click);

            // Salir
            this.btnSalir.Location = new System.Drawing.Point(1060, 635); this.btnSalir.Size = new System.Drawing.Size(80, 38);
            this.btnSalir.Text = "Salir"; this.btnSalir.BackColor = System.Drawing.Color.IndianRed;
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1160, 685);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.grpOrdenes);
            this.Controls.Add(this.grpPago);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMPagarOrdenCompra_GO44";
            this.Text = "Pagar Orden de Compra";
            this.Load += new System.EventHandler(this.FRMPagarOrdenCompra_GO44_Load);

            this.grpOrdenes.ResumeLayout(false); this.grpOrdenes.PerformLayout();
            this.grpPago.ResumeLayout(false); this.grpPago.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrdenes)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox grpOrdenes;
        private System.Windows.Forms.DataGridView dgvOrdenes;
        private System.Windows.Forms.Label lblAvisoSinOrdenes;
        private System.Windows.Forms.Button btnActualizar;

        private System.Windows.Forms.GroupBox grpPago;
        private System.Windows.Forms.Label lblDetalleOC;
        private System.Windows.Forms.Label lblDetalleTotal;
        private System.Windows.Forms.Label lblCVU;
        private System.Windows.Forms.TextBox txtCVU;
        private System.Windows.Forms.Label lblBanco;
        private System.Windows.Forms.TextBox txtBanco;
        private System.Windows.Forms.Label lblTitular;
        private System.Windows.Forms.TextBox txtTitular;
        private System.Windows.Forms.Label lblMonto;
        private System.Windows.Forms.TextBox txtMonto;
        private System.Windows.Forms.Label lblComprobante;
        private System.Windows.Forms.TextBox txtComprobante;
        private System.Windows.Forms.Button btnPagar;

        private System.Windows.Forms.Button btnSalir;
    }
}
