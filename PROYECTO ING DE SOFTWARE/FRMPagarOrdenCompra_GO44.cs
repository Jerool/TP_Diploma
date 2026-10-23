using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// CU09 "Pagar Orden de Compra".
    /// Lista las OCs en estado "Pendiente"; al seleccionar una se muestran sus datos y los
    /// datos bancarios del proveedor (CVU, Banco, Titular). El encargado ingresa el monto
    /// abonado y el nro de comprobante, y confirma. La OC pasa a "Pagada".
    /// </summary>
    public partial class FRMPagarOrdenCompra_GO44 : Form
    {
        private readonly BLLOrdenCompra_GO44 _bllOC;
        private readonly BLLPagoOrdenCompra_GO44 _bllPago;

        private BE_OrdenCompra_GO44 _ordenSeleccionada;
        private readonly BindingSource _bsOrdenes = new BindingSource();

        public FRMPagarOrdenCompra_GO44()
        {
            InitializeComponent();
            _bllOC   = new BLLOrdenCompra_GO44();
            _bllPago = new BLLPagoOrdenCompra_GO44();
        }

        private void FRMPagarOrdenCompra_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            CargarOrdenes();

            txtMonto.KeyPress += (s, ev) =>
            {
                if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar) && ev.KeyChar != ',' && ev.KeyChar != '.')
                    ev.Handled = true;
            };
            LimpiarDetalle();
        }

        private void ConfigurarGrilla()
        {
            dgvOrdenes.ReadOnly = true;
            dgvOrdenes.AllowUserToAddRows = false;
            dgvOrdenes.AllowUserToDeleteRows = false;
            dgvOrdenes.AllowUserToResizeRows = false;
            dgvOrdenes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrdenes.MultiSelect = false;
            dgvOrdenes.RowHeadersVisible = false;
            dgvOrdenes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrdenes.BackgroundColor = Color.White;
            dgvOrdenes.AutoGenerateColumns = true;
            dgvOrdenes.DataSource = _bsOrdenes;
            dgvOrdenes.SelectionChanged += dgvOrdenes_SelectionChanged;
        }

        private void CargarOrdenes()
        {
            var ordenes = _bllOC.ListarPorEstado(BE_OrdenCompra_GO44.EstadoOrden.Pendiente);
            _bsOrdenes.DataSource = null;
            _bsOrdenes.DataSource = ordenes ?? new List<BE_OrdenCompra_GO44>();
            _bsOrdenes.ResetBindings(false);

            if (dgvOrdenes.Columns.Count > 0)
            {
                foreach (var col in new[] { "IdSolicitud", "LoginEncargado", "Proveedor", "Lineas", "FechaEmision" })
                    if (dgvOrdenes.Columns.Contains(col)) dgvOrdenes.Columns[col].Visible = false;
                if (dgvOrdenes.Columns.Contains("Id"))           dgvOrdenes.Columns["Id"].Visible = false;
                if (dgvOrdenes.Columns.Contains("NumeroOC"))     dgvOrdenes.Columns["NumeroOC"].HeaderText = "N° OC";
                if (dgvOrdenes.Columns.Contains("NumeroSC"))     dgvOrdenes.Columns["NumeroSC"].HeaderText = "N° SC";
                if (dgvOrdenes.Columns.Contains("FechaEntrega")) { dgvOrdenes.Columns["FechaEntrega"].HeaderText = "Entrega"; dgvOrdenes.Columns["FechaEntrega"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
                if (dgvOrdenes.Columns.Contains("Total"))        { dgvOrdenes.Columns["Total"].HeaderText = "Total"; dgvOrdenes.Columns["Total"].DefaultCellStyle.Format = "N2"; }
                if (dgvOrdenes.Columns.Contains("Estado"))       dgvOrdenes.Columns["Estado"].HeaderText = "Estado";
                if (dgvOrdenes.Columns.Contains("CantidadItems")) dgvOrdenes.Columns["CantidadItems"].Visible = false;
            }

            lblAvisoSinOrdenes.Visible = (ordenes == null || ordenes.Count == 0);
            LimpiarDetalle();
        }

        private void dgvOrdenes_SelectionChanged(object sender, EventArgs e)
        {
            var ocRow = dgvOrdenes.CurrentRow != null ? dgvOrdenes.CurrentRow.DataBoundItem as BE_OrdenCompra_GO44 : null;
            if (ocRow == null) { LimpiarDetalle(); return; }

            // Traer la OC completa (con proveedor y líneas)
            _ordenSeleccionada = _bllOC.BuscarPorId(ocRow.Id);
            if (_ordenSeleccionada == null) { LimpiarDetalle(); return; }

            var prov = _ordenSeleccionada.Proveedor;
            // El listado trae el proveedor con CUIT/Empresa; para CVU/Banco/Titular reusamos lo que haya
            lblDetalleOC.Text = "OC " + _ordenSeleccionada.NumeroOC + "  ·  " +
                                (prov != null ? prov.Empresa : "—");
            lblDetalleTotal.Text = "Total a pagar: $" + _ordenSeleccionada.Total.ToString("N2");
            txtCVU.Text     = prov != null ? prov.CVU : "";
            txtBanco.Text   = prov != null ? prov.Banco : "";
            txtTitular.Text = prov != null ? prov.Titular : "";
            txtMonto.Text   = _ordenSeleccionada.Total.ToString("N2");
            txtComprobante.Text = "";
            btnPagar.Enabled = true;
        }

        private void LimpiarDetalle()
        {
            _ordenSeleccionada = null;
            lblDetalleOC.Text = "Seleccione una orden de compra";
            lblDetalleTotal.Text = "Total a pagar: —";
            txtCVU.Text = ""; txtBanco.Text = ""; txtTitular.Text = "";
            txtMonto.Text = ""; txtComprobante.Text = "";
            btnPagar.Enabled = false;
        }

        private void btnPagar_Click(object sender, EventArgs e)
        {
            if (_ordenSeleccionada == null)
            {
                MessageBox.Show("Seleccione una orden de compra", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal monto;
            if (!decimal.TryParse(txtMonto.Text.Trim(), out monto) || monto <= 0)
            {
                MessageBox.Show("Ingrese un monto válido", "Monto inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMonto.Focus();
                return;
            }

            var pago = new BE_PagoOrdenCompra_GO44
            {
                CVU            = txtCVU.Text.Trim(),
                Banco          = txtBanco.Text.Trim(),
                Titular        = txtTitular.Text.Trim(),
                MontoAbonado   = monto,
                NroComprobante = txtComprobante.Text.Trim()
            };

            var r = MessageBox.Show(
                "¿Confirmar el pago de la orden " + _ordenSeleccionada.NumeroOC +
                " por $" + monto.ToString("N2") + "?",
                "Confirmar pago", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            var vo = _bllPago.PagarOrden(_ordenSeleccionada, pago);
            if (vo.Resultado == BLLPagoOrdenCompra_GO44.ResultadoPago.Exitoso)
            {
                MessageBox.Show(vo.Mensaje, "Pago registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarOrdenes();
            }
            else
            {
                MessageBox.Show(vo.Mensaje, "No se pudo pagar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e) { CargarOrdenes(); }
        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
