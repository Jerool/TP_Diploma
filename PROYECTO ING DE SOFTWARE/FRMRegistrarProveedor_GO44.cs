using BE;
using BLL;
using Servicios;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// CU07 "Registrar proveedor" — modal invocado desde FRMGenerarSolicitudCotizacion.
    /// Al cerrar con OK, expone el proveedor registrado en ProveedorRegistrado para
    /// que la pantalla padre lo pueda seleccionar automáticamente en el combo.
    /// </summary>
    public partial class FRMRegistrarProveedor_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly BLLProveedor_GO44 _bll;

        /// <summary>Proveedor registrado (null si se canceló).</summary>
        public BE_Proveedor_GO44 ProveedorRegistrado { get; private set; }

        public FRMRegistrarProveedor_GO44()
        {
            InitializeComponent();
            _bll = new BLLProveedor_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        private void FRMRegistrarProveedor_GO44_Load(object sender, EventArgs e)
        {
            // Máscaras: solo dígitos + MaxLength
            txtCUIT.MaxLength     = 11; txtCUIT.KeyPress     += SoloDigitos;
            txtTelefono.MaxLength = 10; txtTelefono.KeyPress += SoloDigitos;
            txtCVU.MaxLength      = 22; txtCVU.KeyPress      += SoloDigitos;

            txtNombre.KeyPress   += SoloLetras;
            txtApellido.KeyPress += SoloLetras;
            txtTitular.KeyPress  += SoloLetras;

            ActualizarIdioma();
        }

        /// <summary>Aplica los textos del idioma activo a títulos, etiquetas y botones.</summary>
        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("prov.titulo");
            if (lblTitulo != null)   lblTitulo.Text   = IdiomaManager_GO44.T("prov.titulo");
            if (lblCUIT != null)     lblCUIT.Text      = IdiomaManager_GO44.T("prov.cuit");
            if (lblNombre != null)   lblNombre.Text    = IdiomaManager_GO44.T("prov.nombre");
            if (lblApellido != null) lblApellido.Text  = IdiomaManager_GO44.T("prov.apellido");
            if (lblEmpresa != null)  lblEmpresa.Text   = IdiomaManager_GO44.T("prov.empresa");
            if (lblTelefono != null) lblTelefono.Text  = IdiomaManager_GO44.T("prov.telefono");
            if (lblCVU != null)      lblCVU.Text        = IdiomaManager_GO44.T("prov.cvu");
            if (lblBanco != null)    lblBanco.Text     = IdiomaManager_GO44.T("prov.banco");
            if (lblTitular != null)  lblTitular.Text   = IdiomaManager_GO44.T("prov.titular");
            if (btnGuardar != null)  btnGuardar.Text   = IdiomaManager_GO44.T("prov.btnGuardar");
            if (btnCancelar != null) btnCancelar.Text  = IdiomaManager_GO44.T("prov.btnCancelar");
        }

        private void SoloDigitos(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private void SoloLetras(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ') e.Handled = true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            var prov = new BE_Proveedor_GO44
            {
                CUIT     = txtCUIT.Text.Trim(),
                Nombre   = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Empresa  = txtEmpresa.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                CVU      = txtCVU.Text.Trim(),
                Banco    = txtBanco.Text.Trim(),
                Titular  = txtTitular.Text.Trim()
            };

            var vo = _bll.Registrar(prov);
            switch (vo.Resultado)
            {
                case BLLProveedor_GO44.ResultadoRegistro.Exitoso:
                    MessageBox.Show(vo.Mensaje, IdiomaManager_GO44.T("general.exito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ProveedorRegistrado = vo.Proveedor;
                    if (this.Modal)
                    {
                        // Invocado como modal desde la Solicitud de Cotización («extend»):
                        // cierra y devuelve el proveedor para seleccionarlo en el combo padre.
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        // Embebido en el menú (Maestros → Proveedores): limpia para cargar otro.
                        LimpiarCampos();
                        txtCUIT.Focus();
                    }
                    break;
                case BLLProveedor_GO44.ResultadoRegistro.CuitInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_CUIT, txtCUIT); break;
                case BLLProveedor_GO44.ResultadoRegistro.CuitDuplicado:
                    Aviso(string.Format(IdiomaManager_GO44.T("prov.cuitDuplicado"), prov.CUIT), txtCUIT); break;
                case BLLProveedor_GO44.ResultadoRegistro.NombreInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_NOMBRE, txtNombre); break;
                case BLLProveedor_GO44.ResultadoRegistro.ApellidoInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_APELLIDO, txtApellido); break;
                case BLLProveedor_GO44.ResultadoRegistro.EmpresaVacia:
                    Aviso(IdiomaManager_GO44.T("prov.empresaObligatoria"), txtEmpresa); break;
                case BLLProveedor_GO44.ResultadoRegistro.TelefonoInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_TELEFONO, txtTelefono); break;
                case BLLProveedor_GO44.ResultadoRegistro.CvuInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_CVU, txtCVU); break;
                case BLLProveedor_GO44.ResultadoRegistro.BancoVacio:
                    Aviso(IdiomaManager_GO44.T("prov.bancoObligatorio"), txtBanco); break;
                case BLLProveedor_GO44.ResultadoRegistro.TitularInvalido:
                    Aviso(IdiomaManager_GO44.T("prov.titularInvalido"), txtTitular); break;
                default:
                    MessageBox.Show(vo.Mensaje ?? IdiomaManager_GO44.T("prov.errorRegistrar"),
                        IdiomaManager_GO44.T("general.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void Aviso(string texto, Control foco)
        {
            MessageBox.Show(texto, IdiomaManager_GO44.T("prov.datosInvalidos"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (foco != null) foco.Focus();
        }

        private void LimpiarCampos()
        {
            txtCUIT.Clear();  txtNombre.Clear(); txtApellido.Clear(); txtEmpresa.Clear();
            txtTelefono.Clear(); txtCVU.Clear(); txtBanco.Clear(); txtTitular.Clear();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (this.Modal) DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
