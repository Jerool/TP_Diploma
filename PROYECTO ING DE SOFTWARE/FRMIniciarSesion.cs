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

    public partial class FRMIniciarSesion : Form, IObservadorIdioma_GO44
    {
        private readonly BLLUsuario_GO44 _bllUsuario;
        private ResultadoIntegridad _resultadoIntegridadPrelogin;

        public FRMIniciarSesion()
        {
            InitializeComponent();
            _bllUsuario = new BLLUsuario_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);

            ActualizarIdioma();
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("login.titulo");
            if (lblTitulo != null) lblTitulo.Text = IdiomaManager_GO44.T("login.titulo");
            if (lblSubtitulo != null) lblSubtitulo.Text = IdiomaManager_GO44.T("login.subtitulo");
            if (label1 != null) label1.Text = IdiomaManager_GO44.T("login.login");
            if (label2 != null) label2.Text = IdiomaManager_GO44.T("login.contrasena");
            if (btnIngresar != null) btnIngresar.Text = IdiomaManager_GO44.T("login.btnIngresar");
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            string login = txtLogIn.Text.Trim();
            string contrasena = txtContrasena.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(contrasena))
            {
                MessageBox.Show(IdiomaManager_GO44.T("general.completarCampos"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Validaciones_GO44.EsLoginValido(login))
            {
                MessageBox.Show(Validaciones_GO44.MENSAJE_LOGIN,
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLogIn.Focus();
                return;
            }

            try
            {
                _resultadoIntegridadPrelogin = new BLLIntegridad_GO44().Verificar();
            }
            catch
            {
                _resultadoIntegridadPrelogin = null;
            }

            ResultadoLogin resultado = _bllUsuario.IntentarLogin(login, contrasena);

            switch (resultado)
            {
                case ResultadoLogin.Exitoso:
                    AbrirFormularioSegunRol();
                    break;
                case ResultadoLogin.UsuarioBloqueado:
                case ResultadoLogin.BloqueadoPorIntentos:
                    MessageBox.Show(IdiomaManager_GO44.T("login.bloqueado"),
                                    IdiomaManager_GO44.T("general.accesoDenegado"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case ResultadoLogin.UsuarioInactivo:
                    MessageBox.Show(IdiomaManager_GO44.T("login.inactivo"),
                                    IdiomaManager_GO44.T("general.accesoDenegado"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case ResultadoLogin.ContrasenaIncorrecta:
                    MessageBox.Show(IdiomaManager_GO44.T("login.contrasenaIncorrecta"),
                                    IdiomaManager_GO44.T("general.error"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ResultadoLogin.UsuarioInexistente:
                    MessageBox.Show(IdiomaManager_GO44.T("login.usuarioInexistente"),
                                    IdiomaManager_GO44.T("general.error"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case ResultadoLogin.SesionActiva:
                    MessageBox.Show(IdiomaManager_GO44.T("login.sesionActiva"),
                                    IdiomaManager_GO44.T("general.advertencia"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case ResultadoLogin.Error:
                    MessageBox.Show(IdiomaManager_GO44.T("login.errorUsuario"),
                                    IdiomaManager_GO44.T("general.error"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void AbrirFormularioSegunRol()
        {
            Usuario_GO44 actual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();

            if (!VerificarIntegridad(actual))
            {
                BLLUsuario_GO44.CerrarSesión();
                return;
            }

            if (actual != null && actual.DebeCambiarContrasena)
            {
                MessageBox.Show(IdiomaManager_GO44.T("login.cambioRequeridoMensaje"),
                                IdiomaManager_GO44.T("login.cambioRequeridoTitulo"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                FRMCambiarContrasenia cambio = new FRMCambiarContrasenia(primerLogin: true);
                cambio.Show();
                this.Hide();
                return;
            }

            Form formulario = new FRMMenuPrincipalAdmin();
            formulario.Show();
            this.Hide();
        }

        private bool VerificarIntegridad(Usuario_GO44 actual)
        {
            try
            {
                var bllInt = new BLLIntegridad_GO44();
                ResultadoIntegridad res = _resultadoIntegridadPrelogin ?? bllInt.Verificar();

                if (res.EsIntegra)
                {
                    try { bllInt.IniciarBackupsProgramados(); } catch { }
                    return true;
                }

                string detalleBitacora;
                if (res.Detalles != null && res.Detalles.Count > 0)
                {
                    detalleBitacora = string.Join(" | ",
                        res.Detalles.Select(d => $"[{d.Tipo}] {d.Tabla}#{d.IdRegistro}"));
                }
                else
                {
                    detalleBitacora = string.Join(", ", res.TablasComprometidas);
                }

                BLLBitacora_GO44.Instancia.RegistrarEvento(
                    actual.Login, "Admin", "Integridad comprometida",
                    detalleBitacora, "Alta");

                var bllPermisos = new BLLPermisos_GO44();
                Rol_GO44 rolCompleto = bllPermisos.ObtenerArbolRol(actual.Rol.Id);
                var dataKeys = rolCompleto != null
                    ? rolCompleto.ObtenerPatentes().Select(p => p.DataKey ?? string.Empty).ToList()
                    : new List<string>();

                bool puedeRecalcular = dataKeys.Contains("Integridad.Recalcular");
                bool puedeRestaurar  = dataKeys.Contains("Integridad.Restore");

                if (!puedeRecalcular && !puedeRestaurar)
                {
                    MessageBox.Show(
                        IdiomaManager_GO44.T("integridad.sistemaInactivoMensaje"),
                        IdiomaManager_GO44.T("integridad.sistemaInactivoTitulo"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                using (var frm = new FRMIntegridad(res, puedeRecalcular, puedeRestaurar))
                {
                    DialogResult dr = frm.ShowDialog(this);

                    if (frm.SeRestauroBackup)
                    {
                        MessageBox.Show(
                            IdiomaManager_GO44.T("integridad.cerrandoAppBackup"),
                            IdiomaManager_GO44.T("integridad.titulo"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Application.Exit();
                        return false;
                    }

                    if (frm.SeRecalcularon)
                    {
                        BLLBitacora_GO44.Instancia.RegistrarEvento(
                            actual.Login, "Admin", "Integridad recalculada",
                            "Admin aceptó los cambios externos como válidos.", "Alta");
                        try { bllInt.IniciarBackupsProgramados(); } catch { }
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    IdiomaManager_GO44.T("integridad.errorVerificacion") + "\n\n" + ex.Message,
                    IdiomaManager_GO44.T("general.error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

    }
}
