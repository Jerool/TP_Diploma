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
    public partial class FRMRegistrarProveedor_GO44 : Form
    {
        private readonly BLLProveedor_GO44 _bll;

        /// <summary>Proveedor registrado (null si se canceló).</summary>
        public BE_Proveedor_GO44 ProveedorRegistrado { get; private set; }

        public FRMRegistrarProveedor_GO44()
        {
            InitializeComponent();
            _bll = new BLLProveedor_GO44();
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
                    MessageBox.Show(vo.Mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ProveedorRegistrado = vo.Proveedor;
                    DialogResult = DialogResult.OK;
                    Close();
                    break;
                case BLLProveedor_GO44.ResultadoRegistro.CuitInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_CUIT, txtCUIT); break;
                case BLLProveedor_GO44.ResultadoRegistro.CuitDuplicado:
                    Aviso("Ya existe un proveedor con el CUIT " + prov.CUIT, txtCUIT); break;
                case BLLProveedor_GO44.ResultadoRegistro.NombreInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_NOMBRE, txtNombre); break;
                case BLLProveedor_GO44.ResultadoRegistro.ApellidoInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_APELLIDO, txtApellido); break;
                case BLLProveedor_GO44.ResultadoRegistro.EmpresaVacia:
                    Aviso("La empresa es obligatoria.", txtEmpresa); break;
                case BLLProveedor_GO44.ResultadoRegistro.TelefonoInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_TELEFONO, txtTelefono); break;
                case BLLProveedor_GO44.ResultadoRegistro.CvuInvalido:
                    Aviso(Validaciones_GO44.MENSAJE_CVU, txtCVU); break;
                case BLLProveedor_GO44.ResultadoRegistro.BancoVacio:
                    Aviso("El banco es obligatorio.", txtBanco); break;
                case BLLProveedor_GO44.ResultadoRegistro.TitularInvalido:
                    Aviso("El titular solo puede contener letras y espacios.", txtTitular); break;
                default:
                    MessageBox.Show(vo.Mensaje ?? "Error al registrar el proveedor",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void Aviso(string texto, Control foco)
        {
            MessageBox.Show(texto, "Datos inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (foco != null) foco.Focus();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
