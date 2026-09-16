using BLL;
using Servicios;
using Servicios.Instalacion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    public class FRMSeleccionInstancia : Form
    {
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblInstancia;
        private ComboBox cboInstancias;
        private Button btnDetectar;
        private Button btnContinuar;
        private Button btnCancelar;
        private Label lblEstado;

        public string InstanciaElegida { get; private set; }

        public FRMSeleccionInstancia()
        {
            InicializarComponentes();
            RecargarLista();
        }

        private void InicializarComponentes()
        {
            this.Text = IdiomaManager_GO44.T("instalacion.titulo");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(560, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(227, 242, 253);

            Color azulOscuro = Color.FromArgb(13, 71, 161);
            Font  fuenteBase = new Font("Segoe UI", 9.5F);
            Font  fuenteTit  = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            Font  fuenteSub  = new Font("Segoe UI", 9F);
            Font  fuenteBtn  = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);

            lblTitulo = new Label
            {
                Text = IdiomaManager_GO44.T("instalacion.encabezado"),
                ForeColor = azulOscuro,
                Font = fuenteTit,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            lblSubtitulo = new Label
            {
                Text = IdiomaManager_GO44.T("instalacion.subtitulo"),
                ForeColor = Color.Black,
                Font = fuenteSub,
                AutoSize = true,
                Location = new Point(20, 55)
            };

            lblInstancia = new Label
            {
                Text = IdiomaManager_GO44.T("instalacion.instancia"),
                ForeColor = azulOscuro,
                Font = fuenteBase,
                AutoSize = true,
                Location = new Point(20, 100)
            };

            cboInstancias = new ComboBox
            {
                Location = new Point(20, 122),
                Size = new Size(400, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = fuenteBase
            };

            btnDetectar = new Button
            {
                Text = IdiomaManager_GO44.T("instalacion.btnDetectar"),
                Location = new Point(430, 121),
                Size = new Size(95, 30),
                BackColor = Color.White,
                ForeColor = azulOscuro,
                FlatStyle = FlatStyle.Flat,
                Font = fuenteBtn
            };
            btnDetectar.FlatAppearance.BorderColor = azulOscuro;
            btnDetectar.Click += (s, e) => RecargarLista();

            lblEstado = new Label
            {
                Text = "",
                ForeColor = Color.DarkSlateGray,
                Font = fuenteSub,
                AutoSize = false,
                Size = new Size(505, 40),
                Location = new Point(20, 170)
            };

            btnContinuar = new Button
            {
                Text = IdiomaManager_GO44.T("instalacion.btnContinuar"),
                Location = new Point(320, 235),
                Size = new Size(115, 35),
                BackColor = azulOscuro,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = fuenteBtn
            };
            btnContinuar.FlatAppearance.BorderSize = 0;
            btnContinuar.Click += BtnContinuar_Click;

            btnCancelar = new Button
            {
                Text = IdiomaManager_GO44.T("instalacion.btnCancelar"),
                Location = new Point(445, 235),
                Size = new Size(80, 35),
                BackColor = Color.White,
                ForeColor = azulOscuro,
                FlatStyle = FlatStyle.Flat,
                Font = fuenteBtn
            };
            btnCancelar.FlatAppearance.BorderColor = azulOscuro;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[]
            {
                lblTitulo, lblSubtitulo, lblInstancia,
                cboInstancias, btnDetectar, lblEstado,
                btnContinuar, btnCancelar
            });
        }

        private void RecargarLista()
        {
            lblEstado.Text = IdiomaManager_GO44.T("instalacion.detectando");
            Application.DoEvents();

            List<string> instancias = DetectorInstancias_GO44.DetectarInstancias();

            cboInstancias.Items.Clear();
            foreach (var i in instancias) cboInstancias.Items.Add(i);
            if (cboInstancias.Items.Count > 0) cboInstancias.SelectedIndex = 0;

            lblEstado.Text = string.Format(IdiomaManager_GO44.T("instalacion.detectadas"), instancias.Count);
        }

        private void BtnContinuar_Click(object sender, EventArgs e)
        {
            if (cboInstancias.SelectedItem == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("instalacion.elegirInstancia"),
                    IdiomaManager_GO44.T("general.advertencia"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string instancia = cboInstancias.SelectedItem.ToString();

            this.Cursor = Cursors.WaitCursor;
            btnContinuar.Enabled = false;
            btnCancelar.Enabled = false;
            btnDetectar.Enabled = false;

            try
            {
                lblEstado.Text = IdiomaManager_GO44.T("instalacion.verificando");
                Application.DoEvents();

                bool existe = BLLInstalador_GO44.ExisteBaseDatos(instancia);

                if (!existe)
                {
                    lblEstado.Text = IdiomaManager_GO44.T("instalacion.instalando");
                    Application.DoEvents();

                    BLLInstalador_GO44.InstalarBaseDatos(instancia);

                    MessageBox.Show(
                        IdiomaManager_GO44.T("instalacion.exitoMensaje"),
                        IdiomaManager_GO44.T("instalacion.exitoTitulo"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ConfiguracionBD_GO44.GuardarInstancia(instancia);
                InstanciaElegida = instancia;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    IdiomaManager_GO44.T("instalacion.errorMensaje") + "\n\n" + ex.Message,
                    IdiomaManager_GO44.T("general.error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = IdiomaManager_GO44.T("instalacion.error");
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnContinuar.Enabled = true;
                btnCancelar.Enabled = true;
                btnDetectar.Enabled = true;
            }
        }
    }
}
