using BE;
using BLL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// Reporte de facturas con filtros (fecha desde/hasta, DNI cliente, estado).
    /// Genera un PDF de la factura seleccionada usando GeneradorPdf_GO44
    /// (implementación pura de PDF, sin depender de impresoras ni librerías externas).
    /// </summary>
    public partial class FRMReporteFacturas_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly BLLFactura_GO44 _bll;

        // BindingSource para evitar issues con rebind
        private readonly System.Windows.Forms.BindingSource _bsFacturas = new System.Windows.Forms.BindingSource();

        public FRMReporteFacturas_GO44()
        {
            InitializeComponent();
            _bll = new BLLFactura_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("reporte.titulo");
            if (lblTitulo != null)      lblTitulo.Text      = IdiomaManager_GO44.T("reporte.titulo");
            if (grpFiltros != null)     grpFiltros.Text     = IdiomaManager_GO44.T("reporte.filtros");
            if (chkDesde != null)       chkDesde.Text       = IdiomaManager_GO44.T("reporte.desde");
            if (chkHasta != null)       chkHasta.Text       = IdiomaManager_GO44.T("reporte.hasta");
            if (lblDni != null)         lblDni.Text         = IdiomaManager_GO44.T("reporte.dni");
            if (lblEstado != null)      lblEstado.Text      = IdiomaManager_GO44.T("reporte.estado");
            if (btnBuscar != null)      btnBuscar.Text      = IdiomaManager_GO44.T("reporte.btnBuscar");
            if (btnLimpiar != null)     btnLimpiar.Text     = IdiomaManager_GO44.T("reporte.btnLimpiar");
            if (btnGenerarPDF != null)  btnGenerarPDF.Text  = IdiomaManager_GO44.T("reporte.btnGenerarPDF");
            if (btnSalir != null)       btnSalir.Text       = IdiomaManager_GO44.T("reporte.btnSalir");

            // Recargar el combo de estado preservando la selección
            if (cmbEstado != null)
            {
                int idx = cmbEstado.SelectedIndex;
                cmbEstado.Items.Clear();
                cmbEstado.Items.AddRange(new object[] {
                    IdiomaManager_GO44.T("reporte.estadoTodos"),
                    IdiomaManager_GO44.T("reporte.estadoPendiente"),
                    IdiomaManager_GO44.T("reporte.estadoCobrada"),
                    IdiomaManager_GO44.T("reporte.estadoAnulada")
                });
                cmbEstado.SelectedIndex = Math.Max(0, Math.Min(idx, cmbEstado.Items.Count - 1));
            }

            Buscar();
        }

        private void FRMReporteFacturas_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();

            cmbEstado.Items.Clear();
            cmbEstado.Items.AddRange(new object[] {
                IdiomaManager_GO44.T("reporte.estadoTodos"),
                IdiomaManager_GO44.T("reporte.estadoPendiente"),
                IdiomaManager_GO44.T("reporte.estadoCobrada"),
                IdiomaManager_GO44.T("reporte.estadoAnulada")
            });
            cmbEstado.SelectedIndex = 0;

            // Filtro por DNI: máximo 8 dígitos, solo números
            txtDni.MaxLength = 8;
            txtDni.KeyPress += (s, ev) =>
            {
                if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar))
                    ev.Handled = true;
            };

            // Rango por defecto: desde hace 1 mes hasta HOY
            // Ninguna fecha puede ser posterior al día actual
            dtpDesde.MaxDate = DateTime.Today;
            dtpHasta.MaxDate = DateTime.Today;
            dtpDesde.Value = DateTime.Today.AddMonths(-1);
            dtpHasta.Value = DateTime.Today;
            chkDesde.Checked = true;
            chkHasta.Checked = true;

            ActualizarIdioma();
        }

        private void ConfigurarGrilla()
        {
            dgvFacturas.ReadOnly = true;
            dgvFacturas.AllowUserToAddRows = false;
            dgvFacturas.AllowUserToDeleteRows = false;
            dgvFacturas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFacturas.MultiSelect = false;
            dgvFacturas.RowHeadersVisible = false;
            dgvFacturas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFacturas.AllowUserToResizeColumns = false;
            dgvFacturas.AllowUserToResizeRows    = false;
            dgvFacturas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvFacturas.RowHeadersWidthSizeMode    = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgvFacturas.BackgroundColor = Color.White;
            dgvFacturas.AutoGenerateColumns = true;
            dgvFacturas.DataSource = _bsFacturas;
        }

        // ============ BUSCAR ============

        private void btnBuscar_Click(object sender, EventArgs e) { Buscar(); }

        private void Buscar()
        {
            try
            {
                DateTime? desde = chkDesde.Checked ? (DateTime?)dtpDesde.Value.Date : null;
                DateTime? hasta = chkHasta.Checked ? (DateTime?)dtpHasta.Value.Date : null;
                string dni = txtDni.Text.Trim();
                string estadoSel = cmbEstado.SelectedItem as string;
                string estado = null;
                // Traducir el label del combo a valor de negocio (Pendiente/Cobrada/Anulada)
                if (estadoSel == IdiomaManager_GO44.T("reporte.estadoPendiente")) estado = "Pendiente";
                else if (estadoSel == IdiomaManager_GO44.T("reporte.estadoCobrada")) estado = "Cobrada";
                else if (estadoSel == IdiomaManager_GO44.T("reporte.estadoAnulada")) estado = "Anulada";
                // "(todos)" o "(all)" → null (sin filtro)

                List<BE_Factura_GO44> lista = _bll.ListarConFiltros(desde, hasta, dni, estado);
                _bsFacturas.DataSource = lista;
                _bsFacturas.ResetBindings(false);

                if (dgvFacturas.Columns.Count > 0)
                {
                    if (dgvFacturas.Columns.Contains("Lineas"))       dgvFacturas.Columns["Lineas"].Visible = false;
                    if (dgvFacturas.Columns.Contains("Cliente"))      dgvFacturas.Columns["Cliente"].Visible = false;
                    if (dgvFacturas.Columns.Contains("CantidadItems"))dgvFacturas.Columns["CantidadItems"].Visible = false;
                    if (dgvFacturas.Columns.Contains("DniCliente"))   dgvFacturas.Columns["DniCliente"].HeaderText = IdiomaManager_GO44.T("reporte.col.dniCliente");
                    if (dgvFacturas.Columns.Contains("NumeroFactura"))dgvFacturas.Columns["NumeroFactura"].HeaderText = IdiomaManager_GO44.T("reporte.col.nroFactura");
                    if (dgvFacturas.Columns.Contains("FechaEmision")) dgvFacturas.Columns["FechaEmision"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    if (dgvFacturas.Columns.Contains("Subtotal"))     dgvFacturas.Columns["Subtotal"].DefaultCellStyle.Format = "N2";
                    if (dgvFacturas.Columns.Contains("IVA"))          dgvFacturas.Columns["IVA"].DefaultCellStyle.Format = "N2";
                    if (dgvFacturas.Columns.Contains("Total"))        dgvFacturas.Columns["Total"].DefaultCellStyle.Format = "N2";
                }

                foreach (DataGridViewRow row in dgvFacturas.Rows)
                {
                    BE_Factura_GO44 f = row.DataBoundItem as BE_Factura_GO44;
                    if (f == null) continue;
                    if (f.Estado == BE_Factura_GO44.EstadoFactura.Cobrada)     row.DefaultCellStyle.BackColor = Color.Honeydew;
                    else if (f.Estado == BE_Factura_GO44.EstadoFactura.Anulada) row.DefaultCellStyle.BackColor = Color.MistyRose;
                }

                lblTotal.Text = string.Format(IdiomaManager_GO44.T("reporte.total"),
                    lista != null ? lista.Count : 0,
                    lista != null ? lista.Sum(f => f.Total).ToString("N2") : "0.00");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(IdiomaManager_GO44.T("reporte.errBuscar"), ex.Message),
                    IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            chkDesde.Checked = false;
            chkHasta.Checked = false;
            txtDni.Text = "";
            cmbEstado.SelectedIndex = 0;
            Buscar();
        }

        // ============ GENERAR PDF ============

        private void btnGenerarPDF_Click(object sender, EventArgs e)
        {
            if (dgvFacturas.CurrentRow == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("reporte.seleccFactura"),
                    IdiomaManager_GO44.T("general.advertencia"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BE_Factura_GO44 seleccionada = dgvFacturas.CurrentRow.DataBoundItem as BE_Factura_GO44;
            if (seleccionada == null) return;

            // Traer la factura COMPLETA con líneas (la grilla trae solo encabezado)
            BE_Factura_GO44 completa = _bll.BuscarPorId(seleccionada.Id);
            if (completa == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("reporte.noCargoDetalle"),
                    IdiomaManager_GO44.T("general.error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Pedir dónde guardar el PDF
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = IdiomaManager_GO44.T("reporte.filtro");
                sfd.FileName = "Factura_" + completa.NumeroFactura + ".pdf";
                sfd.Title = IdiomaManager_GO44.T("reporte.guardarTitulo");
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // Armar los datos para el generador PDF
                    string titulo = "TECHFLOW - ELECTROPOINT · FACTURA " + completa.NumeroFactura;
                    string subtitulo =
                        "Fecha: " + completa.FechaEmision.ToString("dd/MM/yyyy HH:mm") +
                        "   |   Cajero: " + completa.LoginCajero +
                        "   |   Cliente: " + (completa.Cliente != null ? completa.Cliente.DNI : "") +
                        "   |   Estado: " + completa.Estado;

                    string[] headers = { "Codigo", "Componente", "Cant", "Precio Unit.", "Subtotal" };
                    // Proporciones que suman 1 (aproximadamente)
                    float[] anchos = { 0.12f, 0.48f, 0.10f, 0.15f, 0.15f };

                    List<string[]> filas = new List<string[]>();
                    if (completa.Lineas != null)
                    {
                        foreach (var l in completa.Lineas)
                        {
                            filas.Add(new string[] {
                                l.ComponenteCodigo ?? "",
                                l.ComponenteNombre ?? "",
                                l.Cantidad.ToString(),
                                "$ " + l.PrecioUnitario.ToString("N2"),
                                "$ " + l.Subtotal.ToString("N2")
                            });
                        }
                    }

                    // Filas de totales (separadas visualmente)
                    filas.Add(new string[] { "", "", "", "", "" });
                    filas.Add(new string[] { "", "", "", "Subtotal:",  "$ " + completa.Subtotal.ToString("N2") });
                    filas.Add(new string[] { "", "", "", "IVA (21%):", "$ " + completa.IVA.ToString("N2") });
                    filas.Add(new string[] { "", "", "", "TOTAL:",     "$ " + completa.Total.ToString("N2") });

                    // Generar el PDF (implementación pura, sin dependencias)
                    GeneradorPdf_GO44 gen = new GeneradorPdf_GO44();
                    gen.Generar(sfd.FileName, titulo, subtitulo, headers, anchos, filas);

                    var r = MessageBox.Show(string.Format(IdiomaManager_GO44.T("reporte.pdfGeneradoMensaje"), sfd.FileName),
                        IdiomaManager_GO44.T("reporte.pdfGeneradoTitulo"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (r == DialogResult.Yes && System.IO.File.Exists(sfd.FileName))
                        System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("reporte.errPDF"), ex.Message),
                        IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
