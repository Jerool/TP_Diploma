using BE;
using BLL;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// Diálogo modal del CU05 Cobrar Venta.
    /// Se abre desde FRMFacturar pasando el monto a pagar; si el usuario
    /// confirma un cobro válido, la propiedad CobroValidado queda con el
    /// BE_Cobro_GO44 listo para persistir (efectivo o tarjeta).
    /// </summary>
    public partial class FRMCobrarVenta_GO44 : Form
    {
        private readonly BLLCobro_GO44 _bllCobro;
        private readonly decimal _montoAPagar;

        /// <summary>Si Resultado==OK contiene el cobro listo para persistir.</summary>
        public BE_Cobro_GO44 CobroValidado { get; private set; }

        public FRMCobrarVenta_GO44(decimal montoAPagar)
        {
            InitializeComponent();
            _bllCobro = new BLLCobro_GO44();
            _montoAPagar = montoAPagar;
        }

        private void FRMCobrarVenta_GO44_Load(object sender, EventArgs e)
        {
            lblTotalPagar.Text = "$ " + _montoAPagar.ToString("N2");
            rbEfectivo.Checked = true;
            ActualizarPanelSegunMetodo();
        }

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
                    lblVuelto.Text = "Falta: $ " + (-vuelto).ToString("N2");
                    lblVuelto.ForeColor = Color.Firebrick;
                }
                else
                {
                    lblVuelto.Text = "Vuelto: $ " + vuelto.ToString("N2");
                    lblVuelto.ForeColor = Color.DarkGreen;
                }
            }
            else
            {
                lblVuelto.Text = "Vuelto: —";
                lblVuelto.ForeColor = Color.DimGray;
            }
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (rbEfectivo.Checked)
            {
                decimal entregado;
                if (!decimal.TryParse(txtMontoEntregado.Text.Trim(), out entregado))
                {
                    MessageBox.Show("Monto entregado inválido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var vo = _bllCobro.PrepararCobroEfectivo(_montoAPagar, entregado);
                if (vo.Resultado != BLLCobro_GO44.ResultadoValidacion.Ok)
                {
                    MessageBox.Show(vo.Mensaje, "No se puede cobrar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                CobroValidado = vo.Cobro;
                MessageBox.Show(vo.Mensaje, "Cobro OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else // Tarjeta
            {
                var vo = _bllCobro.PrepararCobroTarjeta(
                    _montoAPagar,
                    txtNroTarjeta.Text, txtMesVenc.Text, txtAnioVenc.Text, txtCvv.Text,
                    txtTitularNombre.Text, txtTitularApellido.Text, txtBanco.Text);

                if (vo.Resultado != BLLCobro_GO44.ResultadoValidacion.Ok)
                {
                    MessageBox.Show(vo.Mensaje, "No se puede cobrar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                CobroValidado = vo.Cobro;
                MessageBox.Show(vo.Mensaje, "Cobro autorizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
