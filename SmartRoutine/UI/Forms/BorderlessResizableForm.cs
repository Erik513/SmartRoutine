using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    public partial class BorderlessResizableForm : Form
    {
        private const int WmNcHitTest = 0x84;

        private const int HtLeft = 10;
        private const int HtRight = 11;
        private const int HtTop = 12;
        private const int HtTopLeft = 13;
        private const int HtTopRight = 14;
        private const int HtBottom = 15;
        private const int HtBottomLeft = 16;
        private const int HtBottomRight = 17;

        private const int ResizeBorder = 6;
        private const int FormPadding = 2;

        public BorderlessResizableForm()
        {
            ConfigureForm();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg != WmNcHitTest)
                return;

            if (WindowState == FormWindowState.Maximized)
                return;

            IntPtr hitTestResult = GetResizeHitTestResult(m);

            if (hitTestResult != IntPtr.Zero)
                m.Result = hitTestResult;
        }

        private void ConfigureForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
            Padding = new Padding(FormPadding);

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw,
                true);

            UpdateStyles();
        }

        private IntPtr GetResizeHitTestResult(Message message)
        {
            Point screenPoint = GetPointFromLParam(message.LParam);
            Point clientPoint = PointToClient(screenPoint);

            bool left = clientPoint.X < ResizeBorder;
            bool right = clientPoint.X > ClientSize.Width - ResizeBorder;
            bool top = clientPoint.Y < ResizeBorder;
            bool bottom = clientPoint.Y > ClientSize.Height - ResizeBorder;

            if (left && top)
                return (IntPtr)HtTopLeft;

            if (right && top)
                return (IntPtr)HtTopRight;

            if (left && bottom)
                return (IntPtr)HtBottomLeft;

            if (right && bottom)
                return (IntPtr)HtBottomRight;

            if (left)
                return (IntPtr)HtLeft;

            if (right)
                return (IntPtr)HtRight;

            if (top)
                return (IntPtr)HtTop;

            if (bottom)
                return (IntPtr)HtBottom;

            return IntPtr.Zero;
        }

        private static Point GetPointFromLParam(IntPtr lParam)
        {
            int value = lParam.ToInt32();

            int x = value & 0xFFFF;
            int y = value >> 16;

            return new Point(x, y);
        }
    }
}