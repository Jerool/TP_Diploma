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

            // Campos
            this.lblCodigo      = new System.Windows.Forms.Label();
            this.lblNombre      = new System.Windows.Forms.Label();
            this.lblCategoria2  = new System.Windows.Forms.Label();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.lblPrecio      = new System.Windows.Forms.Label();
            this.lblStockActual = new System.Windows.Forms.Label();
            this.lblStockMinimo = new System.Windows.Forms.Label();
            this.txtCodigo      = new System.Windows.Forms.TextBox();
            this.txtNombre      = new System.Windows.Forms.TextBox();
            this.txtCategoria   = new System.Windows.Forms.TextBox();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.txtPrecio      = new System.Windows.Forms.TextBox();
            this.txtStockActual = new System.Windows.Forms.TextBox();
            this.txtStockMinimo = new System.Windows.Forms.TextBox();

            // Botones ABM
            this.btnNuevo           = new System.Windows.Forms.Button();
            this.btnModificarPrecio = new System.Windows.Forms.Button();
            this.btnBajaAlta        = new System.Windows.Forms.Button();
            this.btnAplicar         = new System.Windows.Forms.Button();
            this.btnCancelar        = new System.Windows.Forms.Button();
            this.btnSalir           = new System.Windows.Forms.Button();

            // Filtros y grilla
            this.lblBuscarCodigo = new System.Windows.Forms.Label();
            this.txtBuscarCodigo = new System.Windows.Forms.TextBox();
            this.lblCategoria    = new System.Windows.Forms.Label();
            this.cmbCategoria    = new System.Windows.Forms.ComboBox();
            this.rbSoloActivos   = new System.Windows.Forms.RadioButton();
            this.rbTodos         = new System.Windows.Forms.RadioButton();
            this.dgvComponentes  = new System.Windows.Forms.DataGridView();
            this.lblTotal        = new System.Windows.Forms.Label();
            this.lblMensaje      = new System.Windows.Forms.Label();

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
            // Campos (columna izquierda)
            //
            this.lblCodigo.AutoSize = true;      this.lblCodigo.Location      = new System.Drawing.Point(20, 60);  this.lblCodigo.Text      = "Código:";
            this.lblNombre.AutoSize = true;      this.lblNombre.Location      = new System.Drawing.Point(20, 90);  this.lblNombre.Text      = "Nombre:";
            this.lblCategoria2.AutoSize = true;  this.lblCategoria2.Location  = new System.Drawing.Point(20, 120); this.lblCategoria2.Text  = "Categoría:";
            this.lblDescripcion.AutoSize = true; this.lblDescripcion.Location = new System.Drawing.Point(20, 150); this.lblDescripcion.Text = "Descripción:";
            this.lblPrecio.AutoSize = true;      this.lblPrecio.Location      = new System.Drawing.Point(20, 180); this.lblPrecio.Text      = "Precio:";
            this.lblStockActual.AutoSize = true; this.lblStockActual.Location = new System.Drawing.Point(20, 210); this.lblStockActual.Text = "Stock Actual:";
            this.lblStockMinimo.AutoSize = true; this.lblStockMinimo.Location = new System.Drawing.Point(20, 240); this.lblStockMinimo.Text = "Stock Mínimo:";

            this.txtCodigo.Location      = new System.Drawing.Point(120, 57);  this.txtCodigo.Size      = new System.Drawing.Size(200, 22);
            this.txtNombre.Location      = new System.Drawing.Point(120, 87);  this.txtNombre.Size      = new System.Drawing.Size(300, 22);
            this.txtCategoria.Location   = new System.Drawing.Point(120, 117); this.txtCategoria.Size   = new System.Drawing.Size(200, 22);
            this.txtDescripcion.Location = new System.Drawing.Point(120, 147); this.txtDescripcion.Size = new System.Drawing.Size(400, 22);
            this.txtPrecio.Location      = new System.Drawing.Point(120, 177); this.txtPrecio.Size      = new System.Drawing.Size(150, 22);
            this.txtStockActual.Location = new System.Drawing.Point(120, 207); this.txtStockActual.Size = new System.Drawing.Size(80, 22);
            this.txtStockMinimo.Location = new System.Drawing.Point(120, 237); this.txtStockMinimo.Size = new System.Drawing.Size(80, 22);

            //
            // Botones ABM (columna derecha)
            //
            this.btnNuevo.Location           = new System.Drawing.Point(560, 55); this.btnNuevo.Size           = new System.Drawing.Size(160, 30);
            this.btnNuevo.Text = "Nuevo Producto";
            this.btnNuevo.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnNuevo.ForeColor = System.Drawing.Color.White;
            this.btnNuevo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo.Click += new System.EventHandler(this.btnNuevo_Click);

            this.btnModificarPrecio.Location = new System.Drawing.Point(560, 90); this.btnModificarPrecio.Size = new System.Drawing.Size(160, 30);
            this.btnModificarPrecio.Text = "Modificar Precio";
            this.btnModificarPrecio.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnModificarPrecio.ForeColor = System.Drawing.Color.White;
            this.btnModificarPrecio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModificarPrecio.Click += new System.EventHandler(this.btnModificarPrecio_Click);

            this.btnBajaAlta.Location        = new System.Drawing.Point(560, 125); this.btnBajaAlta.Size        = new System.Drawing.Size(160, 30);
            this.btnBajaAlta.Text = "Baja / Reactivar";
            this.btnBajaAlta.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnBajaAlta.ForeColor = System.Drawing.Color.White;
            this.btnBajaAlta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBajaAlta.Click += new System.EventHandler(this.btnBajaAlta_Click);

            this.btnAplicar.Location  = new System.Drawing.Point(560, 175); this.btnAplicar.Size  = new System.Drawing.Size(160, 30);
            this.btnAplicar.Text = "Aplicar";
            this.btnAplicar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnAplicar.ForeColor = System.Drawing.Color.White;
            this.btnAplicar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAplicar.Click += new System.EventHandler(this.btnAplicar_Click);

            this.btnCancelar.Location = new System.Drawing.Point(740, 175); this.btnCancelar.Size = new System.Drawing.Size(110, 30);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.BackColor = System.Drawing.Color.Silver;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            this.btnSalir.Location    = new System.Drawing.Point(870, 175); this.btnSalir.Size    = new System.Drawing.Size(80, 30);
            this.btnSalir.Text = "Salir";
            this.btnSalir.BackColor = System.Drawing.Color.IndianRed;
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            //
            // lblMensaje
            //
            this.lblMensaje.AutoSize = true;
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblMensaje.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblMensaje.Location = new System.Drawing.Point(20, 285);
            this.lblMensaje.Text = "Modo: Consulta";

            //
            // Filtros
            //
            this.lblBuscarCodigo.AutoSize = true; this.lblBuscarCodigo.Location = new System.Drawing.Point(20, 320); this.lblBuscarCodigo.Text = "Buscar (código/nombre):";
            this.txtBuscarCodigo.Location = new System.Drawing.Point(170, 317); this.txtBuscarCodigo.Size = new System.Drawing.Size(200, 22);
            this.txtBuscarCodigo.TextChanged += new System.EventHandler(this.txtBuscarCodigo_TextChanged);

            this.lblCategoria.AutoSize = true; this.lblCategoria.Location = new System.Drawing.Point(390, 320); this.lblCategoria.Text = "Categoría:";
            this.cmbCategoria.Location = new System.Drawing.Point(465, 317); this.cmbCategoria.Size = new System.Drawing.Size(180, 22);
            this.cmbCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategoria.SelectedIndexChanged += new System.EventHandler(this.cmbCategoria_SelectedIndexChanged);

            this.rbSoloActivos.Location = new System.Drawing.Point(680, 315); this.rbSoloActivos.AutoSize = true;
            this.rbSoloActivos.Text = "Solo activos"; this.rbSoloActivos.Checked = true;
            this.rbSoloActivos.CheckedChanged += new System.EventHandler(this.rbSoloActivos_CheckedChanged);

            this.rbTodos.Location = new System.Drawing.Point(790, 315); this.rbTodos.AutoSize = true;
            this.rbTodos.Text = "Todos";
            this.rbTodos.CheckedChanged += new System.EventHandler(this.rbTodos_CheckedChanged);

            //
            // dgvComponentes
            //
            this.dgvComponentes.Location = new System.Drawing.Point(20, 355);
            this.dgvComponentes.Size = new System.Drawing.Size(1000, 260);
            this.dgvComponentes.BackgroundColor = System.Drawing.Color.White;
            this.dgvComponentes.Name = "dgvComponentes";
            this.dgvComponentes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvComponentes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvComponentes.SelectionChanged += new System.EventHandler(this.dgvComponentes_SelectionChanged);

            //
            // lblTotal
            //
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTotal.Location = new System.Drawing.Point(20, 625);
            this.lblTotal.Text = "Total: 0";
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            //
            // FRMGestionProductos_GO44
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1040, 655);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblCodigo);      this.Controls.Add(this.txtCodigo);
            this.Controls.Add(this.lblNombre);      this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblCategoria2);  this.Controls.Add(this.txtCategoria);
            this.Controls.Add(this.lblDescripcion); this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.lblPrecio);      this.Controls.Add(this.txtPrecio);
            this.Controls.Add(this.lblStockActual); this.Controls.Add(this.txtStockActual);
            this.Controls.Add(this.lblStockMinimo); this.Controls.Add(this.txtStockMinimo);
            this.Controls.Add(this.btnNuevo);
            this.Controls.Add(this.btnModificarPrecio);
            this.Controls.Add(this.btnBajaAlta);
            this.Controls.Add(this.btnAplicar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.lblBuscarCodigo);
            this.Controls.Add(this.txtBuscarCodigo);
            this.Controls.Add(this.lblCategoria);
            this.Controls.Add(this.cmbCategoria);
            this.Controls.Add(this.rbSoloActivos);
            this.Controls.Add(this.rbTodos);
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

        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblCategoria2;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.Label lblPrecio;
        private System.Windows.Forms.Label lblStockActual;
        private System.Windows.Forms.Label lblStockMinimo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.TextBox txtCategoria;
        private System.Windows.Forms.TextBox txtDescripcion;
        private System.Windows.Forms.TextBox txtPrecio;
        private System.Windows.Forms.TextBox txtStockActual;
        private System.Windows.Forms.TextBox txtStockMinimo;

        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Button btnModificarPrecio;
        private System.Windows.Forms.Button btnBajaAlta;
        private System.Windows.Forms.Button btnAplicar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnSalir;

        private System.Windows.Forms.Label lblBuscarCodigo;
        private System.Windows.Forms.TextBox txtBuscarCodigo;
        private System.Windows.Forms.Label lblCategoria;
        private System.Windows.Forms.ComboBox cmbCategoria;
        private System.Windows.Forms.RadioButton rbSoloActivos;
        private System.Windows.Forms.RadioButton rbTodos;
        private System.Windows.Forms.DataGridView dgvComponentes;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblMensaje;
    }
}
