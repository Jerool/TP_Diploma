using BE;
using BLL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// ABM completo de Productos (Componentes):
    ///   • Alta:       nuevo componente (código, nombre, categoría, precio, stock inicial y mínimo)
    ///   • Modificar:  solo PRECIO (el resto es inmutable por decisión de negocio)
    ///   • Baja:       lógica (Activo = 0); si estaba dado de baja se puede Reactivar
    ///   • Consulta:   grilla con filtros; radio para ver Solo activos / Todos
    /// </summary>
    public partial class FRMGestionProductos_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly BLLComponente_GO44 _bll;
        private List<BE_Componente_GO44> _todos;
        private BE_Componente_GO44 _seleccionado;
        private string _modo = "Consulta";

        public FRMGestionProductos_GO44()
        {
            InitializeComponent();
            _bll = new BLLComponente_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        private void FRMGestionProductos_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            ModoConsulta();
            CargarGrilla();
            CargarCombosCategoria();
            ActualizarIdioma();
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("productos.titulo");
            if (lblTitulo != null)         lblTitulo.Text         = IdiomaManager_GO44.T("productos.titulo");
            if (lblCodigo != null)         lblCodigo.Text         = IdiomaManager_GO44.T("productos.codigo");
            if (lblNombre != null)         lblNombre.Text         = IdiomaManager_GO44.T("productos.nombre");
            if (lblCategoria != null)      lblCategoria.Text      = IdiomaManager_GO44.T("productos.filtroCategoria");
            if (lblCategoria2 != null)     lblCategoria2.Text     = IdiomaManager_GO44.T("productos.categoria");
            if (lblDescripcion != null)    lblDescripcion.Text    = IdiomaManager_GO44.T("productos.descripcion");
            if (lblPrecio != null)         lblPrecio.Text         = IdiomaManager_GO44.T("productos.precio");
            if (lblStockActual != null)    lblStockActual.Text    = IdiomaManager_GO44.T("productos.stockActual");
            if (lblStockMinimo != null)    lblStockMinimo.Text    = IdiomaManager_GO44.T("productos.stockMinimo");
            if (lblBuscarCodigo != null)   lblBuscarCodigo.Text   = IdiomaManager_GO44.T("productos.buscar");
            if (btnNuevo != null)          btnNuevo.Text          = IdiomaManager_GO44.T("productos.btnNuevo");
            if (btnModificarPrecio != null)btnModificarPrecio.Text= IdiomaManager_GO44.T("productos.btnModificarPrecio");
            if (btnBajaAlta != null)       btnBajaAlta.Text       = IdiomaManager_GO44.T("productos.btnBajaAlta");
            if (btnAplicar != null)        btnAplicar.Text        = IdiomaManager_GO44.T("productos.btnAplicar");
            if (btnCancelar != null)       btnCancelar.Text       = IdiomaManager_GO44.T("productos.btnCancelar");
            if (btnSalir != null)          btnSalir.Text          = IdiomaManager_GO44.T("productos.btnSalir");
            if (rbSoloActivos != null)     rbSoloActivos.Text     = IdiomaManager_GO44.T("productos.soloActivos");
            if (rbTodos != null)           rbTodos.Text           = IdiomaManager_GO44.T("productos.todos");
            RefrescarLblMensaje();

            // Combo categoría: reemplazar el "(todas)"
            if (cmbCategoria != null && cmbCategoria.Items.Count > 0)
            {
                int idx = cmbCategoria.SelectedIndex;
                cmbCategoria.Items[0] = IdiomaManager_GO44.T("productos.filtroTodas");
                if (idx >= 0) cmbCategoria.SelectedIndex = idx;
            }

            // Refrescar total
            if (lblTotal != null)
                lblTotal.Text = string.Format(IdiomaManager_GO44.T("productos.total"), _todos != null ? _todos.Count : 0);
        }

        private void RefrescarLblMensaje()
        {
            if (lblMensaje == null) return;
            switch (_modo)
            {
                case "Consulta":         lblMensaje.Text = IdiomaManager_GO44.T("productos.modoConsulta"); break;
                case "Nuevo":            lblMensaje.Text = string.Format(IdiomaManager_GO44.T("productos.modo"), IdiomaManager_GO44.T("productos.modoNuevo")); break;
                case "Modificar precio": lblMensaje.Text = string.Format(IdiomaManager_GO44.T("productos.modo"), IdiomaManager_GO44.T("productos.modoModificarPrecio")); break;
                default:                 lblMensaje.Text = string.Format(IdiomaManager_GO44.T("productos.modo"), _modo); break;
            }
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
            dgvComponentes.AllowUserToResizeColumns = false;
            dgvComponentes.AllowUserToResizeRows    = false;
            dgvComponentes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvComponentes.RowHeadersWidthSizeMode    = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        }

        private void CargarGrilla()
        {
            _todos = rbSoloActivos.Checked ? _bll.ListarActivos() : _bll.ListarTodos();
            RefrescarGrilla(_todos);
        }

        private void RefrescarGrilla(List<BE_Componente_GO44> lista)
        {
            dgvComponentes.DataSource = null;
            dgvComponentes.DataSource = lista;

            if (dgvComponentes.Columns.Count > 0)
            {
                if (dgvComponentes.Columns.Contains("Descripcion")) dgvComponentes.Columns["Descripcion"].Visible = false;
                if (dgvComponentes.Columns.Contains("Id"))          dgvComponentes.Columns["Id"].Visible = false;
                if (dgvComponentes.Columns.Contains("Precio"))      dgvComponentes.Columns["Precio"].DefaultCellStyle.Format = "N2";
            }

            foreach (DataGridViewRow row in dgvComponentes.Rows)
            {
                BE_Componente_GO44 c = row.DataBoundItem as BE_Componente_GO44;
                if (c == null) continue;
                if (!c.Activo)
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                else if (c.StockActual == 0)
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                else if (c.StockActual <= c.StockMinimo)
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
            }

            lblTotal.Text = string.Format(IdiomaManager_GO44.T("productos.total"), lista != null ? lista.Count : 0);
        }

        private void CargarCombosCategoria()
        {
            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add(IdiomaManager_GO44.T("productos.filtroTodas"));
            if (_todos != null)
            {
                var cats = _todos.Where(c => !string.IsNullOrEmpty(c.Categoria))
                                 .Select(c => c.Categoria).Distinct().OrderBy(s => s);
                foreach (var cat in cats) cmbCategoria.Items.Add(cat);
            }
            cmbCategoria.SelectedIndex = 0;
        }

        private void FiltrarLista()
        {
            if (_todos == null) return;
            IEnumerable<BE_Componente_GO44> q = _todos;

            string codigo = (txtBuscarCodigo.Text ?? "").Trim().ToUpper();
            if (!string.IsNullOrEmpty(codigo))
                q = q.Where(c => (c.Codigo != null && c.Codigo.ToUpper().Contains(codigo)) ||
                                 (c.Nombre != null && c.Nombre.ToUpper().Contains(codigo)));

            string cat = cmbCategoria.SelectedIndex >= 0 ? cmbCategoria.SelectedItem as string : null;
            string todasEs = IdiomaManager_GO44.T("productos.filtroTodas");
            if (!string.IsNullOrEmpty(cat) && cat != todasEs && cat != "(todas)" && cat != "(all)")
                q = q.Where(c => c.Categoria == cat);

            RefrescarGrilla(q.ToList());
        }

        private void txtBuscarCodigo_TextChanged(object sender, EventArgs e) { FiltrarLista(); }
        private void cmbCategoria_SelectedIndexChanged(object sender, EventArgs e) { FiltrarLista(); }
        private void rbSoloActivos_CheckedChanged(object sender, EventArgs e)  { if (rbSoloActivos.Checked) CargarGrilla(); }
        private void rbTodos_CheckedChanged(object sender, EventArgs e)        { if (rbTodos.Checked)       CargarGrilla(); }

        // ============ Modos ============

        private void ModoConsulta()
        {
            _modo = "Consulta";
            RefrescarLblMensaje();
            LimpiarCampos();
            HabilitarCampos(false);
            btnNuevo.Enabled = true;
            btnModificarPrecio.Enabled = true;
            btnBajaAlta.Enabled = true;
            btnAplicar.Enabled = false;
            btnCancelar.Enabled = false;
            dgvComponentes.Enabled = true;
        }

        private void ModoOperacion(string modo)
        {
            _modo = modo;
            RefrescarLblMensaje();
            btnNuevo.Enabled = false;
            btnModificarPrecio.Enabled = false;
            btnBajaAlta.Enabled = false;
            btnAplicar.Enabled = true;
            btnCancelar.Enabled = true;
            dgvComponentes.Enabled = false;
        }

        private void HabilitarCampos(bool alta)
        {
            // En alta se editan TODOS los campos
            txtCodigo.Enabled      = alta;
            txtNombre.Enabled      = alta;
            txtCategoria.Enabled   = alta;
            txtDescripcion.Enabled = alta;
            txtPrecio.Enabled      = alta;
            txtStockActual.Enabled = alta;
            txtStockMinimo.Enabled = alta;
        }

        private void LimpiarCampos()
        {
            txtCodigo.Text = txtNombre.Text = txtCategoria.Text = txtDescripcion.Text = "";
            txtPrecio.Text = "0,00";
            txtStockActual.Text = "0";
            txtStockMinimo.Text = "0";
        }

        private void dgvComponentes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvComponentes.CurrentRow == null) return;
            _seleccionado = dgvComponentes.CurrentRow.DataBoundItem as BE_Componente_GO44;
            if (_seleccionado == null) return;

            txtCodigo.Text      = _seleccionado.Codigo;
            txtNombre.Text      = _seleccionado.Nombre;
            txtCategoria.Text   = _seleccionado.Categoria;
            txtDescripcion.Text = _seleccionado.Descripcion;
            txtPrecio.Text      = _seleccionado.Precio.ToString("N2");
            txtStockActual.Text = _seleccionado.StockActual.ToString();
            txtStockMinimo.Text = _seleccionado.StockMinimo.ToString();
        }

        // ============ Handlers de acciones ============

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
            HabilitarCampos(true);
            ModoOperacion("Nuevo");
        }

        private void btnModificarPrecio_Click(object sender, EventArgs e)
        {
            if (_seleccionado == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.seleccione"),
                    IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            HabilitarCampos(false);
            txtPrecio.Enabled = true;   // SOLO precio
            ModoOperacion("Modificar precio");
            txtPrecio.Focus();
            txtPrecio.SelectAll();
        }

        private void btnBajaAlta_Click(object sender, EventArgs e)
        {
            if (_seleccionado == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.seleccione"),
                    IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            HabilitarCampos(false);
            ModoOperacion(_seleccionado.Activo ? "Dar de baja" : "Reactivar");
        }

        private void btnAplicar_Click(object sender, EventArgs e)
        {
            switch (_modo)
            {
                case "Nuevo":              Nuevo(); break;
                case "Modificar precio":   ModificarPrecio(); break;
                case "Dar de baja":        DarDeBaja(); break;
                case "Reactivar":          Reactivar(); break;
            }
        }

        private void Nuevo()
        {
            string codigo = txtCodigo.Text.Trim();
            string nombre = txtNombre.Text.Trim();
            string categoria = txtCategoria.Text.Trim();
            string descripcion = txtDescripcion.Text.Trim();

            decimal precio;
            if (!decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out precio) &&
                !decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out precio))
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.precioInvalido"),
                    IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int stockA, stockM;
            if (!int.TryParse(txtStockActual.Text.Trim(), out stockA) || stockA < 0)
            { MessageBox.Show(IdiomaManager_GO44.T("productos.stockActInvalido"),
                    IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!int.TryParse(txtStockMinimo.Text.Trim(), out stockM) || stockM < 0)
            { MessageBox.Show(IdiomaManager_GO44.T("productos.stockMinInvalido"),
                    IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var r = _bll.RegistrarProducto(codigo, nombre, categoria, descripcion, precio, stockA, stockM);

            switch (r)
            {
                case BLLComponente_GO44.ResultadoAltaProducto.Exitoso:
                    MessageBox.Show(IdiomaManager_GO44.T("productos.registradoOk"),
                        IdiomaManager_GO44.T("productos.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ModoConsulta();
                    CargarGrilla();
                    CargarCombosCategoria();
                    break;
                case BLLComponente_GO44.ResultadoAltaProducto.CodigoDuplicado:
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("productos.codigoDuplicado"), codigo),
                        IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCodigo.Focus(); break;
                case BLLComponente_GO44.ResultadoAltaProducto.DatosIncompletos:
                    MessageBox.Show(IdiomaManager_GO44.T("productos.completarCodNom"),
                        IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning); break;
                case BLLComponente_GO44.ResultadoAltaProducto.PrecioInvalido:
                    MessageBox.Show(IdiomaManager_GO44.T("productos.precioMayorCero"),
                        IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning); break;
                case BLLComponente_GO44.ResultadoAltaProducto.StockInvalido:
                    MessageBox.Show(IdiomaManager_GO44.T("productos.stocksNoNegativos"),
                        IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning); break;
                default:
                    MessageBox.Show(IdiomaManager_GO44.T("productos.noSeRegistro"),
                        IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error); break;
            }
        }

        private void ModificarPrecio()
        {
            decimal precio;
            if (!decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out precio) &&
                !decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out precio))
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.precioInvalido"),
                    IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (precio < 0)
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.precioMayorCero2"),
                    IdiomaManager_GO44.T("productos.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool ok = _bll.ActualizarPrecio(_seleccionado.Id, precio);
            if (ok)
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.precioActualizado"),
                    IdiomaManager_GO44.T("productos.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.noSeActualizoPrecio"),
                    IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DarDeBaja()
        {
            var r = MessageBox.Show(string.Format(IdiomaManager_GO44.T("productos.confirmarBaja"), _seleccionado.Codigo),
                                    IdiomaManager_GO44.T("productos.confirmar"),
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            bool ok = _bll.DarDeBaja(_seleccionado.Id);
            if (ok)
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.dadoBajaOk"),
                    IdiomaManager_GO44.T("productos.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.noSeDioBaja"),
                    IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Reactivar()
        {
            var r = MessageBox.Show(string.Format(IdiomaManager_GO44.T("productos.confirmarReactivar"), _seleccionado.Codigo),
                                    IdiomaManager_GO44.T("productos.confirmar"),
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            bool ok = _bll.Reactivar(_seleccionado.Id);
            if (ok)
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.reactivadoOk"),
                    IdiomaManager_GO44.T("productos.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show(IdiomaManager_GO44.T("productos.noSeReactivo"),
                    IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e) { ModoConsulta(); }
        private void btnSalir_Click(object sender, EventArgs e)    { this.Close(); }
    }
}
