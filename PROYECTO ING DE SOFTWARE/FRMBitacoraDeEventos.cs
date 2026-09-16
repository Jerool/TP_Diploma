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

namespace PROYECTO_ING_DE_SOFTWARE
{
    public partial class FRMBitacoraDeEventos : Form, IObservadorIdioma_GO44
    {
        private readonly BLLBitacora_GO44 _bllBitacora;

        private readonly BLLUsuario_GO44 _bllUsuario;

        private const string SIN_FILTRO = "(Todos)";

        public FRMBitacoraDeEventos()
        {
            InitializeComponent();
            _bllBitacora = BLLBitacora_GO44.Instancia;
            _bllUsuario = new BLLUsuario_GO44();

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

            bool puedeVer = rolCompleto.TienePermiso("Bitacora.Ver");
            bool puedeExportar = rolCompleto.TienePermiso("Bitacora.ExportarPDF");

            if (lblLogin != null) lblLogin.Visible = puedeVer;
            if (txtLogin != null) txtLogin.Visible = puedeVer;
            if (lblModulo != null) lblModulo.Visible = puedeVer;
            if (cboModulo != null) cboModulo.Visible = puedeVer;
            if (lblEvento != null) lblEvento.Visible = puedeVer;
            if (cboEvento != null) cboEvento.Visible = puedeVer;
            if (lblCriticidad != null) lblCriticidad.Visible = puedeVer;
            if (cboCriticidad != null) cboCriticidad.Visible = puedeVer;
            if (lblFechaInicio != null) lblFechaInicio.Visible = puedeVer;
            if (dtpFechaInicio != null) dtpFechaInicio.Visible = puedeVer;
            if (lblFechaFin != null) lblFechaFin.Visible = puedeVer;
            if (dtpFechaFin != null) dtpFechaFin.Visible = puedeVer;
            if (btnAplicar != null) btnAplicar.Visible = puedeVer;
            if (btnLimpiar != null) btnLimpiar.Visible = puedeVer;

            if (lblNombre != null) lblNombre.Visible = puedeVer;
            if (txtNombreUsuario != null) txtNombreUsuario.Visible = puedeVer;
            if (lblApellido != null) lblApellido.Visible = puedeVer;
            if (txtApellidoUsuario != null) txtApellidoUsuario.Visible = puedeVer;

            if (btnImprimir != null) btnImprimir.Visible = puedeExportar;

        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("bitacora.titulo");

            if (lblLogin != null) lblLogin.Text = IdiomaManager_GO44.T("bitacora.usuario");
            if (lblModulo != null) lblModulo.Text = IdiomaManager_GO44.T("bitacora.modulo");
            if (lblEvento != null) lblEvento.Text = IdiomaManager_GO44.T("bitacora.evento");
            if (lblFechaInicio != null) lblFechaInicio.Text = IdiomaManager_GO44.T("bitacora.fechaInicio");
            if (lblFechaFin != null) lblFechaFin.Text = IdiomaManager_GO44.T("bitacora.fechaFin");
            if (lblCriticidad != null) lblCriticidad.Text = IdiomaManager_GO44.T("bitacora.criticidad");
            if (lblNombre != null) lblNombre.Text = IdiomaManager_GO44.T("bitacora.nombre");
            if (lblApellido != null) lblApellido.Text = IdiomaManager_GO44.T("bitacora.apellido");

            if (btnAplicar != null) btnAplicar.Text = IdiomaManager_GO44.T("bitacora.aplicar");
            if (btnLimpiar != null) btnLimpiar.Text = IdiomaManager_GO44.T("bitacora.limpiar");
            if (btnImprimir != null) btnImprimir.Text = IdiomaManager_GO44.T("bitacora.imprimir");
            if (btnCancelar != null) btnCancelar.Text = IdiomaManager_GO44.T("bitacora.salir");

            if (dgvBitacora != null && dgvBitacora.Columns.Count > 0)
            {
                if (dgvBitacora.Columns.Contains("Login"))
                    dgvBitacora.Columns["Login"].HeaderText = IdiomaManager_GO44.T("bitacora.usuario");
                if (dgvBitacora.Columns.Contains("ModuloNombre"))
                    dgvBitacora.Columns["ModuloNombre"].HeaderText = IdiomaManager_GO44.T("bitacora.modulo");
                if (dgvBitacora.Columns.Contains("TipoEventoNombre"))
                    dgvBitacora.Columns["TipoEventoNombre"].HeaderText = IdiomaManager_GO44.T("bitacora.tipoEvento");
                if (dgvBitacora.Columns.Contains("Detalle"))
                    dgvBitacora.Columns["Detalle"].HeaderText = IdiomaManager_GO44.T("bitacora.detalle");
                if (dgvBitacora.Columns.Contains("Criticidad"))
                    dgvBitacora.Columns["Criticidad"].HeaderText = IdiomaManager_GO44.T("bitacora.criticidad");
                if (dgvBitacora.Columns.Contains("FechaHora"))
                    dgvBitacora.Columns["FechaHora"].HeaderText = IdiomaManager_GO44.T("bitacora.fechaHora");
            }
        }

