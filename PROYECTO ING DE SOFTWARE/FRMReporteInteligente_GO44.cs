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
    /// Rep. 3 (INT.) — "Estrategia de Ventas".
    /// Cruza la velocidad de salida de cada componente con el stock para calcular rotación,
    /// días de inventario, reposición urgente y stock muerto (con descuento sugerido).
    /// </summary>
    public partial class FRMReporteInteligente_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly BLLReporteInteligente_GO44 _bll;
        private readonly BindingSource _bs = new BindingSource();

        public FRMReporteInteligente_GO44()
        {
            InitializeComponent();
            _bll = new BLLReporteInteligente_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        private void FRMReporteInteligente_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            nudLeadTime.Value = BLLReporteInteligente_GO44.LEAD_TIME_DEFAULT;
            ActualizarIdioma();
        }

        /// <summary>Aplica los textos del idioma activo a títulos, etiquetas, botones y grilla.</summary>
        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("ri.ventana");
            if (lblTitulo != null)   lblTitulo.Text   = IdiomaManager_GO44.T("ri.titulo");
            if (lblLeadTime != null) lblLeadTime.Text = IdiomaManager_GO44.T("ri.leadTime");
            if (btnGenerar != null)  btnGenerar.Text  = IdiomaManager_GO44.T("rep.generar");
            if (btnPromos != null)   btnPromos.Text   = IdiomaManager_GO44.T("ri.btnPromos");
            if (btnExportar != null) btnExportar.Text = IdiomaManager_GO44.T("rep.exportarPdf");
            if (btnSalir != null)    btnSalir.Text    = IdiomaManager_GO44.T("general.salir");

            if (dgv.Columns.Count >= 11)
            {
                dgv.Columns[0].HeaderText  = IdiomaManager_GO44.T("ri.colCodigo");
                dgv.Columns[1].HeaderText  = IdiomaManager_GO44.T("ri.colProducto");
                dgv.Columns[2].HeaderText  = IdiomaManager_GO44.T("ri.colCategoria");
                dgv.Columns[3].HeaderText  = IdiomaManager_GO44.T("ri.colStock");
                dgv.Columns[4].HeaderText  = IdiomaManager_GO44.T("ri.colVend30");
                dgv.Columns[5].HeaderText  = IdiomaManager_GO44.T("ri.colVentaDia");
                dgv.Columns[6].HeaderText  = IdiomaManager_GO44.T("ri.colDiasInv");
                dgv.Columns[7].HeaderText  = IdiomaManager_GO44.T("ri.colRotacion");
                dgv.Columns[8].HeaderText  = IdiomaManager_GO44.T("ri.colDiasSinVenta");
                dgv.Columns[9].HeaderText  = IdiomaManager_GO44.T("ri.colAlerta");
                dgv.Columns[10].HeaderText = IdiomaManager_GO44.T("ri.colDesc");
            }
            Generar();
        }

        private void ConfigurarGrilla()
        {
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear();
            Col("Codigo",            "Código",      10, DataGridViewContentAlignment.MiddleLeft);
            Col("Nombre",            "Producto",    20, DataGridViewContentAlignment.MiddleLeft);
            Col("Categoria",         "Categoría",   13, DataGridViewContentAlignment.MiddleLeft);
            Col("StockActual",       "Stock",        7, DataGridViewContentAlignment.MiddleCenter);
            Col("Vendidas30",        "Vend. 30d",    9, DataGridViewContentAlignment.MiddleCenter);
            Col("VentaDiariaTxt",    "Venta/día",    9, DataGridViewContentAlignment.MiddleCenter);
            Col("DiasInventarioTxt", "Días inv.",    8, DataGridViewContentAlignment.MiddleCenter);
            Col("RotacionTxt",       "Rotación",     8, DataGridViewContentAlignment.MiddleCenter);
            Col("DiasSinVentaTxt",   "Días s/venta", 9, DataGridViewContentAlignment.MiddleCenter);
            Col("Alerta",            "Alerta",      13, DataGridViewContentAlignment.MiddleLeft);
            Col("DescuentoTxt",      "Desc.",        7, DataGridViewContentAlignment.MiddleCenter);
            dgv.DataSource = _bs;
            dgv.CellFormatting += dgv_CellFormatting;
        }

        private void Col(string prop, string header, int fill, DataGridViewContentAlignment align)
        {
            var c = new DataGridViewTextBoxColumn
            {
                DataPropertyName = prop,
                HeaderText = header,
                FillWeight = fill,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            c.DefaultCellStyle.Alignment = align;
            dgv.Columns.Add(c);
        }

        /// <summary>Traduce al vuelo el "nunca" de la columna Días s/venta (viene en texto desde el BE).</summary>
        private void dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null) return;
            if (dgv.Columns[e.ColumnIndex].DataPropertyName == "DiasSinVentaTxt" && e.Value.ToString() == "nunca")
            {
                e.Value = IdiomaManager_GO44.T("ri.nunca");
                e.FormattingApplied = true;
            }
        }

        private void Generar()
        {
            var lista = _bll.Generar((int)nudLeadTime.Value);
            _bs.DataSource = null;
            _bs.DataSource = lista ?? new List<BE_ReporteInteligente_GO44>();
            _bs.ResetBindings(false);

            int urgentes = 0, muertos = 0;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var r = row.DataBoundItem as BE_ReporteInteligente_GO44;
                if (r == null) continue;
                if (r.AlertaCod == "REPONER") { row.DefaultCellStyle.BackColor = Color.LightCoral; urgentes++; }
                else if (r.AlertaCod == "MUERTO") { row.DefaultCellStyle.BackColor = Color.Gainsboro; muertos++; }
            }
            lblResumen.Text = string.Format(IdiomaManager_GO44.T("ri.resumen"), _bs.Count, urgentes, muertos);
        }

        private void btnGenerar_Click(object sender, EventArgs e) { Generar(); }

        private void btnPromos_Click(object sender, EventArgs e)
        {
            using (var frm = new FRMPromociones_GO44())
                frm.ShowDialog(this);
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_bs.Count == 0)
            {
                MessageBox.Show(IdiomaManager_GO44.T("rep.sinDatos"), IdiomaManager_GO44.T("general.advertencia"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "ReporteInteligente_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    string titulo = IdiomaManager_GO44.T("ri.pdfTitulo");
                    string subtitulo = string.Format(IdiomaManager_GO44.T("ri.pdfSubtitulo"),
                        (int)nudLeadTime.Value, DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    string[] headers =
                    {
                        IdiomaManager_GO44.T("ri.colCodigo"), IdiomaManager_GO44.T("ri.colProducto"),
                        IdiomaManager_GO44.T("ri.colStock"), IdiomaManager_GO44.T("ri.colVend30"),
                        IdiomaManager_GO44.T("ri.colDiasInv"), IdiomaManager_GO44.T("ri.colRotacion"),
                        IdiomaManager_GO44.T("ri.pdfColDiasSinVenta"), IdiomaManager_GO44.T("ri.colAlerta")
                    };
                    float[] anchos = { 0.11f, 0.29f, 0.09f, 0.10f, 0.10f, 0.10f, 0.10f, 0.11f };
                    var filas = new List<string[]>();
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        var r = row.DataBoundItem as BE_ReporteInteligente_GO44;
                        if (r == null) continue;
                        string diasSinVenta = r.DiasSinVenta < 0 ? IdiomaManager_GO44.T("ri.nunca") : r.DiasSinVentaTxt;
                        string alerta = r.Alerta + (r.DescuentoSugerido > 0 ? string.Format(IdiomaManager_GO44.T("ri.pdfDescSuffix"), r.DescuentoSugerido.ToString("N0")) : "");
                        filas.Add(new string[] { r.Codigo ?? "", r.Nombre ?? "", r.StockActual.ToString(),
                            r.Vendidas30.ToString(), r.DiasInventarioTxt, r.RotacionTxt, diasSinVenta, alerta });
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
