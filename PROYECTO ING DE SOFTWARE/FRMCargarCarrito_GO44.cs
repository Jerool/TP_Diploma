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
    /// Cargar Carrito — estilo tienda:
    ///   • Buscá el cliente por DNI (si no existe, ofrece registrarlo).
    ///   • La grilla superior muestra TODOS los productos activos; podés filtrar por
    ///     nombre / categoría (tipeás "procesador" y aparecen todos los procesadores).
    ///   • Seleccionás un producto de la grilla, ingresás cantidad y agregás al carrito.
    ///   • La grilla inferior muestra el carrito armado. Podés quitar líneas.
    ///   • Confirmar Venta → transacción atómica (BLLCarrito.ConfirmarCarrito).
    /// </summary>
    public partial class FRMCargarCarrito_GO44 : Form
    {
        private readonly BLLCliente_GO44    _bllCliente;
        private readonly BLLComponente_GO44 _bllComponente;
        private readonly BLLCarrito_GO44    _bllCarrito;

        private BE_Carrito_GO44 _carrito;
        private List<BE_Componente_GO44> _todosProductos;
        private bool _cargando = false;   // evita eventos durante inicialización

        // BindingSources — evitan bug de "Index -1" al rebindar DataSource directamente
        private readonly System.Windows.Forms.BindingSource _bsProductos = new System.Windows.Forms.BindingSource();
        private readonly System.Windows.Forms.BindingSource _bsLineas    = new System.Windows.Forms.BindingSource();

        public FRMCargarCarrito_GO44()
        {
            InitializeComponent();
            _bllCliente    = new BLLCliente_GO44();
            _bllComponente = new BLLComponente_GO44();
            _bllCarrito    = new BLLCarrito_GO44();
        }

        private void FRMCargarCarrito_GO44_Load(object sender, EventArgs e)
        {
            try
            {
                _cargando = true;
                ConfigurarGrillas();
                _carrito = new BE_Carrito_GO44();
                _carrito.LoginVendedor = SessionManager_GO44.Instancia.ObtenerUsuarioActual()?.Login ?? "";
                _cargando = false;
                CargarProductos();
                RefrescarCarritoUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir Cargar Carrito: " + ex.Message + "\n\n" + ex.StackTrace,
                    "Error de inicialización", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarGrillas()
        {
            // Grilla productos
            dgvProductos.ReadOnly = true;
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.AllowUserToDeleteRows = false;
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.MultiSelect = false;
            dgvProductos.RowHeadersVisible = false;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.BackgroundColor = Color.White;
            dgvProductos.AutoGenerateColumns = true;
            dgvProductos.DataSource = _bsProductos;

            // Grilla líneas del carrito
            dgvLineas.ReadOnly = true;
            dgvLineas.AllowUserToAddRows = false;
            dgvLineas.AllowUserToDeleteRows = false;
            dgvLineas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLineas.MultiSelect = false;
            dgvLineas.RowHeadersVisible = false;
            dgvLineas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLineas.BackgroundColor = Color.White;
            dgvLineas.AutoGenerateColumns = true;
            dgvLineas.DataSource = _bsLineas;
        }

        // ============ CLIENTE ============

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            string dni = txtDniCliente.Text.Trim();
            if (string.IsNullOrEmpty(dni))
            {
                MessageBox.Show("Ingrese un DNI", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BE_Cliente_GO44 cli = _bllCliente.BuscarPorDNI(dni);
            if (cli == null)
            {
                var r = MessageBox.Show(
                    "El cliente DNI " + dni + " no está registrado.\n¿Registrarlo ahora?",
                    "Cliente inexistente", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                {
                    FRMGestionClientes_GO44 frm = new FRMGestionClientes_GO44();
                    frm.ShowDialog();
                    cli = _bllCliente.BuscarPorDNI(dni);
                    if (cli == null) return;
                }
                else return;
            }

            _carrito.Cliente = cli;
            lblClienteInfo.Text = "Cliente: " + cli.NombreCompleto + "  ·  DNI " + cli.DNI;
            lblClienteInfo.ForeColor = Color.DarkGreen;
        }

        // ============ PRODUCTOS ============

        private void CargarProductos()
        {
            _todosProductos = _bllComponente.ListarActivos();
            CargarComboCategoria();
            AplicarFiltro();
        }

        private void CargarComboCategoria()
        {
            _cargando = true;
            try
            {
                cmbCategoria.Items.Clear();
                cmbCategoria.Items.Add("(todas)");
                if (_todosProductos != null)
                {
                    var cats = _todosProductos
                        .Where(c => !string.IsNullOrEmpty(c.Categoria))
                        .Select(c => c.Categoria).Distinct().OrderBy(s => s);
                    foreach (var c in cats) cmbCategoria.Items.Add(c);
                }
                if (cmbCategoria.Items.Count > 0)
                    cmbCategoria.SelectedIndex = 0;
            }
            finally { _cargando = false; }
        }

        private void AplicarFiltro()
        {
            if (_cargando) return;
            if (_todosProductos == null) return;
            try
            {
                IEnumerable<BE_Componente_GO44> q = _todosProductos;

                string busca = (txtBuscarProducto.Text ?? "").Trim().ToUpper();
                if (!string.IsNullOrEmpty(busca))
                {
                    q = q.Where(c =>
                        (c.Nombre    != null && c.Nombre.ToUpper().Contains(busca)) ||
                        (c.Categoria != null && c.Categoria.ToUpper().Contains(busca)) ||
                        (c.Codigo    != null && c.Codigo.ToUpper().Contains(busca)));
                }

                string cat = cmbCategoria.SelectedIndex >= 0 ? cmbCategoria.SelectedItem as string : null;
                if (!string.IsNullOrEmpty(cat) && cat != "(todas)")
                    q = q.Where(c => c.Categoria == cat);

                var lista = q.ToList();
                _bsProductos.DataSource = lista;
                _bsProductos.ResetBindings(false);

                if (dgvProductos.Columns.Count > 0)
                {
                    if (dgvProductos.Columns.Contains("Descripcion")) dgvProductos.Columns["Descripcion"].Visible = false;
                    if (dgvProductos.Columns.Contains("Activo"))      dgvProductos.Columns["Activo"].Visible = false;
                    if (dgvProductos.Columns.Contains("Id"))          dgvProductos.Columns["Id"].Visible = false;
                    if (dgvProductos.Columns.Contains("StockMinimo")) dgvProductos.Columns["StockMinimo"].Visible = false;
                    if (dgvProductos.Columns.Contains("Codigo"))      dgvProductos.Columns["Codigo"].HeaderText = "Código";
                    if (dgvProductos.Columns.Contains("Precio"))      dgvProductos.Columns["Precio"].DefaultCellStyle.Format = "N2";
                    if (dgvProductos.Columns.Contains("StockActual")) dgvProductos.Columns["StockActual"].HeaderText = "Stock";
                }

                foreach (DataGridViewRow row in dgvProductos.Rows)
                {
                    BE_Componente_GO44 c = row.DataBoundItem as BE_Componente_GO44;
                    if (c == null) continue;
                    if (c.StockActual == 0)
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                    else if (c.StockActual <= c.StockMinimo)
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                }

                lblContadorProductos.Text = "Productos: " + lista.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al filtrar productos: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtBuscarProducto_TextChanged(object sender, EventArgs e) { if (!_cargando) AplicarFiltro(); }
        private void cmbCategoria_SelectedIndexChanged(object sender, EventArgs e) { if (!_cargando) AplicarFiltro(); }
        private void btnLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            txtBuscarProducto.Text = "";
            cmbCategoria.SelectedIndex = 0;
        }

        // ============ AGREGAR AL CARRITO ============

        private void btnAgregarLinea_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un producto de la grilla", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            BE_Componente_GO44 seleccionado = dgvProductos.CurrentRow.DataBoundItem as BE_Componente_GO44;
            if (seleccionado == null) return;

            int cantidad;
            if (!int.TryParse(txtCantidad.Text.Trim(), out cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Cantidad inválida", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var vo = _bllComponente.SeleccionarPorId(seleccionado.Id, cantidad);
            if (vo.Resultado != BLLComponente_GO44.ResultadoSeleccionComponente.Exitoso)
            {
                MessageBox.Show(vo.Mensaje, "No se puede agregar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _carrito.AgregarLinea(vo.Componente, cantidad);
            RefrescarCarritoUI();
            txtCantidad.Text = "1";
        }

        private void btnQuitarLinea_Click(object sender, EventArgs e)
        {
            if (dgvLineas.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una línea", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            BE_LineaCarrito_GO44 linea = dgvLineas.CurrentRow.DataBoundItem as BE_LineaCarrito_GO44;
            if (linea == null || linea.Componente == null) return;

            _carrito.QuitarLinea(linea.Componente.Id);
            RefrescarCarritoUI();
        }

        // ============ CONFIRMAR ============

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (_carrito.Cliente == null)
            {
                MessageBox.Show("Falta asignar cliente al carrito", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_carrito.EstaVacio())
            {
                MessageBox.Show("El carrito está vacío", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var r = MessageBox.Show(
                "Confirmar venta por $" + _carrito.Total.ToString("N2") + " (" + _carrito.CantidadItems + " items)?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            var vo = _bllCarrito.ConfirmarCarrito(_carrito);
            if (vo.Resultado == BLLCarrito_GO44.ResultadoConfirmacion.Exitoso)
            {
                MessageBox.Show(vo.Mensaje, "Venta confirmada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Reset();
                CargarProductos();   // refrescar stock actualizado
            }
            else
            {
                MessageBox.Show(vo.Mensaje +
                    (vo.ComponenteConProblema != null ? "\nComponente: " + vo.ComponenteConProblema : ""),
                    "No se pudo confirmar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("¿Cancelar el carrito actual? Se perderán las líneas cargadas.",
                                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes) Reset();
        }

        private void Reset()
        {
            _carrito = new BE_Carrito_GO44();
            _carrito.LoginVendedor = SessionManager_GO44.Instancia.ObtenerUsuarioActual()?.Login ?? "";
            txtDniCliente.Text = "";
            txtCantidad.Text = "1";
            lblClienteInfo.Text = "Cliente: (sin asignar)";
            lblClienteInfo.ForeColor = Color.Firebrick;
            RefrescarCarritoUI();
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }

        // ============ UI HELPER ============

        private void RefrescarCarritoUI()
        {
            try
            {
                // Uso BindingSource para evitar el bug de Index -1 al rebindar directo
                _bsLineas.DataSource = null;
                _bsLineas.DataSource = _carrito.Lineas != null ? new List<BE_LineaCarrito_GO44>(_carrito.Lineas) : new List<BE_LineaCarrito_GO44>();
                _bsLineas.ResetBindings(false);

                // Ajuste de columnas solo si ya fueron auto-generadas (>=1)
                if (dgvLineas.Columns.Count > 0)
                {
                    if (dgvLineas.Columns.Contains("Componente")) dgvLineas.Columns["Componente"].Visible = false;
                    if (dgvLineas.Columns.Contains("Id"))         dgvLineas.Columns["Id"].Visible = false;
                    if (dgvLineas.Columns.Contains("IdCarrito"))  dgvLineas.Columns["IdCarrito"].Visible = false;

                    // DisplayIndex se aplica solo si ya hay columnas suficientes
                    int visibles = 0;
                    foreach (DataGridViewColumn col in dgvLineas.Columns)
                        if (col.Visible) visibles++;

                    if (dgvLineas.Columns.Contains("ComponenteCodigo"))
                    {
                        dgvLineas.Columns["ComponenteCodigo"].HeaderText = "Código";
                        if (visibles >= 1) dgvLineas.Columns["ComponenteCodigo"].DisplayIndex = 0;
                    }
                    if (dgvLineas.Columns.Contains("ComponenteNombre"))
                    {
                        dgvLineas.Columns["ComponenteNombre"].HeaderText = "Componente";
                        if (visibles >= 2) dgvLineas.Columns["ComponenteNombre"].DisplayIndex = 1;
                    }
                    if (dgvLineas.Columns.Contains("Cantidad") && visibles >= 3)
                        dgvLineas.Columns["Cantidad"].DisplayIndex = 2;
                    if (dgvLineas.Columns.Contains("PrecioUnitario"))
                    {
                        dgvLineas.Columns["PrecioUnitario"].HeaderText = "Precio Unit.";
                        dgvLineas.Columns["PrecioUnitario"].DefaultCellStyle.Format = "N2";
                        if (visibles >= 4) dgvLineas.Columns["PrecioUnitario"].DisplayIndex = 3;
                    }
                    if (dgvLineas.Columns.Contains("Subtotal"))
                    {
                        dgvLineas.Columns["Subtotal"].DefaultCellStyle.Format = "N2";
                        if (visibles >= 5) dgvLineas.Columns["Subtotal"].DisplayIndex = 4;
                    }
                }

                lblCantItems.Text = "Items: " + _carrito.CantidadItems;
                lblTotal.Text     = "Total: $ " + _carrito.Total.ToString("N2");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al refrescar carrito: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
