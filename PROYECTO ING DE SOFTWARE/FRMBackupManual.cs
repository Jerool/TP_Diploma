using BLL;
using Servicios;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    public partial class FRMBackupManual : Form, IObservadorIdioma_GO44
    {
        private readonly BLLIntegridad_GO44 _bll;

        private Label lblTitulo;
        private Label lblSubtitulo;
        private Button btnCrear;
        private Button btnRestaurar;
        private Button btnCerrar;

        public FRMBackupManual()
        {
            _bll = new BLLIntegridad_GO44();

            InicializarComponentes();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);

            AplicarPermisos();
            ActualizarIdioma();
        }

        private void InicializarComponentes()
        {
            Color azulOscuro = Color.FromArgb(13, 71, 161);
            Color azulClaro  = Color.FromArgb(227, 242, 253);
            Font  fuenteTit  = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            Font  fuenteSub  = new Font("Segoe UI", 10F);
            Font  fuenteBtn  = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

            this.Size = new Size(600, 320);
            this.BackColor = azulClaro;
            this.Font = new Font("Segoe UI", 9.5F);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            lblTitulo = new Label
            {
                ForeColor = azulOscuro, Font = fuenteTit, AutoSize = true,
                Location = new Point(30, 30)
            };

            lblSubtitulo = new Label
            {
                ForeColor = Color.DimGray, Font = fuenteSub, AutoSize = false,
                Size = new Size(540, 60),
                Location = new Point(30, 65)
            };

            btnCrear = new Button
            {
                Location = new Point(30, 150),
                Size = new Size(260, 55),
                BackColor = azulOscuro, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = fuenteBtn
            };
            btnCrear.FlatAppearance.BorderSize = 0;
            btnCrear.Click += BtnCrear_Click;

            btnRestaurar = new Button
            {
                Location = new Point(310, 150),
                Size = new Size(260, 55),
                BackColor = azulOscuro, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = fuenteBtn
            };
            btnRestaurar.FlatAppearance.BorderSize = 0;
            btnRestaurar.Click += BtnRestaurar_Click;

            btnCerrar = new Button
            {
                Location = new Point(450, 240),
                Size = new Size(120, 35),
                BackColor = Color.White, ForeColor = azulOscuro,
                FlatStyle = FlatStyle.Flat, Font = fuenteBtn
            };
            btnCerrar.FlatAppearance.BorderColor = azulOscuro;
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitulo, lblSubtitulo, btnCrear, btnRestaurar, btnCerrar });
        }

        private void AplicarPermisos()
        {
            var actual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (actual == null || actual.Rol == null) return;

            var bllPermisos = new BLLPermisos_GO44();
            var rol = bllPermisos.ObtenerArbolRol(actual.Rol.Id);
            if (rol == null) return;

            var dataKeys = rol.ObtenerPatentes()
                .Select(p => p.DataKey ?? string.Empty)
                .ToList();

            btnCrear.Visible     = dataKeys.Contains("Backup.Crear");
            btnRestaurar.Visible = dataKeys.Contains("Integridad.Restore");
        }

        public void ActualizarIdioma()
        {
            if (lblTitulo != null)    lblTitulo.Text    = IdiomaManager_GO44.T("backup.titulo");
            if (lblSubtitulo != null) lblSubtitulo.Text = IdiomaManager_GO44.T("backup.subtitulo");
            if (btnCrear != null)     btnCrear.Text     = IdiomaManager_GO44.T("backup.btnCrear");
            if (btnRestaurar != null) btnRestaurar.Text = IdiomaManager_GO44.T("backup.btnRestaurar");
            if (btnCerrar != null)    btnCerrar.Text    = IdiomaManager_GO44.T("backup.btnCerrar");
        }

        private void BtnCrear_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string ruta = _bll.HacerBackupAutomatico();
                MessageBox.Show(
                    string.Format(IdiomaManager_GO44.T("backup.crearExito"), ruta),
                    IdiomaManager_GO44.T("general.exito"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    IdiomaManager_GO44.T("general.error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void BtnRestaurar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title  = IdiomaManager_GO44.T("backup.ofdTitulo");
                ofd.Filter = IdiomaManager_GO44.T("backup.ofdFiltro");
                ofd.CheckFileExists = true;

                string ultimo = _bll.ObtenerUltimoBackup();
                if (!string.IsNullOrEmpty(ultimo))
                {
                    ofd.InitialDirectory = System.IO.Path.GetDirectoryName(ultimo);
                    ofd.FileName = System.IO.Path.GetFileName(ultimo);
                }

                if (ofd.ShowDialog() != DialogResult.OK) return;

                DialogResult r = MessageBox.Show(
                    IdiomaManager_GO44.T("backup.confirmRestaurarMensaje"),
                    IdiomaManager_GO44.T("backup.confirmRestaurarTitulo"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) return;

                this.Cursor = Cursors.WaitCursor;
                try
                {
                    _bll.RestaurarBackupDesdeRuta(ofd.FileName);
                    MessageBox.Show(
                        IdiomaManager_GO44.T("backup.restaurarExito"),
                        IdiomaManager_GO44.T("general.exito"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        IdiomaManager_GO44.T("general.error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }
    }
}
