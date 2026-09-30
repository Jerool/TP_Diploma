using BE;
using BLL;
using Servicios;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// Diálogo modal del CU05 Cobrar Venta.
    /// Se abre desde FRMFacturar pasando el monto a pagar; si el usuario
    /// confirma un cobro válido, la propiedad CobroValidado queda con el
    /// BE_Cobro_GO44 listo para persistir (efectivo o tarjeta).
    ///
    /// Validaciones UI de tarjeta:
    ///   · N° tarjeta: solo dígitos, se formatea "1234 5678 9012 3456" (máx 19 dígitos)
    ///   · Mes y año de vencimiento: solo dígitos, mes 2, año 4
    ///   · CVV: solo dígitos, 3 o 4
    ///   · Titular nombre/apellido: solo letras y espacios
    /// </summary>
    public partial class FRMCobrarVenta_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly BLLCobro_GO44 _bllCobro;
        private readonly decimal _montoAPagar;

        // Flag interno para no re-disparar el TextChanged mientras reformateamos
        private bool _formateandoTarjeta = false;

        /// <summary>Si Resultado==OK contiene el cobro listo para persistir.</summary>
        public BE_Cobro_GO44 CobroValidado { get; private set; }

        public FRMCobrarVenta_GO44(decimal montoAPagar)
        {
            InitializeComponent();
            _bllCobro = new BLLCobro_GO44();
            _montoAPagar = montoAPagar;

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);

            ConfigurarValidacionesTarjeta();
        }

        // ============ i18n ============

        public void ActualizarIdioma()
        {
            this.Text                = IdiomaManager_GO44.T("cobrar.titulo");
            lblTitulo.Text           = IdiomaManager_GO44.T("cobrar.titulo");
            lblTotalLabel.Text       = IdiomaManager_GO44.T("cobrar.totalLabel");
            grpMetodo.Text           = IdiomaManager_GO44.T("cobrar.metodoPago");
            rbEfectivo.Text          = IdiomaManager_GO44.T("cobrar.efectivo");
            rbTarjeta.Text           = IdiomaManager_GO44.T("cobrar.tarjeta");
            grpEfectivo.Text         = IdiomaManager_GO44.T("cobrar.efectivo");
            grpTarjeta.Text          = IdiomaManager_GO44.T("cobrar.tarjeta");
            lblMontoEntregado.Text   = IdiomaManager_GO44.T("cobrar.montoEntregado");
            lblNroTarjeta.Text       = IdiomaManager_GO44.T("cobrar.nroTarjeta");
            lblVenc.Text             = IdiomaManager_GO44.T("cobrar.vencimiento");
            lblCvv.Text              = IdiomaManager_GO44.T("cobrar.cvv");
            lblTitular.Text          = IdiomaManager_GO44.T("cobrar.titular");
            lblBanco.Text            = IdiomaManager_GO44.T("cobrar.banco");
            btnConfirmar.Text        = IdiomaManager_GO44.T("cobrar.btnConfirmar");
            btnCancelar.Text         = IdiomaManager_GO44.T("cobrar.btnCancelar");

            // Placeholders / cue-banner de titular
            SetCueBanner(txtTitularNombre,   IdiomaManager_GO44.T("cobrar.titularNombre"));
            SetCueBanner(txtTitularApellido, IdiomaManager_GO44.T("cobrar.titularApellido"));

            // Refrescar el label "Vuelto" con el estado actual
            CalcularVuelto();
        }

        // Placeholder nativo de Windows (cue banner) para TextBoxes
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        private static void SetCueBanner(TextBox txt, string texto)
        {
            if (txt != null && txt.IsHandleCreated)
                SendMessage(txt.Handle, EM_SETCUEBANNER, (IntPtr)1, texto);
        }

        // ============ Load ============

        private void FRMCobrarVenta_GO44_Load(object sender, EventArgs e)
        {
            lblTotalPagar.Text = "$ " + _montoAPagar.ToString("N2");
            rbEfectivo.Checked = true;
            ActualizarPanelSegunMetodo();
            ActualizarIdioma();
        }

        // ============ Validaciones UI de tarjeta ============

        private void ConfigurarValidacionesTarjeta()
        {
            // N° tarjeta: solo dígitos, formatea con espacios cada 4 (19 dígitos + 4 espacios = 23 chars)
            txtNroTarjeta.MaxLength = 23;
            txtNroTarjeta.KeyPress += SoloDigitos_KeyPress;
            txtNroTarjeta.TextChanged += TxtNroTarjeta_TextChanged;

            // Mes vencimiento: solo dígitos, 2 dígitos, auto-avanza al año
            txtMesVenc.MaxLength = 2;
            txtMesVenc.KeyPress += SoloDigitos_KeyPress;
            txtMesVenc.TextChanged += (s, e) =>
            {
                if (txtMesVenc.Text.Length == 2) txtAnioVenc.Focus();
            };

            // Año vencimiento: solo dígitos, 4 dígitos, auto-avanza al CVV
            txtAnioVenc.MaxLength = 4;
            txtAnioVenc.KeyPress += SoloDigitos_KeyPress;
            txtAnioVenc.TextChanged += (s, e) =>
            {
                if (txtAnioVenc.Text.Length == 4) txtCvv.Focus();
            };

            // CVV: solo dígitos, 3 o 4 dígitos
            txtCvv.MaxLength = 4;
            txtCvv.KeyPress += SoloDigitos_KeyPress;

            // Titular nombre / apellido: solo letras y espacios
            txtTitularNombre.KeyPress += SoloLetras_KeyPress;
            txtTitularApellido.KeyPress += SoloLetras_KeyPress;

            // Monto entregado: solo dígitos, punto y coma decimal
            txtMontoEntregado.KeyPress += MontoDecimal_KeyPress;
        }

        private void SoloDigitos_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void SoloLetras_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
                e.Handled = true;
        }

        private void MontoDecimal_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (char.IsControl(c)) return;
            if (char.IsDigit(c)) return;
            // permitir un solo separador decimal (. o ,)
            if ((c == '.' || c == ',') && !((TextBox)sender).Text.Contains(".") && !((TextBox)sender).Text.Contains(","))
                return;
            e.Handled = true;
        }

        /// <summary>
        /// Formatea el N° tarjeta con espacios cada 4 dígitos: "1234 5678 9012 3456".
        /// Mantiene la posición del cursor lo más cerca posible de donde estaba.
        /// </summary>
        private void TxtNroTarjeta_TextChanged(object sender, EventArgs e)
        {
            if (_formateandoTarjeta) return;
            _formateandoTarjeta = true;
            try
            {
                int caretOriginal = txtNroTarjeta.SelectionStart;
                string original = txtNroTarjeta.Text;

                // Sacar todo lo que no sea dígito
                string soloDigitos = Regex.Replace(original, @"\D", "");
                if (soloDigitos.Length > 19) soloDigitos = soloDigitos.Substring(0, 19);

                // Insertar espacio cada 4 dígitos
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < soloDigitos.Length; i++)
                {
                    if (i > 0 && i % 4 == 0) sb.Append(' ');
                    sb.Append(soloDigitos[i]);
                }
                string formateado = sb.ToString();

                if (formateado != original)
                {
                    // Calcular cuántos dígitos había antes del cursor original
                    int digitosAntesCaret = 0;
                    for (int i = 0; i < Math.Min(caretOriginal, original.Length); i++)
                        if (char.IsDigit(original[i])) digitosAntesCaret++;

                    txtNroTarjeta.Text = formateado;

                    // Recolocar el caret pasado los mismos N dígitos + espacios intermedios
                    int nuevoCaret = digitosAntesCaret + (digitosAntesCaret > 0 ? (digitosAntesCaret - 1) / 4 : 0);
                    if (nuevoCaret > formateado.Length) nuevoCaret = formateado.Length;
                    txtNroTarjeta.SelectionStart = nuevoCaret;
                    txtNroTarjeta.SelectionLength = 0;
                }
            }
            finally
            {
                _formateandoTarjeta = false;
            }
        }

        // ============ Método (efectivo / tarjeta) ============

        private void rbEfectivo_CheckedChanged(object sender, EventArgs e) { ActualizarPanelSegunMetodo(); }
        private void rbTarjeta_CheckedChanged(object sender, EventArgs e)  { ActualizarPanelSegunMetodo(); }

        private void ActualizarPanelSegunMetodo()
        {
            grpEfectivo.Visible = rbEfectivo.Checked;
            grpTarjeta.Visible  = rbTarjeta.Checked;
            if (rbEfectivo.Checked)
            {
                txtMontoEntregado.Text = _montoAPagar.ToString("N2");
                CalcularVuelto();
            }
        }

        private void txtMontoEntregado_TextChanged(object sender, EventArgs e) { CalcularVuelto(); }

        private void CalcularVuelto()
        {
            decimal entregado;
            if (decimal.TryParse(txtMontoEntregado.Text.Trim(), out entregado))
            {
                decimal vuelto = entregado - _montoAPagar;
                if (vuelto < 0)
                {
                    lblVuelto.Text = IdiomaManager_GO44.T("cobrar.falta") + " $ " + (-vuelto).ToString("N2");
                    lblVuelto.ForeColor = Color.Firebrick;
                }
                else
                {
                    lblVuelto.Text = IdiomaManager_GO44.T("cobrar.vuelto") + " $ " + vuelto.ToString("N2");
                    lblVuelto.ForeColor = Color.DarkGreen;
                }
            }
            else
            {
                lblVuelto.Text = IdiomaManager_GO44.T("cobrar.vueltoInicial");
                lblVuelto.ForeColor = Color.DimGray;
            }
        }

        // ============ Confirmar ============

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (rbEfectivo.Checked)
            {
                decimal entregado;
                if (!decimal.TryParse(txtMontoEntregado.Text.Trim(), out entregado))
                {
                    MessageBox.Show(IdiomaManager_GO44.T("cobrar.errMontoInvalido"),
                        IdiomaManager_GO44.T("general.advertencia"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var vo = _bllCobro.PrepararCobroEfectivo(_montoAPagar, entregado);
                if (vo.Resultado != BLLCobro_GO44.ResultadoValidacion.Ok)
                {
                    MessageBox.Show(TraducirResultado(vo),
                        IdiomaManager_GO44.T("cobrar.tituloNoCobrar"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                CobroValidado = vo.Cobro;
                MessageBox.Show(vo.Mensaje,
                    IdiomaManager_GO44.T("cobrar.tituloCobroOk"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else // Tarjeta
            {
                // Validaciones UI antes de mandar al BLL (para errores más específicos + evitar rebote)
                string nroDigitos = Regex.Replace(txtNroTarjeta.Text ?? "", @"\D", "");
                if (nroDigitos.Length < 13 || nroDigitos.Length > 19)
                {
                    MessageBox.Show(IdiomaManager_GO44.T("cobrar.errNroTarjeta"),
                        IdiomaManager_GO44.T("cobrar.tituloNoCobrar"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNroTarjeta.Focus(); return;
                }
                if (txtMesVenc.Text.Length == 0 || txtAnioVenc.Text.Length != 4)
                {
                    MessageBox.Show(IdiomaManager_GO44.T("cobrar.errMesInvalido"),
                        IdiomaManager_GO44.T("cobrar.tituloNoCobrar"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMesVenc.Focus(); return;
                }
                if (txtCvv.Text.Length < 3)
                {
                    MessageBox.Show(IdiomaManager_GO44.T("cobrar.errCvv"),
                        IdiomaManager_GO44.T("cobrar.tituloNoCobrar"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCvv.Focus(); return;
                }

                var vo = _bllCobro.PrepararCobroTarjeta(
                    _montoAPagar,
                    nroDigitos, txtMesVenc.Text, txtAnioVenc.Text, txtCvv.Text,
                    txtTitularNombre.Text, txtTitularApellido.Text, txtBanco.Text);

                if (vo.Resultado != BLLCobro_GO44.ResultadoValidacion.Ok)
                {
                    MessageBox.Show(TraducirResultado(vo),
                        IdiomaManager_GO44.T("cobrar.tituloNoCobrar"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                CobroValidado = vo.Cobro;
                MessageBox.Show(vo.Mensaje,
                    IdiomaManager_GO44.T("cobrar.tituloCobroAutorizado"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>Traduce el enum del BLL a texto en el idioma actual.</summary>
        private string TraducirResultado(BLLCobro_GO44.ValidacionCobroVO vo)
        {
            switch (vo.Resultado)
            {
                case BLLCobro_GO44.ResultadoValidacion.MontoInvalido:         return IdiomaManager_GO44.T("cobrar.errMontoInvalido") + "\n\n" + vo.Mensaje;
                case BLLCobro_GO44.ResultadoValidacion.NroTarjetaInvalido:    return IdiomaManager_GO44.T("cobrar.errNroTarjeta");
                case BLLCobro_GO44.ResultadoValidacion.VencimientoInvalido:   return IdiomaManager_GO44.T("cobrar.errTarjetaVencida");
                case BLLCobro_GO44.ResultadoValidacion.CvvInvalido:           return IdiomaManager_GO44.T("cobrar.errCvv");
                case BLLCobro_GO44.ResultadoValidacion.TitularIncompleto:     return IdiomaManager_GO44.T("cobrar.errTitular");
                case BLLCobro_GO44.ResultadoValidacion.BancoRequerido:        return IdiomaManager_GO44.T("cobrar.errBanco");
                default: return vo.Mensaje ?? "Error";
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
