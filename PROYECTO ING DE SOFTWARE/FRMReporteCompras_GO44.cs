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
    /// Rep. 2 (RFN2) — "Recomposición de componentes".
    /// Consolida las órdenes de compra con su estado de recepción (pedido vs recibido),
    /// datos del proveedor/pago y totales. Permite filtrar por fecha y estado, y exportar a PDF.
    /// </summary>
    public partial class FRMReporteCompras_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly BLLOrdenCompra_GO44 _bllOC;
        private readonly BLLRecepcionMercaderia_GO44 _bllRecepcion;
        private readonly BLLPagoOrdenCompra_GO44 _bllPago;
        private readonly BLLProveedor_GO44 _bllProveedor;

        private readonly BindingSource _bsOrdenes = new BindingSource();
        private readonly BindingSource _bsDetalle = new BindingSource();
        private BE_OrdenCompra_GO44 _ordenSel;

        // Valores de estado reales (coinciden con EstadoOrdenCompra.ToString()); el índice 0 = "(todos)".
        private static readonly string[] EstadosFiltro = { null, "Pendiente", "Pagada", "Recibida", "RecibidaConFaltantes", "Cancelada" };

        public FRMReporteCompras_GO44()
        {
            InitializeComponent();
            _bllOC        = new BLLOrdenCompra_GO44();
            _bllRecepcion = new BLLRecepcionMercaderia_GO44();
            _bllPago      = new BLLPagoOrdenCompra_GO44();
            _bllProveedor = new BLLProveedor_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        /// <summary>Fila del detalle: pedido vs recibido por componente.</summary>
        private class DetalleVO
        {
            public string Codigo { get; set; }
            public string Descripcion { get; set; }
            public int Pedida { get; set; }
            public int Recibida { get; set; }
            public int Faltante { get { return Pedida - Recibida; } }
            public decimal CostoUnit { get; set; }
            public decimal Subtotal { get; set; }
        }

        private void FRMReporteCompras_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrillas();
            PoblarEstados();
            dtpDesde.ShowCheckBox = true; dtpDesde.Checked = false;
            dtpHasta.ShowCheckBox = true; dtpHasta.Checked = false;
            ActualizarIdioma();
            Buscar();
        }

        /// <summary>Carga el combo de estados preservando el seleccionado (0 = todos).</summary>
        private void PoblarEstados()
        {
            int sel = cmbEstado.SelectedIndex < 0 ? 0 : cmbEstado.SelectedIndex;
            cmbEstado.Items.Clear();
            cmbEstado.Items.AddRange(new object[]
            {
                IdiomaManager_GO44.T("rc.estadoTodos"),
                IdiomaManager_GO44.T("rc.estPendiente"),
                IdiomaManager_GO44.T("rc.estPagada"),
                IdiomaManager_GO44.T("rc.estRecibida"),
                IdiomaManager_GO44.T("rc.estRecibidaConFaltantes"),
                IdiomaManager_GO44.T("rc.estCancelada")
            });
            cmbEstado.SelectedIndex = sel < cmbEstado.Items.Count ? sel : 0;
        }

        /// <summary>Aplica los textos del idioma activo a títulos, etiquetas, botones y grillas.</summary>
        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("rc.ventana");
            if (lblTitulo != null)   lblTitulo.Text   = IdiomaManager_GO44.T("rc.titulo");
            if (lblDesde != null)    lblDesde.Text    = IdiomaManager_GO44.T("rc.desde");
            if (lblHasta != null)    lblHasta.Text    = IdiomaManager_GO44.T("rc.hasta");
            if (lblEstado != null)   lblEstado.Text   = IdiomaManager_GO44.T("rc.estado");
            if (btnBuscar != null)   btnBuscar.Text   = IdiomaManager_GO44.T("rep.buscar");
            if (grpDetalle != null)  grpDetalle.Text  = IdiomaManager_GO44.T("rc.detalle");
            if (btnExportar != null) btnExportar.Text = IdiomaManager_GO44.T("rep.exportarPdf");
            if (btnSalir != null)    btnSalir.Text    = IdiomaManager_GO44.T("general.salir");
            PoblarEstados();
            Buscar();
        }

        private void ConfigurarGrillas()
        {
            foreach (var g in new[] { dgvOrdenes, dgvDetalle })
            {
                g.ReadOnly = true;
                g.AllowUserToAddRows = false;
                g.AllowUserToDeleteRows = false;
                g.AllowUserToResizeRows = false;
                g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                g.MultiSelect = false;
                g.RowHeadersVisible = false;
                g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                g.BackgroundColor = Color.White;
                g.AutoGenerateColumns = true;
            }
            dgvOrdenes.DataSource = _bsOrdenes;
            dgvDetalle.DataSource = _bsDetalle;
            dgvOrdenes.SelectionChanged += (s, e) => MostrarDetalle();
        }

        private void Buscar()
        {
            var todas = _bllOC.Listar() ?? new List<BE_OrdenCompra_GO44>();
            DateTime? desde = dtpDesde.Checked ? (DateTime?)dtpDesde.Value.Date : null;
            DateTime? hasta = dtpHasta.Checked ? (DateTime?)dtpHasta.Value.Date.AddDays(1).AddSeconds(-1) : null;
            string estado = cmbEstado.SelectedIndex > 0 ? EstadosFiltro[cmbEstado.SelectedIndex] : null;

            var filtradas = new List<BE_OrdenCompra_GO44>();
            foreach (var oc in todas)
            {
                if (desde.HasValue && oc.FechaEmision < desde.Value) continue;
                if (hasta.HasValue && oc.FechaEmision > hasta.Value) continue;
                if (estado != null && oc.Estado.ToString() != estado) continue;
                filtradas.Add(oc);
            }

            _bsOrdenes.DataSource = null;
            _bsOrdenes.DataSource = filtradas;
            _bsOrdenes.ResetBindings(false);

            if (dgvOrdenes.Columns.Count > 0)
            {
                foreach (var col in new[] { "Id", "IdSolicitud", "UserNameEncargado", "Proveedor", "Lineas", "CantidadItems" })
                    if (dgvOrdenes.Columns.Contains(col)) dgvOrdenes.Columns[col].Visible = false;
                if (dgvOrdenes.Columns.Contains("NumeroOC"))     dgvOrdenes.Columns["NumeroOC"].HeaderText = IdiomaManager_GO44.T("rc.colNumeroOC");
                if (dgvOrdenes.Columns.Contains("NumeroSC"))     dgvOrdenes.Columns["NumeroSC"].HeaderText = IdiomaManager_GO44.T("rc.colNumeroSC");
                if (dgvOrdenes.Columns.Contains("FechaEmision")) { dgvOrdenes.Columns["FechaEmision"].HeaderText = IdiomaManager_GO44.T("rc.colEmision"); dgvOrdenes.Columns["FechaEmision"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
                if (dgvOrdenes.Columns.Contains("FechaEntrega")) { dgvOrdenes.Columns["FechaEntrega"].HeaderText = IdiomaManager_GO44.T("rc.colEntrega"); dgvOrdenes.Columns["FechaEntrega"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
                if (dgvOrdenes.Columns.Contains("Total"))        { dgvOrdenes.Columns["Total"].HeaderText = IdiomaManager_GO44.T("rc.colTotal"); dgvOrdenes.Columns["Total"].DefaultCellStyle.Format = "N2"; }
                if (dgvOrdenes.Columns.Contains("Estado"))       dgvOrdenes.Columns["Estado"].HeaderText = IdiomaManager_GO44.T("rc.colEstado");
            }
            lblContador.Text = string.Format(IdiomaManager_GO44.T("rc.contador"), _bsOrdenes.Count);
            MostrarDetalle();
        }

        private void MostrarDetalle()
        {
            var ocRow = dgvOrdenes.CurrentRow != null ? dgvOrdenes.CurrentRow.DataBoundItem as BE_OrdenCompra_GO44 : null;
            if (ocRow == null)
            {
                _ordenSel = null;
                _bsDetalle.DataSource = null; _bsDetalle.ResetBindings(false);
                lblProveedor.Text = IdiomaManager_GO44.T("rc.proveedorVacio");
                lblPago.Text = IdiomaManager_GO44.T("rc.pagoVacio");
                return;
            }

            _ordenSel = _bllOC.BuscarPorId(ocRow.Id);
            if (_ordenSel == null) return;

            // Proveedor completo (datos bancarios)
            if (_ordenSel.Proveedor != null)
            {
                var prov = _bllProveedor.BuscarPorCUIT(_ordenSel.Proveedor.CUIT);
                if (prov != null) _ordenSel.Proveedor = prov;
            }
            var p = _ordenSel.Proveedor;
            lblProveedor.Text = p != null
                ? string.Format(IdiomaManager_GO44.T("rc.proveedorFmt"), p.Empresa, p.CUIT, p.CVU, p.Banco)
                : IdiomaManager_GO44.T("rc.proveedorVacio");

            // Recepción (pedido vs recibido)
            var recepcion = _bllRecepcion.BuscarPorOrden(_ordenSel.Id);
            var recibidas = new Dictionary<int, int>();
            if (recepcion != null && recepcion.Lineas != null)
                foreach (var lr in recepcion.Lineas)
                    if (lr.Componente != null) recibidas[lr.Componente.Id] = lr.CantidadRecibida;

            var detalle = new List<DetalleVO>();
            foreach (var l in _ordenSel.Lineas)
            {
                int rec = (l.Componente != null && recibidas.ContainsKey(l.Componente.Id)) ? recibidas[l.Componente.Id] : 0;
                detalle.Add(new DetalleVO
                {
                    Codigo = l.ComponenteCodigo,
                    Descripcion = l.ComponenteNombre,
                    Pedida = l.Cantidad,
                    Recibida = rec,
                    CostoUnit = l.PrecioUnitario,
                    Subtotal = l.Subtotal
                });
            }
            _bsDetalle.DataSource = null;
            _bsDetalle.DataSource = detalle;
            _bsDetalle.ResetBindings(false);
            if (dgvDetalle.Columns.Count > 0)
            {
                if (dgvDetalle.Columns.Contains("Codigo"))      dgvDetalle.Columns["Codigo"].HeaderText = IdiomaManager_GO44.T("rc.colCodigo");
                if (dgvDetalle.Columns.Contains("Descripcion")) dgvDetalle.Columns["Descripcion"].HeaderText = IdiomaManager_GO44.T("rc.colComponente");
                if (dgvDetalle.Columns.Contains("Pedida"))      dgvDetalle.Columns["Pedida"].HeaderText = IdiomaManager_GO44.T("rc.colPedida");
                if (dgvDetalle.Columns.Contains("Recibida"))    dgvDetalle.Columns["Recibida"].HeaderText = IdiomaManager_GO44.T("rc.colRecibida");
                if (dgvDetalle.Columns.Contains("Faltante"))    dgvDetalle.Columns["Faltante"].HeaderText = IdiomaManager_GO44.T("rc.colFaltante");
                if (dgvDetalle.Columns.Contains("CostoUnit"))   { dgvDetalle.Columns["CostoUnit"].HeaderText = IdiomaManager_GO44.T("rc.colCostoUnit"); dgvDetalle.Columns["CostoUnit"].DefaultCellStyle.Format = "N2"; }
                if (dgvDetalle.Columns.Contains("Subtotal"))    { dgvDetalle.Columns["Subtotal"].HeaderText = IdiomaManager_GO44.T("rc.colSubtotal"); dgvDetalle.Columns["Subtotal"].DefaultCellStyle.Format = "N2"; }
            }
            foreach (DataGridViewRow row in dgvDetalle.Rows)
            {
                var d = row.DataBoundItem as DetalleVO;
                if (d != null && d.Faltante > 0) row.DefaultCellStyle.BackColor = Color.MistyRose;
            }

            var pago = _bllPago.BuscarPagoDeOrden(_ordenSel.Id);
            lblPago.Text = pago != null
                ? string.Format(IdiomaManager_GO44.T("rc.pagoFmt"), pago.MontoAbonado.ToString("N2"), pago.FechaOperacion.ToString("dd/MM/yyyy"), pago.NroComprobante)
                : IdiomaManager_GO44.T("rc.pagoNoPagada");
        }

        private void btnBuscar_Click(object sender, EventArgs e) { Buscar(); }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_ordenSel == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("rc.seleccioneOrden"), IdiomaManager_GO44.T("general.advertencia"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "ReporteCompra_" + _ordenSel.NumeroOC + ".pdf";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var p = _ordenSel.Proveedor;
                    string titulo = string.Format(IdiomaManager_GO44.T("rc.pdfTitulo"), _ordenSel.NumeroOC);
                    string subtitulo = string.Format(IdiomaManager_GO44.T("rc.pdfSubtitulo"),
                        _ordenSel.Estado,
                        _ordenSel.FechaEmision.ToString("dd/MM/yyyy"),
                        _ordenSel.FechaEntrega.ToString("dd/MM/yyyy"),
                        (p != null ? p.Empresa : ""));

                    string[] headers =
                    {
                        IdiomaManager_GO44.T("rc.colCodigo"), IdiomaManager_GO44.T("rc.colComponente"),
                        IdiomaManager_GO44.T("rc.colPedida"), IdiomaManager_GO44.T("rc.colRecibida"),
                        IdiomaManager_GO44.T("rc.colCostoUnit"), IdiomaManager_GO44.T("rc.colSubtotal")
                    };
                    float[] anchos = { 0.12f, 0.40f, 0.12f, 0.12f, 0.12f, 0.12f };
                    var filas = new List<string[]>();
                    foreach (DataGridViewRow row in dgvDetalle.Rows)
                    {
                        var d = row.DataBoundItem as DetalleVO;
                        if (d == null) continue;
                        filas.Add(new string[] { d.Codigo ?? "", d.Descripcion ?? "", d.Pedida.ToString(),
                            d.Recibida.ToString(), "$ " + d.CostoUnit.ToString("N2"), "$ " + d.Subtotal.ToString("N2") });
                    }
                    filas.Add(new string[] { "", "", "", "", "", "" });
                    filas.Add(new string[] { "", "", "", "", IdiomaManager_GO44.T("rc.pdfTotal"), "$ " + _ordenSel.Total.ToString("N2") });
                    if (p != null)
                    {
                        filas.Add(new string[] { "", "", "", "", "", "" });
                        filas.Add(new string[] { IdiomaManager_GO44.T("rc.pdfProveedor"), p.Empresa + " (CUIT " + p.CUIT + ")", "", "", "", "" });
                        filas.Add(new string[] { IdiomaManager_GO44.T("rc.pdfPago"), "CVU " + p.CVU + " · " + p.Banco + " · " + p.Titular, "", "", "", "" });
                    }

                    new BLL.GeneradorPdf_GO44().Generar(sfd.FileName, titulo, subtitulo, headers, anchos, filas);
                    var abrir = MessageBox.Show(string.Format(IdiomaManager_GO44.T("rep.pdfGenerado"), sfd.FileName),
                        IdiomaManager_GO44.T("rep.reporte"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (abrir == DialogResult.Yes && System.IO.File.Exists(sfd.FileName))
                        System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("rep.pdfError"), ex.Message), IdiomaManager_GO44.T("general.error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
