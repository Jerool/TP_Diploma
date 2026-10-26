using BE;
using BLL;
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
    public partial class FRMReporteInteligente_GO44 : Form
    {
        private readonly BLLReporteInteligente_GO44 _bll;
        private readonly BindingSource _bs = new BindingSource();

        public FRMReporteInteligente_GO44()
        {
            InitializeComponent();
            _bll = new BLLReporteInteligente_GO44();
        }

        private void FRMReporteInteligente_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            nudLeadTime.Value = BLLReporteInteligente_GO44.LEAD_TIME_DEFAULT;
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
                if (r.Alerta == "Reponer urgente") { row.DefaultCellStyle.BackColor = Color.LightCoral; urgentes++; }
                else if (r.Alerta == "Stock muerto") { row.DefaultCellStyle.BackColor = Color.Gainsboro; muertos++; }
            }
            lblResumen.Text = "Productos: " + _bs.Count + "   ·   Reponer urgente: " + urgentes + "   ·   Stock muerto: " + muertos;
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
                MessageBox.Show("No hay datos para exportar", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "ReporteInteligente_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    string titulo = "TECHFLOW - ESTRATEGIA DE VENTAS (Reporte Inteligente)";
                    string subtitulo = "Tiempo de entrega estimado: " + (int)nudLeadTime.Value +
                                       " días   |   Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    string[] headers = { "Código", "Producto", "Stock", "Vend.30d", "Días inv.", "Rotación", "Días s/v", "Alerta" };
                    float[] anchos = { 0.11f, 0.29f, 0.09f, 0.10f, 0.10f, 0.10f, 0.10f, 0.11f };
                    var filas = new List<string[]>();
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        var r = row.DataBoundItem as BE_ReporteInteligente_GO44;
                        if (r == null) continue;
                        string alerta = r.Alerta + (r.DescuentoSugerido > 0 ? " (" + r.DescuentoSugerido.ToString("N0") + "% desc.)" : "");
                        filas.Add(new string[] { r.Codigo ?? "", r.Nombre ?? "", r.StockActual.ToString(),
                            r.Vendidas30.ToString(), r.DiasInventarioTxt, r.RotacionTxt, r.DiasSinVentaTxt, alerta });
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
