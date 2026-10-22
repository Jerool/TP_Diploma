namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMRegistrarProveedor_GO44
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
            this.lblCUIT = new System.Windows.Forms.Label();
            this.txtCUIT = new System.Windows.Forms.TextBox();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblApellido = new System.Windows.Forms.Label();
            this.txtApellido = new System.Windows.Forms.TextBox();
            this.lblEmpresa = new System.Windows.Forms.Label();
            this.txtEmpresa = new System.Windows.Forms.TextBox();
            this.lblTelefono = new System.Windows.Forms.Label();
            this.txtTelefono = new System.Windows.Forms.TextBox();
            this.lblCVU = new System.Windows.Forms.Label();
            this.txtCVU = new System.Windows.Forms.TextBox();
            this.lblBanco = new System.Windows.Forms.Label();
            this.txtBanco = new System.Windows.Forms.TextBox();
            this.lblTitular = new System.Windows.Forms.Label();
            this.txtTitular = new System.Windows.Forms.TextBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();

            System.Drawing.Color azul = System.Drawing.Color.FromArgb(13, 71, 161);

            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 15);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = azul;
            this.lblTitulo.Text = "Registrar Proveedor";

            // Fila 1: CUIT
            int y = 65;
            this.lblCUIT.AutoSize = true; this.lblCUIT.Location = new System.Drawing.Point(20, y + 3); this.lblCUIT.Text = "CUIT:";
            this.txtCUIT.Location = new System.Drawing.Point(140, y); this.txtCUIT.Size = new System.Drawing.Size(200, 22);

            // Fila 2: Nombre
            y += 35;
            this.lblNombre.AutoSize = true; this.lblNombre.Location = new System.Drawing.Point(20, y + 3); this.lblNombre.Text = "Nombre:";
            this.txtNombre.Location = new System.Drawing.Point(140, y); this.txtNombre.Size = new System.Drawing.Size(200, 22);

            // Fila 3: Apellido
            y += 35;
            this.lblApellido.AutoSize = true; this.lblApellido.Location = new System.Drawing.Point(20, y + 3); this.lblApellido.Text = "Apellido:";
            this.txtApellido.Location = new System.Drawing.Point(140, y); this.txtApellido.Size = new System.Drawing.Size(200, 22);

            // Fila 4: Empresa
            y += 35;
            this.lblEmpresa.AutoSize = true; this.lblEmpresa.Location = new System.Drawing.Point(20, y + 3); this.lblEmpresa.Text = "Empresa:";
            this.txtEmpresa.Location = new System.Drawing.Point(140, y); this.txtEmpresa.Size = new System.Drawing.Size(380, 22);

            // Fila 5: Teléfono
            y += 35;
            this.lblTelefono.AutoSize = true; this.lblTelefono.Location = new System.Drawing.Point(20, y + 3); this.lblTelefono.Text = "Teléfono:";
            this.txtTelefono.Location = new System.Drawing.Point(140, y); this.txtTelefono.Size = new System.Drawing.Size(200, 22);

            // Fila 6: CVU
            y += 35;
            this.lblCVU.AutoSize = true; this.lblCVU.Location = new System.Drawing.Point(20, y + 3); this.lblCVU.Text = "CVU:";
            this.txtCVU.Location = new System.Drawing.Point(140, y); this.txtCVU.Size = new System.Drawing.Size(380, 22);

            // Fila 7: Banco
            y += 35;
            this.lblBanco.AutoSize = true; this.lblBanco.Location = new System.Drawing.Point(20, y + 3); this.lblBanco.Text = "Banco:";
            this.txtBanco.Location = new System.Drawing.Point(140, y); this.txtBanco.Size = new System.Drawing.Size(380, 22);

            // Fila 8: Titular
            y += 35;
            this.lblTitular.AutoSize = true; this.lblTitular.Location = new System.Drawing.Point(20, y + 3); this.lblTitular.Text = "Titular:";
            this.txtTitular.Location = new System.Drawing.Point(140, y); this.txtTitular.Size = new System.Drawing.Size(380, 22);

            // Botones
            y += 50;
            this.btnGuardar.Location = new System.Drawing.Point(280, y); this.btnGuardar.Size = new System.Drawing.Size(120, 35);
            this.btnGuardar.Text = "Guardar"; this.btnGuardar.BackColor = azul; this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);

            this.btnCancelar.Location = new System.Drawing.Point(410, y); this.btnCancelar.Size = new System.Drawing.Size(110, 35);
            this.btnCancelar.Text = "Cancelar"; this.btnCancelar.BackColor = System.Drawing.Color.IndianRed; this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            // Form
            this.ClientSize = new System.Drawing.Size(540, y + 55);
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Registrar Proveedor";
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblCUIT); this.Controls.Add(this.txtCUIT);
            this.Controls.Add(this.lblNombre); this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblApellido); this.Controls.Add(this.txtApellido);
            this.Controls.Add(this.lblEmpresa); this.Controls.Add(this.txtEmpresa);
            this.Controls.Add(this.lblTelefono); this.Controls.Add(this.txtTelefono);
            this.Controls.Add(this.lblCVU); this.Controls.Add(this.txtCVU);
            this.Controls.Add(this.lblBanco); this.Controls.Add(this.txtBanco);
            this.Controls.Add(this.lblTitular); this.Controls.Add(this.txtTitular);
            this.Controls.Add(this.btnGuardar); this.Controls.Add(this.btnCancelar);
            this.Load += new System.EventHandler(this.FRMRegistrarProveedor_GO44_Load);
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblCUIT; private System.Windows.Forms.TextBox txtCUIT;
        private System.Windows.Forms.Label lblNombre; private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblApellido; private System.Windows.Forms.TextBox txtApellido;
        private System.Windows.Forms.Label lblEmpresa; private System.Windows.Forms.TextBox txtEmpresa;
        private System.Windows.Forms.Label lblTelefono; private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.Label lblCVU; private System.Windows.Forms.TextBox txtCVU;
        private System.Windows.Forms.Label lblBanco; private System.Windows.Forms.TextBox txtBanco;
        private System.Windows.Forms.Label lblTitular; private System.Windows.Forms.TextBox txtTitular;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
