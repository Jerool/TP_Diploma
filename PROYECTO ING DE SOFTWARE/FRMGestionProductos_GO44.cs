using BE;
using BLL;
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
    public partial class FRMGestionProductos_GO44 : Form
    {
        private readonly BLLComponente_GO44 _bll;
        private List<BE_Componente_GO44> _todos;
        private BE_Componente_GO44 _seleccionado;
        private string _modo = "Consulta";

        public FRMGestionProductos_GO44()
        {
            InitializeComponent();
            _bll = new BLLComponente_GO44();
        }

        private void FRMGestionProductos_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            ModoConsulta();
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

            lblTotal.Text = "Total: " + (lista != null ? lista.Count : 0);
        }

        private void CargarCombosCategoria()
        {
            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add("(todas)");
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
            if (!string.IsNullOrEmpty(cat) && cat != "(todas)")
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
            lblMensaje.Text = "Modo: Consulta";
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
            lblMensaje.Text = "Modo: " + modo;
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
                MessageBox.Show("Seleccione un producto", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("Seleccione un producto", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("Precio inválido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int stockA, stockM;
            if (!int.TryParse(txtStockActual.Text.Trim(), out stockA) || stockA < 0)
            { MessageBox.Show("Stock actual inválido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!int.TryParse(txtStockMinimo.Text.Trim(), out stockM) || stockM < 0)
            { MessageBox.Show("Stock mínimo inválido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var r = _bll.RegistrarProducto(codigo, nombre, categoria, descripcion, precio, stockA, stockM);

            switch (r)
            {
                case BLLComponente_GO44.ResultadoAltaProducto.Exitoso:
                    MessageBox.Show("Producto registrado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ModoConsulta();
                    CargarGrilla();
                    CargarCombosCategoria();
                    break;
                case BLLComponente_GO44.ResultadoAltaProducto.CodigoDuplicado:
                    MessageBox.Show("Ya existe un producto con código " + codigo, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCodigo.Focus(); break;
                case BLLComponente_GO44.ResultadoAltaProducto.DatosIncompletos:
                    MessageBox.Show("Complete al menos código y nombre", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); break;
                case BLLComponente_GO44.ResultadoAltaProducto.PrecioInvalido:
                    MessageBox.Show("El precio debe ser mayor o igual a 0", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); break;
                case BLLComponente_GO44.ResultadoAltaProducto.StockInvalido:
                    MessageBox.Show("Los stocks no pueden ser negativos", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); break;
                default:
                    MessageBox.Show("No se pudo registrar el producto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); break;
            }
        }

        private void ModificarPrecio()
        {
            decimal precio;
            if (!decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out precio) &&
                !decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out precio))
            {
                MessageBox.Show("Precio inválido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (precio < 0)
            {
                MessageBox.Show("El precio debe ser >= 0", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool ok = _bll.ActualizarPrecio(_seleccionado.Id, precio);
            if (ok)
            {
                MessageBox.Show("Precio actualizado", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show("No se pudo actualizar el precio", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DarDeBaja()
        {
            var r = MessageBox.Show("¿Dar de baja el producto " + _seleccionado.Codigo + "?\n\n" +
                                    "Ya no aparecerá en la lista de productos disponibles del carrito.",
                                    "Confirmar baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            bool ok = _bll.DarDeBaja(_seleccionado.Id);
            if (ok)
            {
                MessageBox.Show("Producto dado de baja", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show("No se pudo dar de baja", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Reactivar()
        {
            var r = MessageBox.Show("¿Reactivar el producto " + _seleccionado.Codigo + "?",
                                    "Confirmar reactivación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            bool ok = _bll.Reactivar(_seleccionado.Id);
            if (ok)
            {
                MessageBox.Show("Producto reactivado", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show("No se pudo reactivar", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e) { ModoConsulta(); }
        private void btnSalir_Click(object sender, EventArgs e)    { this.Close(); }
    }
}
