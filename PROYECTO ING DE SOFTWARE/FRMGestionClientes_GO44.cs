using BE;
using BLL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Drawing;
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
                    MessageBox.Show("El email no tiene un formato válido", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    break;
                default:
                    MessageBox.Show("No se pudo registrar el cliente", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void Modificar()
        {
            string mail = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(mail))
            {
                MessageBox.Show("Ingrese un email", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
    }
}
