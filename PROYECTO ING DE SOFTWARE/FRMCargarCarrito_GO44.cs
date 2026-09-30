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
    public partial class FRMCargarCarrito_GO44 : Form, IObservadorIdioma_GO44
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

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
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
                ActualizarIdioma();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(IdiomaManager_GO44.T("carrito.errApertura"), ex.Message + "\n\n" + ex.StackTrace),
                    IdiomaManager_GO44.T("carrito.errAperturaTitulo"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("carrito.titulo");
            if (lblTitulo != null)         lblTitulo.Text         = IdiomaManager_GO44.T("carrito.titulo");
            if (grpCliente != null)        grpCliente.Text        = IdiomaManager_GO44.T("carrito.grpCliente");
            if (lblDniCliente != null)     lblDniCliente.Text     = IdiomaManager_GO44.T("carrito.dni");
            if (btnBuscarCliente != null)  btnBuscarCliente.Text  = IdiomaManager_GO44.T("carrito.btnBuscarCli");
            if (grpProductos != null)      grpProductos.Text      = IdiomaManager_GO44.T("carrito.grpProductos");
            if (lblBuscarProducto != null) lblBuscarProducto.Text = IdiomaManager_GO44.T("carrito.buscar");
            if (lblCategoria != null)      lblCategoria.Text      = IdiomaManager_GO44.T("carrito.categoria");
            if (btnLimpiarBusqueda != null)btnLimpiarBusqueda.Text= IdiomaManager_GO44.T("carrito.btnLimpiar");
            if (lblCantidad != null)       lblCantidad.Text       = IdiomaManager_GO44.T("carrito.cantidad");
            if (btnAgregarLinea != null)   btnAgregarLinea.Text   = IdiomaManager_GO44.T("carrito.btnAgregar");
            if (grpCarrito != null)        grpCarrito.Text        = IdiomaManager_GO44.T("carrito.grpCarrito");
            if (btnQuitarLinea != null)    btnQuitarLinea.Text    = IdiomaManager_GO44.T("carrito.btnQuitar");
            if (btnConfirmar != null)      btnConfirmar.Text      = IdiomaManager_GO44.T("carrito.btnConfirmar");
            if (btnCancelar != null)       btnCancelar.Text       = IdiomaManager_GO44.T("carrito.btnCancelar");
            if (btnSalir != null)          btnSalir.Text          = IdiomaManager_GO44.T("carrito.btnSalir");

            if (_carrito != null && _carrito.Cliente == null && lblClienteInfo != null)
                lblClienteInfo.Text = IdiomaManager_GO44.T("carrito.clienteSinAsignar");

            // Reemplazar el "(todas)" en el combo si estaba
            if (cmbCategoria != null && cmbCategoria.Items.Count > 0)
            {
                // El primer item siempre es el "(todas)" — lo sustituimos
                int idx = cmbCategoria.SelectedIndex;
                cmbCategoria.Items[0] = IdiomaManager_GO44.T("carrito.categoriaTodas");
                if (idx >= 0) cmbCategoria.SelectedIndex = idx;
            }

            RefrescarCarritoUI();
            AplicarFiltro();
        }

        private void ConfigurarGrillas()
        {
            // Campo DNI: máximo 8 dígitos, solo números
            txtDniCliente.MaxLength = 8;
            txtDniCliente.KeyPress += SoloDigitos_KeyPress;

            // Grilla productos
            dgvProductos.ReadOnly = true;
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.AllowUserToDeleteRows = false;
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.MultiSelect = false;
            dgvProductos.RowHeadersVisible = false;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.AllowUserToResizeColumns = false;
            dgvProductos.AllowUserToResizeRows    = false;
            dgvProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvProductos.RowHeadersWidthSizeMode    = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
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
            dgvLineas.AllowUserToResizeColumns = false;
            dgvLineas.AllowUserToResizeRows    = false;
            dgvLineas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvLineas.RowHeadersWidthSizeMode    = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgvLineas.BackgroundColor = Color.White;
            dgvLineas.AutoGenerateColumns = true;
            dgvLineas.DataSource = _bsLineas;
        }

        // ============ CLIENTE ============

        private void SoloDigitos_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            string dni = txtDniCliente.Text.Trim();
            if (!Validaciones_GO44.EsDniValido(dni))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_DNI,
                    IdiomaManager_GO44.T("carrito.dniInvalido"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDniCliente.Focus();
                return;
            }

            BE_Cliente_GO44 cli = _bllCliente.BuscarPorDNI(dni);
            if (cli == null)
            {
                var r = MessageBox.Show(
                    string.Format(IdiomaManager_GO44.T("carrito.clienteInexistenteMensaje"), dni),
                    IdiomaManager_GO44.T("carrito.clienteInexistenteTitulo"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
            lblClienteInfo.Text = string.Format(IdiomaManager_GO44.T("carrito.clienteInfo"), cli.NombreCompleto, cli.DNI);
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
                cmbCategoria.Items.Add(IdiomaManager_GO44.T("carrito.categoriaTodas"));
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
                string todasLbl = IdiomaManager_GO44.T("carrito.categoriaTodas");
                if (!string.IsNullOrEmpty(cat) && cat != todasLbl && cat != "(todas)" && cat != "(all)")
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
                    if (dgvProductos.Columns.Contains("Codigo"))      dgvProductos.Columns["Codigo"].HeaderText = IdiomaManager_GO44.T("carrito.col.codigo");
                    if (dgvProductos.Columns.Contains("Precio"))      dgvProductos.Columns["Precio"].DefaultCellStyle.Format = "N2";
                    if (dgvProductos.Columns.Contains("StockActual")) dgvProductos.Columns["StockActual"].HeaderText = IdiomaManager_GO44.T("carrito.col.stock");
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

                lblContadorProductos.Text = string.Format(IdiomaManager_GO44.T("carrito.contadorProd"), lista.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(IdiomaManager_GO44.T("carrito.errFiltrar"), ex.Message),
                    IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(IdiomaManager_GO44.T("carrito.seleccProd"),
                    IdiomaManager_GO44.T("general.advertencia"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            BE_Componente_GO44 seleccionado = dgvProductos.CurrentRow.DataBoundItem as BE_Componente_GO44;
            if (seleccionado == null) return;

            int cantidad;
            if (!int.TryParse(txtCantidad.Text.Trim(), out cantidad) || cantidad <= 0)
            {
                MessageBox.Show(IdiomaManager_GO44.T("carrito.cantInvalida"),
                    IdiomaManager_GO44.T("general.advertencia"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var vo = _bllComponente.SeleccionarPorId(seleccionado.Id, cantidad);
            if (vo.Resultado != BLLComponente_GO44.ResultadoSeleccionComponente.Exitoso)
            {
                MessageBox.Show(vo.Mensaje,
                    IdiomaManager_GO44.T("carrito.noSePuedeAgregar"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(IdiomaManager_GO44.T("carrito.seleccLinea"),
                    IdiomaManager_GO44.T("general.advertencia"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(IdiomaManager_GO44.T("carrito.faltaCliente"),
                    IdiomaManager_GO44.T("general.advertencia"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_carrito.EstaVacio())
            {
                MessageBox.Show(IdiomaManager_GO44.T("carrito.carritoVacio"),
                    IdiomaManager_GO44.T("general.advertencia"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var r = MessageBox.Show(
                string.Format(IdiomaManager_GO44.T("carrito.confirmarVenta"), _carrito.Total.ToString("N2"), _carrito.CantidadItems),
                IdiomaManager_GO44.T("carrito.confirmarTitulo"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            var vo = _bllCarrito.ConfirmarCarrito(_carrito);
            if (vo.Resultado == BLLCarrito_GO44.ResultadoConfirmacion.Exitoso)
            {
                MessageBox.Show(vo.Mensaje,
                    IdiomaManager_GO44.T("carrito.ventaConfirmadaTitulo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Reset();
                CargarProductos();   // refrescar stock actualizado
            }
            else
            {
                MessageBox.Show(vo.Mensaje +
                    (vo.ComponenteConProblema != null ? string.Format(IdiomaManager_GO44.T("carrito.componenteProblema"), vo.ComponenteConProblema) : ""),
                    IdiomaManager_GO44.T("carrito.noSePudoConfirmar"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show(IdiomaManager_GO44.T("carrito.cancelarConfirm"),
                                    IdiomaManager_GO44.T("carrito.confirmarTitulo"),
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes) Reset();
        }

        private void Reset()
        {
            _carrito = new BE_Carrito_GO44();
            _carrito.LoginVendedor = SessionManager_GO44.Instancia.ObtenerUsuarioActual()?.Login ?? "";
            txtDniCliente.Text = "";
            txtCantidad.Text = "1";
            lblClienteInfo.Text = IdiomaManager_GO44.T("carrito.clienteSinAsignar");
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
                        dgvLineas.Columns["ComponenteCodigo"].HeaderText = IdiomaManager_GO44.T("carrito.col.codigo");
                        if (visibles >= 1) dgvLineas.Columns["ComponenteCodigo"].DisplayIndex = 0;
                    }
                    if (dgvLineas.Columns.Contains("ComponenteNombre"))
                    {
                        dgvLineas.Columns["ComponenteNombre"].HeaderText = IdiomaManager_GO44.T("carrito.col.componente");
                        if (visibles >= 2) dgvLineas.Columns["ComponenteNombre"].DisplayIndex = 1;
                    }
                    if (dgvLineas.Columns.Contains("Cantidad") && visibles >= 3)
                        dgvLineas.Columns["Cantidad"].DisplayIndex = 2;
                    if (dgvLineas.Columns.Contains("PrecioUnitario"))
                    {
                        dgvLineas.Columns["PrecioUnitario"].HeaderText = IdiomaManager_GO44.T("carrito.col.precioUnit");
                        dgvLineas.Columns["PrecioUnitario"].DefaultCellStyle.Format = "N2";
                        if (visibles >= 4) dgvLineas.Columns["PrecioUnitario"].DisplayIndex = 3;
                    }
                    if (dgvLineas.Columns.Contains("Subtotal"))
                    {
                        dgvLineas.Columns["Subtotal"].DefaultCellStyle.Format = "N2";
                        if (visibles >= 5) dgvLineas.Columns["Subtotal"].DisplayIndex = 4;
                    }
                }

                lblCantItems.Text = string.Format(IdiomaManager_GO44.T("carrito.items"), _carrito.CantidadItems);
                lblTotal.Text     = string.Format(IdiomaManager_GO44.T("carrito.total"), _carrito.Total.ToString("N2"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(IdiomaManager_GO44.T("carrito.errRefrescar"), ex.Message),
                    IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
