using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// "Productos - C" — Bitácora de cambios de productos (tabla Componentes_C).
    /// Muestra el historial de versiones de cada componente (generado por triggers SCD2) con
    /// filtros por Código, Nombre y rango de fechas. Permite marcar manualmente la versión vigente.
    /// Ubicación: menú Maestros.
    /// </summary>
    public partial class FRMProductosCambios_GO44 : Form
    {
        private readonly BLLComponenteCambio_GO44 _bll;
        private readonly BindingSource _bs = new BindingSource();

        public FRMProductosCambios_GO44()
        {
            InitializeComponent();
            _bll = new BLLComponenteCambio_GO44();
        }

        private void FRMProductosCambios_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            dtpFechaIni.ShowCheckBox = true; dtpFechaIni.Checked = false;
            dtpFechaFin.ShowCheckBox = true; dtpFechaFin.Checked = false;
            Buscar();
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

            // Columnas de la bitácora (PRODUCTOS_C): Cod_Prod · Fecha · Hora · Nombre · Exist. · Desc. · Act.
            AgregarColumna("Codigo",      "Cod_Prod", 12, DataGridViewContentAlignment.MiddleLeft);
            AgregarColumna("FechaCambio", "Fecha",    12, DataGridViewContentAlignment.MiddleCenter);
            AgregarColumna("HoraCambio",  "Hora",     10, DataGridViewContentAlignment.MiddleCenter);
            AgregarColumna("Nombre",      "Nombre",   20, DataGridViewContentAlignment.MiddleLeft);
            AgregarColumna("StockActual", "Exist.",   10, DataGridViewContentAlignment.MiddleCenter);
            AgregarColumna("Descripcion", "Desc.",    28, DataGridViewContentAlignment.MiddleLeft);
            AgregarColumna("ActInt",      "Act.",      8, DataGridViewContentAlignment.MiddleCenter);

            dgv.DataSource = _bs;
        }

        private void AgregarColumna(string prop, string header, int fillWeight, DataGridViewContentAlignment align)
        {
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = prop,
                HeaderText = header,
                FillWeight = fillWeight,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            col.DefaultCellStyle.Alignment = align;
            dgv.Columns.Add(col);
        }

        private void Buscar()
        {
            DateTime? fi = dtpFechaIni.Checked ? (DateTime?)dtpFechaIni.Value.Date : null;
            DateTime? ff = dtpFechaFin.Checked ? (DateTime?)dtpFechaFin.Value.Date.AddDays(1).AddSeconds(-1) : null;

            var lista = _bll.ListarConFiltros(txtCodigo.Text.Trim(), txtNombre.Text.Trim(), fi, ff);
            _bs.DataSource = null;
            _bs.DataSource = lista ?? new List<BE_ComponenteCambio_GO44>();
            _bs.ResetBindings(false);

            foreach (DataGridViewRow row in dgv.Rows)
            {
                var c = row.DataBoundItem as BE_ComponenteCambio_GO44;
                if (c == null) continue;
                if (c.Act) row.DefaultCellStyle.BackColor = Color.Honeydew;          // versión vigente (Act = 1)
                if (!c.ActivoProducto) row.DefaultCellStyle.BackColor = Color.MistyRose; // producto dado de baja
            }

            lblContador.Text = "Registros: " + _bs.Count;
        }

        private void btnBuscar_Click(object sender, EventArgs e) { Buscar(); }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtCodigo.Clear(); txtNombre.Clear();
            dtpFechaIni.Checked = false; dtpFechaFin.Checked = false;
            Buscar();
        }

        private void btnMarcar_Click(object sender, EventArgs e)
        {
            var sel = dgv.CurrentRow != null ? dgv.CurrentRow.DataBoundItem as BE_ComponenteCambio_GO44 : null;
            if (sel == null)
            {
                MessageBox.Show("Seleccione una versión del historial", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (sel.Act)
            {
                MessageBox.Show("Esa versión ya es la vigente.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var r = MessageBox.Show(
                "¿Marcar esta versión de '" + sel.Codigo + "' como la vigente (Act = 1)?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            if (_bll.MarcarActivo(sel.Id))
            {
                MessageBox.Show("Versión marcada como vigente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Buscar();
            }
            else
            {
                MessageBox.Show("No se pudo actualizar la versión.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
