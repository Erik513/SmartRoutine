using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    public class FormDragHandle : IDisposable
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        public Control DragHandle { get; private set; }

        private bool _mouseDown;
        private Point _mouseDownPosition;

        private bool _isRestoringFromMaximized;
        public bool AllowWindowSnapAndMaximize { get; set; } = true;

        public FormDragHandle(Control control, bool allowWindowSnapAndMaximize = true)
        {
            DragHandle = control ?? throw new ArgumentNullException(nameof(control));
            AllowWindowSnapAndMaximize = allowWindowSnapAndMaximize;


            DragHandle.MouseUp += DragHandle_MouseUp;
            DragHandle.MouseDown += OnMouseDown;
            DragHandle.MouseMove += OnMouseMove;
        }

        private void DragHandle_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var form = DragHandle.FindForm();

            if (form == null)
                return;

            // Doppelklick
            if (e.Clicks == 2)
            {
                if (AllowWindowSnapAndMaximize)
                {
                    ToggleMaximize(form);
                }

                return;
            }

            _mouseDown = true;
            _mouseDownPosition = Cursor.Position;
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseDown || e.Button != MouseButtons.Left)
                return;

            var form = DragHandle.FindForm();

            if (form == null)
                return;

            // Erst ab kleiner Bewegung wirklich draggen
            int dragDistance = Math.Abs(Cursor.Position.X - _mouseDownPosition.X) +
                               Math.Abs(Cursor.Position.Y - _mouseDownPosition.Y);

            if (dragDistance < 4)
                return;

            _mouseDown = false;

            if (form.WindowState == FormWindowState.Maximized)
            {
                if (!AllowWindowSnapAndMaximize)
                    return;

                if (_isRestoringFromMaximized)
                    return;

                _isRestoringFromMaximized = true;

                form.WindowState = FormWindowState.Normal;

                Timer timer = new Timer();
                timer.Interval = 30;

                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    timer.Dispose();

                    // AKTUELLE Mausposition holen
                    var cursor = Cursor.Position;
                    var screen = Screen.FromPoint(cursor);

                    form.Location = new Point(
                        cursor.X - form.Width / 2,
                        Math.Max(screen.WorkingArea.Top, cursor.Y - 10)
                    );

                    ReleaseCapture();
                    SendMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);

                    SnapToTopIfNeeded(form);
                    KeepFormOnScreen(form);

                    _isRestoringFromMaximized = false;
                };

                timer.Start();

                return;
            }

            ReleaseCapture();
            SendMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);

            SnapToTopIfNeeded(form);
            KeepFormOnScreen(form);
        }
        private void ToggleMaximize(Form form)
        {
            if (form == null)
                return;

            if (form.WindowState == FormWindowState.Maximized)
            {
                form.WindowState = FormWindowState.Normal;
                return;
            }

            Timer timer = new Timer();
            timer.Interval = 30;

            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();

                form.WindowState = FormWindowState.Maximized;
            };

            timer.Start();
        }

        private void SnapToTopIfNeeded(Form form)
        {
            if (!AllowWindowSnapAndMaximize)
                return;

            if (form == null || form.WindowState == FormWindowState.Maximized)
                return;

            var screen = Screen.FromPoint(Cursor.Position);
            var area = screen.WorkingArea;

            if (Cursor.Position.Y <= area.Top + 4)
            {
                Timer timer = new Timer();
                timer.Interval = 30;

                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    timer.Dispose();

                    form.WindowState = FormWindowState.Maximized;
                };

                timer.Start();
            }
        }

        private void KeepFormOnScreen(Form form)
        {
            if (form == null || form.WindowState == FormWindowState.Maximized)
                return;

            var screen = Screen.FromControl(form);
            var area = screen.WorkingArea;

            bool outsideScreen =
                form.Right < area.Left + 50 ||
                form.Left > area.Right - 50 ||
                form.Bottom < area.Top + 50 ||
                form.Top > area.Bottom - 50;

            if (!outsideScreen)
                return;

            int centerX = area.Left + (area.Width - form.Width) / 2;
            int centerY = area.Top + (area.Height - form.Height) / 2;

            form.Location = new Point(centerX, centerY);
        }

        public static FormDragHandle Create(Control control, bool allowWindowSnapAndMaximize = true)
        {
            return new FormDragHandle(control, allowWindowSnapAndMaximize);
        }

        public void Dispose()
        {
            if (DragHandle != null)
            {
                DragHandle.MouseDown -= OnMouseDown;
                DragHandle.MouseMove -= OnMouseMove;
                DragHandle.MouseUp -= DragHandle_MouseUp;

                DragHandle = null;
            }
        }
    }

    public static class FormDragExtensions
    {
        public static FormDragHandle MakeDragHandle(this Control control, bool allowWindowSnapAndMaximize = true)
        {
            return new FormDragHandle(control, allowWindowSnapAndMaximize );
        }
    }
}