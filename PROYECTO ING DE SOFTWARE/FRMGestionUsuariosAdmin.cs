using Servicios;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;

namespace PROYECTO_ING_DE_SOFTWARE
{

    public partial class FRMGestionUsuariosAdmin : Form, IObservadorIdioma_GO44
    {
        private readonly BLLUsuario_GO44 _bll;

        private string _modo = "Consulta";

        private Usuario_GO44 _usuarioSeleccionado = null;

        public FRMGestionUsuariosAdmin()
        {
            InitializeComponent();
            _bll = new BLLUsuario_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);

            ActualizarIdioma();
            AplicarPermisos();

        }

        private void AplicarPermisos()
        {
            Usuario_GO44 actual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (actual == null || actual.Rol == null) return;

            var bllPermisos = new BLLPermisos_GO44();
            Rol_GO44 rolCompleto = bllPermisos.ObtenerArbolRol(actual.Rol.Id);
            if (rolCompleto == null) return;

            if (btnCrear != null)
                btnCrear.Visible = rolCompleto.TienePermiso("Usuarios.Crear");
            if (btnModificar != null)
                btnModificar.Visible = rolCompleto.TienePermiso("Usuarios.Modificar");
            if (btnDesbloquear != null)
                btnDesbloquear.Visible = rolCompleto.TienePermiso("Usuarios.Desbloquear");
            if (btnActivarDesactivar != null)
                btnActivarDesactivar.Visible = rolCompleto.TienePermiso("Usuarios.Activar");
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("usuarios.titulo");

            if (label1 != null) label1.Text = IdiomaManager_GO44.T("usuarios.dni");
            if (label2 != null) label2.Text = IdiomaManager_GO44.T("usuarios.apellido");
            if (label3 != null) label3.Text = IdiomaManager_GO44.T("usuarios.nombre");
            if (label4 != null) label4.Text = IdiomaManager_GO44.T("usuarios.email");
            if (label5 != null) label5.Text = IdiomaManager_GO44.T("usuarios.rol");
            if (label6 != null) label6.Text = IdiomaManager_GO44.T("usuarios.userName");
            if (label7 != null) label7.Text = IdiomaManager_GO44.T("usuarios.bloqueado");
            if (label8 != null) label8.Text = IdiomaManager_GO44.T("usuarios.activo");

            if (btnCrear != null) btnCrear.Text = IdiomaManager_GO44.T("usuarios.crear");
            if (btnModificar != null) btnModificar.Text = IdiomaManager_GO44.T("usuarios.modificar");
            if (btnDesbloquear != null) btnDesbloquear.Text = IdiomaManager_GO44.T("usuarios.desbloquear");
            if (btnActivarDesactivar != null) btnActivarDesactivar.Text = IdiomaManager_GO44.T("usuarios.activarDesactivar");
            if (btnAplicar != null) btnAplicar.Text = IdiomaManager_GO44.T("usuarios.aplicar");
            if (btnCancelar != null) btnCancelar.Text = IdiomaManager_GO44.T("usuarios.cancelar");
            if (btnSalir != null) btnSalir.Text = IdiomaManager_GO44.T("usuarios.salir");

            if (rbActivos != null) rbActivos.Text = IdiomaManager_GO44.T("usuarios.activos");
            if (rbTodos != null) rbTodos.Text = IdiomaManager_GO44.T("usuarios.todos");

            if (lblMensaje != null) lblMensaje.Text = TraducirModo(_modo);

            ConfigurarColumnasGrilla();
        }

        private string TraducirModo(string modo)
        {
            string etiquetaModo = IdiomaManager_GO44.T("usuarios.modo");
            return etiquetaModo + " " + modo;
        }

        private void FRMPrincipalAdmin_Load(object sender, EventArgs e)
        {
            ConfigurarGrillaSoloLectura();
            CargarRoles();
            ModoConsulta();
            CargarGrilla(soloActivos: true);
            rbActivos.Checked = true;
        }

