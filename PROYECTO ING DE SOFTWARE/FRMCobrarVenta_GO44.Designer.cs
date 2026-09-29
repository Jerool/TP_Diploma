namespace PROYECTO_ING_DE_SOFTWARE
{
    partial class FRMCobrarVenta_GO44
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
            this.lblTotalLabel = new System.Windows.Forms.Label();
            this.lblTotalPagar = new System.Windows.Forms.Label();
            this.grpMetodo = new System.Windows.Forms.GroupBox();
            this.rbEfectivo = new System.Windows.Forms.RadioButton();
            this.rbTarjeta = new System.Windows.Forms.RadioButton();
            this.grpEfectivo = new System.Windows.Forms.GroupBox();
            this.lblMontoEntregado = new System.Windows.Forms.Label();
            this.txtMontoEntregado = new System.Windows.Forms.TextBox();
            this.lblVuelto = new System.Windows.Forms.Label();
            this.grpTarjeta = new System.Windows.Forms.GroupBox();
            this.lblNroTarjeta = new System.Windows.Forms.Label();
            this.txtNroTarjeta = new System.Windows.Forms.TextBox();
            this.lblVenc = new System.Windows.Forms.Label();
            this.txtMesVenc = new System.Windows.Forms.TextBox();
            this.txtAnioVenc = new System.Windows.Forms.TextBox();
            this.lblSep = new System.Windows.Forms.Label();
            this.lblCvv = new System.Windows.Forms.Label();
            this.txtCvv = new System.Windows.Forms.TextBox();
            this.lblTitular = new System.Windows.Forms.Label();
            this.txtTitularNombre = new System.Windows.Forms.TextBox();
            this.txtTitularApellido = new System.Windows.Forms.TextBox();
            this.lblBanco = new System.Windows.Forms.Label();
            this.txtBanco = new System.Windows.Forms.TextBox();
            this.btnConfirmar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.grpMetodo.SuspendLayout();
            this.grpEfectivo.SuspendLayout();
            this.grpTarjeta.SuspendLayout();
            this.SuspendLayout();

            // Título y total
            this.lblTitulo.AutoSize = true; this.lblTitulo.Location = new System.Drawing.Point(20, 15);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.lblTitulo.Text = "Cobrar Venta";

            this.lblTotalLabel.AutoSize = true; this.lblTotalLabel.Location = new System.Drawing.Point(20, 55); this.lblTotalLabel.Text = "Total a pagar:";
            this.lblTotalLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);

            this.lblTotalPagar.AutoSize = true; this.lblTotalPagar.Location = new System.Drawing.Point(140, 52);
            this.lblTotalPagar.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.lblTotalPagar.ForeColor = System.Drawing.Color.SeaGreen;
            this.lblTotalPagar.Text = "$ 0,00";

            // GroupBox método de pago
            this.grpMetodo.Location = new System.Drawing.Point(20, 100);
            this.grpMetodo.Size = new System.Drawing.Size(510, 60);
            this.grpMetodo.Text = "Método de pago";
            this.grpMetodo.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.grpMetodo.Controls.Add(this.rbEfectivo);
            this.grpMetodo.Controls.Add(this.rbTarjeta);

            this.rbEfectivo.AutoSize = true; this.rbEfectivo.Location = new System.Drawing.Point(15, 25); this.rbEfectivo.Text = "Efectivo";
            this.rbEfectivo.ForeColor = System.Drawing.Color.Black;
            this.rbEfectivo.CheckedChanged += new System.EventHandler(this.rbEfectivo_CheckedChanged);

            this.rbTarjeta.AutoSize = true; this.rbTarjeta.Location = new System.Drawing.Point(150, 25); this.rbTarjeta.Text = "Tarjeta";
            this.rbTarjeta.ForeColor = System.Drawing.Color.Black;
            this.rbTarjeta.CheckedChanged += new System.EventHandler(this.rbTarjeta_CheckedChanged);

            // GroupBox Efectivo
            this.grpEfectivo.Location = new System.Drawing.Point(20, 170);
            this.grpEfectivo.Size = new System.Drawing.Size(510, 80);
            this.grpEfectivo.Text = "Efectivo";
            this.grpEfectivo.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.grpEfectivo.Controls.Add(this.lblMontoEntregado);
            this.grpEfectivo.Controls.Add(this.txtMontoEntregado);
            this.grpEfectivo.Controls.Add(this.lblVuelto);

            this.lblMontoEntregado.AutoSize = true; this.lblMontoEntregado.Location = new System.Drawing.Point(15, 30); this.lblMontoEntregado.Text = "Monto entregado:";
            this.lblMontoEntregado.ForeColor = System.Drawing.Color.Black;
            this.txtMontoEntregado.Location = new System.Drawing.Point(140, 27); this.txtMontoEntregado.Size = new System.Drawing.Size(120, 22);
            this.txtMontoEntregado.TextChanged += new System.EventHandler(this.txtMontoEntregado_TextChanged);
            this.lblVuelto.AutoSize = true; this.lblVuelto.Location = new System.Drawing.Point(285, 30);
            this.lblVuelto.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblVuelto.Text = "Vuelto: —";

            // GroupBox Tarjeta
            this.grpTarjeta.Location = new System.Drawing.Point(20, 170);
            this.grpTarjeta.Size = new System.Drawing.Size(510, 230);
            this.grpTarjeta.Text = "Tarjeta";
            this.grpTarjeta.ForeColor = System.Drawing.Color.FromArgb(13, 71, 161);
            this.grpTarjeta.Visible = false;
            this.grpTarjeta.Controls.Add(this.lblNroTarjeta);
            this.grpTarjeta.Controls.Add(this.txtNroTarjeta);
            this.grpTarjeta.Controls.Add(this.lblVenc);
            this.grpTarjeta.Controls.Add(this.txtMesVenc);
            this.grpTarjeta.Controls.Add(this.txtAnioVenc);
            this.grpTarjeta.Controls.Add(this.lblSep);
            this.grpTarjeta.Controls.Add(this.lblCvv);
            this.grpTarjeta.Controls.Add(this.txtCvv);
            this.grpTarjeta.Controls.Add(this.lblTitular);
            this.grpTarjeta.Controls.Add(this.txtTitularNombre);
            this.grpTarjeta.Controls.Add(this.txtTitularApellido);
            this.grpTarjeta.Controls.Add(this.lblBanco);
            this.grpTarjeta.Controls.Add(this.txtBanco);

            this.lblNroTarjeta.AutoSize = true; this.lblNroTarjeta.Location = new System.Drawing.Point(15, 30); this.lblNroTarjeta.Text = "N° tarjeta:";
            this.lblNroTarjeta.ForeColor = System.Drawing.Color.Black;
            this.txtNroTarjeta.Location = new System.Drawing.Point(140, 27); this.txtNroTarjeta.Size = new System.Drawing.Size(250, 22); this.txtNroTarjeta.MaxLength = 25;

            this.lblVenc.AutoSize = true; this.lblVenc.Location = new System.Drawing.Point(15, 65); this.lblVenc.Text = "Vencimiento:";
            this.lblVenc.ForeColor = System.Drawing.Color.Black;
            this.txtMesVenc.Location = new System.Drawing.Point(140, 62); this.txtMesVenc.Size = new System.Drawing.Size(40, 22); this.txtMesVenc.MaxLength = 2; this.txtMesVenc.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblSep.AutoSize = true; this.lblSep.Location = new System.Drawing.Point(185, 65); this.lblSep.Text = "/"; this.lblSep.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSep.ForeColor = System.Drawing.Color.Black;
            this.txtAnioVenc.Location = new System.Drawing.Point(200, 62); this.txtAnioVenc.Size = new System.Drawing.Size(60, 22); this.txtAnioVenc.MaxLength = 4; this.txtAnioVenc.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            this.lblCvv.AutoSize = true; this.lblCvv.Location = new System.Drawing.Point(290, 65); this.lblCvv.Text = "CVV:";
            this.lblCvv.ForeColor = System.Drawing.Color.Black;
            this.txtCvv.Location = new System.Drawing.Point(340, 62); this.txtCvv.Size = new System.Drawing.Size(60, 22); this.txtCvv.MaxLength = 4;
            this.txtCvv.PasswordChar = '*';

            this.lblTitular.AutoSize = true; this.lblTitular.Location = new System.Drawing.Point(15, 100); this.lblTitular.Text = "Titular:";
            this.lblTitular.ForeColor = System.Drawing.Color.Black;
            this.txtTitularNombre.Location = new System.Drawing.Point(140, 97); this.txtTitularNombre.Size = new System.Drawing.Size(150, 22);
            this.txtTitularApellido.Location = new System.Drawing.Point(300, 97); this.txtTitularApellido.Size = new System.Drawing.Size(150, 22);

            this.lblBanco.AutoSize = true; this.lblBanco.Location = new System.Drawing.Point(15, 135); this.lblBanco.Text = "Banco:";
            this.lblBanco.ForeColor = System.Drawing.Color.Black;
            this.txtBanco.Location = new System.Drawing.Point(140, 132); this.txtBanco.Size = new System.Drawing.Size(310, 22);

            // Botones
            this.btnConfirmar.Location = new System.Drawing.Point(280, 420); this.btnConfirmar.Size = new System.Drawing.Size(150, 35);
            this.btnConfirmar.Text = "✓ Confirmar cobro";
            this.btnConfirmar.BackColor = System.Drawing.Color.SeaGreen;
            this.btnConfirmar.ForeColor = System.Drawing.Color.White;
            this.btnConfirmar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnConfirmar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);

            this.btnCancelar.Location = new System.Drawing.Point(440, 420); this.btnCancelar.Size = new System.Drawing.Size(90, 35);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.BackColor = System.Drawing.Color.IndianRed;
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(227, 242, 253);
            this.ClientSize = new System.Drawing.Size(550, 470);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblTotalLabel);
            this.Controls.Add(this.lblTotalPagar);
            this.Controls.Add(this.grpMetodo);
            this.Controls.Add(this.grpEfectivo);
            this.Controls.Add(this.grpTarjeta);
            this.Controls.Add(this.btnConfirmar);
            this.Controls.Add(this.btnCancelar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FRMCobrarVenta_GO44";
            this.Text = "Cobrar Venta";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.FRMCobrarVenta_GO44_Load);
            this.grpMetodo.ResumeLayout(false); this.grpMetodo.PerformLayout();
            this.grpEfectivo.ResumeLayout(false); this.grpEfectivo.PerformLayout();
            this.grpTarjeta.ResumeLayout(false); this.grpTarjeta.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblTotalLabel;
        private System.Windows.Forms.Label lblTotalPagar;
        private System.Windows.Forms.GroupBox grpMetodo;
        private System.Windows.Forms.RadioButton rbEfectivo;
        private System.Windows.Forms.RadioButton rbTarjeta;
        private System.Windows.Forms.GroupBox grpEfectivo;
        private System.Windows.Forms.Label lblMontoEntregado;
        private System.Windows.Forms.TextBox txtMontoEntregado;
        private System.Windows.Forms.Label lblVuelto;
        private System.Windows.Forms.GroupBox grpTarjeta;
        private System.Windows.Forms.Label lblNroTarjeta;
        private System.Windows.Forms.TextBox txtNroTarjeta;
        private System.Windows.Forms.Label lblVenc;
        private System.Windows.Forms.TextBox txtMesVenc;
        private System.Windows.Forms.TextBox txtAnioVenc;
        private System.Windows.Forms.Label lblSep;
        private System.Windows.Forms.Label lblCvv;
        private System.Windows.Forms.TextBox txtCvv;
        private System.Windows.Forms.Label lblTitular;
        private System.Windows.Forms.TextBox txtTitularNombre;
        private System.Windows.Forms.TextBox txtTitularApellido;
        private System.Windows.Forms.Label lblBanco;
        private System.Windows.Forms.TextBox txtBanco;
        private System.Windows.Forms.Button btnConfirmar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
