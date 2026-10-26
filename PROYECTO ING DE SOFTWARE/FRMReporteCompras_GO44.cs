using BE;
using BLL;
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
    public partial class FRMReporteCompras_GO44 : Form
    {
        private readonly BLLOrdenCompra_GO44 _bllOC;
        private readonly BLLRecepcionMercaderia_GO44 _bllRecepcion;
        private readonly BLLPagoOrdenCompra_GO44 _bllPago;
        private readonly BLLProveedor_GO44 _bllProveedor;

        private readonly BindingSource _bsOrdenes = new BindingSource();
        private readonly BindingSource _bsDetalle = new BindingSource();
        private BE_OrdenCompra_GO44 _ordenSel;

        public FRMReporteCompras_GO44()
        {
            InitializeComponent();
            _bllOC        = new BLLOrdenCompra_GO44();
            _bllRecepcion = new BLLRecepcionMercaderia_GO44();
            _bllPago      = new BLLPagoOrdenCompra_GO44();
            _bllProveedor = new BLLProveedor_GO44();
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
            cmbEstado.Items.AddRange(new object[] { "(todos)", "Pendiente", "Pagada", "Recibida", "RecibidaConFaltantes", "Cancelada" });
            cmbEstado.SelectedIndex = 0;
            dtpDesde.ShowCheckBox = true; dtpDesde.Checked = false;
            dtpHasta.ShowCheckBox = true; dtpHasta.Checked = false;
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
            string estado = cmbEstado.SelectedIndex > 0 ? cmbEstado.SelectedItem.ToString() : null;

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
                foreach (var col in new[] { "Id", "IdSolicitud", "LoginEncargado", "Proveedor", "Lineas", "CantidadItems" })
                    if (dgvOrdenes.Columns.Contains(col)) dgvOrdenes.Columns[col].Visible = false;
                if (dgvOrdenes.Columns.Contains("NumeroOC"))     dgvOrdenes.Columns["NumeroOC"].HeaderText = "N° OC";
                if (dgvOrdenes.Columns.Contains("NumeroSC"))     dgvOrdenes.Columns["NumeroSC"].HeaderText = "N° SC";
                if (dgvOrdenes.Columns.Contains("FechaEmision")) { dgvOrdenes.Columns["FechaEmision"].HeaderText = "Emisión"; dgvOrdenes.Columns["FechaEmision"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
                if (dgvOrdenes.Columns.Contains("FechaEntrega")) { dgvOrdenes.Columns["FechaEntrega"].HeaderText = "Entrega"; dgvOrdenes.Columns["FechaEntrega"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
                if (dgvOrdenes.Columns.Contains("Total"))        { dgvOrdenes.Columns["Total"].HeaderText = "Total"; dgvOrdenes.Columns["Total"].DefaultCellStyle.Format = "N2"; }
                if (dgvOrdenes.Columns.Contains("Estado"))       dgvOrdenes.Columns["Estado"].HeaderText = "Estado";
            }
            lblContador.Text = "Órdenes: " + _bsOrdenes.Count;
            MostrarDetalle();
        }

        private void MostrarDetalle()
        {
            var ocRow = dgvOrdenes.CurrentRow != null ? dgvOrdenes.CurrentRow.DataBoundItem as BE_OrdenCompra_GO44 : null;
            if (ocRow == null)
            {
                _ordenSel = null;
                _bsDetalle.DataSource = null; _bsDetalle.ResetBindings(false);
                lblProveedor.Text = "Proveedor: —";
                lblPago.Text = "Pago: —";
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
                ? "Proveedor: " + p.Empresa + "  ·  CUIT " + p.CUIT + "  ·  CVU " + p.CVU + "  ·  " + p.Banco
                : "Proveedor: —";

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
                if (dgvDetalle.Columns.Contains("Codigo"))      dgvDetalle.Columns["Codigo"].HeaderText = "Cód.";
                if (dgvDetalle.Columns.Contains("Descripcion")) dgvDetalle.Columns["Descripcion"].HeaderText = "Componente";
                if (dgvDetalle.Columns.Contains("Pedida"))      dgvDetalle.Columns["Pedida"].HeaderText = "Pedida";
                if (dgvDetalle.Columns.Contains("Recibida"))    dgvDetalle.Columns["Recibida"].HeaderText = "Recibida";
                if (dgvDetalle.Columns.Contains("Faltante"))    dgvDetalle.Columns["Faltante"].HeaderText = "Faltante";
                if (dgvDetalle.Columns.Contains("CostoUnit"))   { dgvDetalle.Columns["CostoUnit"].HeaderText = "Costo unit."; dgvDetalle.Columns["CostoUnit"].DefaultCellStyle.Format = "N2"; }
                if (dgvDetalle.Columns.Contains("Subtotal"))    { dgvDetalle.Columns["Subtotal"].HeaderText = "Subtotal"; dgvDetalle.Columns["Subtotal"].DefaultCellStyle.Format = "N2"; }
            }
            foreach (DataGridViewRow row in dgvDetalle.Rows)
            {
                var d = row.DataBoundItem as DetalleVO;
                if (d != null && d.Faltante > 0) row.DefaultCellStyle.BackColor = Color.MistyRose;
            }

            var pago = _bllPago.BuscarPagoDeOrden(_ordenSel.Id);
            lblPago.Text = pago != null
                ? "Pago: $" + pago.MontoAbonado.ToString("N2") + "  ·  " + pago.FechaOperacion.ToString("dd/MM/yyyy") + "  ·  Comp. " + pago.NroComprobante
                : "Pago: (no pagada)";
        }

        private void btnBuscar_Click(object sender, EventArgs e) { Buscar(); }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_ordenSel == null)
            {
                MessageBox.Show("Seleccione una orden de compra para exportar", "Aviso",
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
                    string titulo = "TECHFLOW - RECOMPOSICIÓN DE COMPONENTES · " + _ordenSel.NumeroOC;
                    string subtitulo =
                        "Estado: " + _ordenSel.Estado +
                        "   |   Emisión: " + _ordenSel.FechaEmision.ToString("dd/MM/yyyy") +
                        "   |   Entrega: " + _ordenSel.FechaEntrega.ToString("dd/MM/yyyy") +
                        "   |   Proveedor: " + (p != null ? p.Empresa : "");

                    string[] headers = { "Cód.", "Componente", "Pedida", "Recibida", "Costo unit.", "Subtotal" };
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
                    filas.Add(new string[] { "", "", "", "", "TOTAL:", "$ " + _ordenSel.Total.ToString("N2") });
                    if (p != null)
                    {
                        filas.Add(new string[] { "", "", "", "", "", "" });
                        filas.Add(new string[] { "Proveedor", p.Empresa + " (CUIT " + p.CUIT + ")", "", "", "", "" });
                        filas.Add(new string[] { "Pago", "CVU " + p.CVU + " · " + p.Banco + " · " + p.Titular, "", "", "", "" });
                    }

                    new BLL.GeneradorPdf_GO44().Generar(sfd.FileName, titulo, subtitulo, headers, anchos, filas);
                    var abrir = MessageBox.Show("Reporte generado en:\n" + sfd.FileName + "\n\n¿Abrirlo?",
                        "Reporte", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (abrir == DialogResult.Yes && System.IO.File.Exists(sfd.FileName))
                        System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo generar el PDF: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
