using SmartRoutine.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI
{
    public partial class ToastForm : Form
    {
        private Timer _closeTimer;
        private Label _messageLabel;
        private Panel _contentPanel;
        private static ToastForm _currentToast;

        public ToastForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(350, 80);
            this.BackColor = UIStyles.Colors.Primary;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Opacity = 0.90;

            // Abgerundete Ecken
            this.Paint += (s, e) =>
            {
                using (var path = GetRoundedRectanglePath(this.ClientRectangle, 12))
                {
                    this.Region = new Region(path);
                }
            };

            // Content Panel mit Farbakzent
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 10, 15, 10),
                BackColor = Color.Transparent
            };

            // Icon für Erfolg (optional)
            var successIcon = new Label
            {
                Text = "✓",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Size = new Size(30, 30),
                Location = new Point(10, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Nachrichten-Label
            _messageLabel = new Label
            {
                Text = "",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                AutoSize = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(40, 0, 10, 0)
            };

            _contentPanel.Controls.Add(_messageLabel);
            _contentPanel.Controls.Add(successIcon);
            this.Controls.Add(_contentPanel);

            // Timer
            _closeTimer = new Timer();
            _closeTimer.Interval = 2500;
            _closeTimer.Tick += (s, e) =>
            {
                _closeTimer.Stop();
                this.Close();
                _currentToast = null;
            };
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void ShowToast(string message, Form owner)
        {
            if (owner == null) return;

            // Vorherige Toast schließen
            if (_currentToast != null && !_currentToast.IsDisposed)
            {
                _currentToast.Close();
            }

            _currentToast = new ToastForm();
            _currentToast._messageLabel.Text = message;

            _currentToast.Location = new Point(
                owner.Location.X + (owner.Width / 2) - (_currentToast.Width / 2),
                owner.Location.Y + (int)(owner.Height * 0.75) - (_currentToast.Height / 2)
            );

            _currentToast._closeTimer.Start();
            _currentToast.Show(owner);
        }
    }
}