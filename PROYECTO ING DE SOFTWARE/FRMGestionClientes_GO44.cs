using BE;
using BLL;
using Servicios;
using Servicios.Serializacion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    public partial class FRMGestionClientes_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly BLLCliente_GO44 _bll;
        private string _modo = "Consulta";
        private BE_Cliente_GO44 _clienteSeleccionado = null;
        // true cuando la grilla muestra clientes des-serializados de un XML (no los de la base).
        private bool _datosDeserializados = false;

        public FRMGestionClientes_GO44()
        {
            InitializeComponent();
            _bll = new BLLCliente_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        private void FRMGestionClientes_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            ModoConsulta();
            CargarGrilla();
            ActualizarIdioma();
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("clientes.titulo");
            if (lblTitulo != null)   lblTitulo.Text   = IdiomaManager_GO44.T("clientes.titulo");
            if (lblDNI != null)      lblDNI.Text      = IdiomaManager_GO44.T("clientes.dni");
            if (lblApellido != null) lblApellido.Text = IdiomaManager_GO44.T("clientes.apellido");
            if (lblNombre != null)   lblNombre.Text   = IdiomaManager_GO44.T("clientes.nombre");
            if (lblEmail != null)    lblEmail.Text    = IdiomaManager_GO44.T("clientes.email");
            if (lblTelefono != null) lblTelefono.Text = IdiomaManager_GO44.T("clientes.telefono");
            if (btnNuevo != null)    btnNuevo.Text    = IdiomaManager_GO44.T("clientes.btnNuevo");
            if (btnModificar != null)btnModificar.Text= IdiomaManager_GO44.T("clientes.btnModificar");
            if (btnActivarDesactivar != null) btnActivarDesactivar.Text = IdiomaManager_GO44.T("clientes.btnActivarDesactivar");
            if (btnAplicar != null)  btnAplicar.Text  = IdiomaManager_GO44.T("clientes.btnAplicar");
            if (btnCancelar != null) btnCancelar.Text = IdiomaManager_GO44.T("clientes.btnCancelar");
            if (btnSalir != null)    btnSalir.Text    = IdiomaManager_GO44.T("clientes.btnSalir");
            if (btnSerializar != null)    btnSerializar.Text    = IdiomaManager_GO44.T("clientes.btnSerializar");
            if (btnDesserializar != null) btnDesserializar.Text = IdiomaManager_GO44.T("clientes.btnDesserializar");
            if (btnLimpiarGrilla != null) btnLimpiarGrilla.Text = IdiomaManager_GO44.T("clientes.btnLimpiarGrilla");
            if (btnActualizar != null)    btnActualizar.Text    = IdiomaManager_GO44.T("clientes.btnActualizar");

            // Tooltips traducibles de los 4 botones.
            if (ttClientes != null)
            {
                if (btnSerializar != null)    ttClientes.SetToolTip(btnSerializar,    IdiomaManager_GO44.T("clientes.tipSerializar"));
                if (btnDesserializar != null) ttClientes.SetToolTip(btnDesserializar, IdiomaManager_GO44.T("clientes.tipDesserializar"));
                if (btnLimpiarGrilla != null) ttClientes.SetToolTip(btnLimpiarGrilla, IdiomaManager_GO44.T("clientes.tipLimpiarGrilla"));
                if (btnActualizar != null)    ttClientes.SetToolTip(btnActualizar,    IdiomaManager_GO44.T("clientes.tipActualizar"));
            }
            RefrescarLblMensaje();
        }

        private void RefrescarLblMensaje()
        {
            if (lblMensaje == null) return;
            switch (_modo)
            {
                case "Consulta":  lblMensaje.Text = IdiomaManager_GO44.T("clientes.modoConsulta"); break;
                case "Nuevo":     lblMensaje.Text = string.Format(IdiomaManager_GO44.T("clientes.modo"), IdiomaManager_GO44.T("clientes.modoNuevo")); break;
                case "Modificar": lblMensaje.Text = string.Format(IdiomaManager_GO44.T("clientes.modo"), IdiomaManager_GO44.T("clientes.modoModificar")); break;
                default:          lblMensaje.Text = string.Format(IdiomaManager_GO44.T("clientes.modo"), _modo); break;
            }
        }

        private void ConfigurarGrilla()
        {
            dgvClientes.ReadOnly = true;
            dgvClientes.AllowUserToAddRows = false;
            dgvClientes.AllowUserToDeleteRows = false;
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.MultiSelect = false;
            dgvClientes.RowHeadersVisible = false;
            dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // Bloquear resize de columnas y filas
            dgvClientes.AllowUserToResizeColumns = false;
            dgvClientes.AllowUserToResizeRows    = false;
            dgvClientes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvClientes.RowHeadersWidthSizeMode    = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            // Aplicar máscaras en los TextBox (usa Validaciones_GO44)
            txtDNI.MaxLength      = 8;
            txtTelefono.MaxLength = 10;
            txtDNI.KeyPress      += SoloDigitos_KeyPress;
            txtTelefono.KeyPress += SoloDigitos_KeyPress;
        }

        // Filtro de teclado: solo dígitos y control (backspace, etc.)
        private void SoloDigitos_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void CargarGrilla()
        {
            // Datos reales desde la base: la grilla deja el modo "des-serializado".
            _datosDeserializados = false;
            MostrarClientesEnGrilla(_bll.Listar());
        }

        /// <summary>
        /// Vuelca una lista de clientes en la grilla (oculta NombreCompleto y pinta en rojo los
        /// inactivos). La usan tanto la carga desde la base como la des-serialización de un XML.
        /// </summary>
        private void MostrarClientesEnGrilla(List<BE_Cliente_GO44> lista)
        {
            dgvClientes.DataSource = null;
            dgvClientes.DataSource = lista;

            if (dgvClientes.Columns.Contains("NombreCompleto")) dgvClientes.Columns["NombreCompleto"].Visible = false;

            foreach (DataGridViewRow row in dgvClientes.Rows)
            {
                object v = row.Cells["Activo"].Value;
                if (v != null && v is bool && !(bool)v)
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
            }
        }

        private void ModoConsulta()
        {
            _modo = "Consulta";
            RefrescarLblMensaje();
            LimpiarCampos();
            HabilitarCampos(false);
            btnNuevo.Enabled = true;
            btnModificar.Enabled = true;
            btnActivarDesactivar.Enabled = true;
            btnAplicar.Enabled = false;
            btnCancelar.Enabled = false;
            dgvClientes.Enabled = true;
        }

        private void ModoOperacion(string modo)
        {
            _modo = modo;
            RefrescarLblMensaje();
            btnNuevo.Enabled = false;
            btnModificar.Enabled = false;
            btnActivarDesactivar.Enabled = false;
            btnAplicar.Enabled = true;
            btnCancelar.Enabled = true;
        }

        private void HabilitarCampos(bool habilitar)
        {
            txtDNI.Enabled = habilitar;
            txtApellido.Enabled = habilitar;
            txtNombre.Enabled = habilitar;
            txtEmail.Enabled = habilitar;
            txtTelefono.Enabled = habilitar;
        }

        private void LimpiarCampos()
        {
            txtDNI.Text = txtApellido.Text = txtNombre.Text = txtEmail.Text = txtTelefono.Text = "";
        }

        private void dgvClientes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvClientes.CurrentRow == null) return;
            _clienteSeleccionado = dgvClientes.CurrentRow.DataBoundItem as BE_Cliente_GO44;
            if (_clienteSeleccionado == null) return;

            txtDNI.Text      = _clienteSeleccionado.DNI;
            txtApellido.Text = _clienteSeleccionado.Apellido;
            txtNombre.Text   = _clienteSeleccionado.Nombre;
            txtEmail.Text    = _clienteSeleccionado.Email;
            txtTelefono.Text = _clienteSeleccionado.Telefono;
        }

        /// <summary>
        /// true (y avisa) si la grilla muestra datos des-serializados de un XML: el ABM
        /// (alta/modificación/baja) no debe operar sobre esas filas. Con "Actualizar" se vuelve a
        /// los datos reales de la base.
        /// </summary>
        private bool BloquearPorDeserializados()
        {
            if (!_datosDeserializados) return false;
            MessageBox.Show(IdiomaManager_GO44.T("clientes.abmDeserializado"),
                IdiomaManager_GO44.T("clientes.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return true;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            if (BloquearPorDeserializados()) return;
            LimpiarCampos();
            HabilitarCampos(true);
            dgvClientes.Enabled = false;
            ModoOperacion("Nuevo");
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (BloquearPorDeserializados()) return;
            if (_clienteSeleccionado == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("clientes.seleccione"),
                    IdiomaManager_GO44.T("clientes.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            HabilitarCampos(false);
            txtEmail.Enabled = true;
            txtTelefono.Enabled = true;
            dgvClientes.Enabled = false;
            ModoOperacion("Modificar");
        }

        private void btnActivarDesactivar_Click(object sender, EventArgs e)
        {
            if (BloquearPorDeserializados()) return;
            if (_clienteSeleccionado == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("clientes.seleccione"),
                    IdiomaManager_GO44.T("clientes.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            HabilitarCampos(false);
            dgvClientes.Enabled = false;
            ModoOperacion("ActivarDesactivar");
        }

        private void btnAplicar_Click(object sender, EventArgs e)
        {
            switch (_modo)
            {
                case "Nuevo":              Nuevo(); break;
                case "Modificar":          Modificar(); break;
                case "ActivarDesactivar":  ActivarDesactivar(); break;
            }
        }

        private void Nuevo()
        {
            string dni = txtDNI.Text.Trim();
            string ape = txtApellido.Text.Trim();
            string nom = txtNombre.Text.Trim();
            string mail = txtEmail.Text.Trim();
            string tel = txtTelefono.Text.Trim();

            // Validaciones cliente-side reutilizando Validaciones_GO44 de Servicios
            if (!Validaciones_GO44.EsDniValido(dni))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_DNI, IdiomaManager_GO44.T("clientes.dniInvalido"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDNI.Focus(); return;
            }
            if (!Validaciones_GO44.EsApellidoValido(ape))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_APELLIDO, IdiomaManager_GO44.T("clientes.apellidoInvalido"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtApellido.Focus(); return;
            }
            if (!Validaciones_GO44.EsNombreValido(nom))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_NOMBRE, IdiomaManager_GO44.T("clientes.nombreInvalido"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus(); return;
            }
            if (!Validaciones_GO44.EsEmailValido(mail))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_EMAIL, IdiomaManager_GO44.T("clientes.emailInvalido"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus(); return;
            }
            if (!string.IsNullOrEmpty(tel) && !Validaciones_GO44.EsTelefonoValido(tel))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_TELEFONO, IdiomaManager_GO44.T("clientes.telefonoInvalido"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelefono.Focus(); return;
            }

            BLLCliente_GO44.ResultadoRegistroCliente r = _bll.RegistrarCliente(dni, ape, nom, mail, tel);

            switch (r)
            {
                case BLLCliente_GO44.ResultadoRegistroCliente.Exitoso:
                    MessageBox.Show(IdiomaManager_GO44.T("clientes.registradoOk"),
                        IdiomaManager_GO44.T("clientes.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ModoConsulta();
                    CargarGrilla();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.DNIVacio:
                    MessageBox.Show(IdiomaManager_GO44.T("clientes.dniVacio"),
                        IdiomaManager_GO44.T("clientes.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDNI.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.DNIDuplicado:
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("clientes.dniDuplicado"), dni),
                        IdiomaManager_GO44.T("clientes.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDNI.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.DatosIncompletos:
                    MessageBox.Show(IdiomaManager_GO44.T("clientes.completarApeNom"),
                        IdiomaManager_GO44.T("clientes.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.EmailInvalido:
                    MessageBox.Show(Validaciones_GO44.MENSAJE_EMAIL, IdiomaManager_GO44.T("clientes.emailInvalido"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.EmailDuplicado:
                    MessageBox.Show(IdiomaManager_GO44.T("clientes.emailDuplicado"),
                        IdiomaManager_GO44.T("clientes.emailInvalido"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.TelefonoInvalido:
                    MessageBox.Show(Validaciones_GO44.MENSAJE_TELEFONO, IdiomaManager_GO44.T("clientes.telefonoInvalido"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTelefono.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.TelefonoDuplicado:
                    MessageBox.Show(IdiomaManager_GO44.T("clientes.telefonoDuplicado"),
                        IdiomaManager_GO44.T("clientes.telefonoInvalido"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTelefono.Focus();
                    break;
                default:
                    string detalle = _bll.UltimoErrorMensaje;
                    string extra = string.IsNullOrEmpty(detalle) ? "" : "\n\n" + detalle;
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("clientes.noSeRegistro"), extra),
                        IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void Modificar()
        {
            string mail = txtEmail.Text.Trim();
            if (!Validaciones_GO44.EsEmailValido(mail))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_EMAIL, IdiomaManager_GO44.T("clientes.emailInvalido"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus(); return;
            }

            // Chequeo previo — email duplicado en otro cliente
            if (_bll.ExisteEmail(mail, dniExcluir: _clienteSeleccionado.DNI))
            {
                MessageBox.Show(IdiomaManager_GO44.T("clientes.emailOtroDuplicado"),
                    IdiomaManager_GO44.T("clientes.emailInvalido"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            bool ok = _bll.ModificarEmail(_clienteSeleccionado.DNI, mail);
            if (ok)
            {
                MessageBox.Show(IdiomaManager_GO44.T("clientes.modificadoOk"),
                    IdiomaManager_GO44.T("clientes.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show(IdiomaManager_GO44.T("clientes.noSeModifico"),
                    IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActivarDesactivar()
        {
            bool nuevoEstado = !_clienteSeleccionado.Activo;
            bool ok = _bll.ActivarDesactivar(_clienteSeleccionado.DNI, nuevoEstado);

            if (ok)
            {
                MessageBox.Show(IdiomaManager_GO44.T("clientes.modificadoOk"),
                    IdiomaManager_GO44.T("clientes.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show(IdiomaManager_GO44.T("clientes.noSePudoCambiarEstado"),
                    IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            ModoConsulta();
            CargarGrilla();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ============ SERIALIZACIÓN XML / UTILIDADES DE GRILLA ============

        private void btnSerializar_Click(object sender, EventArgs e)
        {
            // Serializa a XML lo que SE VE en la grilla (no lo que hay en la base).
            List<BE_Cliente_GO44> lista = ClientesEnGrilla();
            if (lista.Count == 0)
            {
                MessageBox.Show(IdiomaManager_GO44.T("clientes.sinExportar"),
                    IdiomaManager_GO44.T("clientes.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "XML (*.xml)|*.xml";
                sfd.FileName = "Clientes_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml";
                sfd.Title = IdiomaManager_GO44.T("clientes.btnSerializar");
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    string origen = Environment.MachineName + " - " +
                        (SessionManager_GO44.Instancia.ObtenerUsuarioActual()?.Login ?? "desconocido");
                    SerializadorClientes_GO44.ExportarAXml(lista, sfd.FileName, origen);
                    MessageBox.Show(sfd.FileName,
                        IdiomaManager_GO44.T("clientes.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("clientes.errExportar"), ex.Message),
                        IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Clientes que se ven actualmente en la grilla (filas visibles, vía DataBoundItem).</summary>
        private List<BE_Cliente_GO44> ClientesEnGrilla()
        {
            var lista = new List<BE_Cliente_GO44>();
            foreach (DataGridViewRow row in dgvClientes.Rows)
            {
                if (!row.Visible) continue;
                BE_Cliente_GO44 cli = row.DataBoundItem as BE_Cliente_GO44;
                if (cli != null) lista.Add(cli);
            }
            return lista;
        }

        private void btnDesserializar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "XML (*.xml)|*.xml";
                ofd.Title = IdiomaManager_GO44.T("clientes.btnDesserializar");
                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var export = SerializadorClientes_GO44.ImportarDeXml(ofd.FileName);
                    if (export == null || export.Clientes == null || export.Clientes.Count == 0)
                    {
                        MessageBox.Show(IdiomaManager_GO44.T("clientes.sinImportar"),
                            IdiomaManager_GO44.T("clientes.aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Mostrar en la grilla SIN guardar nada en la base.
                    // La grilla queda en "modo des-serializado" y el ABM no opera sobre estas filas.
                    _datosDeserializados = true;
                    _clienteSeleccionado = null;
                    MostrarClientesEnGrilla(export.Clientes);

                    MessageBox.Show(
                        string.Format(IdiomaManager_GO44.T("clientes.deserializadoOk"), export.Clientes.Count),
                        IdiomaManager_GO44.T("clientes.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("clientes.errImportar"), ex.Message),
                        IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLimpiarGrilla_Click(object sender, EventArgs e)
        {
            // Vacía la grilla (no toca la base). Sale del modo des-serializado.
            _datosDeserializados = false;
            dgvClientes.DataSource = null;
            _clienteSeleccionado = null;
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            // Vuelve a cargar los clientes reales desde la base.
            CargarGrilla();
        }
    }
}
