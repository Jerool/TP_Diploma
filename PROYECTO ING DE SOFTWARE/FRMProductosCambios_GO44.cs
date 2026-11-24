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
    /// "Componentes - C" — Bitácora de cambios de componentes (tabla Componentes_C).
    /// Muestra el historial de versiones de cada componente (generado por triggers SCD2) con
    /// filtros por Código, Nombre y rango de fechas. Permite marcar manualmente la versión vigente.
    /// Ubicación: menú Maestros.
    /// </summary>
    public partial class FRMProductosCambios_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly BLLComponenteCambio_GO44 _bll;
        private readonly BindingSource _bs = new BindingSource();

        public FRMProductosCambios_GO44()
        {
            InitializeComponent();
            _bll = new BLLComponenteCambio_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        private void FRMProductosCambios_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            ActualizarIdioma();
            EstablecerVistaInicial();
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("cambios.titulo");
            if (lblTitulo != null)   lblTitulo.Text   = IdiomaManager_GO44.T("cambios.titulo");
            if (lblCodigo != null)   lblCodigo.Text   = IdiomaManager_GO44.T("cambios.lblCodigo");
            if (lblNombre != null)   lblNombre.Text   = IdiomaManager_GO44.T("cambios.lblNombre");
            if (lblFechaIni != null) lblFechaIni.Text = IdiomaManager_GO44.T("cambios.lblFechaIni");
            if (lblFechaFin != null) lblFechaFin.Text = IdiomaManager_GO44.T("cambios.lblFechaFin");
            if (btnBuscar != null)   btnBuscar.Text   = IdiomaManager_GO44.T("cambios.btnBuscar");
            if (btnSalir != null)    btnSalir.Text    = IdiomaManager_GO44.T("cambios.btnSalir");
            if (btnLimpiar != null)  btnLimpiar.Text  = IdiomaManager_GO44.T("cambios.btnLimpiar");
            if (btnMarcar != null)   btnMarcar.Text   = IdiomaManager_GO44.T("cambios.btnMarcar");
            ActualizarEncabezadosGrilla();
            if (lblContador != null) lblContador.Text = string.Format(IdiomaManager_GO44.T("cambios.registros"), _bs.Count);
        }

        /// <summary>Traduce los encabezados de las columnas de la grilla (se re-aplica al cambiar de idioma).</summary>
        private void ActualizarEncabezadosGrilla()
        {
            if (dgv.Columns.Count == 0) return;
            if (dgv.Columns.Contains("Codigo"))      dgv.Columns["Codigo"].HeaderText      = IdiomaManager_GO44.T("cambios.colCodigo");
            if (dgv.Columns.Contains("FechaCambio")) dgv.Columns["FechaCambio"].HeaderText = IdiomaManager_GO44.T("cambios.colFecha");
            if (dgv.Columns.Contains("HoraCambio"))  dgv.Columns["HoraCambio"].HeaderText  = IdiomaManager_GO44.T("cambios.colHora");
            if (dgv.Columns.Contains("Nombre"))      dgv.Columns["Nombre"].HeaderText      = IdiomaManager_GO44.T("cambios.colNombre");
            if (dgv.Columns.Contains("StockActual")) dgv.Columns["StockActual"].HeaderText = IdiomaManager_GO44.T("cambios.colStock");
            if (dgv.Columns.Contains("Descripcion")) dgv.Columns["Descripcion"].HeaderText = IdiomaManager_GO44.T("cambios.colDesc");
            if (dgv.Columns.Contains("ActInt"))      dgv.Columns["ActInt"].HeaderText      = IdiomaManager_GO44.T("cambios.colAct");
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
                Name = prop,                 // para poder re-traducir el encabezado por nombre
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

            lblContador.Text = string.Format(IdiomaManager_GO44.T("cambios.registros"), _bs.Count);
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
                MessageBox.Show(IdiomaManager_GO44.T("cambios.seleccioneVersion"), IdiomaManager_GO44.T("cambios.aviso"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (sel.Act)
            {
                MessageBox.Show(IdiomaManager_GO44.T("cambios.yaVigente"), IdiomaManager_GO44.T("cambios.aviso"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var r = MessageBox.Show(
                string.Format(IdiomaManager_GO44.T("cambios.confirmarRestaurar"), sel.Codigo),
                IdiomaManager_GO44.T("cambios.confirmarRestaurarTitulo"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            if (_bll.MarcarActivo(sel.Id))
            {
                MessageBox.Show(IdiomaManager_GO44.T("cambios.restauradoOk"), IdiomaManager_GO44.T("cambios.exito"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Buscar();
            }
            else
            {
                MessageBox.Show(IdiomaManager_GO44.T("cambios.noSeRestauro"), IdiomaManager_GO44.T("general.error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
