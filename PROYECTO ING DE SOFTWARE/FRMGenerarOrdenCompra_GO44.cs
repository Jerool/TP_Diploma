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
    /// CU08 "Generar Orden de Compra".
    /// La OC se arma a partir de una Solicitud de Cotización (CU06):
    ///   - El encargado elige una SC del combo (estados Pendiente/Cotizada).
    ///   - Se cargan sus líneas en la grilla; el encargado completa el PRECIO UNITARIO cotizado.
    ///   - Elige la fecha de entrega y confirma.
    /// El total se recalcula a medida que se cargan los precios.
    /// </summary>
    public partial class FRMGenerarOrdenCompra_GO44 : Form, IObservadorIdioma_GO44
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

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        private void FRMGenerarOrdenCompra_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            dtpEntrega.Value = DateTime.Now.AddDays(7);
            dtpEntrega.MinDate = DateTime.Now.Date;
            CargarSolicitudes();
            NuevaOrden();
            ActualizarIdioma();
        }

        /// <summary>Aplica los textos del idioma activo a títulos, etiquetas, botones y encabezados de la grilla.</summary>
        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("oc.titulo");
            if (lblTitulo != null)    lblTitulo.Text    = IdiomaManager_GO44.T("oc.titulo");
            if (grpOrigen != null)    grpOrigen.Text    = IdiomaManager_GO44.T("oc.grpOrigen");
            if (lblSolicitud != null) lblSolicitud.Text = IdiomaManager_GO44.T("oc.lblSolicitud");
            if (btnCargarSC != null)  btnCargarSC.Text  = IdiomaManager_GO44.T("oc.btnCargarSC");
            if (lblAvisoSinSC != null) lblAvisoSinSC.Text = IdiomaManager_GO44.T("oc.avisoSinSC");
            if (lblEntrega != null)   lblEntrega.Text   = IdiomaManager_GO44.T("oc.lblEntrega");
            if (grpLineas != null)    grpLineas.Text    = IdiomaManager_GO44.T("oc.grpLineas");
            if (btnGenerar != null)   btnGenerar.Text   = IdiomaManager_GO44.T("oc.btnGenerar");
            if (btnPagar != null)     btnPagar.Text     = IdiomaManager_GO44.T("oc.btnPagar");
            if (btnCancelar != null)  btnCancelar.Text  = IdiomaManager_GO44.T("oc.btnCancelar");
            if (btnSalir != null)     btnSalir.Text     = IdiomaManager_GO44.T("oc.btnSalir");
            ActualizarEncabezadosGrilla();
        }

        /// <summary>Traduce los encabezados de la grilla de líneas (se re-aplica al cambiar de idioma).</summary>
        private void ActualizarEncabezadosGrilla()
        {
            if (dgvLineas.Columns.Count == 0) return;
            if (dgvLineas.Columns.Contains("ComponenteCodigo")) dgvLineas.Columns["ComponenteCodigo"].HeaderText = IdiomaManager_GO44.T("oc.colCodigo");
            if (dgvLineas.Columns.Contains("ComponenteNombre")) dgvLineas.Columns["ComponenteNombre"].HeaderText = IdiomaManager_GO44.T("oc.colComponente");
            if (dgvLineas.Columns.Contains("Cantidad"))         dgvLineas.Columns["Cantidad"].HeaderText         = IdiomaManager_GO44.T("oc.colCantidad");
            if (dgvLineas.Columns.Contains("Subtotal"))         dgvLineas.Columns["Subtotal"].HeaderText         = IdiomaManager_GO44.T("oc.colSubtotal");
            if (dgvLineas.Columns.Contains("PrecioUnitario"))   dgvLineas.Columns["PrecioUnitario"].HeaderText   = IdiomaManager_GO44.T("oc.colPrecioUnit");
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
            lblProveedor.Text = IdiomaManager_GO44.T("oc.proveedorVacio");
            lblSCInfo.Text = IdiomaManager_GO44.T("oc.scOrigenVacio");
            lblTotal.Text = IdiomaManager_GO44.T("oc.totalCero");
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
                MessageBox.Show(IdiomaManager_GO44.T("oc.seleccioneSC"), IdiomaManager_GO44.T("general.advertencia"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Traer la SC completa (con líneas y proveedor)
            var sc = _bllSC.BuscarPorId(scSel.Id);
            if (sc == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("oc.noCargoSC"), IdiomaManager_GO44.T("general.error"),
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
                _ordenEnCurso.Lineas.Add(new BE_LineaOrdenCompra_GO44
                {
                    Componente = l.Componente,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = 0m
                });

            lblProveedor.Text = string.Format(IdiomaManager_GO44.T("oc.proveedorInfo"),
                sc.Proveedor != null ? sc.Proveedor.Empresa + " (" + sc.Proveedor.CUIT + ")" : "—");
            lblSCInfo.Text = string.Format(IdiomaManager_GO44.T("oc.scOrigen"), sc.NumeroSC);
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
                if (dgvLineas.Columns.Contains("ComponenteCodigo"))    { dgvLineas.Columns["ComponenteCodigo"].HeaderText = IdiomaManager_GO44.T("oc.colCodigo"); dgvLineas.Columns["ComponenteCodigo"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("ComponenteNombre"))    { dgvLineas.Columns["ComponenteNombre"].HeaderText = IdiomaManager_GO44.T("oc.colComponente"); dgvLineas.Columns["ComponenteNombre"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("Cantidad"))            { dgvLineas.Columns["Cantidad"].HeaderText = IdiomaManager_GO44.T("oc.colCantidad"); dgvLineas.Columns["Cantidad"].ReadOnly = true; }
                if (dgvLineas.Columns.Contains("Subtotal"))            { dgvLineas.Columns["Subtotal"].HeaderText = IdiomaManager_GO44.T("oc.colSubtotal"); dgvLineas.Columns["Subtotal"].ReadOnly = true; dgvLineas.Columns["Subtotal"].DefaultCellStyle.Format = "N2"; }
                if (dgvLineas.Columns.Contains("PrecioUnitario"))
                {
                    dgvLineas.Columns["PrecioUnitario"].HeaderText = IdiomaManager_GO44.T("oc.colPrecioUnit");
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
            decimal total = _bllOC.RecalcularTotal(_ordenEnCurso);
            lblTotal.Text = string.Format(IdiomaManager_GO44.T("oc.total"), total.ToString("N2"));
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            if (_bllOC.EstaVacia(_ordenEnCurso))
            {
                MessageBox.Show(IdiomaManager_GO44.T("oc.primeroCargueSC"), IdiomaManager_GO44.T("general.advertencia"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _ordenEnCurso.FechaEntrega = dtpEntrega.Value.Date;

            string resumen = ConstruirPreview();
            var r = MessageBox.Show(resumen + "\n\n" + IdiomaManager_GO44.T("oc.confirmarGenerar"),
                IdiomaManager_GO44.T("oc.previewTitulo"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            var vo = _bllOC.GenerarOrden(_ordenEnCurso);
            if (vo.Resultado == BLLOrdenCompra_GO44.ResultadoGeneracion.Exitoso)
            {
                MessageBox.Show(vo.Mensaje, IdiomaManager_GO44.T("oc.ordenGenerada"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                NuevaOrden();
                CargarSolicitudes();
            }
            else
            {
                MessageBox.Show(vo.Mensaje, IdiomaManager_GO44.T("oc.noSeGenero"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ConstruirPreview()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(IdiomaManager_GO44.T("oc.pvEncabezado"));
            sb.AppendLine(string.Format(IdiomaManager_GO44.T("oc.scOrigen"), _ordenEnCurso.NumeroSC));
            if (_ordenEnCurso.Proveedor != null)
            {
                sb.AppendLine(string.Format(IdiomaManager_GO44.T("oc.pvProveedor"), _ordenEnCurso.Proveedor.Empresa));
                sb.AppendLine(string.Format(IdiomaManager_GO44.T("oc.pvCuit"), _ordenEnCurso.Proveedor.CUIT));
            }
            sb.AppendLine(string.Format(IdiomaManager_GO44.T("oc.pvEntrega"), dtpEntrega.Value.ToString("dd/MM/yyyy")));
            sb.AppendLine();
            sb.AppendLine(IdiomaManager_GO44.T("oc.pvComponentes"));
            foreach (var l in _ordenEnCurso.Lineas)
                sb.AppendLine("  · " + l.ComponenteCodigo + " - " + l.ComponenteNombre +
                              "  x" + l.Cantidad + " @ $" + l.PrecioUnitario.ToString("N2") +
                              " = $" + l.Subtotal.ToString("N2"));
            sb.AppendLine();
            sb.AppendLine(string.Format(IdiomaManager_GO44.T("oc.pvTotal"), _bllOC.RecalcularTotal(_ordenEnCurso).ToString("N2")));
            return sb.ToString();
        }

        private void btnPagar_Click(object sender, EventArgs e)
        {
            // «include» CU09 — abre la pantalla de Pagar Orden de Compra (modal).
            using (var frm = new FRMPagarOrdenCompra_GO44())
                frm.ShowDialog(this);
        }

        private void btnCancelar_Click(object sender, EventArgs e) { NuevaOrden(); }
        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
