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
    public partial class FRMAyuda_GO44 : Form
    {
        private readonly string _temaInicial;

        public FRMAyuda_GO44() : this(null) { }

        public FRMAyuda_GO44(string temaInicial)
        {
            InitializeComponent();
            _temaInicial = temaInicial;
        }

        private void FRMAyuda_GO44_Load(object sender, EventArgs e)
        {
            CargarArbol();
            if (!string.IsNullOrEmpty(_temaInicial))
                SeleccionarTema(_temaInicial);
            else if (tvTemas.Nodes.Count > 0)
                tvTemas.SelectedNode = tvTemas.Nodes[0];
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
            txtContenido.Text = n.Contenido ?? "";
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
                    string[] headers = { "Tema", "Descripción" };
                    float[] anchos = { 0.35f, 0.65f };
                    List<string[]> filas = AyudaManager_GO44.FilasParaPdf();
                    new BLL.GeneradorPdf_GO44().Generar(sfd.FileName,
                        "AYUDA EN LÍNEA — TechFlow / ElectroPoint",
                        "Menú escalera de temas · Generado " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        headers, anchos, filas);
                    var abrir = MessageBox.Show("Ayuda exportada a:\n" + sfd.FileName + "\n\n¿Abrirla?",
                        "Ayuda", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (abrir == DialogResult.Yes && System.IO.File.Exists(sfd.FileName))
                        System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo generar el PDF: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e) { this.Close(); }
    }
}
