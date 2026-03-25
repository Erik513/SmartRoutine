using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    public class FormDragHandle : IDisposable
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        /// <summary>
        /// Das Control, das als Drag-Handle fungiert
        /// </summary>
        public Control DragHandle { get; private set; }

        private Timer _refreshTimer;
        private bool _isDragging;

        /// <summary>
        /// Erstellt einen neuen FormDragHandle für das angegebene Control
        /// </summary>
        /// <param name="control">Das Control, das als Drag-Handle fungieren soll</param>
        public FormDragHandle(Control control)
        {
            DragHandle = control ?? throw new ArgumentNullException(nameof(control));
            DragHandle.MouseMove += OnMouseMove;
            DragHandle.DoubleClick += OnDoubleClick;
            DragHandle.MouseUp += OnMouseUp;

            // Für Resize-Optimierung
            var form = DragHandle.FindForm();
            if (form != null)
            {
                form.ResizeBegin += OnFormResizeBegin;
                form.ResizeEnd += OnFormResizeEnd;
            }
        }

        private void OnDoubleClick(object sender, EventArgs e)
        {
            var form = DragHandle.FindForm();
            if (form != null)
            {
                form.SuspendLayout();
                form.WindowState = form.WindowState == FormWindowState.Normal
                    ? FormWindowState.Maximized
                    : FormWindowState.Normal;
                form.ResumeLayout(true);

                // Verzögertes Refreshen
                form.BeginInvoke(new Action(() => RefreshFormControls(form)));
            }
        }

        private List<Control> GetAllControls(Control container)
        {
            var controls = new List<Control>();
            foreach (Control c in container.Controls)
            {
                controls.Add(c);
                controls.AddRange(GetAllControls(c));
            }
            return controls;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var form = DragHandle.FindForm();
                if (form != null)
                {
                    _isDragging = true;

                    // Wenn maximiert -> zuerst in Normal wechseln
                    if (form.WindowState == FormWindowState.Maximized)
                    {
                        form.WindowState = FormWindowState.Normal;
                        // Sofort refreshen
                        RefreshFormNow(form);

                        // Position für Drag neu berechnen
                        var screen = Screen.FromControl(form);
                        form.Location = new Point(
                            Cursor.Position.X - (form.Width / 2),
                            Math.Max(screen.WorkingArea.Top, Cursor.Position.Y - 10)
                        );
                    }

                    ReleaseCapture();
                    SendMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                    CenterToScreenIfUnderTaskbar();
                    SnapToTopIfNeeded();
                }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var form = DragHandle.FindForm();
                if (form != null)
                {
                    _isDragging = false;
                    // Nach Drag-Release refreshen
                    form.BeginInvoke(new Action(() => RefreshFormControls(form)));
                }
            }
        }

        private void OnFormResizeBegin(object sender, EventArgs e)
        {
            // Beim Beginn des Resizings: Buttons ausblenden für flüssigeres Resizing
            var form = DragHandle.FindForm();
            if (form != null)
            {
                HideWindowButtons(form, true);
            }
        }

        private void OnFormResizeEnd(object sender, EventArgs e)
        {
            // Nach dem Resizing: Buttons wieder einblenden und neu zeichnen
            var form = DragHandle.FindForm();
            if (form != null)
            {
                HideWindowButtons(form, false);

                // Verzögertes Refreshen für alle Controls
                form.BeginInvoke(new Action(() => RefreshFormControls(form)));

                // Zusätzlich einen Timer für erneutes Refreshen (falls nötig)
                _refreshTimer?.Dispose();
                _refreshTimer = new Timer { Interval = 50 };
                _refreshTimer.Tick += (s, args) =>
                {
                    RefreshFormControls(form);
                    _refreshTimer?.Stop();
                    _refreshTimer?.Dispose();
                };
                _refreshTimer.Start();
            }
        }

        private void HideWindowButtons(Form form, bool hide)
        {
            // Durchsucht die Form nach Fenster-Buttons und blendet sie ein/aus
            foreach (Control control in GetAllControls(form))
            {
                // Erkenne Buttons an typischen Eigenschaften
                if (control is Button button &&
                    (button.Text == "✕" || button.Text == "🗖" || button.Text == "🗕" ||
                     button.Text == "❐" || button.Text == "🗗" || button.Text == "🗙"))
                {
                    button.Visible = !hide;

                    // Wenn eingeblendet wird, Position aktualisieren und neu zeichnen
                    if (!hide)
                    {
                        button.Invalidate();
                        button.Update();
                    }
                }
            }
        }

        private void SnapToTopIfNeeded()
        {
            var form = DragHandle.FindForm();
            if (form != null && form.WindowState != FormWindowState.Maximized)
            {
                var screen = Screen.FromControl(form);
                var mousePos = Cursor.Position;

                if (mousePos.Y <= screen.WorkingArea.Top + 2)
                {
                    form.SuspendLayout();
                    form.WindowState = FormWindowState.Maximized;
                    form.ResumeLayout(true);
                    form.BeginInvoke(new Action(() => RefreshFormControls(form)));
                }
            }
        }

        private void RefreshFormNow(Form form)
        {
            form.SuspendLayout();
            form.PerformLayout();
            form.ResumeLayout(false);
            form.Invalidate(true);
            form.Update();
        }

        private void RefreshFormControls(Form form)
        {
            form.SuspendLayout();
            foreach (Control c in GetAllControls(form))
            {
                if (c.Visible)
                {
                    c.Invalidate();
                    c.Update();
                }
            }
            form.ResumeLayout(true);
            form.Invalidate(true);
            form.Update();
        }

        private void CenterToScreenIfUnderTaskbar()
        {
            var form = DragHandle.FindForm();
            if (form != null)
            {
                int taskBarHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
                int visibleAreaTop = Screen.PrimaryScreen.WorkingArea.Top + taskBarHeight;
                int panelTopRelativeToScreen = form.Top + 20;

                if (panelTopRelativeToScreen > Screen.PrimaryScreen.Bounds.Height - visibleAreaTop)
                {
                    int centerX = Screen.PrimaryScreen.Bounds.Width / 2;
                    int centerY = Screen.PrimaryScreen.Bounds.Height / 2;
                    form.Location = new Point(centerX - form.Width / 2, centerY - form.Height / 2);
                }
            }
        }

        /// <summary>
        /// Statische Hilfsmethode für einfache Verwendung
        /// </summary>
        public static FormDragHandle Create(Control control)
        {
            return new FormDragHandle(control);
        }

        /// <summary>
        /// Entfernt alle Event-Handler und gibt Ressourcen frei
        /// </summary>
        public void Dispose()
        {
            if (DragHandle != null)
            {
                DragHandle.MouseMove -= OnMouseMove;
                DragHandle.DoubleClick -= OnDoubleClick;
                DragHandle.MouseUp -= OnMouseUp;

                var form = DragHandle.FindForm();
                if (form != null)
                {
                    form.ResizeBegin -= OnFormResizeBegin;
                    form.ResizeEnd -= OnFormResizeEnd;
                }

                DragHandle = null;
            }

            _refreshTimer?.Dispose();
        }
    }

    /// <summary>
    /// Statische Hilfsklasse für schnellen Zugriff
    /// </summary>
    public static class FormDragExtensions
    {
        /// <summary>
        /// Macht dieses Control zu einem Form-Drag-Handle
        /// </summary>
        public static FormDragHandle MakeDragHandle(this Control control)
        {
            return new FormDragHandle(control);
        }
    }
}