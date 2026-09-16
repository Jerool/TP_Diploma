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
    /// Gestión de Productos (Componentes). Consulta con filtro por código / categoría.
    /// Nota: ABM completo (alta / modificación de precio y stock) queda como TD para
    /// próxima iteración — requiere sumar SPs de INSERT/UPDATE.
    /// </summary>
    public partial class FRMGestionProductos_GO44 : Form
    {
        private readonly BLLComponente_GO44 _bll;
        private List<BE_Componente_GO44> _todos;

        public FRMGestionProductos_GO44()
        {
            InitializeComponent();
            _bll = new BLLComponente_GO44();
        }

        private void FRMGestionProductos_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            CargarGrilla();
            CargarCombosCategoria();
        }

        private void ConfigurarGrilla()
        {
            dgvComponentes.ReadOnly = true;
            dgvComponentes.AllowUserToAddRows = false;
            dgvComponentes.AllowUserToDeleteRows = false;
            dgvComponentes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvComponentes.MultiSelect = false;
            dgvComponentes.RowHeadersVisible = false;
            dgvComponentes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void CargarGrilla()
        {
            _todos = _bll.ListarActivos();
            RefrescarGrilla(_todos);
        }

        private void RefrescarGrilla(List<BE_Componente_GO44> lista)
        {
            dgvComponentes.DataSource = null;
            dgvComponentes.DataSource = lista;

            if (dgvComponentes.Columns.Contains("Descripcion")) dgvComponentes.Columns["Descripcion"].Visible = false;
            if (dgvComponentes.Columns.Contains("Activo"))      dgvComponentes.Columns["Activo"].Visible = false;

            foreach (DataGridViewRow row in dgvComponentes.Rows)
            {
                BE_Componente_GO44 c = row.DataBoundItem as BE_Componente_GO44;
                if (c != null && c.StockActual <= c.StockMinimo)
                    row.DefaultCellStyle.BackColor = Color.LightYellow;   // resaltar stock bajo
                if (c != null && c.StockActual == 0)
                    row.DefaultCellStyle.BackColor = Color.LightCoral;    // sin stock
            }

            lblTotal.Text = "Total componentes: " + (lista != null ? lista.Count : 0);
        }

        private void CargarCombosCategoria()
        {
            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add("(todas)");
            if (_todos != null)
            {
                var categorias = _todos.Where(c => !string.IsNullOrEmpty(c.Categoria))
                                       .Select(c => c.Categoria).Distinct().OrderBy(s => s);
                foreach (var cat in categorias) cmbCategoria.Items.Add(cat);
            }
            cmbCategoria.SelectedIndex = 0;
        }

        private void FiltrarLista()
        {
            if (_todos == null) return;
            IEnumerable<BE_Componente_GO44> q = _todos;

            string codigo = txtBuscarCodigo.Text.Trim();
            if (!string.IsNullOrEmpty(codigo))
                q = q.Where(c => c.Codigo != null && c.Codigo.ToUpper().Contains(codigo.ToUpper()));

            string cat = cmbCategoria.SelectedItem as string;
            if (!string.IsNullOrEmpty(cat) && cat != "(todas)")
                q = q.Where(c => c.Categoria == cat);

            RefrescarGrilla(q.ToList());
        }

        private void txtBuscarCodigo_TextChanged(object sender, EventArgs e) { FiltrarLista(); }
        private void cmbCategoria_SelectedIndexChanged(object sender, EventArgs e) { FiltrarLista(); }

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            txtBuscarCodigo.Text = "";
            CargarGrilla();
            CargarCombosCategoria();
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
