using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// "Componentes - C" — Bitácora de cambios de componentes (tabla Componentes_C).
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
            EstablecerVistaInicial();
        }

        /// <summary>
        /// Estado inicial de la pantalla (consigna "Control de cambios"): limpia los filtros de
        /// texto, deja las fechas tildadas mostrando el ÚLTIMO MES (desde hace un mes hasta hoy)
        /// y recarga la grilla. Es la vista a la que también vuelve el botón LIMPIAR.
        /// </summary>
        private void EstablecerVistaInicial()
        {
            txtCodigo.Clear();
            txtNombre.Clear();

            dtpFechaIni.ShowCheckBox = true;
            dtpFechaIni.Value = DateTime.Today.AddMonths(-1);
            dtpFechaIni.Checked = true;

            dtpFechaFin.ShowCheckBox = true;
            dtpFechaFin.Value = DateTime.Today;
            dtpFechaFin.Checked = true;

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

            // Columnas de la bitácora (COMPONENTES_C): Cod_Comp · Fecha · Hora · Nombre · Exist. · Desc. · Act.
            AgregarColumna("Codigo",      "Cod_Comp", 12, DataGridViewContentAlignment.MiddleLeft);
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

            lblContador.Text = "Registros: " + _bs.Count;
        }

        private void btnBuscar_Click(object sender, EventArgs e) { Buscar(); }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            // LIMPIAR = volver exactamente a la vista inicial (último mes).
            EstablecerVistaInicial();
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
                MessageBox.Show("Esa versión ya es la vigente del componente.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var r = MessageBox.Show(
                "¿Restaurar el componente '" + sel.Codigo + "' a esta versión?\r\n\r\n" +
                "Se sobreescriben nombre, precio, stock y estado del componente con los de esta fila, " +
                "y queda registrada una versión nueva en la bitácora.",
                "Confirmar restauración", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            if (_bll.MarcarActivo(sel.Id))
            {
                MessageBox.Show("Componente restaurado a la versión seleccionada.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Buscar();
            }
            else
            {
                MessageBox.Show("No se pudo restaurar el componente.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