        private void FRMBitacoraDeEventos_Load(object sender, EventArgs e)
        {
            ConfigurarGrillaSoloLectura();
            CargarCombos();
            EstablecerFechasPorDefecto();
            CargarGrillaPorDefecto();
        }

        private void ConfigurarGrillaSoloLectura()
        {
            dgvBitacora.ReadOnly = true;
            dgvBitacora.AllowUserToAddRows = false;
            dgvBitacora.AllowUserToDeleteRows = false;
            dgvBitacora.AllowUserToResizeRows = false;
            dgvBitacora.AllowUserToResizeColumns = false;
            dgvBitacora.AllowUserToOrderColumns = false;
            dgvBitacora.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBitacora.MultiSelect = false;
            dgvBitacora.RowHeadersVisible = false;
            dgvBitacora.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBitacora.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvBitacora.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        }

        private void EstablecerFechasPorDefecto()
        {

            DateTime hoy = DateTime.Now.Date;
            DateTime hace3Dias = hoy.AddDays(-3);

            dtpFechaInicio.MinDate = hace3Dias;
            dtpFechaInicio.MaxDate = hoy;
            dtpFechaInicio.Value = hace3Dias;
            dtpFechaFin.MinDate = hace3Dias;
            dtpFechaFin.MaxDate = hoy;
            dtpFechaFin.Value = hoy;
        }

        private void CargarCombos()
        {
            List<string> modulos = new List<string> { SIN_FILTRO };
            modulos.AddRange(_bllBitacora.ListarModulos());
            cboModulo.DataSource = modulos;

            List<string> eventos = new List<string> { SIN_FILTRO };
            eventos.AddRange(_bllBitacora.ListarTiposEvento());
            cboEvento.DataSource = eventos;

            List<string> criticidades = new List<string> { SIN_FILTRO };
            criticidades.AddRange(_bllBitacora.ListarCriticidades());
            cboCriticidad.DataSource = criticidades;
        }

        private void CargarGrillaPorDefecto()
        {
            DateTime fechaFinReal = dtpFechaFin.Value.Date.AddDays(1).AddSeconds(-1);
            CargarGrilla(_bllBitacora.Filtrar(
                login: null,
                modulo: null,
                tipoEvento: null,
                criticidad: null,
                fechaInicio: dtpFechaInicio.Value.Date,
                fechaFin: fechaFinReal));
        }

