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
        private readonly BLLProveedor_GO44 _bllProveedor;

        private BE_OrdenCompra_GO44 _ordenSeleccionada;
        private readonly BindingSource _bsOrdenes = new BindingSource();

        public FRMPagarOrdenCompra_GO44()
        {
            InitializeComponent();
            _bllOC        = new BLLOrdenCompra_GO44();
            _bllPago      = new BLLPagoOrdenCompra_GO44();
            _bllProveedor = new BLLProveedor_GO44();
        }

        private void FRMPagarOrdenCompra_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();

            txtMonto.KeyPress += (s, ev) =>
            {
                if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar) && ev.KeyChar != ',' && ev.KeyChar != '.')
                    ev.Handled = true;
            };

            // Cargar al final: así la auto-selección de la grilla completa el detalle sin que
            // nada lo borre después.
            CargarOrdenes();
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

            // Al bindear, la grilla auto-selecciona la primera fila; cargamos su detalle
            // explícitamente (antes se llamaba LimpiarDetalle acá y borraba lo recién cargado).
            if (ordenes != null && ordenes.Count > 0)
            {
                dgvOrdenes.ClearSelection();
                dgvOrdenes.Rows[0].Selected = true;
                MostrarDetalleOrden();
            }
            else
            {
                LimpiarDetalle();
            }
        }

        private void dgvOrdenes_SelectionChanged(object sender, EventArgs e)
        {
            MostrarDetalleOrden();
        }

        private void MostrarDetalleOrden()
        {
            // CurrentRow puede ser null si la selección se hizo por código; usar SelectedRows como fallback.
            DataGridViewRow fila = dgvOrdenes.CurrentRow;
            if (fila == null && dgvOrdenes.SelectedRows.Count > 0) fila = dgvOrdenes.SelectedRows[0];
            var ocRow = fila != null ? fila.DataBoundItem as BE_OrdenCompra_GO44 : null;
            if (ocRow == null) { LimpiarDetalle(); return; }

            // Traer la OC completa (con proveedor y líneas)
            _ordenSeleccionada = _bllOC.BuscarPorId(ocRow.Id);
            if (_ordenSeleccionada == null) { LimpiarDetalle(); return; }

            // El listado/BuscarPorId traen el proveedor solo con CUIT/Empresa; para los datos
            // bancarios (CVU/Banco/Titular) hay que cargar el proveedor completo.
            if (_ordenSeleccionada.Proveedor != null)
            {
                var provCompleto = _bllProveedor.BuscarPorCUIT(_ordenSeleccionada.Proveedor.CUIT);
                if (provCompleto != null) _ordenSeleccionada.Proveedor = provCompleto;
            }

            var prov = _ordenSeleccionada.Proveedor;
            lblDetalleOC.Text = "OC " + _ordenSeleccionada.NumeroOC + "  ·  " +
                                (prov != null ? prov.Empresa : "—");
            lblDetalleTotal.Text = "Total a pagar: $" + _ordenSeleccionada.Total.ToString("N2");
            txtCVU.Text     = prov != null ? prov.CVU : "";
            txtBanco.Text   = prov != null ? prov.Banco : "";
            txtTitular.Text = prov != null ? prov.Titular : "";
            txtMonto.Text   = _ordenSeleccionada.Total.ToString("N2");
            txtComprobante.Text = GenerarNroComprobante();
            btnPagar.Enabled = true;
        }

        /// <summary>
        /// Genera un número de comprobante único con formato COMP-yyyyMMddHHmmss.
        /// Lo asigna el sistema (no se carga a mano).
        /// </summary>
        private static string GenerarNroComprobante()
        {
            return "COMP-" + DateTime.Now.ToString("yyyyMMddHHmmss");
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

            var ordenPagada = _ordenSeleccionada;   // referencia antes de recargar la grilla
            var vo = _bllPago.PagarOrden(_ordenSeleccionada, pago);
            if (vo.Resultado == BLLPagoOrdenCompra_GO44.ResultadoPago.Exitoso)
            {
                MessageBox.Show(vo.Mensaje, "Pago registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OfrecerComprobantePdf(ordenPagada, vo.Pago);
                CargarOrdenes();
            }
            else
            {
                MessageBox.Show(vo.Mensaje, "No se pudo pagar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ofrece generar y guardar el comprobante de pago en PDF (reusa GeneradorPdf_GO44,
        /// el mismo generador nativo que las facturas).
        /// </summary>
        private void OfrecerComprobantePdf(BE_OrdenCompra_GO44 orden, BE_PagoOrdenCompra_GO44 pago)
        {
            if (orden == null || pago == null) return;

            var r = MessageBox.Show("¿Desea generar el comprobante de pago en PDF?",
                "Comprobante de pago", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "Comprobante_" + pago.NroComprobante + ".pdf";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var prov = orden.Proveedor;
                    string titulo = "TECHFLOW - ELECTROPOINT · COMPROBANTE DE PAGO " + pago.NroComprobante;
                    string subtitulo =
                        "OC: " + orden.NumeroOC +
                        "   |   Fecha: " + pago.FechaOperacion.ToString("dd/MM/yyyy HH:mm") +
                        "   |   Proveedor: " + (prov != null ? prov.Empresa : "") +
                        "   |   Estado: " + pago.Estado;

                    string[] headers = { "Codigo", "Componente", "Cant", "Precio Unit.", "Subtotal" };
                    float[] anchos = { 0.12f, 0.48f, 0.10f, 0.15f, 0.15f };

                    var filas = new List<string[]>();
                    if (orden.Lineas != null)
                    {
                        foreach (var l in orden.Lineas)
                            filas.Add(new string[] {
                                l.ComponenteCodigo ?? "",
                                l.ComponenteNombre ?? "",
                                l.Cantidad.ToString(),
                                "$ " + l.PrecioUnitario.ToString("N2"),
                                "$ " + l.Subtotal.ToString("N2")
                            });
                    }

                    // Totales y datos del pago
                    filas.Add(new string[] { "", "", "", "", "" });
                    filas.Add(new string[] { "", "", "", "TOTAL OC:",  "$ " + orden.Total.ToString("N2") });
                    filas.Add(new string[] { "", "", "", "ABONADO:",   "$ " + pago.MontoAbonado.ToString("N2") });
                    filas.Add(new string[] { "", "", "", "", "" });
                    filas.Add(new string[] { "DATOS DEL PAGO", "", "", "", "" });
                    filas.Add(new string[] { "CVU",         prov != null ? prov.CVU : "",     "", "", "" });
                    filas.Add(new string[] { "Banco",       prov != null ? prov.Banco : "",   "", "", "" });
                    filas.Add(new string[] { "Titular",     prov != null ? prov.Titular : "", "", "", "" });
                    filas.Add(new string[] { "Comprobante", pago.NroComprobante,               "", "", "" });

                    var gen = new GeneradorPdf_GO44();
                    gen.Generar(sfd.FileName, titulo, subtitulo, headers, anchos, filas);

                    var abrir = MessageBox.Show("Comprobante generado en:\n" + sfd.FileName + "\n\n¿Desea abrirlo?",
                        "Comprobante de pago", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (abrir == DialogResult.Yes && System.IO.File.Exists(sfd.FileName))
                        System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo generar el comprobante PDF: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e) { CargarOrdenes(); }
        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
