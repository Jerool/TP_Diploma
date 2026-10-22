using BE;
using BLL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// CU06 "Generar solicitud de cotización".
    /// Layout:
    ///   - Combo de proveedores + botón "Registrar proveedor" (dispara CU07 — «extend»)
    ///   - Grilla izquierda: componentes disponibles (con filtro por código/nombre/categoría)
    ///   - Grilla derecha: líneas de la SC en curso
    ///   - Botones: Agregar / Quitar / Finalizar / Cancelar
    /// Al apretar "Finalizar" muestra un preview antes de guardar.
    /// </summary>
    public partial class FRMGenerarSolicitudCotizacion_GO44 : Form
    {
        private readonly BLLComponente_GO44 _bllComponente;
        private readonly BLLProveedor_GO44  _bllProveedor;
        private readonly BLLSolicitudCotizacion_GO44 _bllSC;

        private List<BE_Componente_GO44> _todosComponentes;
        private BE_SolicitudCotizacion_GO44 _solicitudEnCurso;

        private readonly BindingSource _bsComponentes = new BindingSource();
        private readonly BindingSource _bsLineas      = new BindingSource();

        public FRMGenerarSolicitudCotizacion_GO44()
        {
            InitializeComponent();
            _bllComponente = new BLLComponente_GO44();
            _bllProveedor  = new BLLProveedor_GO44();
            _bllSC         = new BLLSolicitudCotizacion_GO44();
        }

        private void FRMGenerarSolicitudCotizacion_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrillas();
            NuevaSolicitud();
            CargarComponentes();
            CargarProveedores();

            // Cantidad: solo dígitos
            txtCantidad.MaxLength = 3;
            txtCantidad.KeyPress += (s, ev) =>
            {
                if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true;
            };
        }

        private void NuevaSolicitud()
        {
            _solicitudEnCurso = new BE_SolicitudCotizacion_GO44();
            RefrescarGrillaLineas();
        }

        private void ConfigurarGrillas()
        {
            foreach (var g in new[] { dgvComponentes, dgvLineas })
            {
                g.ReadOnly = true;
                g.AllowUserToAddRows = false;
                g.AllowUserToDeleteRows = false;
                g.AllowUserToResizeColumns = false;
                g.AllowUserToResizeRows = false;
                g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                g.MultiSelect = false;
                g.RowHeadersVisible = false;
                g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                g.BackgroundColor = Color.White;
                g.AutoGenerateColumns = true;
            }
            dgvComponentes.DataSource = _bsComponentes;
            dgvLineas.DataSource = _bsLineas;
        }

        // ===== Componentes (grilla izquierda) =====

        private void CargarComponentes()
        {
            _todosComponentes = _bllComponente.ListarActivos();
            AplicarFiltroComponentes();
        }

        private void AplicarFiltroComponentes()
        {
            if (_todosComponentes == null) return;
            IEnumerable<BE_Componente_GO44> q = _todosComponentes;
            string filtro = (txtBuscar.Text ?? "").Trim().ToUpper();
            if (!string.IsNullOrEmpty(filtro))
            {
                q = q.Where(c =>
                    (c.Codigo    != null && c.Codigo.ToUpper().Contains(filtro)) ||
                    (c.Nombre    != null && c.Nombre.ToUpper().Contains(filtro)) ||
                    (c.Categoria != null && c.Categoria.ToUpper().Contains(filtro)));
            }
            if (chkBajoStock.Checked)
                q = q.Where(c => c.StockActual <= c.StockMinimo);

            var lista = q.ToList();
            _bsComponentes.DataSource = lista;
            _bsComponentes.ResetBindings(false);

            // Ocultar columnas que no interesan
            if (dgvComponentes.Columns.Count > 0)
            {
                if (dgvComponentes.Columns.Contains("Id"))          dgvComponentes.Columns["Id"].Visible = false;
                if (dgvComponentes.Columns.Contains("Descripcion")) dgvComponentes.Columns["Descripcion"].Visible = false;
                if (dgvComponentes.Columns.Contains("Precio"))      dgvComponentes.Columns["Precio"].Visible = false;
                if (dgvComponentes.Columns.Contains("Activo"))      dgvComponentes.Columns["Activo"].Visible = false;
                if (dgvComponentes.Columns.Contains("Codigo"))      dgvComponentes.Columns["Codigo"].HeaderText = "Código";
                if (dgvComponentes.Columns.Contains("StockActual")) dgvComponentes.Columns["StockActual"].HeaderText = "Stock";
                if (dgvComponentes.Columns.Contains("StockMinimo")) dgvComponentes.Columns["StockMinimo"].HeaderText = "Stock mín.";
            }

            foreach (DataGridViewRow row in dgvComponentes.Rows)
            {
                var c = row.DataBoundItem as BE_Componente_GO44;
                if (c == null) continue;
                if (c.StockActual <= c.StockMinimo) row.DefaultCellStyle.BackColor = Color.LightYellow;
                if (c.StockActual == 0) row.DefaultCellStyle.BackColor = Color.LightCoral;
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)  { AplicarFiltroComponentes(); }
        private void chkBajoStock_CheckedChanged(object sender, EventArgs e) { AplicarFiltroComponentes(); }

        // ===== Proveedores (combo + extend CU07) =====

        private void CargarProveedores()
        {
            var proveedores = _bllProveedor.ListarActivos();
            // Reset del binding: con lista vacía SelectedItem queda null y el combo se ve vacío.
            // NO llamar cmbProveedor.Items.Clear() porque tira "Items cannot be modified when DataSource is set".
            cmbProveedor.DataSource = null;
            cmbProveedor.DataSource = proveedores;
            cmbProveedor.DisplayMember = "Empresa";   // muestra la Empresa en el combo
            cmbProveedor.ValueMember   = "CUIT";

            lblAvisoSinProveedores.Visible = (proveedores == null || proveedores.Count == 0);
        }

        private void btnRegistrarProveedor_Click(object sender, EventArgs e)
        {
            // «extend» CU07
            using (var frm = new FRMRegistrarProveedor_GO44())
            {
                if (frm.ShowDialog() == DialogResult.OK && frm.ProveedorRegistrado != null)
                {
                    CargarProveedores();
                    // Seleccionar el proveedor recién registrado (usa ValueMember = "CUIT")
                    try { cmbProveedor.SelectedValue = frm.ProveedorRegistrado.CUIT; } catch { }
                }
            }
        }

        // ===== Agregar / Quitar línea =====

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (dgvComponentes.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un componente de la grilla", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var comp = dgvComponentes.CurrentRow.DataBoundItem as BE_Componente_GO44;
            if (comp == null) return;

            int cantidad;
            if (!int.TryParse(txtCantidad.Text.Trim(), out cantidad) || cantidad < 1 || cantidad > 999)
            {
                MessageBox.Show("La cantidad debe estar entre 1 y 999", "Cantidad inválida",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidad.Focus();
                return;
            }

            _solicitudEnCurso.AgregarLinea(comp, cantidad);
            RefrescarGrillaLineas();
            txtCantidad.Text = "1";
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            if (dgvLineas.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una línea de la cotización", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var linea = dgvLineas.CurrentRow.DataBoundItem as BE_LineaSolicitudCotizacion_GO44;
            if (linea == null || linea.Componente == null) return;

            _solicitudEnCurso.QuitarLinea(linea.Componente.Id);
            RefrescarGrillaLineas();
        }

        private void RefrescarGrillaLineas()
        {
            _bsLineas.DataSource = null;
            _bsLineas.DataSource = new List<BE_LineaSolicitudCotizacion_GO44>(_solicitudEnCurso.Lineas);
            _bsLineas.ResetBindings(false);

            if (dgvLineas.Columns.Count > 0)
            {
                if (dgvLineas.Columns.Contains("Id"))                 dgvLineas.Columns["Id"].Visible = false;
                if (dgvLineas.Columns.Contains("IdSolicitud"))        dgvLineas.Columns["IdSolicitud"].Visible = false;
                if (dgvLineas.Columns.Contains("Componente"))         dgvLineas.Columns["Componente"].Visible = false;
                if (dgvLineas.Columns.Contains("ComponenteCodigo"))   dgvLineas.Columns["ComponenteCodigo"].HeaderText = "Código";
                if (dgvLineas.Columns.Contains("ComponenteNombre"))   dgvLineas.Columns["ComponenteNombre"].HeaderText = "Componente";
                if (dgvLineas.Columns.Contains("ComponenteCategoria"))dgvLineas.Columns["ComponenteCategoria"].HeaderText = "Categoría";
            }

            lblContadorLineas.Text = $"Items: {_solicitudEnCurso.Lineas.Count}  ·  Unidades totales: {_solicitudEnCurso.CantidadItems}";
        }

        // ===== Finalizar (preview + confirmar) =====

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            if (_solicitudEnCurso.EstaVacia())
            {
                MessageBox.Show("Debe agregar al menos un componente a la cotización", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var prov = cmbProveedor.SelectedItem as BE_Proveedor_GO44;
            if (prov == null)
            {
                MessageBox.Show("Debe seleccionar un proveedor", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _solicitudEnCurso.Proveedor = prov;

            // Preview — texto resumen
            string resumen = ConstruirPreview();
            var r = MessageBox.Show(resumen + "\n\n¿Confirmar y guardar la solicitud?",
                "Preview — Solicitud de Cotización",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            // Guardar
            var vo = _bllSC.GenerarSolicitud(_solicitudEnCurso);
            if (vo.Resultado == BLLSolicitudCotizacion_GO44.ResultadoGeneracion.Exitoso)
            {
                MessageBox.Show(vo.Mensaje, "Solicitud generada",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                NuevaSolicitud();
                CargarComponentes(); // refresca stocks por si cambiaron
            }
            else
            {
                MessageBox.Show(vo.Mensaje, "No se pudo generar",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ConstruirPreview()
        {
            var prov = _solicitudEnCurso.Proveedor;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("── SOLICITUD DE COTIZACIÓN ──");
            sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Proveedor: {prov.Empresa}");
            sb.AppendLine($"CUIT: {prov.CUIT}");
            sb.AppendLine();
            sb.AppendLine("── COMPONENTES ──");
            foreach (var l in _solicitudEnCurso.Lineas)
                sb.AppendLine($"  · {l.ComponenteCodigo} - {l.ComponenteNombre}  ×{l.Cantidad}");
            sb.AppendLine();
            sb.AppendLine($"Total items: {_solicitudEnCurso.Lineas.Count}");
            sb.AppendLine($"Unidades totales: {_solicitudEnCurso.CantidadItems}");
            return sb.ToString();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (!_solicitudEnCurso.EstaVacia())
            {
                var r = MessageBox.Show("¿Cancelar la solicitud actual? Se perderán las líneas cargadas.",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
            }
            NuevaSolicitud();
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