        private void CargarGrilla(List<Bitacora_GO44> registros)
        {
            dgvBitacora.DataSource = null;
            dgvBitacora.DataSource = registros;
            ConfigurarColumnas();
            if (dgvBitacora.Rows.Count > 0)
            {
                dgvBitacora.ClearSelection();
                dgvBitacora.Rows[0].Selected = true;
                dgvBitacora.CurrentCell = dgvBitacora.Rows[0].Cells[0];
            }
            else
            {
                txtNombreUsuario.Text = "";
                txtApellidoUsuario.Text = "";
            }
        }
        private void ConfigurarColumnas()
        {
            if (dgvBitacora.Columns.Count == 0) return;
            string[] aOcultar = { "Modulo", "TipoEvento" };
            foreach (string c in aOcultar)
            if (dgvBitacora.Columns.Contains(c)) dgvBitacora.Columns[c].Visible = false;

            if (dgvBitacora.Columns.Contains("Login")) dgvBitacora.Columns["Login"].HeaderText = IdiomaManager_GO44.T("bitacora.usuario");
            if (dgvBitacora.Columns.Contains("ModuloNombre")) dgvBitacora.Columns["ModuloNombre"].HeaderText = IdiomaManager_GO44.T("bitacora.modulo");
            if (dgvBitacora.Columns.Contains("TipoEventoNombre")) dgvBitacora.Columns["TipoEventoNombre"].HeaderText = IdiomaManager_GO44.T("bitacora.tipoEvento");
            if (dgvBitacora.Columns.Contains("Detalle")) dgvBitacora.Columns["Detalle"].HeaderText = IdiomaManager_GO44.T("bitacora.detalle");
            if (dgvBitacora.Columns.Contains("Evento")) dgvBitacora.Columns["Evento"].Visible = false;
            if (dgvBitacora.Columns.Contains("Criticidad")) dgvBitacora.Columns["Criticidad"].HeaderText = IdiomaManager_GO44.T("bitacora.criticidad");
            if (dgvBitacora.Columns.Contains("FechaHora"))
            {
                dgvBitacora.Columns["FechaHora"].HeaderText = IdiomaManager_GO44.T("bitacora.fechaHora");
                dgvBitacora.Columns["FechaHora"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            }
        }

        private void btnAplicar_Click(object sender, EventArgs e)
        {
            if (dtpFechaFin.Value < dtpFechaInicio.Value)
            {
                MessageBox.Show(IdiomaManager_GO44.T("bitacora.fechaInvalida"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string login = txtLogin.Text.Trim();
            string modulo = ValorCombo(cboModulo);
            string evento = ValorCombo(cboEvento);
            string criticidad = ValorCombo(cboCriticidad);
            DateTime fechaFinReal = dtpFechaFin.Value.Date.AddDays(1).AddSeconds(-1);
            List<Bitacora_GO44> resultados = _bllBitacora.Filtrar(
            login, modulo, evento, criticidad,
            dtpFechaInicio.Value.Date, fechaFinReal);

            CargarGrilla(resultados);
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtLogin.Text = "";
            cboModulo.SelectedIndex = 0;
            cboEvento.SelectedIndex = 0;
            cboCriticidad.SelectedIndex = 0;
            EstablecerFechasPorDefecto();
            CargarGrillaPorDefecto();
        }
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            if (dgvBitacora.Rows.Count == 0)
            {
                MessageBox.Show(IdiomaManager_GO44.T("bitacora.sinRegistros"),
                                IdiomaManager_GO44.T("general.informacion"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = IdiomaManager_GO44.T("bitacora.sfdFiltro");
                sfd.Title = IdiomaManager_GO44.T("bitacora.sfdTitulo");
                sfd.FileName = $"Bitacora_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    string[] headers = {
                        IdiomaManager_GO44.T("bitacora.usuario"),
                        IdiomaManager_GO44.T("bitacora.modulo"),
                        IdiomaManager_GO44.T("bitacora.tipoEvento"),
                        IdiomaManager_GO44.T("bitacora.detalle"),
                        IdiomaManager_GO44.T("bitacora.criticidad"),
                        IdiomaManager_GO44.T("bitacora.fechaHora")
                    };

                    float[] proporciones = { 0.12f, 0.12f, 0.22f, 0.22f, 0.10f, 0.22f };

                    List<string[]> filas = new List<string[]>();
                    foreach (DataGridViewRow row in dgvBitacora.Rows)
                    {
                        if (row.IsNewRow) continue;
                        filas.Add(new string[]
                        {
                            ValorCelda(row, "Login"),
                            ValorCelda(row, "ModuloNombre"),
                            ValorCelda(row, "TipoEventoNombre"),
                            ValorCelda(row, "Detalle"),
                            ValorCelda(row, "Criticidad"),
                            ValorCelda(row, "FechaHora")
                        });
                    }

                    string subtitulo = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss} - " +
                                       $"Total de registros: {filas.Count}";

                    GeneradorPdf_GO44 generador = new GeneradorPdf_GO44();
                    generador.Generar(sfd.FileName, "Bitacora de Eventos", subtitulo,
                                      headers, proporciones, filas);

                    string mensaje = $"{IdiomaManager_GO44.T("bitacora.pdfGenerado")}\n{sfd.FileName}\n\n{IdiomaManager_GO44.T("bitacora.pdfAbrirAhora")}";
                    DialogResult abrir = MessageBox.Show(mensaje,
                                                         IdiomaManager_GO44.T("general.exito"),
                                                         MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (abrir == DialogResult.Yes)
                        System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(IdiomaManager_GO44.T("bitacora.errorPdf") + "\n\n" + ex.Message,
                                    IdiomaManager_GO44.T("general.error"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string ValorCelda(DataGridViewRow fila, string columna)
        {
            if (!dgvBitacora.Columns.Contains(columna)) return "";
            object val = fila.Cells[columna].Value;
            if (val == null) return "";
            if (val is DateTime dt) return dt.ToString("dd/MM/yyyy HH:mm:ss");
            return val.ToString();
        }

        private void dgvBitacora_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBitacora.CurrentRow == null) return;
            Bitacora_GO44 registro = dgvBitacora.CurrentRow.DataBoundItem as Bitacora_GO44;
            if (registro == null) return;

            try
            {
                Usuario_GO44 usuario = _bllUsuario.BuscarPorLogin(registro.Login);
                if (usuario != null)
                {
                    txtNombreUsuario.Text = usuario.Nombre;
                    txtApellidoUsuario.Text = usuario.Apellido;
                }
                else
                {
                    txtNombreUsuario.Text = "(Usuario inexistente)";
                    txtApellidoUsuario.Text = "";
                }
            }
            catch
            {
                txtNombreUsuario.Text = "";
                txtApellidoUsuario.Text = "";
            }
        }

        private string ValorCombo(ComboBox cbo)
        {
            string seleccion = cbo.SelectedItem?.ToString();
            return seleccion == SIN_FILTRO ? null : seleccion;
        }
    }
}
