namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMGestionClientes_GO44
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
            this.lblDNI = new System.Windows.Forms.Label();
            this.lblApellido = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.lblTelefono = new System.Windows.Forms.Label();
            this.txtDNI = new System.Windows.Forms.TextBox();
            this.txtApellido = new System.Windows.Forms.TextBox();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtTelefono = new System.Windows.Forms.TextBox();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.btnModificar = new System.Windows.Forms.Button();
            this.btnActivarDesactivar = new System.Windows.Forms.Button();
            this.btnAplicar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.dgvClientes = new System.Windows.Forms.DataGridView();
            this.lblMensaje = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClientes)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTitulo.Location = new System.Drawing.Point(20, 15);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Text = "Gestión de Clientes";
            //
            // Labels
            //
            this.lblDNI.AutoSize = true;      this.lblDNI.Location      = new System.Drawing.Point(20, 60);  this.lblDNI.Text = "DNI:";
            this.lblApellido.AutoSize = true; this.lblApellido.Location = new System.Drawing.Point(20, 90);  this.lblApellido.Text = "Apellido:";
            this.lblNombre.AutoSize = true;   this.lblNombre.Location   = new System.Drawing.Point(20, 120); this.lblNombre.Text = "Nombre:";
            this.lblEmail.AutoSize = true;    this.lblEmail.Location    = new System.Drawing.Point(20, 150); this.lblEmail.Text = "Email:";
            this.lblTelefono.AutoSize = true; this.lblTelefono.Location = new System.Drawing.Point(20, 180); this.lblTelefono.Text = "Teléfono:";
            //
            // TextBoxes
            //
            this.txtDNI.Location      = new System.Drawing.Point(120, 57);  this.txtDNI.Size      = new System.Drawing.Size(200, 22);
            this.txtApellido.Location = new System.Drawing.Point(120, 87);  this.txtApellido.Size = new System.Drawing.Size(200, 22);
            this.txtNombre.Location   = new System.Drawing.Point(120, 117); this.txtNombre.Size   = new System.Drawing.Size(200, 22);
            this.txtEmail.Location    = new System.Drawing.Point(120, 147); this.txtEmail.Size    = new System.Drawing.Size(250, 22);
            this.txtTelefono.Location = new System.Drawing.Point(120, 177); this.txtTelefono.Size = new System.Drawing.Size(200, 22);
            //
            // Botones (columna derecha)
            //
            this.btnNuevo.Location             = new System.Drawing.Point(400, 55);  this.btnNuevo.Size             = new System.Drawing.Size(140, 30);
            this.btnNuevo.Text = "Nuevo Cliente";
            this.btnNuevo.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnNuevo.ForeColor = System.Drawing.Color.White;
            this.btnNuevo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo.Click += new System.EventHandler(this.btnNuevo_Click);

            this.btnModificar.Location         = new System.Drawing.Point(400, 90);  this.btnModificar.Size         = new System.Drawing.Size(140, 30);
            this.btnModificar.Text = "Modificar";
            this.btnModificar.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnModificar.ForeColor = System.Drawing.Color.White;
            this.btnModificar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModificar.Click += new System.EventHandler(this.btnModificar_Click);

            this.btnActivarDesactivar.Location = new System.Drawing.Point(400, 125); this.btnActivarDesactivar.Size = new System.Drawing.Size(140, 30);
            this.btnActivarDesactivar.Text = "Activar / Desactivar";
            this.btnActivarDesactivar.BackColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.btnActivarDesactivar.ForeColor = System.Drawing.Color.White;
            this.btnActivarDesactivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActivarDesactivar.Click += new System.EventHandler(this.btnActivarDesactivar_Click);

            this.btnAplicar.Location  = new System.Drawing.Point(400, 170); this.btnAplicar.Size  = new System.Drawing.Size(140, 30);
            this.btnAplicar.Text = "Aplicar";
            this.btnAplicar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnAplicar.ForeColor = System.Drawing.Color.White;
            this.btnAplicar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAplicar.Click += new System.EventHandler(this.btnAplicar_Click);

            this.btnCancelar.Location = new System.Drawing.Point(560, 170); this.btnCancelar.Size = new System.Drawing.Size(120, 30);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.BackColor = System.Drawing.Color.Silver;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            this.btnSalir.Location    = new System.Drawing.Point(700, 170); this.btnSalir.Size    = new System.Drawing.Size(80, 30);
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
            this.lblMensaje.Location = new System.Drawing.Point(20, 220);
            this.lblMensaje.Text = "Modo: Consulta";
            //
            // dgvClientes
            //
            this.dgvClientes.Location = new System.Drawing.Point(20, 250);
            this.dgvClientes.Size     = new System.Drawing.Size(760, 250);
            this.dgvClientes.Name = "dgvClientes";
            this.dgvClientes.BackgroundColor = System.Drawing.Color.White;
            this.dgvClientes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvClientes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvClientes.SelectionChanged += new System.EventHandler(this.dgvClientes_SelectionChanged);
            //
            // FRMGestionClientes_GO44
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(800, 520);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblDNI);
            this.Controls.Add(this.lblApellido);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.lblTelefono);
            this.Controls.Add(this.txtDNI);
            this.Controls.Add(this.txtApellido);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.txtTelefono);
            this.Controls.Add(this.btnNuevo);
            this.Controls.Add(this.btnModificar);
            this.Controls.Add(this.btnActivarDesactivar);
            this.Controls.Add(this.btnAplicar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.dgvClientes);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FRMGestionClientes_GO44";
            this.Text = "Gestión de Clientes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FRMGestionClientes_GO44_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvClientes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDNI;
        private System.Windows.Forms.Label lblApellido;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Label lblTelefono;
        private System.Windows.Forms.TextBox txtDNI;
        private System.Windows.Forms.TextBox txtApellido;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Button btnModificar;
        private System.Windows.Forms.Button btnActivarDesactivar;
        private System.Windows.Forms.Button btnAplicar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Label lblMensaje;
        private System.Windows.Forms.DataGridView dgvClientes;
    }
}
