using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// Plan de Promociones y gráfico de más vendidos (extensión del Reporte Inteligente).
    /// Grafica los componentes con más salida y propone combos/promociones automáticamente:
    /// liquidar stock muerto, armar combos (top-seller + producto parado) y reponer urgentes.
    /// </summary>
    public partial class FRMPromociones_GO44 : Form
    {
        private readonly BLLReporteInteligente_GO44 _bll;
        private readonly BindingSource _bsPromos = new BindingSource();
        private List<BE_ReporteInteligente_GO44> _topVendidos = new List<BE_ReporteInteligente_GO44>();

        private static readonly Color Azul = Color.FromArgb(13, 71, 161);

        public FRMPromociones_GO44()
        {
            InitializeComponent();
            _bll = new BLLReporteInteligente_GO44();
        }

        private void FRMPromociones_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            nudLeadTime.Value = BLLReporteInteligente_GO44.LEAD_TIME_DEFAULT;
            pnlGrafico.Paint += pnlGrafico_Paint;
            Generar();
        }

        private void ConfigurarGrilla()
        {
            dgvPromos.ReadOnly = true;
            dgvPromos.AllowUserToAddRows = false;
            dgvPromos.AllowUserToDeleteRows = false;
            dgvPromos.AllowUserToResizeRows = false;
            dgvPromos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPromos.MultiSelect = false;
            dgvPromos.RowHeadersVisible = false;
            dgvPromos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPromos.BackgroundColor = Color.White;
            dgvPromos.AutoGenerateColumns = false;
            dgvPromos.Columns.Clear();
            Col("Tipo",         "Tipo",      13, DataGridViewContentAlignment.MiddleLeft);
            Col("Titulo",       "Propuesta", 30, DataGridViewContentAlignment.MiddleLeft);
            Col("Detalle",      "Detalle",   47, DataGridViewContentAlignment.MiddleLeft);
            Col("DescuentoTxt", "Desc.",     10, DataGridViewContentAlignment.MiddleCenter);
            dgvPromos.DataSource = _bsPromos;
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
            dgvPromos.Columns.Add(c);
        }

        private void Generar()
        {
            var datos = _bll.Generar((int)nudLeadTime.Value);
            _topVendidos = _bll.MasVendidos(datos, 8);
            var promos = _bll.ArmarPromociones(datos);

            _bsPromos.DataSource = null;
            _bsPromos.DataSource = promos ?? new List<BE_Promocion_GO44>();
            _bsPromos.ResetBindings(false);

            foreach (DataGridViewRow row in dgvPromos.Rows)
            {
                var p = row.DataBoundItem as BE_Promocion_GO44;
                if (p == null) continue;
                if (p.Tipo == "Combo") row.DefaultCellStyle.BackColor = Color.LightCyan;
                else if (p.Tipo == "Liquidación") row.DefaultCellStyle.BackColor = Color.Gainsboro;
                else if (p.Tipo == "Reposición") row.DefaultCellStyle.BackColor = Color.MistyRose;
            }

            lblResumen.Text = "Propuestas: " + _bsPromos.Count;
            pnlGrafico.Invalidate();
        }

        private void pnlGrafico_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle rect = pnlGrafico.ClientRectangle;
            g.Clear(Color.White);

            using (var fTitulo = new Font("Segoe UI Semibold", 10F, FontStyle.Bold))
                TextRenderer.DrawText(g, "Componentes más vendidos (últimos 30 días)",
                    fTitulo, new Rectangle(0, 4, rect.Width, 20), Azul,
                    TextFormatFlags.HorizontalCenter);

            if (_topVendidos == null || _topVendidos.Count == 0)
            {
                using (var f = new Font("Segoe UI", 10F, FontStyle.Italic))
                    TextRenderer.DrawText(g, "Sin ventas registradas para graficar", f, rect, Color.Gray,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            int marginL = 45, marginR = 20, marginT = 32, marginB = 42;
            int plotW = rect.Width - marginL - marginR;
            int plotH = rect.Height - marginT - marginB;
            int n = _topVendidos.Count;
            int max = _topVendidos.Max(x => x.Vendidas30);
            if (max <= 0) max = 1;

            using (var penAxis = new Pen(Color.Gainsboro))
            {
                g.DrawLine(penAxis, marginL, marginT, marginL, marginT + plotH);
                g.DrawLine(penAxis, marginL, marginT + plotH, marginL + plotW, marginT + plotH);
            }

            int gap = 14;
            int barW = Math.Max(10, (plotW - gap * (n + 1)) / n);
            using (var brush = new SolidBrush(Azul))
            using (var fVal = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var fLbl = new Font("Segoe UI", 8F))
            {
                for (int i = 0; i < n; i++)
                {
                    var item = _topVendidos[i];
                    int h = (int)((double)item.Vendidas30 / max * plotH);
                    if (h < 2) h = 2;
                    int x = marginL + gap + i * (barW + gap);
                    int y = marginT + plotH - h;
                    g.FillRectangle(brush, x, y, barW, h);
                    TextRenderer.DrawText(g, item.Vendidas30.ToString(), fVal,
                        new Rectangle(x - 6, y - 16, barW + 12, 14), Azul, TextFormatFlags.HorizontalCenter);
                    TextRenderer.DrawText(g, item.Codigo, fLbl,
                        new Rectangle(x - 8, marginT + plotH + 4, barW + 16, 28), Color.Black,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
                }
            }
        }

        private void btnGenerar_Click(object sender, EventArgs e) { Generar(); }
        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
