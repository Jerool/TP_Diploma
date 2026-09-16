using BLL;
using Servicios;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{

    public partial class FRMGestionPermisos : Form, IObservadorIdioma_GO44
    {
        private readonly BLLPermisos_GO44 _bll;

        private string _modoFamilia = "Crear";
        private int _idFamiliaEdicion = 0;

        private string _modoRol = "Crear";
        private int _idRolEdicion = 0;

        public FRMGestionPermisos()
        {
            InitializeComponent();
            _bll = new BLLPermisos_GO44();

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);

            AplicarEstilos();
        }

        private void AplicarEstilos()
        {
            Color azulOscuro    = Color.FromArgb(13, 71, 161);
            Color azulClaro     = Color.FromArgb(227, 242, 253);
            Color blanco        = Color.White;
            Font  fuenteBase    = new Font("Segoe UI", 9F);
            Font  fuenteTitulo  = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            Font  fuenteGroup   = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            Font  fuenteBtn     = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            Font  fuenteTabs    = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

            this.BackColor = azulClaro;
            this.Font = fuenteBase;

            tabControl.Font = fuenteTabs;
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.ItemSize = new Size(140, 34);
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += TabControl_DrawItem;

            tabPatentes.BackColor = azulClaro;
            tabFamilias.BackColor = azulClaro;
            tabRoles.BackColor    = azulClaro;

            EstilarTitulo(lblTitPatentes, azulOscuro, fuenteTitulo);
            EstilarTitulo(lblTitFamilias, azulOscuro, fuenteTitulo);
            EstilarTitulo(lblTitRoles,    azulOscuro, fuenteTitulo);

            EstilarGrilla(dgvPatentes, azulOscuro, azulClaro, blanco, fuenteBase);
            EstilarGrilla(dgvFamilias, azulOscuro, azulClaro, blanco, fuenteBase);
            EstilarGrilla(dgvRoles,    azulOscuro, azulClaro, blanco, fuenteBase);

            EstilarGroupBox(gbCrearFamilia, azulOscuro, fuenteGroup);
            EstilarGroupBox(gbCrearRol,     azulOscuro, fuenteGroup);

            foreach (Label lbl in new[] { lblNombreFamilia, lblPatentesFamilia, lblSubfamilias,
                                          lblNombreRol, lblPatentesRol, lblFamiliasRol })
            {
                lbl.ForeColor = azulOscuro;
                lbl.Font = fuenteBase;
            }

            foreach (TextBox txt in new[] { txtNombreFamilia, txtNombreRol })
            {
                txt.BorderStyle = BorderStyle.FixedSingle;
                txt.Font = fuenteBase;
            }

            foreach (CheckedListBox clb in new[] { clbPatentesFamilia, clbSubfamilias,
                                                   clbPatentesRol, clbFamiliasRol })
            {
                clb.BackColor = blanco;
                clb.BorderStyle = BorderStyle.FixedSingle;
                clb.Font = fuenteBase;
            }

            EstilarBotonPrimario(btnModificarFamilia, azulOscuro, blanco, fuenteBtn);
            EstilarBotonPrimario(btnModificarRol,     azulOscuro, blanco, fuenteBtn);
            EstilarBotonPrimario(btnGuardarFamilia,   azulOscuro, blanco, fuenteBtn);
            EstilarBotonPrimario(btnGuardarRol,       azulOscuro, blanco, fuenteBtn);

            EstilarBotonSecundario(btnEliminarFamilia, azulOscuro, blanco, fuenteBtn);
            EstilarBotonSecundario(btnEliminarRol,     azulOscuro, blanco, fuenteBtn);
            EstilarBotonSecundario(btnLimpiarFamilia,  azulOscuro, blanco, fuenteBtn);
            EstilarBotonSecundario(btnLimpiarRol,      azulOscuro, blanco, fuenteBtn);
        }

        private static void EstilarTitulo(Label lbl, Color color, Font fuente)
        {
            lbl.ForeColor = color;
            lbl.Font = fuente;
        }

        private static void EstilarGroupBox(GroupBox gb, Color color, Font fuente)
        {
            gb.ForeColor = color;
            gb.Font = fuente;
        }

        private static void EstilarGrilla(DataGridView dgv, Color azulOscuro, Color azulClaro, Color blanco, Font fuente)
        {
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = blanco;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = azulOscuro;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = blanco;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = azulOscuro;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(4);
            dgv.ColumnHeadersHeight = 32;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.DefaultCellStyle.BackColor = blanco;
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.DefaultCellStyle.Font = fuente;
            dgv.DefaultCellStyle.SelectionBackColor = azulOscuro;
            dgv.DefaultCellStyle.SelectionForeColor = blanco;
            dgv.DefaultCellStyle.Padding = new Padding(2);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = azulClaro;
            dgv.EnableHeadersVisualStyles = false;
            dgv.GridColor = azulClaro;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 26;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private static void EstilarBotonPrimario(Button btn, Color azulOscuro, Color blanco, Font fuente)
        {
            btn.BackColor = azulOscuro;
            btn.ForeColor = blanco;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = fuente;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
        }

        private static void EstilarBotonSecundario(Button btn, Color azulOscuro, Color blanco, Font fuente)
        {
            btn.BackColor = blanco;
            btn.ForeColor = azulOscuro;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = azulOscuro;
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = fuente;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
        }

        private void FRMGestionPermisos_Load(object sender, EventArgs e)
        {
            AplicarPermisosTabs();
            RecargarTodo();
            ActualizarIdioma();
        }

        private void AplicarPermisosTabs()
        {
            Usuario_GO44 actual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            if (actual == null || actual.Rol == null) return;

            Rol_GO44 rolCompleto = _bll.ObtenerArbolRol(actual.Rol.Id);
            if (rolCompleto == null) return;

            var dataKeys = rolCompleto.ObtenerPatentes()
                .Select(p => p.DataKey ?? string.Empty)
                .ToList();

            bool puedePatentes = dataKeys.Contains("Permisos.Patentes");
            bool puedeFamilias = dataKeys.Contains("Permisos.Familias");
            bool puedeRoles    = dataKeys.Contains("Permisos.Roles");

            if (tabControl.TabPages.Contains(tabPatentes)) tabControl.TabPages.Remove(tabPatentes);
            if (tabControl.TabPages.Contains(tabFamilias)) tabControl.TabPages.Remove(tabFamilias);
            if (tabControl.TabPages.Contains(tabRoles))    tabControl.TabPages.Remove(tabRoles);

            if (puedePatentes) tabControl.TabPages.Add(tabPatentes);
            if (puedeFamilias) tabControl.TabPages.Add(tabFamilias);
            if (puedeRoles)    tabControl.TabPages.Add(tabRoles);
        }

        private void RecargarTodo()
        {
            CargarPatentes();
            CargarFamilias();
            CargarRoles();
        }

        private void CargarPatentes()
        {
            List<Patente_GO44> patentes = _bll.ListarPatentes();

            foreach (var p in patentes)
                p.Nombre = TraducirNombrePatente(p);

            dgvPatentes.DataSource = null;
            dgvPatentes.DataSource = patentes;
            if (dgvPatentes.Columns.Contains("Id"))
                dgvPatentes.Columns["Id"].Visible = false;
            if (dgvPatentes.Columns.Contains("DataKey"))
                dgvPatentes.Columns["DataKey"].Visible = false;
            AplicarHeadersPatentes();

            clbPatentesFamilia.Items.Clear();
            clbPatentesRol.Items.Clear();
            foreach (var p in patentes)
            {
                clbPatentesFamilia.Items.Add(p, false);
                clbPatentesRol.Items.Add(p, false);
            }
        }

        private string TraducirNombrePatente(Patente_GO44 p)
        {
            if (p == null || string.IsNullOrEmpty(p.DataKey)) return p?.Nombre ?? string.Empty;
            string clave = "patente." + p.DataKey;
            string traducido = IdiomaManager_GO44.T(clave);
            return traducido == clave ? p.Nombre : traducido;
        }

        private void AplicarHeadersPatentes()
        {
            if (dgvPatentes.Columns.Contains("Nombre"))
                dgvPatentes.Columns["Nombre"].HeaderText = IdiomaManager_GO44.T("permisos.colNombre");
            if (dgvPatentes.Columns.Contains("DataKey"))
                dgvPatentes.Columns["DataKey"].HeaderText = IdiomaManager_GO44.T("permisos.colDataKey");
        }

        private void AplicarHeadersFamilias()
        {
            if (dgvFamilias.Columns.Contains("Nombre"))
                dgvFamilias.Columns["Nombre"].HeaderText = IdiomaManager_GO44.T("permisos.colNombre");
        }

        private void AplicarHeadersRoles()
        {
            if (dgvRoles.Columns.Contains("Nombre"))
                dgvRoles.Columns["Nombre"].HeaderText = IdiomaManager_GO44.T("permisos.colNombre");
        }

        private void CargarFamilias()
        {
            List<Familia_GO44> familias = _bll.ListarFamilias();

            dgvFamilias.DataSource = null;
            dgvFamilias.DataSource = familias;
            if (dgvFamilias.Columns.Contains("Id"))
                dgvFamilias.Columns["Id"].Visible = false;
            if (dgvFamilias.Columns.Contains("Hijos"))
                dgvFamilias.Columns["Hijos"].Visible = false;
            AplicarHeadersFamilias();

            clbSubfamilias.Items.Clear();
            clbFamiliasRol.Items.Clear();
            foreach (var f in familias)
            {
                clbSubfamilias.Items.Add(f, false);
                clbFamiliasRol.Items.Add(f, false);
            }
        }

        private void CargarRoles()
        {
            List<Rol_GO44> roles = _bll.ListarRoles();
            dgvRoles.DataSource = null;
            dgvRoles.DataSource = roles;
            if (dgvRoles.Columns.Contains("Id"))
                dgvRoles.Columns["Id"].Visible = false;
            if (dgvRoles.Columns.Contains("Hijos"))
                dgvRoles.Columns["Hijos"].Visible = false;
            AplicarHeadersRoles();
        }

        // ═══════════════════════════════════════════════════════════════════
        // FAMILIAS
        // ═══════════════════════════════════════════════════════════════════

        private void btnGuardarFamilia_Click(object sender, EventArgs e)
        {
            switch (_modoFamilia)
            {
                case "Crear":     CrearFamilia();     break;
                case "Modificar": ModificarFamilia(); break;
                case "Eliminar":  EliminarFamilia();  break;
            }
        }

        private void CrearFamilia()
        {
            string nombre = txtNombreFamilia.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show(IdiomaManager_GO44.T("permisos.ingresaNombreFamilia"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombreFamilia.Focus();
                return;
            }

            List<int> idsPatentes    = clbPatentesFamilia.CheckedItems.Cast<Patente_GO44>().Select(p => p.Id).ToList();
            List<int> idsSubfamilias = clbSubfamilias.CheckedItems.Cast<Familia_GO44>().Select(f => f.Id).ToList();

            try
            {
                _bll.CrearFamilia(nombre, idsPatentes, idsSubfamilias);
                MessageBox.Show(IdiomaManager_GO44.T("permisos.familiaCreada"),
                                IdiomaManager_GO44.T("general.exito"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                VolverAModoCrearFamilia();
                RecargarTodo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, IdiomaManager_GO44.T("general.error"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ModificarFamilia()
        {
            string nombre = txtNombreFamilia.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show(IdiomaManager_GO44.T("permisos.ingresaNombreFamilia"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombreFamilia.Focus();
                return;
            }

            List<int> idsPatentes    = clbPatentesFamilia.CheckedItems.Cast<Patente_GO44>().Select(p => p.Id).ToList();
            List<int> idsSubfamilias = clbSubfamilias.CheckedItems.Cast<Familia_GO44>().Select(f => f.Id).ToList();

            try
            {
                _bll.ModificarFamilia(_idFamiliaEdicion, nombre, idsPatentes, idsSubfamilias);
                MessageBox.Show(IdiomaManager_GO44.T("permisos.familiaModificada"),
                                IdiomaManager_GO44.T("general.exito"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                VolverAModoCrearFamilia();
                RecargarTodo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, IdiomaManager_GO44.T("general.error"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EliminarFamilia()
        {
            Familia_GO44 fam = dgvFamilias.CurrentRow?.DataBoundItem as Familia_GO44;
            if (fam == null) return;

            DialogResult r = MessageBox.Show(
                $"{IdiomaManager_GO44.T("permisos.confirmEliminarFamilia")} '{fam.Nombre}'?",
                IdiomaManager_GO44.T("permisos.tituloEliminar"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) { VolverAModoCrearFamilia(); return; }

            try
            {
                _bll.EliminarFamilia(fam.Id);
                MessageBox.Show(IdiomaManager_GO44.T("permisos.familiaEliminada"),
                                IdiomaManager_GO44.T("general.exito"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                VolverAModoCrearFamilia();
                RecargarTodo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, IdiomaManager_GO44.T("permisos.noSePuedeEliminar"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnModificarFamilia_Click(object sender, EventArgs e)
        {
            if (dgvFamilias.CurrentRow == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("permisos.seleccioneFamilia"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Familia_GO44 fam = dgvFamilias.CurrentRow.DataBoundItem as Familia_GO44;
            if (fam == null) return;

            Familia_GO44 arbol = _bll.ObtenerArbolFamilia(fam.Id);
            if (arbol == null) return;

            txtNombreFamilia.Text = arbol.Nombre;

            HashSet<int> idsPat = new HashSet<int>(arbol.Hijos.OfType<Patente_GO44>().Select(p => p.Id));
            HashSet<int> idsSub = new HashSet<int>(arbol.Hijos.OfType<Familia_GO44>().Select(f => f.Id));

            for (int i = 0; i < clbPatentesFamilia.Items.Count; i++)
            {
                var p = (Patente_GO44)clbPatentesFamilia.Items[i];
                clbPatentesFamilia.SetItemChecked(i, idsPat.Contains(p.Id));
            }
            for (int i = 0; i < clbSubfamilias.Items.Count; i++)
            {
                var f = (Familia_GO44)clbSubfamilias.Items[i];
                clbSubfamilias.SetItemChecked(i, idsSub.Contains(f.Id));
            }

            EntrarAModoFamilia("Modificar", fam.Id);
        }

        private void btnEliminarFamilia_Click(object sender, EventArgs e)
        {
            if (dgvFamilias.CurrentRow == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("permisos.seleccioneFamilia"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            EntrarAModoFamilia("Eliminar", ((Familia_GO44)dgvFamilias.CurrentRow.DataBoundItem).Id);
        }

        private void btnLimpiarFamilia_Click(object sender, EventArgs e)
        {
            VolverAModoCrearFamilia();
        }

        private void EntrarAModoFamilia(string modo, int idFamilia)
        {
            _modoFamilia = modo;
            _idFamiliaEdicion = idFamilia;
            btnEliminarFamilia.Enabled = false;
            btnModificarFamilia.Enabled = false;
            dgvFamilias.Enabled = false;
            btnLimpiarFamilia.Text = IdiomaManager_GO44.T("general.cancelar");
            gbCrearFamilia.Text = modo == "Modificar"
                ? $"{IdiomaManager_GO44.T("permisos.editandoFamilia")} {txtNombreFamilia.Text}"
                : (modo == "Eliminar"
                    ? IdiomaManager_GO44.T("permisos.confirmarEliminacion")
                    : IdiomaManager_GO44.T("permisos.crearFamilia"));
        }

        private void VolverAModoCrearFamilia()
        {
            _modoFamilia = "Crear";
            _idFamiliaEdicion = 0;
            txtNombreFamilia.Clear();
            for (int i = 0; i < clbPatentesFamilia.Items.Count; i++) clbPatentesFamilia.SetItemChecked(i, false);
            for (int i = 0; i < clbSubfamilias.Items.Count; i++) clbSubfamilias.SetItemChecked(i, false);
            btnEliminarFamilia.Enabled = true;
            btnModificarFamilia.Enabled = true;
            dgvFamilias.Enabled = true;
            btnLimpiarFamilia.Text = IdiomaManager_GO44.T("permisos.limpiar");
            gbCrearFamilia.Text = IdiomaManager_GO44.T("permisos.crearFamilia");
        }



        private void btnGuardarRol_Click(object sender, EventArgs e)
        {
            switch (_modoRol)
            {
                case "Crear":     CrearRol();     break;
                case "Modificar": ModificarRol(); break;
                case "Eliminar":  EliminarRol();  break;
            }
        }

        private void CrearRol()
        {
            string nombre = txtNombreRol.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show(IdiomaManager_GO44.T("permisos.ingresaNombreRol"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombreRol.Focus();
                return;
            }

            List<int> idsPatentes = clbPatentesRol.CheckedItems.Cast<Patente_GO44>().Select(p => p.Id).ToList();
            List<int> idsFamilias = clbFamiliasRol.CheckedItems.Cast<Familia_GO44>().Select(f => f.Id).ToList();

            try
            {
                _bll.CrearRol(nombre, idsPatentes, idsFamilias);
                MessageBox.Show(IdiomaManager_GO44.T("permisos.rolCreado"),
                                IdiomaManager_GO44.T("general.exito"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                VolverAModoCrearRol();
                RecargarTodo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, IdiomaManager_GO44.T("general.error"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ModificarRol()
        {
            string nombre = txtNombreRol.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show(IdiomaManager_GO44.T("permisos.ingresaNombreRol"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombreRol.Focus();
                return;
            }

            List<int> idsPatentes = clbPatentesRol.CheckedItems.Cast<Patente_GO44>().Select(p => p.Id).ToList();
            List<int> idsFamilias = clbFamiliasRol.CheckedItems.Cast<Familia_GO44>().Select(f => f.Id).ToList();

            try
            {
                _bll.ModificarRol(_idRolEdicion, nombre, idsPatentes, idsFamilias);

                if (EsRolDelUsuarioActual(_idRolEdicion))
                {
                    ForzarReloginPorCambioDePropioRol();
                    return;
                }

                MessageBox.Show(IdiomaManager_GO44.T("permisos.rolModificado"),
                                IdiomaManager_GO44.T("general.exito"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                VolverAModoCrearRol();
                RecargarTodo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, IdiomaManager_GO44.T("general.error"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool EsRolDelUsuarioActual(int idRol)
        {
            Usuario_GO44 actual = SessionManager_GO44.Instancia.ObtenerUsuarioActual();
            return actual != null && actual.Rol != null && actual.Rol.Id == idRol;
        }

        private void ForzarReloginPorCambioDePropioRol()
        {
            MessageBox.Show(
                IdiomaManager_GO44.T("permisos.mensajeRolPropio"),
                IdiomaManager_GO44.T("permisos.tituloRolPropio"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            BLLUsuario_GO44.CerrarSesión();

            var login = new FRMIniciarSesion();
            login.Show();

            var menuPrincipal = Application.OpenForms
                .OfType<FRMMenuPrincipalAdmin>()
                .FirstOrDefault();
            if (menuPrincipal != null)
                menuPrincipal.Close();
        }

        private void EliminarRol()
        {
            Rol_GO44 rol = dgvRoles.CurrentRow?.DataBoundItem as Rol_GO44;
            if (rol == null) return;

            DialogResult r = MessageBox.Show(
                $"{IdiomaManager_GO44.T("permisos.confirmEliminarRol")} '{rol.Nombre}'?",
                IdiomaManager_GO44.T("permisos.tituloEliminar"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) { VolverAModoCrearRol(); return; }

            try
            {
                _bll.EliminarRol(rol.Id);
                MessageBox.Show(IdiomaManager_GO44.T("permisos.rolEliminado"),
                                IdiomaManager_GO44.T("general.exito"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                VolverAModoCrearRol();
                RecargarTodo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, IdiomaManager_GO44.T("permisos.noSePuedeEliminar"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

     

        private void btnEliminarRol_Click(object sender, EventArgs e)
        {
            if (dgvRoles.CurrentRow == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("permisos.seleccioneRol"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            EntrarAModoRol("Eliminar", ((Rol_GO44)dgvRoles.CurrentRow.DataBoundItem).Id);
        }

        private void btnLimpiarRol_Click(object sender, EventArgs e)
        {
            VolverAModoCrearRol();
        }


        private void EntrarAModoRol(string modo, int idRol)
        {
            _modoRol = modo;
            _idRolEdicion = idRol;
            btnEliminarRol.Enabled = false;
            btnModificarRol.Enabled = false;
            dgvRoles.Enabled = false;
            btnLimpiarRol.Text = IdiomaManager_GO44.T("general.cancelar");
            gbCrearRol.Text = modo == "Modificar"
                ? $"{IdiomaManager_GO44.T("permisos.editandoRol")} {txtNombreRol.Text}"
                : (modo == "Eliminar"
                    ? IdiomaManager_GO44.T("permisos.confirmarEliminacion")
                    : IdiomaManager_GO44.T("permisos.crearRol"));
        }

        private void VolverAModoCrearRol()
        {
            _modoRol = "Crear";
            _idRolEdicion = 0;
            txtNombreRol.Clear();
            for (int i = 0; i < clbPatentesRol.Items.Count; i++) clbPatentesRol.SetItemChecked(i, false);
            for (int i = 0; i < clbFamiliasRol.Items.Count; i++) clbFamiliasRol.SetItemChecked(i, false);
            btnEliminarRol.Enabled = true;
            btnModificarRol.Enabled = true;
            dgvRoles.Enabled = true;
            btnLimpiarRol.Text = IdiomaManager_GO44.T("permisos.limpiar");
            gbCrearRol.Text = IdiomaManager_GO44.T("permisos.crearRol");
        }


        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color azulOscuro = Color.FromArgb(13, 71, 161);
            Color azulClaro  = Color.FromArgb(227, 242, 253);
            Color blanco     = Color.White;

            TabControl tc = (TabControl)sender;
            TabPage page = tc.TabPages[e.Index];
            Rectangle rect = tc.GetTabRect(e.Index);
            bool seleccionada = (e.Index == tc.SelectedIndex);

            using (SolidBrush fondo = new SolidBrush(seleccionada ? azulOscuro : blanco))
                e.Graphics.FillRectangle(fondo, rect);

            using (Pen borde = new Pen(azulOscuro, 1))
                e.Graphics.DrawRectangle(borde, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

            TextRenderer.DrawText(
                e.Graphics,
                page.Text,
                tc.Font,
                rect,
                seleccionada ? blanco : azulOscuro,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("permisos.titulo");

            if (tabPatentes != null) tabPatentes.Text = IdiomaManager_GO44.T("permisos.tabPatentes");
            if (tabFamilias != null) tabFamilias.Text = IdiomaManager_GO44.T("permisos.tabFamilias");
            if (tabRoles != null) tabRoles.Text = IdiomaManager_GO44.T("permisos.tabRoles");

            if (lblTitPatentes != null) lblTitPatentes.Text = IdiomaManager_GO44.T("permisos.titPatentes");

            if (lblTitFamilias != null) lblTitFamilias.Text = IdiomaManager_GO44.T("permisos.titFamilias");
            if (btnModificarFamilia != null) btnModificarFamilia.Text = IdiomaManager_GO44.T("permisos.modificarFamilia");
            if (btnEliminarFamilia != null) btnEliminarFamilia.Text = IdiomaManager_GO44.T("permisos.eliminarFamilia");
            if (gbCrearFamilia != null) gbCrearFamilia.Text = IdiomaManager_GO44.T("permisos.crearFamilia");
            if (lblNombreFamilia != null) lblNombreFamilia.Text = IdiomaManager_GO44.T("permisos.nombre");
            if (lblPatentesFamilia != null) lblPatentesFamilia.Text = IdiomaManager_GO44.T("permisos.patentesAIncluir");
            if (lblSubfamilias != null) lblSubfamilias.Text = IdiomaManager_GO44.T("permisos.subfamilias");
            if (btnGuardarFamilia != null) btnGuardarFamilia.Text = IdiomaManager_GO44.T("permisos.guardar");
            if (btnLimpiarFamilia != null) btnLimpiarFamilia.Text = IdiomaManager_GO44.T("permisos.limpiar");

            if (lblTitRoles != null) lblTitRoles.Text = IdiomaManager_GO44.T("permisos.titRoles");
            if (btnModificarRol != null) btnModificarRol.Text = IdiomaManager_GO44.T("permisos.modificarRol");
            if (btnEliminarRol != null) btnEliminarRol.Text = IdiomaManager_GO44.T("permisos.eliminarRol");
            if (gbCrearRol != null) gbCrearRol.Text = IdiomaManager_GO44.T("permisos.crearRol");
            if (lblNombreRol != null) lblNombreRol.Text = IdiomaManager_GO44.T("permisos.nombre");
            if (lblPatentesRol != null) lblPatentesRol.Text = IdiomaManager_GO44.T("permisos.patentesIndividuales");
            if (lblFamiliasRol != null) lblFamiliasRol.Text = IdiomaManager_GO44.T("permisos.familias");
            if (btnGuardarRol != null) btnGuardarRol.Text = IdiomaManager_GO44.T("permisos.guardar");
            if (btnLimpiarRol != null) btnLimpiarRol.Text = IdiomaManager_GO44.T("permisos.limpiar");

            if (dgvPatentes != null && dgvPatentes.Columns.Count > 0)
                RecargarTodo();
        }

        

        private void btnModificarRol_Click_1(object sender, EventArgs e)
        {

        }

        private void btnModificarRol_Click(object sender, EventArgs e)
        {
            if (dgvRoles.CurrentRow == null)
            {
                MessageBox.Show(IdiomaManager_GO44.T("permisos.seleccioneRol"),
                                IdiomaManager_GO44.T("general.advertencia"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Rol_GO44 rol = dgvRoles.CurrentRow.DataBoundItem as Rol_GO44;
            if (rol == null) return;

            Rol_GO44 arbol = _bll.ObtenerArbolRol(rol.Id);
            if (arbol == null) return;

            txtNombreRol.Text = arbol.Nombre;

            HashSet<int> idsPat = new HashSet<int>(arbol.Hijos.OfType<Patente_GO44>().Select(p => p.Id));
            HashSet<int> idsFam = new HashSet<int>(arbol.Hijos.OfType<Familia_GO44>().Select(f => f.Id));

            for (int i = 0; i < clbPatentesRol.Items.Count; i++)
            {
                var p = (Patente_GO44)clbPatentesRol.Items[i];
                clbPatentesRol.SetItemChecked(i, idsPat.Contains(p.Id));
            }
            for (int i = 0; i < clbFamiliasRol.Items.Count; i++)
            {
                var f = (Familia_GO44)clbFamiliasRol.Items[i];
                clbFamiliasRol.SetItemChecked(i, idsFam.Contains(f.Id));
            }

            EntrarAModoRol("Modificar", rol.Id);
        }
    }
}
