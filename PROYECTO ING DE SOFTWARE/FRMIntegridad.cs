using BLL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    public partial class FRMIntegridad : Form, IObservadorIdioma_GO44
    {
        private readonly BLLIntegridad_GO44 _bll;
        private readonly ResultadoIntegridad _resultado;

        public bool SeRestauroBackup { get; private set; }
        public bool SeRecalcularon { get; private set; }

        public FRMIntegridad(ResultadoIntegridad resultado)
            : this(resultado, puedeRecalcular: true, puedeRestaurar: true)
        {
        }

        public FRMIntegridad(ResultadoIntegridad resultado, bool puedeRecalcular, bool puedeRestaurar)
        {
            InitializeComponent();
            _bll = new BLLIntegridad_GO44();
            _resultado = resultado;

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);

            AplicarEstilos();
            CargarTablas();
            ActualizarIdioma();

            if (btnRestore != null) btnRestore.Visible = puedeRecalcular;
            if (btnBackup != null)  btnBackup.Visible  = puedeRestaurar;
        }

        private void AplicarEstilos()
        {
            Color azulOscuro    = Color.FromArgb(13, 71, 161);
            Color azulClaro     = Color.FromArgb(227, 242, 253);
            Color rojo          = Color.FromArgb(198, 40, 40);
            Color blanco        = Color.White;
            Font  fuenteBase    = new Font("Segoe UI", 10F);
            Font  fuenteTitulo  = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            Font  fuenteIcono   = new Font("Segoe UI Semibold", 32F, FontStyle.Bold);
            Font  fuenteBtn     = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

            this.BackColor = azulClaro;
            this.Font = fuenteBase;

            pnlIcono.BackColor = rojo;
            lblIcono.ForeColor = blanco;
            lblIcono.Font = fuenteIcono;

            lblTitulo.ForeColor = rojo;
            lblTitulo.Font = fuenteTitulo;
            lblMensaje.ForeColor = azulOscuro;
            lblMensaje.Font = fuenteBase;
            lblTablas.ForeColor = azulOscuro;
            lblTablas.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);

            lstTablas.BackColor = blanco;
            lstTablas.BorderStyle = BorderStyle.FixedSingle;
            lstTablas.Font = new Font("Consolas", 10F);
            lstTablas.ForeColor = azulOscuro;

            EstilarBotonPrimario(btnRestore, azulOscuro, blanco, fuenteBtn);
            EstilarBotonPrimario(btnBackup,  azulOscuro, blanco, fuenteBtn);
            EstilarBotonSecundario(btnCancelar, azulOscuro, blanco, fuenteBtn);
        }

        private static void EstilarBotonPrimario(Button btn, Color fondo, Color texto, Font fuente)
        {
            btn.BackColor = fondo;
            btn.ForeColor = texto;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = fuente;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
        }

        private static void EstilarBotonSecundario(Button btn, Color borde, Color blanco, Font fuente)
        {
            btn.BackColor = blanco;
            btn.ForeColor = borde;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = borde;
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = fuente;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
        }

        private void CargarTablas()
        {
            lstTablas.Items.Clear();

            if (_resultado.Detalles != null && _resultado.Detalles.Count > 0)
            {
                foreach (var d in _resultado.Detalles)
                {
                    string accion = TraducirTipoTampering(d.Tipo);
                    lstTablas.Items.Add($"• [{accion}] {d.Tabla} → registro {d.IdRegistro}");
                }
            }
            else
            {
                foreach (var t in _resultado.TablasComprometidas)
                    lstTablas.Items.Add("• " + t);
            }
        }

        private string TraducirTipoTampering(TipoTampering tipo)
        {
            switch (tipo)
            {
                case TipoTampering.Insertado: return IdiomaManager_GO44.T("integridad.tipoInsertado");
                case TipoTampering.Modificado: return IdiomaManager_GO44.T("integridad.tipoModificado");
                case TipoTampering.Eliminado: return IdiomaManager_GO44.T("integridad.tipoEliminado");
                default: return tipo.ToString();
            }
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("integridad.titulo");
            if (lblTitulo != null) lblTitulo.Text = IdiomaManager_GO44.T("integridad.tituloAlerta");
            if (lblMensaje != null) lblMensaje.Text = IdiomaManager_GO44.T("integridad.mensaje");
            if (lblTablas != null)
            {
                lblTablas.Text = (_resultado?.Detalles != null && _resultado.Detalles.Count > 0)
                    ? IdiomaManager_GO44.T("integridad.detallesTitulo")
                    : IdiomaManager_GO44.T("integridad.tablasAfectadas");
            }
            if (btnRestore != null) btnRestore.Text = IdiomaManager_GO44.T("integridad.botonRestore");
            if (btnBackup != null) btnBackup.Text = IdiomaManager_GO44.T("integridad.botonBackup");
            if (btnCancelar != null) btnCancelar.Text = IdiomaManager_GO44.T("general.cancelar");
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show(
                IdiomaManager_GO44.T("integridad.confirmRestore"),
                IdiomaManager_GO44.T("integridad.titulo"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            try
            {
                _bll.Recalcular();
                SeRecalcularon = true;
                MessageBox.Show(IdiomaManager_GO44.T("integridad.restoreOk"),
                                IdiomaManager_GO44.T("general.exito"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, IdiomaManager_GO44.T("general.error"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            string rutaSeleccionada;

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

                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                rutaSeleccionada = ofd.FileName;
            }

            string mensaje = IdiomaManager_GO44.T("integridad.confirmBackup") +
                             "\n\n" + IdiomaManager_GO44.T("integridad.archivoARestaurar") + "\n" + rutaSeleccionada;

            DialogResult r = MessageBox.Show(mensaje,
                                             IdiomaManager_GO44.T("integridad.titulo"),
                                             MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;

            try
            {
                _bll.RestaurarBackupDesdeRuta(rutaSeleccionada);
                SeRestauroBackup = true;
                MessageBox.Show(IdiomaManager_GO44.T("integridad.backupOk"),
                                IdiomaManager_GO44.T("general.exito"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(IdiomaManager_GO44.T("integridad.backupError") + "\n\n" + ex.Message,
                                IdiomaManager_GO44.T("general.error"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
