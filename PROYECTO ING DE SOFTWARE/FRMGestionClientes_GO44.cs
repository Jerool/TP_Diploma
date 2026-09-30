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
    public partial class FRMGestionClientes_GO44 : Form
    {
        private readonly BLLCliente_GO44 _bll;
        private string _modo = "Consulta";
        private BE_Cliente_GO44 _clienteSeleccionado = null;

        public FRMGestionClientes_GO44()
        {
            InitializeComponent();
            _bll = new BLLCliente_GO44();
        }

        private void FRMGestionClientes_GO44_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            ModoConsulta();
            CargarGrilla();
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
            List<BE_Cliente_GO44> lista = _bll.Listar();
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
            lblMensaje.Text = "Modo: Consulta";
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
            lblMensaje.Text = "Modo: " + modo;
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

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
            HabilitarCampos(true);
            dgvClientes.Enabled = false;
            ModoOperacion("Nuevo");
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (_clienteSeleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (_clienteSeleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(Validaciones_GO44.MENSAJE_DNI, "DNI inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDNI.Focus(); return;
            }
            if (!Validaciones_GO44.EsApellidoValido(ape))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_APELLIDO, "Apellido inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtApellido.Focus(); return;
            }
            if (!Validaciones_GO44.EsNombreValido(nom))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_NOMBRE, "Nombre inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus(); return;
            }
            if (!Validaciones_GO44.EsEmailValido(mail))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_EMAIL, "Email inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus(); return;
            }
            if (!string.IsNullOrEmpty(tel) && !Validaciones_GO44.EsTelefonoValido(tel))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_TELEFONO, "Teléfono inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelefono.Focus(); return;
            }

            BLLCliente_GO44.ResultadoRegistroCliente r = _bll.RegistrarCliente(dni, ape, nom, mail, tel);

            switch (r)
            {
                case BLLCliente_GO44.ResultadoRegistroCliente.Exitoso:
                    MessageBox.Show("Cliente registrado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ModoConsulta();
                    CargarGrilla();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.DNIVacio:
                    MessageBox.Show("El DNI no puede estar vacío", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDNI.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.DNIDuplicado:
                    MessageBox.Show("Ya existe un cliente con DNI " + dni, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDNI.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.DatosIncompletos:
                    MessageBox.Show("Complete apellido y nombre", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.EmailInvalido:
                    MessageBox.Show(Validaciones_GO44.MENSAJE_EMAIL, "Email inválido",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.EmailDuplicado:
                    MessageBox.Show("Ya existe un cliente registrado con ese email.\n" +
                                    "Cada cliente debe tener un email único.",
                        "Email duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.TelefonoInvalido:
                    MessageBox.Show(Validaciones_GO44.MENSAJE_TELEFONO, "Teléfono inválido",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTelefono.Focus();
                    break;
                case BLLCliente_GO44.ResultadoRegistroCliente.TelefonoDuplicado:
                    MessageBox.Show("Ya existe un cliente registrado con ese teléfono.\n" +
                                    "Cada cliente debe tener un teléfono único.",
                        "Teléfono duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTelefono.Focus();
                    break;
                default:
                    string detalle = _bll.UltimoErrorMensaje;
                    MessageBox.Show("No se pudo registrar el cliente." +
                                    (string.IsNullOrEmpty(detalle) ? "" : "\n\nDetalle: " + detalle),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void Modificar()
        {
            string mail = txtEmail.Text.Trim();
            if (!Validaciones_GO44.EsEmailValido(mail))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_EMAIL, "Email inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus(); return;
            }

            // Chequeo previo — email duplicado en otro cliente
            if (_bll.ExisteEmail(mail, dniExcluir: _clienteSeleccionado.DNI))
            {
                MessageBox.Show("Ya existe otro cliente registrado con ese email.\n" +
                                "Cada cliente debe tener un email único.",
                    "Email duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            bool ok = _bll.ModificarEmail(_clienteSeleccionado.DNI, mail);
            if (ok)
            {
                MessageBox.Show("Cliente modificado", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show("No se pudo modificar (verifique el email)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActivarDesactivar()
        {
            bool nuevoEstado = !_clienteSeleccionado.Activo;
            bool ok = _bll.ActivarDesactivar(_clienteSeleccionado.DNI, nuevoEstado);

            if (ok)
            {
                string msg = nuevoEstado ? "Cliente activado" : "Cliente desactivado";
                MessageBox.Show(msg, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla();
            }
            else
            {
                MessageBox.Show("No se pudo cambiar el estado", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // ============ SERIALIZACIÓN XML ============

        private void btnExportarXml_Click(object sender, EventArgs e)
        {
            List<BE_Cliente_GO44> lista = _bll.Listar();
            if (lista == null || lista.Count == 0)
            {
                MessageBox.Show("No hay clientes para exportar", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Archivo XML (*.xml)|*.xml";
                sfd.FileName = "Clientes_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml";
                sfd.Title = "Exportar clientes a XML";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    string origen = Environment.MachineName + " - " +
                        (SessionManager_GO44.Instancia.ObtenerUsuarioActual()?.Login ?? "desconocido");
                    SerializadorClientes_GO44.ExportarAXml(lista, sfd.FileName, origen);
                    MessageBox.Show(
                        "Se exportaron " + lista.Count + " clientes correctamente.\n\nArchivo: " + sfd.FileName,
                        "Exportación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al exportar:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportarXml_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Archivo XML (*.xml)|*.xml";
                ofd.Title = "Importar clientes desde XML";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var export = SerializadorClientes_GO44.ImportarDeXml(ofd.FileName);
                    if (export == null || export.Clientes == null || export.Clientes.Count == 0)
                    {
                        MessageBox.Show("El archivo no contiene clientes", "Aviso",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var confirm = MessageBox.Show(
                        "Se encontraron " + export.Clientes.Count + " clientes en el archivo.\n" +
                        "Origen: " + (export.Origen ?? "s/d") + "\n" +
                        "Exportado: " + export.FechaExportacion.ToString("dd/MM/yyyy HH:mm") + "\n\n" +
                        "¿Importar los que no existan en la BD?",
                        "Confirmar importación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirm != DialogResult.Yes) return;

                    int importados = 0, saltados = 0, errores = 0;
                    foreach (BE_Cliente_GO44 cli in export.Clientes)
                    {
                        if (cli == null || string.IsNullOrWhiteSpace(cli.DNI)) { saltados++; continue; }
                        if (_bll.ExisteDNI(cli.DNI)) { saltados++; continue; }

                        var r = _bll.RegistrarCliente(cli.DNI, cli.Apellido, cli.Nombre, cli.Email, cli.Telefono);
                        if (r == BLLCliente_GO44.ResultadoRegistroCliente.Exitoso) importados++;
                        else errores++;
                    }

                    MessageBox.Show(
                        "Importación finalizada:\n\n" +
                        "  ✓ Importados: " + importados + "\n" +
                        "  ↷ Ya existían (saltados): " + saltados + "\n" +
                        "  ✗ Errores: " + errores,
                        "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarGrilla();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al importar:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
