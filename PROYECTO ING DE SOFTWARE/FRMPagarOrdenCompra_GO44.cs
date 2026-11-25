using BE;
using BLL;
using Servicios;
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
    public partial class FRMPagarOrdenCompra_GO44 : Form, IObservadorIdioma_GO44
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

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
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
            ActualizarIdioma();
        }

        /// <summary>Aplica los textos del idioma activo a títulos, etiquetas, botones y encabezados de la grilla.</summary>
        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("pago.titulo");
            if (lblTitulo != null)          lblTitulo.Text          = IdiomaManager_GO44.T("pago.titulo");
            if (grpOrdenes != null)         grpOrdenes.Text         = IdiomaManager_GO44.T("pago.grpOrdenes");
            if (lblAvisoSinOrdenes != null) lblAvisoSinOrdenes.Text = IdiomaManager_GO44.T("pago.avisoSinOrdenes");
            if (btnActualizar != null)      btnActualizar.Text      = IdiomaManager_GO44.T("pago.btnActualizar");
            if (grpPago != null)            grpPago.Text            = IdiomaManager_GO44.T("pago.grpPago");
            if (lblCVU != null)             lblCVU.Text             = IdiomaManager_GO44.T("pago.lblCVU");
            if (lblBanco != null)           lblBanco.Text           = IdiomaManager_GO44.T("pago.lblBanco");
            if (lblTitular != null)         lblTitular.Text         = IdiomaManager_GO44.T("pago.lblTitular");
            if (lblMonto != null)           lblMonto.Text           = IdiomaManager_GO44.T("pago.lblMonto");
            if (lblComprobante != null)     lblComprobante.Text     = IdiomaManager_GO44.T("pago.lblComprobante");
            if (btnPagar != null)           btnPagar.Text           = IdiomaManager_GO44.T("pago.btnPagar");
            if (btnSalir != null)           btnSalir.Text           = IdiomaManager_GO44.T("pago.btnSalir");
            ActualizarEncabezadosGrilla();
            if (_ordenSeleccionada == null)
            {
                if (lblDetalleOC != null)    lblDetalleOC.Text    = IdiomaManager_GO44.T("pago.seleccioneOrdenDetalle");
                if (lblDetalleTotal != null) lblDetalleTotal.Text = IdiomaManager_GO44.T("pago.totalVacio");
            }
        }

        /// <summary>Traduce los encabezados de la grilla de órdenes.</summary>
        private void ActualizarEncabezadosGrilla()
        {
            if (dgvOrdenes.Columns.Count == 0) return;
            if (dgvOrdenes.Columns.Contains("NumeroOC"))     dgvOrdenes.Columns["NumeroOC"].HeaderText     = IdiomaManager_GO44.T("pago.colNumeroOC");
            if (dgvOrdenes.Columns.Contains("NumeroSC"))     dgvOrdenes.Columns["NumeroSC"].HeaderText     = IdiomaManager_GO44.T("pago.colNumeroSC");
            if (dgvOrdenes.Columns.Contains("FechaEntrega")) dgvOrdenes.Columns["FechaEntrega"].HeaderText = IdiomaManager_GO44.T("pago.colEntrega");
            if (dgvOrdenes.Columns.Contains("Total"))        dgvOrdenes.Columns["Total"].HeaderText        = IdiomaManager_GO44.T("pago.colTotal");
            if (dgvOrdenes.Columns.Contains("Estado"))       dgvOrdenes.Columns["Estado"].HeaderText       = IdiomaManager_GO44.T("pago.colEstado");
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
                foreach (var col in new[] { "IdSolicitud", "UserNameEncargado", "Proveedor", "Lineas", "FechaEmision" })
                    if (dgvOrdenes.Columns.Contains(col)) dgvOrdenes.Columns[col].Visible = false;
                if (dgvOrdenes.Columns.Contains("Id"))           dgvOrdenes.Columns["Id"].Visible = false;
                if (dgvOrdenes.Columns.Contains("NumeroOC"))     dgvOrdenes.Columns["NumeroOC"].HeaderText = IdiomaManager_GO44.T("pago.colNumeroOC");
                if (dgvOrdenes.Columns.Contains("NumeroSC"))     dgvOrdenes.Columns["NumeroSC"].HeaderText = IdiomaManager_GO44.T("pago.colNumeroSC");
                if (dgvOrdenes.Columns.Contains("FechaEntrega")) { dgvOrdenes.Columns["FechaEntrega"].HeaderText = IdiomaManager_GO44.T("pago.colEntrega"); dgvOrdenes.Columns["FechaEntrega"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
                if (dgvOrdenes.Columns.Contains("Total"))        { dgvOrdenes.Columns["Total"].HeaderText = IdiomaManager_GO44.T("pago.colTotal"); dgvOrdenes.Columns["Total"].DefaultCellStyle.Format = "N2"; }
                if (dgvOrdenes.Columns.Contains("Estado"))       dgvOrdenes.Columns["Estado"].HeaderText = IdiomaManager_GO44.T("pago.colEstado");
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
            lblDetalleOC.Text = string.Format(IdiomaManager_GO44.T("pago.detalleOC"),
                                _ordenSeleccionada.NumeroOC, prov != null ? prov.Empresa : "—");
            lblDetalleTotal.Text = string.Format(IdiomaManager_GO44.T("pago.totalAPagar"), _ordenSeleccionada.Total.ToString("N2"));
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
            lblDetalleOC.Text = IdiomaManager_GO44.T("pago.seleccioneOrdenDetalle");
            lblDetalleTotal.Text = IdiomaManager_GO44.T("pago.totalVacio");
            txtCVU.Text = ""; txtBanco.Text = ""; txtTitular.Text = "";
            txtMonto.Text = ""; txtComprobante.Text = "";
            btnPagar.Enabled = false;
        }

        private void btnPagar_Click(object sender, EventArgs e)
        {
            if (_ordenSeleccionada == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("pago.seleccioneOrdenDetalle"), IdiomaManager_GO44.T("general.advertencia"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal monto;
            if (!decimal.TryParse(txtMonto.Text.Trim(), out monto) || monto <= 0)
            {
                MessageBox.Show(IdiomaManager_GO44.T("pago.montoInvalidoMsg"), IdiomaManager_GO44.T("pago.montoInvalidoTit"),
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
                string.Format(IdiomaManager_GO44.T("pago.confirmarPagoMsg"), _ordenSeleccionada.NumeroOC, monto.ToString("N2")),
                IdiomaManager_GO44.T("pago.confirmarPagoTit"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            var ordenPagada = _ordenSeleccionada;   // referencia antes de recargar la grilla
            var vo = _bllPago.PagarOrden(_ordenSeleccionada, pago);
            if (vo.Resultado == BLLPagoOrdenCompra_GO44.ResultadoPago.Exitoso)
            {
                MessageBox.Show(vo.Mensaje, IdiomaManager_GO44.T("pago.pagoRegistrado"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                OfrecerComprobantePdf(ordenPagada, vo.Pago);
                CargarOrdenes();
            }
            else
            {
                MessageBox.Show(vo.Mensaje, IdiomaManager_GO44.T("pago.noSePudoPagar"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ofrece generar y guardar el comprobante de pago en PDF (reusa GeneradorPdf_GO44,
        /// el mismo generador nativo que las facturas).
        /// </summary>
        private void OfrecerComprobantePdf(BE_OrdenCompra_GO44 orden, BE_PagoOrdenCompra_GO44 pago)
        {
            if (orden == null || pago == null) return;

            var r = MessageBox.Show(IdiomaManager_GO44.T("pago.generarPdfMsg"),
                IdiomaManager_GO44.T("pago.comprobanteTit"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "Comprobante_" + pago.NroComprobante + ".pdf";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var prov = orden.Proveedor;
                    string titulo = string.Format(IdiomaManager_GO44.T("pago.pdfTitulo"), pago.NroComprobante);
                    string subtitulo = string.Format(IdiomaManager_GO44.T("pago.pdfSubtitulo"),
                        orden.NumeroOC,
                        pago.FechaOperacion.ToString("dd/MM/yyyy HH:mm"),
                        prov != null ? prov.Empresa : "",
                        pago.Estado);

                    string[] headers = { IdiomaManager_GO44.T("pago.pdfColCodigo"), IdiomaManager_GO44.T("pago.pdfColComponente"), IdiomaManager_GO44.T("pago.pdfColCant"), IdiomaManager_GO44.T("pago.pdfColPrecio"), IdiomaManager_GO44.T("pago.pdfColSubtotal") };
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
                    filas.Add(new string[] { "", "", "", IdiomaManager_GO44.T("pago.pdfTotalOC"),  "$ " + orden.Total.ToString("N2") });
                    filas.Add(new string[] { "", "", "", IdiomaManager_GO44.T("pago.pdfAbonado"),   "$ " + pago.MontoAbonado.ToString("N2") });
                    filas.Add(new string[] { "", "", "", "", "" });
                    filas.Add(new string[] { IdiomaManager_GO44.T("pago.pdfDatosPago"), "", "", "", "" });
                    filas.Add(new string[] { IdiomaManager_GO44.T("pago.pdfCVU"),         prov != null ? prov.CVU : "",     "", "", "" });
                    filas.Add(new string[] { IdiomaManager_GO44.T("pago.pdfBanco"),       prov != null ? prov.Banco : "",   "", "", "" });
                    filas.Add(new string[] { IdiomaManager_GO44.T("pago.pdfTitular"),     prov != null ? prov.Titular : "", "", "", "" });
                    filas.Add(new string[] { IdiomaManager_GO44.T("pago.pdfComprobante"), pago.NroComprobante,               "", "", "" });

                    var gen = new GeneradorPdf_GO44();
                    gen.Generar(sfd.FileName, titulo, subtitulo, headers, anchos, filas);

                    var abrir = MessageBox.Show(string.Format(IdiomaManager_GO44.T("pago.pdfGenerado"), sfd.FileName),
                        IdiomaManager_GO44.T("pago.comprobanteTit"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (abrir == DialogResult.Yes && System.IO.File.Exists(sfd.FileName))
                        System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("pago.pdfError"), ex.Message),
                        IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e) { CargarOrdenes(); }
        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
