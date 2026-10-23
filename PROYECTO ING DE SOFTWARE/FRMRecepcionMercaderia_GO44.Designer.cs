namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMRecepcionMercaderia_GO44
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

            this.grpRecepcion = new System.Windows.Forms.GroupBox();
            this.lblDetalleOC = new System.Windows.Forms.Label();
            this.dgvLineas = new System.Windows.Forms.DataGridView();
            this.lblAyuda = new System.Windows.Forms.Label();
            this.lblObs = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.btnRecibir = new System.Windows.Forms.Button();

            this.btnSalir = new System.Windows.Forms.Button();

            this.grpOrdenes.SuspendLayout();
            this.grpRecepcion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrdenes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).BeginInit();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            // Título
            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Recepción de Mercadería";

            // Grupo Órdenes (arriba)
            this.grpOrdenes.Location = new System.Drawing.Point(20, 45);
            this.grpOrdenes.Size = new System.Drawing.Size(1120, 230);
            this.grpOrdenes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOrdenes.Text = "Órdenes pagadas pendientes de recepción"; this.grpOrdenes.ForeColor = azul;
            this.grpOrdenes.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpOrdenes.Controls.Add(this.dgvOrdenes);
            this.grpOrdenes.Controls.Add(this.lblAvisoSinOrdenes);
            this.grpOrdenes.Controls.Add(this.btnActualizar);

            this.dgvOrdenes.Location = new System.Drawing.Point(15, 30);
            this.dgvOrdenes.Size = new System.Drawing.Size(1090, 150);
            this.dgvOrdenes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right))));

            this.lblAvisoSinOrdenes.AutoSize = true; this.lblAvisoSinOrdenes.Location = new System.Drawing.Point(15, 190);
            this.lblAvisoSinOrdenes.ForeColor = System.Drawing.Color.Firebrick;
            this.lblAvisoSinOrdenes.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.lblAvisoSinOrdenes.Text = "No hay órdenes pagadas pendientes de recepción.";
            this.lblAvisoSinOrdenes.Visible = false;

            this.btnActualizar.Location = new System.Drawing.Point(985, 188); this.btnActualizar.Size = new System.Drawing.Size(120, 30);
            this.btnActualizar.Text = "↻ Actualizar"; this.btnActualizar.BackColor = System.Drawing.Color.Gray;
            this.btnActualizar.ForeColor = System.Drawing.Color.White;
            this.btnActualizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActualizar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);

            // Grupo Recepción (abajo)
            this.grpRecepcion.Location = new System.Drawing.Point(20, 285);
            this.grpRecepcion.Size = new System.Drawing.Size(1120, 335);
            this.grpRecepcion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpRecepcion.Text = "Detalle de recepción"; this.grpRecepcion.ForeColor = azul;
            this.grpRecepcion.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpRecepcion.Controls.Add(this.lblDetalleOC);
            this.grpRecepcion.Controls.Add(this.dgvLineas);
            this.grpRecepcion.Controls.Add(this.lblAyuda);
            this.grpRecepcion.Controls.Add(this.lblObs);
            this.grpRecepcion.Controls.Add(this.txtObservaciones);
            this.grpRecepcion.Controls.Add(this.btnRecibir);

            this.lblDetalleOC.AutoSize = true; this.lblDetalleOC.Location = new System.Drawing.Point(15, 28);
            this.lblDetalleOC.ForeColor = System.Drawing.Color.Black; this.lblDetalleOC.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblDetalleOC.Text = "Seleccione una orden de compra pagada";

            this.dgvLineas.Location = new System.Drawing.Point(15, 55);
            this.dgvLineas.Size = new System.Drawing.Size(740, 220);
            this.dgvLineas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));

            this.lblAyuda.AutoSize = true; this.lblAyuda.Location = new System.Drawing.Point(15, 282);
            this.lblAyuda.ForeColor = System.Drawing.Color.DimGray; this.lblAyuda.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblAyuda.Text = "Columna 'Recibida' (amarilla) editable. Las filas en rosa indican faltantes respecto a la OC.";
            this.lblAyuda.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.lblObs.AutoSize = true; this.lblObs.Location = new System.Drawing.Point(775, 55); this.lblObs.Text = "Observaciones:";
            this.lblObs.ForeColor = System.Drawing.Color.Black; this.lblObs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblObs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));

            this.txtObservaciones.Location = new System.Drawing.Point(775, 78); this.txtObservaciones.Size = new System.Drawing.Size(330, 140);
            this.txtObservaciones.Multiline = true; this.txtObservaciones.MaxLength = 400;
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));

            this.btnRecibir.Location = new System.Drawing.Point(775, 230); this.btnRecibir.Size = new System.Drawing.Size(330, 44);
            this.btnRecibir.Text = "✓ Registrar recepción"; this.btnRecibir.BackColor = System.Drawing.Color.SeaGreen;
            this.btnRecibir.ForeColor = System.Drawing.Color.White;
            this.btnRecibir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecibir.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnRecibir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRecibir.Click += new System.EventHandler(this.btnRecibir_Click);

            // Salir
            this.btnSalir.Location = new System.Drawing.Point(1060, 630); this.btnSalir.Size = new System.Drawing.Size(80, 38);
            this.btnSalir.Text = "Salir"; this.btnSalir.BackColor = System.Drawing.Color.IndianRed;
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1160, 680);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.grpOrdenes);
            this.Controls.Add(this.grpRecepcion);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMRecepcionMercaderia_GO44";
            this.Text = "Recepción de Mercadería";
            this.Load += new System.EventHandler(this.FRMRecepcionMercaderia_GO44_Load);

            this.grpOrdenes.ResumeLayout(false); this.grpOrdenes.PerformLayout();
            this.grpRecepcion.ResumeLayout(false); this.grpRecepcion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrdenes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).EndInit();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox grpOrdenes;
        private System.Windows.Forms.DataGridView dgvOrdenes;
        private System.Windows.Forms.Label lblAvisoSinOrdenes;
        private System.Windows.Forms.Button btnActualizar;

        private System.Windows.Forms.GroupBox grpRecepcion;
        private System.Windows.Forms.Label lblDetalleOC;
        private System.Windows.Forms.DataGridView dgvLineas;
        private System.Windows.Forms.Label lblAyuda;
        private System.Windows.Forms.Label lblObs;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.Button btnRecibir;

        private System.Windows.Forms.Button btnSalir;
    }
}
