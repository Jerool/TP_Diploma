using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// CU08 "Generar Orden de Compra".
    /// La OC se arma a partir de una Solicitud de Cotización (CU06):
    ///   - El encargado elige una SC del combo (estados Pendiente/Cotizada).
    ///   - Se cargan sus líneas en la grilla; el encargado completa el PRECIO UNITARIO cotizado.
    ///   - Elige la fecha de entrega y confirma.
    /// El total se recalcula a medida que se cargan los precios.
    /// </summary>
    public partial class FRMGenerarOrdenCompra_GO44 : Form
    {
        private readonly BLLSolicitudCotizacion_GO44 _bllSC;
        private readonly BLLOrdenCompra_GO44 _bllOC;

        private BE_OrdenCompra_GO44 _ordenEnCurso;
        private readonly BindingSource _bsLineas = new BindingSource();

        public FRMGenerarOrdenCompra_GO44()
        {
            InitializeComponent();
            _bllSC = new BLLSolicitudCotizacion_GO44();
            _bllOC = new BLLOrdenCompra_GO44();
        }

        private void FRMGenerarOrdenCompra_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            dtpEntrega.Value = DateTime.Now.AddDays(7);
            dtpEntrega.MinDate = DateTime.Now.Date;
            CargarSolicitudes();
            NuevaOrden();
        }

        private void ConfigurarGrilla()
        {
            dgvLineas.AllowUserToAddRows = false;
            dgvLineas.AllowUserToDeleteRows = false;
            dgvLineas.AllowUserToResizeRows = false;
            dgvLineas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLineas.MultiSelect = false;
            dgvLineas.RowHeadersVisible = false;
            dgvLineas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLineas.BackgroundColor = Color.White;
            dgvLineas.AutoGenerateColumns = true;
            dgvLineas.DataSource = _bsLineas;
            dgvLineas.CellEndEdit += dgvLineas_CellEndEdit;
        }

        private void NuevaOrden()
        {
            _ordenEnCurso = new BE_OrdenCompra_GO44();
            RefrescarGrilla();
            lblProveedor.Text = "Proveedor: —";
            lblSCInfo.Text = "SC de origen: —";
            lblTotal.Text = "Total: $0.00";
        }

        private void CargarSolicitudes()
        {
            // Solo SCs que todavía no se convirtieron (Pendiente/Cotizada)
            var todas = _bllSC.Listar();
            var disponibles = new List<BE_SolicitudCotizacion_GO44>();
            if (todas != null)
            {
                foreach (var sc in todas)
                    if (sc.Estado != BE_SolicitudCotizacion_GO44.EstadoSolicitud.Cancelada)
                        disponibles.Add(sc);
            }
            cmbSolicitud.DataSource = null;
            cmbSolicitud.DataSource = disponibles;
            cmbSolicitud.DisplayMember = "NumeroSC";
            cmbSolicitud.ValueMember = "Id";

            lblAvisoSinSC.Visible = (disponibles.Count == 0);
            btnCargarSC.Enabled = (disponibles.Count > 0);
        }

        private void btnCargarSC_Click(object sender, EventArgs e)
        {
            var scSel = cmbSolicitud.SelectedItem as BE_SolicitudCotizacion_GO44;
            if (scSel == null)
            {
                MessageBox.Show("Seleccione una solicitud de cotización", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Traer la SC completa (con líneas y proveedor)
            var sc = _bllSC.BuscarPorId(scSel.Id);
            if (sc == null)
            {
                MessageBox.Show("No se pudo cargar la solicitud", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _ordenEnCurso = new BE_OrdenCompra_GO44
            {
                IdSolicitud = sc.Id,
                NumeroSC = sc.NumeroSC,
                Proveedor = sc.Proveedor
            };
            foreach (var l in sc.Lineas)
                _ordenEnCurso.Lineas.Add(new BE_LineaOrdenCompra_GO44(l.Componente, l.Cantidad, 0m));

            lblProveedor.Text = "Proveedor: " + (sc.Proveedor != null ? sc.Proveedor.Empresa + " (" + sc.Proveedor.CUIT + ")" : "—");
            lblSCInfo.Text = "SC de origen: " + sc.NumeroSC;
            RefrescarGrilla();
            RecalcularTotal();
        }

        private void RefrescarGrilla()
        {
            _bsLineas.DataSource = null;
            _bsLineas.DataSource = new List<BE_LineaOrdenCompra_GO44>(_ordenEnCurso.Lineas);
            _bsLineas.ResetBindings(false);

            if (dgvLineas.Columns.Count > 0)
            {
                if (dgvLineas.Columns.Contains("Id"))                  dgvLineas.Columns["Id"].Visible = false;
                if (dgvLineas.Columns.Contains("IdOrdenCompra"))       dgvLineas.Columns["IdOrdenCompra"].Visible = false;
                if (dgvLineas.Columns.Contains("Componente"))          dgvLineas.Columns["Componente"].Visible = false;
                if (dgvLineas.Columns.Contains("ComponenteCategoria")) dgvLineas.Columns["ComponenteCategoria"].Visible = false;
                if (dgvLineas.Columns.Contains("ComponenteCodigo"))    { dgvLineas.Columns["ComponenteCodigo"].HeaderText = "Código"; dgvLineas.Columns["ComponenteCodigo"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("ComponenteNombre"))    { dgvLineas.Columns["ComponenteNombre"].HeaderText = "Componente"; dgvLineas.Columns["ComponenteNombre"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("Cantidad"))            { dgvLineas.Columns["Cantidad"].HeaderText = "Cantidad"; dgvLineas.Columns["Cantidad"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("Subtotal"))            { dgvLineas.Columns["Subtotal"].HeaderText = "Subtotal"; dgvLineas.Columns["Subtotal"].ReadOnly = true; dgvLineas.Columns["Subtotal"].DefaultCellStyle.Format = "N2"; }
                if (dgvLineas.Columns.Contains("PrecioUnitario"))
                {
                    dgvLineas.Columns["PrecioUnitario"].HeaderText = "Precio unit.";
                    dgvLineas.Columns["PrecioUnitario"].ReadOnly = false;
                    dgvLineas.Columns["PrecioUnitario"].DefaultCellStyle.Format = "N2";
                    dgvLineas.Columns["PrecioUnitario"].DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }
        }

        private void dgvLineas_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Volcar el precio editado al objeto en curso (la grilla está bindeada a una copia)
            var fila = dgvLineas.Rows[e.RowIndex].DataBoundItem as BE_LineaOrdenCompra_GO44;
            if (fila != null && fila.Componente != null)
            {
                foreach (var l in _ordenEnCurso.Lineas)
                    if (l.Componente != null && l.Componente.Id == fila.Componente.Id)
                        l.PrecioUnitario = fila.PrecioUnitario;
            }
            dgvLineas.InvalidateRow(e.RowIndex); // refresca el subtotal mostrado
            RecalcularTotal();
        }

        private void RecalcularTotal()
        {
            decimal total = _ordenEnCurso.RecalcularTotal();
            lblTotal.Text = "Total: $" + total.ToString("N2");
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            if (_ordenEnCurso.EstaVacia())
            {
                MessageBox.Show("Primero cargue una solicitud de cotización", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _ordenEnCurso.FechaEntrega = dtpEntrega.Value.Date;

            string resumen = ConstruirPreview();
            var r = MessageBox.Show(resumen + "\n\n¿Confirmar y generar la orden de compra?",
                "Preview — Orden de Compra", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            var vo = _bllOC.GenerarOrden(_ordenEnCurso);
            if (vo.Resultado == BLLOrdenCompra_GO44.ResultadoGeneracion.Exitoso)
            {
                MessageBox.Show(vo.Mensaje, "Orden generada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NuevaOrden();
                CargarSolicitudes();
            }
            else
            {
                MessageBox.Show(vo.Mensaje, "No se pudo generar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ConstruirPreview()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("── ORDEN DE COMPRA ──");
            sb.AppendLine("SC de origen: " + _ordenEnCurso.NumeroSC);
            if (_ordenEnCurso.Proveedor != null)
            {
                sb.AppendLine("Proveedor: " + _ordenEnCurso.Proveedor.Empresa);
                sb.AppendLine("CUIT: " + _ordenEnCurso.Proveedor.CUIT);
            }
            sb.AppendLine("Fecha de entrega: " + dtpEntrega.Value.ToString("dd/MM/yyyy"));
            sb.AppendLine();
            sb.AppendLine("── COMPONENTES ──");
            foreach (var l in _ordenEnCurso.Lineas)
                sb.AppendLine("  · " + l.ComponenteCodigo + " - " + l.ComponenteNombre +
                              "  x" + l.Cantidad + " @ $" + l.PrecioUnitario.ToString("N2") +
                              " = $" + l.Subtotal.ToString("N2"));
            sb.AppendLine();
            sb.AppendLine("TOTAL: $" + _ordenEnCurso.RecalcularTotal().ToString("N2"));
            return sb.ToString();
        }

        private void btnCancelar_Click(object sender, EventArgs e) { NuevaOrden(); }
        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
