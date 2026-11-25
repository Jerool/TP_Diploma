using Servicios;
using Servicios.Ayuda;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// Ayuda en línea — "menú escalera" (árbol de temas) con contenido y exportación a PDF.
    /// Se puede abrir en un tema puntual (ayuda contextual F1 por formulario).
    /// </summary>
    public partial class FRMAyuda_GO44 : Form, IObservadorIdioma_GO44
    {
        private readonly string _temaInicial;

        public FRMAyuda_GO44() : this(null) { }

        public FRMAyuda_GO44(string temaInicial)
        {
            InitializeComponent();
            _temaInicial = temaInicial;

            IdiomaManager_GO44.Instancia.Suscribir(this);
            this.FormClosed += (s, e) => IdiomaManager_GO44.Instancia.Desuscribir(this);
        }

        private void FRMAyuda_GO44_Load(object sender, EventArgs e)
        {
            ActualizarIdioma();
            CargarArbol();
            if (!string.IsNullOrEmpty(_temaInicial))
                SeleccionarTema(_temaInicial);
            else if (tvTemas.Nodes.Count > 0)
                tvTemas.SelectedNode = tvTemas.Nodes[0];
        }

        /// <summary>Aplica los textos del idioma activo y reconstruye el árbol (sus títulos cambian de idioma).</summary>
        public void ActualizarIdioma()
        {
            this.Text = IdiomaManager_GO44.T("ayuda.ventana");
            if (lblTitulo != null)     lblTitulo.Text     = IdiomaManager_GO44.T("ayuda.ventana");
            if (grpContenido != null)  grpContenido.Text  = IdiomaManager_GO44.T("ayuda.grpTema");
            if (btnExportar != null)   btnExportar.Text   = IdiomaManager_GO44.T("ayuda.btnExportar");
            if (btnSalir != null)      btnSalir.Text      = IdiomaManager_GO44.T("general.salir");

            // Si ya hay árbol cargado, reconstruirlo preservando el nodo seleccionado (por posición).
            if (tvTemas != null && tvTemas.Nodes.Count > 0)
            {
                int[] camino = CaminoDeNodo(tvTemas.SelectedNode);
                CargarArbol();
                SeleccionarPorCamino(camino);
            }
        }

        private void CargarArbol()
        {
            tvTemas.BeginUpdate();
            tvTemas.Nodes.Clear();
            foreach (var raiz in AyudaManager_GO44.ObtenerArbol())
                tvTemas.Nodes.Add(ConstruirNodo(raiz));
            tvTemas.ExpandAll();
            tvTemas.EndUpdate();
        }

        private TreeNode ConstruirNodo(NodoAyuda_GO44 n)
        {
            var tn = new TreeNode(n.Titulo) { Tag = n };
            foreach (var h in n.Hijos)
                tn.Nodes.Add(ConstruirNodo(h));
            return tn;
        }

        /// <summary>Índices (nivel a nivel) que ubican a un nodo dentro del árbol; null si no hay.</summary>
        private int[] CaminoDeNodo(TreeNode nodo)
        {
            if (nodo == null) return null;
            var pila = new List<int>();
            while (nodo != null) { pila.Insert(0, nodo.Index); nodo = nodo.Parent; }
            return pila.ToArray();
        }

        private void SeleccionarPorCamino(int[] camino)
        {
            if (camino == null || camino.Length == 0)
            {
                if (tvTemas.Nodes.Count > 0) tvTemas.SelectedNode = tvTemas.Nodes[0];
                return;
            }
            TreeNodeCollection nivel = tvTemas.Nodes;
            TreeNode actual = null;
            foreach (int idx in camino)
            {
                if (idx < 0 || idx >= nivel.Count) break;
                actual = nivel[idx];
                nivel = actual.Nodes;
            }
            if (actual != null) { tvTemas.SelectedNode = actual; actual.EnsureVisible(); }
            else if (tvTemas.Nodes.Count > 0) tvTemas.SelectedNode = tvTemas.Nodes[0];
        }

        /// <summary>Navega a un tema sin reabrir el form (lo usa el menú cuando la Ayuda ya está abierta).</summary>
        public void IrATema(string tema)
        {
            if (string.IsNullOrEmpty(tema))
            {
                if (tvTemas.Nodes.Count > 0) tvTemas.SelectedNode = tvTemas.Nodes[0];
            }
            else SeleccionarTema(tema);
        }

        private void SeleccionarTema(string titulo)
        {
            TreeNode encontrado = BuscarNodo(tvTemas.Nodes, titulo);
            if (encontrado != null) { tvTemas.SelectedNode = encontrado; encontrado.EnsureVisible(); }
            else if (tvTemas.Nodes.Count > 0) tvTemas.SelectedNode = tvTemas.Nodes[0];
        }

        private TreeNode BuscarNodo(TreeNodeCollection nodos, string titulo)
        {
            foreach (TreeNode n in nodos)
            {
                if (string.Equals(n.Text, titulo, StringComparison.OrdinalIgnoreCase)) return n;
                var hijo = BuscarNodo(n.Nodes, titulo);
                if (hijo != null) return hijo;
            }
            return null;
        }

        private void tvTemas_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var n = e.Node != null ? e.Node.Tag as NodoAyuda_GO44 : null;
            if (n == null) return;
            lblTema.Text = n.Titulo;
            // El TextBox multilínea necesita CR+LF; el contenido viene con '\n'.
            txtContenido.Text = (n.Contenido ?? "").Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "Ayuda_TechFlow.pdf";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    string[] headers = { IdiomaManager_GO44.T("ayuda.pdfColTema"), IdiomaManager_GO44.T("ayuda.pdfColDesc") };
                    float[] anchos = { 0.35f, 0.65f };
                    List<string[]> filas = AyudaManager_GO44.FilasParaPdf();
                    new BLL.GeneradorPdf_GO44().Generar(sfd.FileName,
                        IdiomaManager_GO44.T("ayuda.pdfTitulo"),
                        string.Format(IdiomaManager_GO44.T("ayuda.pdfSubtitulo"), DateTime.Now.ToString("dd/MM/yyyy HH:mm")),
                        headers, anchos, filas);
                    var abrir = MessageBox.Show(string.Format(IdiomaManager_GO44.T("ayuda.pdfGenerado"), sfd.FileName),
                        IdiomaManager_GO44.T("ayuda.tituloMsg"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (abrir == DialogResult.Yes && System.IO.File.Exists(sfd.FileName))
                        System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(IdiomaManager_GO44.T("rep.pdfError"), ex.Message), IdiomaManager_GO44.T("general.error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
