using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// CU10 "Recepción de mercadería". Actor: Almacenista.
    /// Lista las OCs en estado "Pagada". Al seleccionar una se cargan sus líneas con la cantidad
    /// pedida y una columna editable "Cantidad recibida" (precargada con la pedida). Al confirmar,
    /// el sistema suma al stock lo recibido y, si hay faltantes, marca la OC "Recibida con faltantes".
    /// </summary>
    public partial class FRMRecepcionMercaderia_GO44 : Form
    {
        private readonly BLLOrdenCompra_GO44 _bllOC;
        private readonly BLLRecepcionMercaderia_GO44 _bllRecepcion;

        private BE_OrdenCompra_GO44 _ordenSeleccionada;
        private BE_RecepcionMercaderia_GO44 _recepcionEnCurso;
        private readonly BindingSource _bsOrdenes = new BindingSource();
        private readonly BindingSource _bsLineas  = new BindingSource();

        public FRMRecepcionMercaderia_GO44()
        {
            InitializeComponent();
            _bllOC        = new BLLOrdenCompra_GO44();
            _bllRecepcion = new BLLRecepcionMercaderia_GO44();
        }

        private void FRMRecepcionMercaderia_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrillas();
            CargarOrdenes();
        }

        private void ConfigurarGrillas()
        {
            dgvOrdenes.ReadOnly = true;
            dgvOrdenes.AllowUserToAddRows = false;
            dgvOrdenes.AllowUserToDeleteRows = false;
            dgvOrdenes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrdenes.MultiSelect = false;
            dgvOrdenes.RowHeadersVisible = false;
            dgvOrdenes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrdenes.BackgroundColor = Color.White;
            dgvOrdenes.AutoGenerateColumns = true;
            dgvOrdenes.DataSource = _bsOrdenes;
            dgvOrdenes.SelectionChanged += dgvOrdenes_SelectionChanged;

            dgvLineas.AllowUserToAddRows = false;
            dgvLineas.AllowUserToDeleteRows = false;
            dgvLineas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLineas.MultiSelect = false;
            dgvLineas.RowHeadersVisible = false;
            dgvLineas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLineas.BackgroundColor = Color.White;
            dgvLineas.AutoGenerateColumns = true;
            dgvLineas.DataSource = _bsLineas;
            dgvLineas.CellEndEdit += dgvLineas_CellEndEdit;
        }

        private void CargarOrdenes()
        {
            var ordenes = _bllOC.ListarPorEstado(BE_OrdenCompra_GO44.EstadoOrden.Pagada);
            _bsOrdenes.DataSource = null;
            _bsOrdenes.DataSource = ordenes ?? new List<BE_OrdenCompra_GO44>();
            _bsOrdenes.ResetBindings(false);

            if (dgvOrdenes.Columns.Count > 0)
            {
                foreach (var col in new[] { "IdSolicitud", "LoginEncargado", "Proveedor", "Lineas", "FechaEmision", "CantidadItems" })
                    if (dgvOrdenes.Columns.Contains(col)) dgvOrdenes.Columns[col].Visible = false;
                if (dgvOrdenes.Columns.Contains("Id"))           dgvOrdenes.Columns["Id"].Visible = false;
                if (dgvOrdenes.Columns.Contains("NumeroOC"))     dgvOrdenes.Columns["NumeroOC"].HeaderText = "N° OC";
                if (dgvOrdenes.Columns.Contains("NumeroSC"))     dgvOrdenes.Columns["NumeroSC"].HeaderText = "N° SC";
                if (dgvOrdenes.Columns.Contains("FechaEntrega")) { dgvOrdenes.Columns["FechaEntrega"].HeaderText = "Entrega"; dgvOrdenes.Columns["FechaEntrega"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
                if (dgvOrdenes.Columns.Contains("Total"))        { dgvOrdenes.Columns["Total"].HeaderText = "Total"; dgvOrdenes.Columns["Total"].DefaultCellStyle.Format = "N2"; }
                if (dgvOrdenes.Columns.Contains("Estado"))       dgvOrdenes.Columns["Estado"].HeaderText = "Estado";
            }

            lblAvisoSinOrdenes.Visible = (ordenes == null || ordenes.Count == 0);

            // La grilla auto-selecciona la primera fila; cargamos su detalle explícitamente
            // (antes LimpiarDetalle borraba lo recién cargado por la auto-selección).
            if (ordenes != null && ordenes.Count > 0)
            {
                dgvOrdenes.ClearSelection();
                dgvOrdenes.Rows[0].Selected = true;
                MostrarDetalleOrden();
            }
            else
            {
                LimpiarDetalle();
            }
        }

        private void dgvOrdenes_SelectionChanged(object sender, EventArgs e)
        {
            MostrarDetalleOrden();
        }

        private void MostrarDetalleOrden()
        {
            DataGridViewRow fila = dgvOrdenes.CurrentRow;
            if (fila == null && dgvOrdenes.SelectedRows.Count > 0) fila = dgvOrdenes.SelectedRows[0];
            var ocRow = fila != null ? fila.DataBoundItem as BE_OrdenCompra_GO44 : null;
            if (ocRow == null) { LimpiarDetalle(); return; }

            _ordenSeleccionada = _bllOC.BuscarPorId(ocRow.Id);
            if (_ordenSeleccionada == null) { LimpiarDetalle(); return; }

            // Armar la recepción en curso: una línea por cada línea de la OC, recibida = pedida
            _recepcionEnCurso = new BE_RecepcionMercaderia_GO44 { IdOrdenCompra = _ordenSeleccionada.Id };
            foreach (var l in _ordenSeleccionada.Lineas)
                _recepcionEnCurso.Lineas.Add(new BE_LineaRecepcion_GO44(l.Componente, l.Cantidad, l.Cantidad));

            lblDetalleOC.Text = "OC " + _ordenSeleccionada.NumeroOC + "  ·  " +
                                (_ordenSeleccionada.Proveedor != null ? _ordenSeleccionada.Proveedor.Empresa : "—");
            RefrescarGrillaLineas();
            btnRecibir.Enabled = true;
        }

        private void RefrescarGrillaLineas()
        {
            _bsLineas.DataSource = null;
            _bsLineas.DataSource = _recepcionEnCurso != null
                ? new List<BE_LineaRecepcion_GO44>(_recepcionEnCurso.Lineas)
                : new List<BE_LineaRecepcion_GO44>();
            _bsLineas.ResetBindings(false);

            if (dgvLineas.Columns.Count > 0)
            {
                foreach (var col in new[] { "Id", "IdRecepcion", "Componente", "ComponenteCategoria" })
                    if (dgvLineas.Columns.Contains(col)) dgvLineas.Columns[col].Visible = false;
                if (dgvLineas.Columns.Contains("ComponenteCodigo")) { dgvLineas.Columns["ComponenteCodigo"].HeaderText = "Código"; dgvLineas.Columns["ComponenteCodigo"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("ComponenteNombre")) { dgvLineas.Columns["ComponenteNombre"].HeaderText = "Componente"; dgvLineas.Columns["ComponenteNombre"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("CantidadPedida"))   { dgvLineas.Columns["CantidadPedida"].HeaderText = "Pedida"; dgvLineas.Columns["CantidadPedida"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("Discrepancia"))     { dgvLineas.Columns["Discrepancia"].HeaderText = "Faltante"; dgvLineas.Columns["Discrepancia"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("CantidadRecibida"))
                {
                    dgvLineas.Columns["CantidadRecibida"].HeaderText = "Recibida";
                    dgvLineas.Columns["CantidadRecibida"].ReadOnly = false;
                    dgvLineas.Columns["CantidadRecibida"].DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }
            PintarDiscrepancias();
        }

        private void dgvLineas_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var fila = dgvLineas.Rows[e.RowIndex].DataBoundItem as BE_LineaRecepcion_GO44;
            if (fila != null && fila.Componente != null)
            {
                // Clamp 0..pedida y volcar al objeto en curso
                if (fila.CantidadRecibida < 0) fila.CantidadRecibida = 0;
                if (fila.CantidadRecibida > fila.CantidadPedida) fila.CantidadRecibida = fila.CantidadPedida;
                foreach (var l in _recepcionEnCurso.Lineas)
                    if (l.Componente != null && l.Componente.Id == fila.Componente.Id)
                        l.CantidadRecibida = fila.CantidadRecibida;
            }
            dgvLineas.InvalidateRow(e.RowIndex);
            PintarDiscrepancias();
        }

        private void PintarDiscrepancias()
        {
            foreach (DataGridViewRow row in dgvLineas.Rows)
            {
                var l = row.DataBoundItem as BE_LineaRecepcion_GO44;
                if (l == null) continue;
                row.DefaultCellStyle.BackColor = l.CantidadRecibida < l.CantidadPedida ? Color.MistyRose : Color.White;
            }
        }

        private void LimpiarDetalle()
        {
            _ordenSeleccionada = null;
            _recepcionEnCurso = null;
            lblDetalleOC.Text = "Seleccione una orden de compra pagada";
            _bsLineas.DataSource = null;
            _bsLineas.ResetBindings(false);
            txtObservaciones.Text = "";
            btnRecibir.Enabled = false;
        }

        private void btnRecibir_Click(object sender, EventArgs e)
        {
            if (_ordenSeleccionada == null || _recepcionEnCurso == null)
            {
                MessageBox.Show("Seleccione una orden de compra", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _recepcionEnCurso.Observaciones = string.IsNullOrWhiteSpace(txtObservaciones.Text) ? null : txtObservaciones.Text.Trim();

            bool hayFaltantes = _recepcionEnCurso.TieneFaltantes();
            string msg = hayFaltantes
                ? "Hay componentes con faltantes. La orden quedará en estado 'Recibida con faltantes'.\n\n¿Confirmar la recepción?"
                : "¿Confirmar la recepción completa de la orden " + _ordenSeleccionada.NumeroOC + "?";
            var r = MessageBox.Show(msg, "Confirmar recepción", MessageBoxButtons.YesNo,
                hayFaltantes ? MessageBoxIcon.Warning : MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            var vo = _bllRecepcion.RecibirMercaderia(_ordenSeleccionada, _recepcionEnCurso);
            if (vo.Resultado == BLLRecepcionMercaderia_GO44.ResultadoRecepcion.Exitoso)
            {
                MessageBox.Show(vo.Mensaje, "Recepción registrada",
                    MessageBoxButtons.OK, vo.ConFaltantes ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                CargarOrdenes();
            }
            else
            {
                MessageBox.Show(vo.Mensaje, "No se pudo recibir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e) { CargarOrdenes(); }
        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
