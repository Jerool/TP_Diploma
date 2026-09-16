using BLL;
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
using static BLL.BLLUsuario_GO44;

namespace PROYECTO_ING_DE_SOFTWARE
{

    public partial class FRMCambiarContrasenia : Form, IObservadorIdioma_GO44
    {
        private readonly BLLUsuario_GO44 _bll;
        private readonly bool _primerLogin;

        public FRMCambiarContrasenia(bool primerLogin = false)
        {
            InitializeComponent();
            txtUsuario.Text = SessionManager_GO44.Instancia.ObtenerUsuarioActual().Login;
            _bll = new BLLUsuario_GO44();
            _primerLogin = primerLogin;

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);

            ActualizarIdioma();
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("cambiarClave.titulo");
            if (lblTitulo != null) lblTitulo.Text = IdiomaManager_GO44.T("cambiarClave.titulo");
            if (label1 != null) label1.Text = IdiomaManager_GO44.T("cambiarClave.usuario");
            if (label2 != null) label2.Text = IdiomaManager_GO44.T("cambiarClave.actual");
            if (label3 != null) label3.Text = IdiomaManager_GO44.T("cambiarClave.nueva");
            if (label4 != null) label4.Text = IdiomaManager_GO44.T("cambiarClave.confirmar");
            if (btnAceptar != null) btnAceptar.Text = IdiomaManager_GO44.T("cambiarClave.btnAceptar");
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            string login = txtUsuario.Text.Trim();
            string contrasenaActual = txtContrasenia.Text.Trim();
            string nuevaContrasena = txtNuevaconstrasenia.Text.Trim();
            string confirmar = txtConfirmarContrasenia.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(contrasenaActual) ||
                string.IsNullOrEmpty(nuevaContrasena) || string.IsNullOrEmpty(confirmar))
            {
                MessageBox.Show(IdiomaManager_GO44.T("general.completarCampos"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Validaciones_GO44.EsContrasenaValida(nuevaContrasena))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_CONTRASENA,
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNuevaconstrasenia.Focus();
                return;
            }

            ResultadoCambioContrasena resultado =
                _bll.CambiarContrasena(login, contrasenaActual, nuevaContrasena, confirmar);

            switch (resultado)
            {
                case ResultadoCambioContrasena.Exitoso:
                    MessageBox.Show(IdiomaManager_GO44.T("cambiarClave.exito"),
                                    IdiomaManager_GO44.T("general.exito"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (_primerLogin) AbrirMenuPrincipalSegunRol();
                    this.Close();
                    break;
                case ResultadoCambioContrasena.ContrasenaActualIncorrecta:
                    MessageBox.Show(IdiomaManager_GO44.T("cambiarClave.actualIncorrecta"),
                                    IdiomaManager_GO44.T("general.error"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ResultadoCambioContrasena.ContrasenasNoCoinciden:
                    MessageBox.Show(IdiomaManager_GO44.T("cambiarClave.noCoinciden"),
                                    IdiomaManager_GO44.T("general.error"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ResultadoCambioContrasena.NuevaIgualActual:
                    MessageBox.Show(IdiomaManager_GO44.T("cambiarClave.iguales"),
                                    IdiomaManager_GO44.T("general.advertencia"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNuevaconstrasenia.Focus();
                    break;
                case ResultadoCambioContrasena.UsuarioInexistente:
                    MessageBox.Show(IdiomaManager_GO44.T("cambiarClave.usuarioInexistente"),
                                    IdiomaManager_GO44.T("general.error"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void AbrirMenuPrincipalSegunRol()
        {
            Form menu = new FRMMenuPrincipalAdmin();
            menu.Show();
        }
    }
}
