using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    public partial class BorderlessResizableForm : Form
    {
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int RESIZE_BORDER = 6; // Size of "resizable" Area

        private bool _isResizing = false;

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;

            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    m.Result = (IntPtr)0;
                    return;
                }
                Point pos = PointToClient(new Point(m.LParam.ToInt32()));
                if (pos.X < RESIZE_BORDER && pos.Y < RESIZE_BORDER)
                    m.Result = (IntPtr)HTTOPLEFT;
                else if (pos.X > ClientSize.Width - RESIZE_BORDER && pos.Y < RESIZE_BORDER)
                    m.Result = (IntPtr)HTTOPRIGHT;
                else if (pos.X < RESIZE_BORDER && pos.Y > ClientSize.Height - RESIZE_BORDER)
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                else if (pos.X > ClientSize.Width - RESIZE_BORDER && pos.Y > ClientSize.Height - RESIZE_BORDER)
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                else if (pos.X < RESIZE_BORDER)
                    m.Result = (IntPtr)HTLEFT;
                else if (pos.X > ClientSize.Width - RESIZE_BORDER)
                    m.Result = (IntPtr)HTRIGHT;
                else if (pos.Y < RESIZE_BORDER)
                    m.Result = (IntPtr)HTTOP;
                else if (pos.Y > ClientSize.Height - RESIZE_BORDER)
                    m.Result = (IntPtr)HTBOTTOM;
            }
        }
        protected override void OnResize(EventArgs e)
        {
            if (!_isResizing)
            {
                base.OnResize(e);
            }
            else
            {
                // Während des Resizings nur das Nötigste machen
                this.Invalidate();
            }
        }
        /// <summary>
        /// Erstellt ein fensterloses Formular ohne Standard-Titelleiste, 
        /// das dennoch in der Größe verändert werden kann.
        /// </summary>
        /// <remarks>
        /// Die Klasse überschreibt die WndProc-Methode, um WM_NCHITTEST-Nachrichten abzufangen.
        /// Dadurch wird ein 6 Pixel breiter Rand um das Formular als Resize-Bereich definiert,
        /// der alle 8 Ziehpunkte (Ecken und Kanten) für die Größenänderung unterstützt.
        /// </remarks>
        public BorderlessResizableForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.ResizeRedraw, true);
            this.Padding = new Padding(2);
        }
    }
}
