using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PROYECTO_ING_DE_SOFTWARE
{
    /// <summary>
    /// Visita guiada (coach-marks) — ayuda contextual "con posición de mouse".
    /// Muestra una tarjeta flotante que va señalando cada opción del menú, con navegación
    /// Anterior / Siguiente / Saltar visita y un contador "X de N".
    /// </summary>
    public partial class FRMVisitaGuiada_GO44 : Form
    {
        /// <summary>Paso de la visita: apunta a un ítem del menú con su título y descripción.</summary>
        public class PasoVisita_GO44
        {
            public string Titulo { get; set; }
            public string Texto { get; set; }
            public ToolStripItem Target { get; set; }

            public PasoVisita_GO44(string titulo, string texto, ToolStripItem target)
            {
                Titulo = titulo; Texto = texto; Target = target;
            }
        }

        private readonly MenuStrip _menu;
        private readonly List<PasoVisita_GO44> _pasos;
        private int _idx;

        public FRMVisitaGuiada_GO44(MenuStrip menu, List<PasoVisita_GO44> pasos)
        {
            InitializeComponent();
            _menu = menu;
            _pasos = pasos ?? new List<PasoVisita_GO44>();
        }

        private void FRMVisitaGuiada_GO44_Load(object sender, EventArgs e)
        {
            if (_pasos.Count == 0) { Close(); return; }
            Mostrar(0);
        }

        private void Mostrar(int i)
        {
            if (i < 0) i = 0;
            if (i >= _pasos.Count) i = _pasos.Count - 1;
            _idx = i;

            var p = _pasos[_idx];
            lblTitulo.Text = p.Titulo;
            lblContador.Text = (_idx + 1) + " de " + _pasos.Count;
            lblTexto.Text = p.Texto;
            btnAnterior.Enabled = _idx > 0;
            btnSiguiente.Text = _idx == _pasos.Count - 1 ? "Finalizar" : "Siguiente";

            Posicionar(p.Target);
        }

        private void Posicionar(ToolStripItem target)
        {
            if (target == null || _menu == null) { CenterToScreen(); return; }

            Rectangle b = target.Bounds; // relativo al MenuStrip
            Point abajoIzq = _menu.PointToScreen(new Point(b.Left, b.Bottom));
            int x = abajoIzq.X;
            int y = abajoIzq.Y + 6;

            Rectangle area = Screen.FromPoint(abajoIzq).WorkingArea;
            if (x + Width > area.Right) x = area.Right - Width - 10;
            if (y + Height > area.Bottom) y = abajoIzq.Y - Height - 6; // si no entra abajo, va arriba
            if (x < area.Left) x = area.Left + 10;
            if (y < area.Top) y = area.Top + 10;

            Location = new Point(x, y);
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if (_idx >= _pasos.Count - 1) Close();
            else Mostrar(_idx + 1);
        }

        private void btnAnterior_Click(object sender, EventArgs e) { Mostrar(_idx - 1); }
        private void btnSaltar_Click(object sender, EventArgs e) { Close(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(13, 71, 161), 2))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
