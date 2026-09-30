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
    /// Pantalla del CU04 Generar Factura.
    /// El cajero ingresa el DNI del cliente, ve el carrito pendiente y presiona
    /// "Cobrar venta" que abre FRMCobrarVenta_GO44 como modal. Al volver, se
    /// genera la factura + registra el cobro en una única transacción atómica.
    /// </summary>
    public partial class FRMFacturar_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly BLLFactura_GO44 _bllFactura;
        private readonly BLLCliente_GO44 _bllCliente;

        private BE_Carrito_GO44 _carrito;
        private BE_Cliente_GO44 _cliente;
        private BE_Factura_GO44 _ultimaFactura;

        // BindingSource para evitar el bug de Index -1 al rebindar
        private readonly System.Windows.Forms.BindingSource _bsLineas = new System.Windows.Forms.BindingSource();

        public FRMFacturar_GO44()
        {
            InitializeComponent();
            _bllFactura = new BLLFactura_GO44();
            _bllCliente = new BLLCliente_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        private void FRMFacturar_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            LimpiarUI();
            ActualizarIdioma();
        }

        public void ActualizarIdioma()
        {
            this.Text          = IdiomaManager_GO44.T("facturar.titulo");
            if (lblTitulo != null)   lblTitulo.Text   = IdiomaManager_GO44.T("facturar.titulo");
            if (grpCliente != null)  grpCliente.Text  = IdiomaManager_GO44.T("facturar.grpCliente");
            if (grpCarrito != null)  grpCarrito.Text  = IdiomaManager_GO44.T("facturar.grpCarrito");
            if (lblDni != null)      lblDni.Text      = IdiomaManager_GO44.T("facturar.dni");
            if (btnBuscar != null)   btnBuscar.Text   = IdiomaManager_GO44.T("facturar.btnBuscar");
            if (btnCobrar != null)   btnCobrar.Text   = IdiomaManager_GO44.T("facturar.btnCobrar");
            if (btnImprimir != null) btnImprimir.Text = IdiomaManager_GO44.T("facturar.btnImprimir");
            if (btnSalir != null)    btnSalir.Text    = IdiomaManager_GO44.T("facturar.btnSalir");
            if (_carrito == null && lblCliente != null)
                lblCliente.Text = IdiomaManager_GO44.T("facturar.clienteSinAsignar");
        }

        private void ConfigurarGrilla()
        {
            // Campo DNI: máximo 8 dígitos, solo números
            txtDni.MaxLength = 8;
            txtDni.KeyPress += SoloDigitos_KeyPress;

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

        // ============ BUSCAR CLIENTE + CARRITO ============

        private void SoloDigitos_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string dni = txtDni.Text.Trim();
            if (!Validaciones_GO44.EsDniValido(dni))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_DNI,
                    IdiomaManager_GO44.T("facturar.dniInvalido"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDni.Focus();
                return;
            }

            _cliente = _bllCliente.BuscarPorDNI(dni);
            if (_cliente == null)
            {
                var r = MessageBox.Show(
                    string.Format(IdiomaManager_GO44.T("facturar.clienteInexistenteMensaje"), dni),
                    IdiomaManager_GO44.T("facturar.clienteInexistenteTitulo"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                {
                    var frmCli = new FRMGestionClientes_GO44();
                    frmCli.ShowDialog();
                    _cliente = _bllCliente.BuscarPorDNI(dni);
                    if (_cliente == null) return;
                }
                else return;
            }

            _carrito = _bllFactura.ObtenerCarritoPendiente(dni);
            if (_carrito == null)
            {
                string nombre = _cliente.NombreCompleto;   // capturar antes de LimpiarUI (que setea _cliente = null)
                MessageBox.Show(string.Format(IdiomaManager_GO44.T("facturar.sinCarritoMensaje"), nombre),
                    IdiomaManager_GO44.T("facturar.sinCarritoTitulo"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarUI();
                lblCliente.Text = string.Format(IdiomaManager_GO44.T("facturar.clienteSinCarrito"), nombre);
                lblCliente.ForeColor = Color.Firebrick;
                return;
            }
            _carrito.Cliente = _cliente;

            MostrarCarrito();
        }

        private void MostrarCarrito()
        {
            lblCliente.Text = string.Format(IdiomaManager_GO44.T("facturar.cliente"), _cliente.NombreCompleto, _cliente.DNI);
            lblCliente.ForeColor = Color.DarkGreen;

            _bsLineas.DataSource = _carrito.Lineas;
            _bsLineas.ResetBindings(false);

            if (dgvLineas.Columns.Count > 0)
            {
                if (dgvLineas.Columns.Contains("Componente"))       dgvLineas.Columns["Componente"].Visible = false;
                if (dgvLineas.Columns.Contains("Id"))               dgvLineas.Columns["Id"].Visible = false;
                if (dgvLineas.Columns.Contains("IdCarrito"))        dgvLineas.Columns["IdCarrito"].Visible = false;
                if (dgvLineas.Columns.Contains("ComponenteCodigo")) dgvLineas.Columns["ComponenteCodigo"].HeaderText = "Código";
                if (dgvLineas.Columns.Contains("ComponenteNombre")) dgvLineas.Columns["ComponenteNombre"].HeaderText = "Componente";
                if (dgvLineas.Columns.Contains("PrecioUnitario"))
                {
                    dgvLineas.Columns["PrecioUnitario"].HeaderText = "Precio Unit.";
                    dgvLineas.Columns["PrecioUnitario"].DefaultCellStyle.Format = "N2";
                }
                if (dgvLineas.Columns.Contains("Subtotal")) dgvLineas.Columns["Subtotal"].DefaultCellStyle.Format = "N2";
            }

            decimal subtotal = _carrito.Total;
            decimal iva      = _bllFactura.CalcularIVA(_carrito);
            decimal total    = _bllFactura.CalcularTotalConIVA(_carrito);

            lblSubtotal.Text = string.Format(IdiomaManager_GO44.T("facturar.subtotal"), subtotal.ToString("N2"));
            lblIVA.Text      = string.Format(IdiomaManager_GO44.T("facturar.iva"), (_bllFactura.AlicuotaIVA * 100).ToString("0"), iva.ToString("N2"));
            lblTotal.Text    = string.Format(IdiomaManager_GO44.T("facturar.total"), total.ToString("N2"));

            btnCobrar.Enabled = true;
            btnImprimir.Enabled = false;
        }

        // ============ COBRAR (CU05 include) ============

        private void btnCobrar_Click(object sender, EventArgs e)
        {
            if (_carrito == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("facturar.avisoBuscar"),
                    IdiomaManager_GO44.T("general.advertencia"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal total = _bllFactura.CalcularTotalConIVA(_carrito);
            using (var frmCobro = new FRMCobrarVenta_GO44(total))
            {
                if (frmCobro.ShowDialog() != DialogResult.OK) return;

                BE_Cobro_GO44 cobro = frmCobro.CobroValidado;
                if (cobro == null) return;

                var vo = _bllFactura.GenerarFacturaYCobrar(_carrito, cobro);
                if (vo.Resultado == BLLFactura_GO44.ResultadoFactura.Exitoso)
                {
                    _ultimaFactura = vo.Factura;
                    MessageBox.Show(vo.Mensaje,
                        IdiomaManager_GO44.T("facturar.facturaGeneradaTitulo"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnImprimir.Enabled = true;
                    btnCobrar.Enabled = false;
                }
                else
                {
                    MessageBox.Show(vo.Mensaje,
                        IdiomaManager_GO44.T("facturar.noSePudoFacturar"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ============ IMPRIMIR ============

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            if (_ultimaFactura == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = IdiomaManager_GO44.T("facturar.guardarFacturaFiltro");
                sfd.FileName = _ultimaFactura.NumeroFactura + ".txt";
                sfd.Title = IdiomaManager_GO44.T("facturar.guardarFacturaTitulo");
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    string contenido = GenerarTextoFactura(_ultimaFactura);
                    System.IO.File.WriteAllText(sfd.FileName, contenido, System.Text.Encoding.UTF8);
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("facturar.facturaGuardadaMensaje"), sfd.FileName),
                        IdiomaManager_GO44.T("facturar.facturaGuardadaTitulo"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("facturar.errorGuardar"), ex.Message),
                        IdiomaManager_GO44.T("general.error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GenerarTextoFactura(BE_Factura_GO44 f)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("========================================================");
            sb.AppendLine("           TECHFLOW - ELECTROPOINT");
            sb.AppendLine("           FACTURA " + f.NumeroFactura);
            sb.AppendLine("========================================================");
            sb.AppendLine("Fecha: " + f.FechaEmision.ToString("dd/MM/yyyy HH:mm"));
            sb.AppendLine("Cajero: " + f.LoginCajero);
            sb.AppendLine();
            sb.AppendLine("Cliente:");
            sb.AppendLine("  DNI:    " + (f.Cliente != null ? f.Cliente.DNI : ""));
            sb.AppendLine("  Nombre: " + (f.Cliente != null ? f.Cliente.NombreCompleto : ""));
            sb.AppendLine();
            sb.AppendLine("--------------------------------------------------------");
            sb.AppendLine(string.Format("{0,-10} {1,-30} {2,4} {3,12} {4,12}", "Código", "Componente", "Cant", "Precio", "Subtotal"));
            sb.AppendLine("--------------------------------------------------------");
            if (f.Lineas != null)
            {
                foreach (var l in f.Lineas)
                {
                    string nom = l.ComponenteNombre ?? "";
                    if (nom.Length > 30) nom = nom.Substring(0, 30);
                    sb.AppendLine(string.Format("{0,-10} {1,-30} {2,4} {3,12} {4,12}",
                        l.ComponenteCodigo, nom, l.Cantidad,
                        l.PrecioUnitario.ToString("N2"), l.Subtotal.ToString("N2")));
                }
            }
            sb.AppendLine("--------------------------------------------------------");
            sb.AppendLine(string.Format("{0,50} {1,12}", "Subtotal:", f.Subtotal.ToString("N2")));
            sb.AppendLine(string.Format("{0,50} {1,12}", "IVA:", f.IVA.ToString("N2")));
            sb.AppendLine(string.Format("{0,50} {1,12}", "TOTAL:", f.Total.ToString("N2")));
            sb.AppendLine("========================================================");
            sb.AppendLine();
            sb.AppendLine("Gracias por su compra.");
            return sb.ToString();
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }

        // ============ UI ============

        private void LimpiarUI()
        {
            _bsLineas.DataSource = null;
            _carrito = null;
            _cliente = null;
            _ultimaFactura = null;
            txtDni.Text = "";
            lblCliente.Text = IdiomaManager_GO44.T("facturar.clienteSinAsignar");
            lblCliente.ForeColor = Color.Firebrick;
            lblSubtotal.Text = IdiomaManager_GO44.T("facturar.subtotalCero");
            lblIVA.Text      = IdiomaManager_GO44.T("facturar.ivaCero");
            lblTotal.Text    = IdiomaManager_GO44.T("facturar.totalCero");
            btnCobrar.Enabled = false;
            btnImprimir.Enabled = false;
        }
    }
}
