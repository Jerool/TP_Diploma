namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMGestionProductos_GO44
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
            this.lblBuscarCodigo = new System.Windows.Forms.Label();
            this.lblCategoria = new System.Windows.Forms.Label();
            this.txtBuscarCodigo = new System.Windows.Forms.TextBox();
            this.cmbCategoria = new System.Windows.Forms.ComboBox();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.dgvComponentes = new System.Windows.Forms.DataGridView();
            this.lblTotal = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComponentes)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTitulo.Location = new System.Drawing.Point(20, 15);
            this.lblTitulo.Text = "Gestión de Productos";
            //
            // Filtros
            //
            this.lblBuscarCodigo.AutoSize = true; this.lblBuscarCodigo.Location = new System.Drawing.Point(20, 60); this.lblBuscarCodigo.Text = "Buscar código:";
            this.txtBuscarCodigo.Location = new System.Drawing.Point(120, 57); this.txtBuscarCodigo.Size = new System.Drawing.Size(200, 22);
            this.txtBuscarCodigo.TextChanged += new System.EventHandler(this.txtBuscarCodigo_TextChanged);

            this.lblCategoria.AutoSize = true; this.lblCategoria.Location = new System.Drawing.Point(350, 60); this.lblCategoria.Text = "Categoría:";
            this.cmbCategoria.Location = new System.Drawing.Point(430, 57); this.cmbCategoria.Size = new System.Drawing.Size(200, 22);
            this.cmbCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategoria.SelectedIndexChanged += new System.EventHandler(this.cmbCategoria_SelectedIndexChanged);

            this.btnRefrescar.Location = new System.Drawing.Point(650, 55); this.btnRefrescar.Size = new System.Drawing.Size(100, 30);
            this.btnRefrescar.Text = "Refrescar";
            this.btnRefrescar.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnRefrescar.ForeColor = System.Drawing.Color.White;
            this.btnRefrescar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefrescar.Click += new System.EventHandler(this.btnRefrescar_Click);
            //
            // dgvComponentes
            //
            this.dgvComponentes.Location = new System.Drawing.Point(20, 100);
            this.dgvComponentes.Size = new System.Drawing.Size(920, 350);
            this.dgvComponentes.BackgroundColor = System.Drawing.Color.White;
            this.dgvComponentes.Name = "dgvComponentes";
            this.dgvComponentes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvComponentes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            //
            // lblTotal
            //
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTotal.Location = new System.Drawing.Point(20, 460);
            this.lblTotal.Text = "Total componentes: 0";
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            //
            // btnSalir
            //
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Location = new System.Drawing.Point(850, 455);
            this.btnSalir.Size = new System.Drawing.Size(90, 30);
            this.btnSalir.Text = "Salir";
            this.btnSalir.BackColor = System.Drawing.Color.IndianRed;
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            //
            // FRMGestionProductos_GO44
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(960, 500);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblBuscarCodigo);
            this.Controls.Add(this.lblCategoria);
            this.Controls.Add(this.txtBuscarCodigo);
            this.Controls.Add(this.cmbCategoria);
            this.Controls.Add(this.btnRefrescar);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.dgvComponentes);
            this.Controls.Add(this.lblTotal);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMGestionProductos_GO44";
            this.Text = "Gestión de Productos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FRMGestionProductos_GO44_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvComponentes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblBuscarCodigo;
        private System.Windows.Forms.Label lblCategoria;
        private System.Windows.Forms.TextBox txtBuscarCodigo;
        private System.Windows.Forms.ComboBox cmbCategoria;
        private System.Windows.Forms.Button btnRefrescar;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.DataGridView dgvComponentes;
        private System.Windows.Forms.Label lblTotal;
    }
}
