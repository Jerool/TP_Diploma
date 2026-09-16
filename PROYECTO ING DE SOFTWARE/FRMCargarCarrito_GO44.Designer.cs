namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMCargarCarrito_GO44
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

            // Cliente
            this.grpCliente = new System.Windows.Forms.GroupBox();
            this.lblDniCliente = new System.Windows.Forms.Label();
            this.txtDniCliente = new System.Windows.Forms.TextBox();
            this.btnBuscarCliente = new System.Windows.Forms.Button();
            this.lblClienteInfo = new System.Windows.Forms.Label();

            // Productos
            this.grpProductos = new System.Windows.Forms.GroupBox();
            this.lblBuscarProducto = new System.Windows.Forms.Label();
            this.txtBuscarProducto = new System.Windows.Forms.TextBox();
            this.lblCategoria = new System.Windows.Forms.Label();
            this.cmbCategoria = new System.Windows.Forms.ComboBox();
            this.btnLimpiarBusqueda = new System.Windows.Forms.Button();
            this.dgvProductos = new System.Windows.Forms.DataGridView();
            this.lblCantidad = new System.Windows.Forms.Label();
            this.txtCantidad = new System.Windows.Forms.TextBox();
            this.btnAgregarLinea = new System.Windows.Forms.Button();
            this.lblContadorProductos = new System.Windows.Forms.Label();

            // Carrito
            this.grpCarrito = new System.Windows.Forms.GroupBox();
            this.dgvLineas = new System.Windows.Forms.DataGridView();
            this.btnQuitarLinea = new System.Windows.Forms.Button();
            this.lblCantItems = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();

            // Botones finales
            this.btnConfirmar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();

            this.grpCliente.SuspendLayout();
            this.grpProductos.SuspendLayout();
            this.grpCarrito.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).BeginInit();
            this.SuspendLayout();

            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTitulo.Location = new System.Drawing.Point(20, 10);
            this.lblTitulo.Text = "Cargar Carrito";

            //
            // grpCliente
            //
            this.grpCliente.Location = new System.Drawing.Point(20, 45);
            this.grpCliente.Size     = new System.Drawing.Size(1140, 70);
            this.grpCliente.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCliente.Text = "Cliente";
            this.grpCliente.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.grpCliente.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpCliente.Controls.Add(this.lblDniCliente);
            this.grpCliente.Controls.Add(this.txtDniCliente);
            this.grpCliente.Controls.Add(this.btnBuscarCliente);
            this.grpCliente.Controls.Add(this.lblClienteInfo);

            this.lblDniCliente.AutoSize = true; this.lblDniCliente.Location = new System.Drawing.Point(15, 30); this.lblDniCliente.Text = "DNI:";
            this.lblDniCliente.ForeColor = System.Drawing.Color.Black;
            this.txtDniCliente.Location = new System.Drawing.Point(60, 27); this.txtDniCliente.Size = new System.Drawing.Size(150, 22);

            this.btnBuscarCliente.Location = new System.Drawing.Point(220, 25); this.btnBuscarCliente.Size = new System.Drawing.Size(100, 30);
            this.btnBuscarCliente.Text = "Buscar";
            this.btnBuscarCliente.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnBuscarCliente.ForeColor = System.Drawing.Color.White;
            this.btnBuscarCliente.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscarCliente.Click += new System.EventHandler(this.btnBuscarCliente_Click);

            this.lblClienteInfo.AutoSize = true;
            this.lblClienteInfo.Location = new System.Drawing.Point(340, 32);
            this.lblClienteInfo.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblClienteInfo.ForeColor = System.Drawing.Color.Firebrick;
            this.lblClienteInfo.Text = "Cliente: (sin asignar)";

            //
            // grpProductos
            //
            this.grpProductos.Location = new System.Drawing.Point(20, 125);
            this.grpProductos.Size     = new System.Drawing.Size(1140, 310);
            this.grpProductos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpProductos.Text = "Productos disponibles";
            this.grpProductos.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.grpProductos.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpProductos.Controls.Add(this.lblBuscarProducto);
            this.grpProductos.Controls.Add(this.txtBuscarProducto);
            this.grpProductos.Controls.Add(this.lblCategoria);
            this.grpProductos.Controls.Add(this.cmbCategoria);
            this.grpProductos.Controls.Add(this.btnLimpiarBusqueda);
            this.grpProductos.Controls.Add(this.dgvProductos);
            this.grpProductos.Controls.Add(this.lblCantidad);
            this.grpProductos.Controls.Add(this.txtCantidad);
            this.grpProductos.Controls.Add(this.btnAgregarLinea);
            this.grpProductos.Controls.Add(this.lblContadorProductos);

            this.lblBuscarProducto.AutoSize = true; this.lblBuscarProducto.Location = new System.Drawing.Point(15, 30); this.lblBuscarProducto.Text = "Buscar:";
            this.lblBuscarProducto.ForeColor = System.Drawing.Color.Black;
            this.txtBuscarProducto.Location = new System.Drawing.Point(70, 27); this.txtBuscarProducto.Size = new System.Drawing.Size(250, 22);
            this.txtBuscarProducto.TextChanged += new System.EventHandler(this.txtBuscarProducto_TextChanged);

            this.lblCategoria.AutoSize = true; this.lblCategoria.Location = new System.Drawing.Point(340, 30); this.lblCategoria.Text = "Categoría:";
            this.lblCategoria.ForeColor = System.Drawing.Color.Black;
            this.cmbCategoria.Location = new System.Drawing.Point(415, 27); this.cmbCategoria.Size = new System.Drawing.Size(200, 22);
            this.cmbCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategoria.SelectedIndexChanged += new System.EventHandler(this.cmbCategoria_SelectedIndexChanged);

            this.btnLimpiarBusqueda.Location = new System.Drawing.Point(630, 25); this.btnLimpiarBusqueda.Size = new System.Drawing.Size(100, 28);
            this.btnLimpiarBusqueda.Text = "Limpiar filtro";
            this.btnLimpiarBusqueda.BackColor = System.Drawing.Color.Silver;
            this.btnLimpiarBusqueda.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiarBusqueda.Click += new System.EventHandler(this.btnLimpiarBusqueda_Click);

            // Grilla productos — ocupa todo el ancho del grupo
            this.dgvProductos.Location = new System.Drawing.Point(15, 60);
            this.dgvProductos.Size     = new System.Drawing.Size(1110, 195);
            this.dgvProductos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));

            this.lblContadorProductos.AutoSize = true;
            this.lblContadorProductos.Location = new System.Drawing.Point(15, 268);
            this.lblContadorProductos.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblContadorProductos.ForeColor = System.Drawing.Color.DimGray;
            this.lblContadorProductos.Text = "Productos: 0";
            this.lblContadorProductos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.lblCantidad.AutoSize = true; this.lblCantidad.Location = new System.Drawing.Point(730, 268); this.lblCantidad.Text = "Cantidad:";
            this.lblCantidad.ForeColor = System.Drawing.Color.Black;
            this.lblCantidad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            this.txtCantidad.Location = new System.Drawing.Point(800, 265); this.txtCantidad.Size = new System.Drawing.Size(60, 22);
            this.txtCantidad.Text = "1";
            this.txtCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtCantidad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            this.btnAgregarLinea.Location = new System.Drawing.Point(880, 262); this.btnAgregarLinea.Size = new System.Drawing.Size(230, 32);
            this.btnAgregarLinea.Text = "► Agregar al carrito";
            this.btnAgregarLinea.BackColor = System.Drawing.Color.SeaGreen;
            this.btnAgregarLinea.ForeColor = System.Drawing.Color.White;
            this.btnAgregarLinea.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnAgregarLinea.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarLinea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAgregarLinea.Click += new System.EventHandler(this.btnAgregarLinea_Click);

            //
            // grpCarrito
            //
            this.grpCarrito.Location = new System.Drawing.Point(20, 445);
            this.grpCarrito.Size     = new System.Drawing.Size(1140, 220);
            this.grpCarrito.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right))));
            this.grpCarrito.Text = "Carrito";
            this.grpCarrito.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.grpCarrito.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.grpCarrito.Controls.Add(this.dgvLineas);
            this.grpCarrito.Controls.Add(this.btnQuitarLinea);
            this.grpCarrito.Controls.Add(this.lblCantItems);
            this.grpCarrito.Controls.Add(this.lblTotal);

            this.dgvLineas.Location = new System.Drawing.Point(15, 25);
            this.dgvLineas.Size     = new System.Drawing.Size(1110, 140);
            this.dgvLineas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));

            this.btnQuitarLinea.Location = new System.Drawing.Point(15, 175);
            this.btnQuitarLinea.Size     = new System.Drawing.Size(150, 30);
            this.btnQuitarLinea.Text = "Quitar línea";
            this.btnQuitarLinea.BackColor = System.Drawing.Color.Silver;
            this.btnQuitarLinea.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuitarLinea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnQuitarLinea.Click += new System.EventHandler(this.btnQuitarLinea_Click);

            this.lblCantItems.AutoSize = true;
            this.lblCantItems.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblCantItems.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblCantItems.Location = new System.Drawing.Point(800, 180);
            this.lblCantItems.Text = "Items: 0";
            this.lblCantItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTotal.Location = new System.Drawing.Point(920, 175);
            this.lblTotal.Text = "Total: $ 0.00";
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            //
            // Botones finales
            //
            this.btnConfirmar.Location = new System.Drawing.Point(770, 680); this.btnConfirmar.Size = new System.Drawing.Size(190, 40);
            this.btnConfirmar.Text = "✓ Confirmar Venta";
            this.btnConfirmar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnConfirmar.ForeColor = System.Drawing.Color.White;
            this.btnConfirmar.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.btnConfirmar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirmar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);

            this.btnCancelar.Location = new System.Drawing.Point(975, 680); this.btnCancelar.Size = new System.Drawing.Size(100, 40);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.BackColor = System.Drawing.Color.Silver;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            this.btnSalir.Location = new System.Drawing.Point(1085, 680); this.btnSalir.Size = new System.Drawing.Size(75, 40);
            this.btnSalir.Text = "Salir";
            this.btnSalir.BackColor = System.Drawing.Color.IndianRed;
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            //
            // FRMCargarCarrito_GO44
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(1180, 740);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.grpCliente);
            this.Controls.Add(this.grpProductos);
            this.Controls.Add(this.grpCarrito);
            this.Controls.Add(this.btnConfirmar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnSalir);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMCargarCarrito_GO44";
            this.Text = "Cargar Carrito";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FRMCargarCarrito_GO44_Load);
            this.grpCliente.ResumeLayout(false);
            this.grpCliente.PerformLayout();
            this.grpProductos.ResumeLayout(false);
            this.grpProductos.PerformLayout();
            this.grpCarrito.ResumeLayout(false);
            this.grpCarrito.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;

        private System.Windows.Forms.GroupBox grpCliente;
        private System.Windows.Forms.Label lblDniCliente;
        private System.Windows.Forms.TextBox txtDniCliente;
        private System.Windows.Forms.Button btnBuscarCliente;
        private System.Windows.Forms.Label lblClienteInfo;

        private System.Windows.Forms.GroupBox grpProductos;
        private System.Windows.Forms.Label lblBuscarProducto;
        private System.Windows.Forms.TextBox txtBuscarProducto;
        private System.Windows.Forms.Label lblCategoria;
        private System.Windows.Forms.ComboBox cmbCategoria;
        private System.Windows.Forms.Button btnLimpiarBusqueda;
        private System.Windows.Forms.DataGridView dgvProductos;
        private System.Windows.Forms.Label lblCantidad;
        private System.Windows.Forms.TextBox txtCantidad;
        private System.Windows.Forms.Button btnAgregarLinea;
        private System.Windows.Forms.Label lblContadorProductos;

        private System.Windows.Forms.GroupBox grpCarrito;
        private System.Windows.Forms.DataGridView dgvLineas;
        private System.Windows.Forms.Button btnQuitarLinea;
        private System.Windows.Forms.Label lblCantItems;
        private System.Windows.Forms.Label lblTotal;

        private System.Windows.Forms.Button btnConfirmar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnSalir;
    }
}