        private void ConfigurarGrillaSoloLectura()
        {
            dgvUsuarios.ReadOnly = true;
            dgvUsuarios.AllowUserToAddRows = false;
            dgvUsuarios.AllowUserToDeleteRows = false;
            dgvUsuarios.AllowUserToResizeRows = false;
            dgvUsuarios.AllowUserToResizeColumns = false;
            dgvUsuarios.AllowUserToOrderColumns = false;
            dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsuarios.MultiSelect = false;
            dgvUsuarios.RowHeadersVisible = false;
            dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvUsuarios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvUsuarios.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        }

        private void CargarRoles()
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.DataSource = _bll.ListarRoles();
            comboBox1.DisplayMember = "Nombre";
            comboBox1.ValueMember = "Id";
        }

        private void CargarGrilla(bool soloActivos)
        {
            List<Usuario_GO44> lista = soloActivos ? _bll.ListarActivos() : _bll.ListarTodos();
            dgvUsuarios.DataSource = null;
            dgvUsuarios.DataSource = lista;

            ConfigurarColumnasGrilla();

            foreach (DataGridViewRow row in dgvUsuarios.Rows)
            {
                if (!(bool)row.Cells["Activo"].Value)
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
            }
        }

        private void ConfigurarColumnasGrilla()
        {
            if (dgvUsuarios.Columns.Count == 0) return;

            string[] aOcultar = {
                "Contrasena", "IntentosFallidos", "UltimoIntentoFallido",
                "Rol", "DebeCambiarContrasena", "Idioma"
            };
            foreach (string col in aOcultar)
                if (dgvUsuarios.Columns.Contains(col))
                    dgvUsuarios.Columns[col].Visible = false;

            if (dgvUsuarios.Columns.Contains("DNI"))
                dgvUsuarios.Columns["DNI"].HeaderText = IdiomaManager_GO44.T("usuarios.dni");
            if (dgvUsuarios.Columns.Contains("Apellido"))
                dgvUsuarios.Columns["Apellido"].HeaderText = IdiomaManager_GO44.T("usuarios.apellido");
            if (dgvUsuarios.Columns.Contains("Nombre"))
                dgvUsuarios.Columns["Nombre"].HeaderText = IdiomaManager_GO44.T("usuarios.nombre");
            if (dgvUsuarios.Columns.Contains("Login"))
                dgvUsuarios.Columns["Login"].HeaderText = IdiomaManager_GO44.T("usuarios.login");
            if (dgvUsuarios.Columns.Contains("RolNombre"))
                dgvUsuarios.Columns["RolNombre"].HeaderText = IdiomaManager_GO44.T("usuarios.rol");
            if (dgvUsuarios.Columns.Contains("Email"))
                dgvUsuarios.Columns["Email"].HeaderText = IdiomaManager_GO44.T("usuarios.email");
            if (dgvUsuarios.Columns.Contains("Bloqueo"))
                dgvUsuarios.Columns["Bloqueo"].HeaderText = IdiomaManager_GO44.T("usuarios.bloqueado");
            if (dgvUsuarios.Columns.Contains("Activo"))
                dgvUsuarios.Columns["Activo"].HeaderText = IdiomaManager_GO44.T("usuarios.activo");
        }

        private void ModoConsulta()
        {
            _modo = "Consulta";
            lblMensaje.Text = TraducirModo(_modo);
            LimpiarCampos();
            HabilitarCampos(false);
            btnCrear.Enabled = true;
            btnModificar.Enabled = true;
            btnDesbloquear.Enabled = true;
            btnActivarDesactivar.Enabled = true;
            btnAplicar.Enabled = false;
            btnCancelar.Enabled = false;
            rbActivos.Enabled = true;
            rbTodos.Enabled = true;
            dgvUsuarios.Enabled = true;
        }

        private void ModoOperacion(string modo)
        {
            _modo = modo;
            lblMensaje.Text = TraducirModo(modo);
            btnCrear.Enabled = false;
            btnModificar.Enabled = false;
            btnDesbloquear.Enabled = false;
            btnActivarDesactivar.Enabled = false;
            btnAplicar.Enabled = true;
            btnCancelar.Enabled = true;
            rbActivos.Enabled = false;
            rbTodos.Enabled = false;
        }

        private void HabilitarCampos(bool habilitar)
        {
            txtDni.Enabled = habilitar;
            txtApellido.Enabled = habilitar;
            txtNombre.Enabled = habilitar;
            txtEmail.Enabled = habilitar;
            comboBox1.Enabled = habilitar;
            txtUser.Enabled = false;
            txtBloqueado.Enabled = false;
            txtActivo.Enabled = false;
        }

        private void LimpiarCampos()
        {
            txtDni.Text = txtApellido.Text = txtNombre.Text =
            txtEmail.Text = txtUser.Text =
            txtBloqueado.Text = txtActivo.Text = "";
            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
            HabilitarCampos(true);
            dgvUsuarios.Enabled = false;
            ModoOperacion("Crear");
        }

        private void btnAplicar_Click(object sender, EventArgs e)
        {
            switch (_modo)
            {
                case "Crear":
                    Crear();
                    break;
                case "Modificar":
                    Modificar();
                    break;
                case "Desbloquear":
                    Desbloquear();
                    break;
                case "ActivarDesactivar":
                    ActivarDesactivar();
                    break;
            }
        }

        private void ActivarDesactivar()
        {
            bool nuevoEstado = !_usuarioSeleccionado.Activo;

            Usuario_GO44 actual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (actual != null && _usuarioSeleccionado.Login == actual.Login)
            {
                MessageBox.Show(IdiomaManager_GO44.T("usuarios.noAutoDesactivar"),
                                IdiomaManager_GO44.T("general.accionNoPermitida"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ModoConsulta();
                CargarGrilla(rbActivos.Checked);
                return;
            }

            _bll.ActivarDesactivar(_usuarioSeleccionado.DNI, nuevoEstado);
            string claveMsj = nuevoEstado ? "usuarios.usuarioActivado" : "usuarios.usuarioDesactivado";
            MessageBox.Show(string.Format(IdiomaManager_GO44.T(claveMsj), _usuarioSeleccionado.Login),
                            IdiomaManager_GO44.T("general.exito"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            ModoConsulta();
            CargarGrilla(rbActivos.Checked);
        }

        private void Desbloquear()
        {
            if (_usuarioSeleccionado.Bloqueo == false)
            {
                MessageBox.Show(IdiomaManager_GO44.T("usuarios.usuarioYaDesbloqueado"),
                                IdiomaManager_GO44.T("general.error"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _bll.Desbloquear(_usuarioSeleccionado.DNI, _usuarioSeleccionado.Login);
            MessageBox.Show(string.Format(IdiomaManager_GO44.T("usuarios.usuarioDesbloqueado"), _usuarioSeleccionado.Login),
                            IdiomaManager_GO44.T("general.exito"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            ModoConsulta();
            CargarGrilla(rbActivos.Checked);
        }

        private void Modificar()
        {
            string email = txtEmail.Text.Trim();
            Rol_GO44 rol = comboBox1.SelectedItem as Rol_GO44;

            if (!Validaciones_GO44.EsEmailValido(email))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_EMAIL,
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }
            if (rol == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("usuarios.rolVacio"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (email != _usuarioSeleccionado.Email)
                _bll.ModificarEmail(_usuarioSeleccionado.DNI, email);

            if (_usuarioSeleccionado.Rol == null || rol.Id != _usuarioSeleccionado.Rol.Id)
                _bll.ModificarRol(_usuarioSeleccionado.DNI, rol);

            MessageBox.Show(IdiomaManager_GO44.T("usuarios.confirmarModificado"),
                            IdiomaManager_GO44.T("general.exito"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            ModoConsulta();
            CargarGrilla(rbActivos.Checked);
        }

        private void Crear()
        {
            string dni = txtDni.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string nombre = txtNombre.Text.Trim();
            string email = txtEmail.Text.Trim();
            Rol_GO44 rol = comboBox1.SelectedItem as Rol_GO44;

            if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(apellido) ||string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email) ||rol == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("general.completarCampos"),IdiomaManager_GO44.T("general.advertencia"),MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Validaciones_GO44.EsDniValido(dni))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_DNI,
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDni.Focus();
                return;
            }

            if (!Validaciones_GO44.EsApellidoValido(apellido))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_APELLIDO,IdiomaManager_GO44.T("general.advertencia"),MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtApellido.Focus();
                return;
            }
            if (!Validaciones_GO44.EsNombreValido(nombre))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_NOMBRE,
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (!Validaciones_GO44.EsEmailValido(email))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_EMAIL,
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            if (_bll.ExisteDNI(dni))
            {
                MessageBox.Show($"{IdiomaManager_GO44.T("usuarios.dniDuplicado")} '{dni}'.",
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDni.Focus();
                return;
            }

            try
            {
                _bll.CrearUsuario(dni, apellido, nombre, email, rol);

                MessageBox.Show(IdiomaManager_GO44.T("usuarios.confirmarCreado"),
                                IdiomaManager_GO44.T("general.exito"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModoConsulta();
                CargarGrilla(rbActivos.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show(IdiomaManager_GO44.T("usuarios.errorCrear") + "\n\n" + ex.Message,
                                IdiomaManager_GO44.T("general.error"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null) return;

            _usuarioSeleccionado = dgvUsuarios.CurrentRow.DataBoundItem as Usuario_GO44;
            if (_usuarioSeleccionado == null) return;

            txtDni.Text = _usuarioSeleccionado.DNI;
            txtApellido.Text = _usuarioSeleccionado.Apellido;
            txtNombre.Text = _usuarioSeleccionado.Nombre;
            txtEmail.Text = _usuarioSeleccionado.Email;
            SeleccionarRolEnCombo(_usuarioSeleccionado.Rol);
            txtUser.Text = _usuarioSeleccionado.Login;
            txtBloqueado.Text = _usuarioSeleccionado.Bloqueo ? "Sí" : "No";
            txtActivo.Text = _usuarioSeleccionado.Activo ? "Sí" : "No";
        }

        private void SeleccionarRolEnCombo(Rol_GO44 rol)
        {
            if (rol == null) { comboBox1.SelectedIndex = -1; return; }

            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                Rol_GO44 r = comboBox1.Items[i] as Rol_GO44;
                if (r != null && r.Id == rol.Id)
                {
                    comboBox1.SelectedIndex = i;
                    return;
                }
            }
            comboBox1.SelectedIndex = -1;
        }

        private void rbActivos_CheckedChanged(object sender, EventArgs e)
        {
            if (rbActivos.Checked) CargarGrilla(soloActivos: true);
        }

        private void rbTodos_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTodos.Checked) CargarGrilla(soloActivos: false);
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            ModoConsulta();
            CargarGrilla(rbActivos.Checked);
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDesbloquear_Click(object sender, EventArgs e)
        {
            if (_usuarioSeleccionado == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("usuarios.seleccionarUsuario"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            HabilitarCampos(false);
            dgvUsuarios.Enabled = false;
            ModoOperacion("Desbloquear");
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (_usuarioSeleccionado == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("usuarios.seleccionarUsuario"),IdiomaManager_GO44.T("general.advertencia"),MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            HabilitarCampos(false);
            txtEmail.Enabled = true;
            comboBox1.Enabled = true;
            dgvUsuarios.Enabled = false;
            ModoOperacion("Modificar");
        }

        private void btnActivarDesactivar_Click(object sender, EventArgs e)
        {
            if (_usuarioSeleccionado == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("usuarios.seleccionarUsuario"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            HabilitarCampos(false);
            dgvUsuarios.Enabled = false;
            ModoOperacion("ActivarDesactivar");
        }
    }
}
