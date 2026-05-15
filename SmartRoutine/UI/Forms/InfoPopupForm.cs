using SmartRoutine.UI.Helpers;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace SmartRoutine.UI.Forms
{
    public partial class InfoPopupForm : Form
    {
        private readonly Label _titleLabel;
        private readonly Label _textLabel;

        public InfoPopupForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;

            BackColor = UIStyles.Colors.PrimaryDark;
            Padding = new Padding(12);

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var layout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(8)
            };

            _titleLabel = new Label
            {
                AutoSize = true,
                Text = "Beschreibung:",
                Font = UIStyles.Fonts.Title,
                ForeColor = UIStyles.Colors.TextPrimary,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 6)
            };

            _textLabel = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(260, 0),
                Font = UIStyles.Fonts.Normal,
                ForeColor = UIStyles.Colors.TextPrimaryDim,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            layout.Controls.Add(_titleLabel);
            layout.Controls.Add(_textLabel);

            Controls.Add(layout);

            this.Load += (s, e) =>
            {
                Region = new Region(CreateRoundedRectangle(this.ClientRectangle, 12));
            };
        }
        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            int diameter = radius * 2;

            var path = new GraphicsPath();

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();

            return path;
        }

        public void ShowInfo(string text, Control owner)
        {
            _textLabel.Text = string.IsNullOrWhiteSpace(text)
                ? "Keine"
                : text;

            var screenPos = owner.PointToScreen(
                new Point(owner.Width + 8, -Height + owner.Height - 10));

            var screen = Screen.FromControl(owner).WorkingArea;

            // Nicht über oberen Bildschirmrand hinaus
            if (screenPos.Y < screen.Top + 10)
                screenPos.Y = screen.Top + 10;

            // Nicht über rechten Rand hinaus
            if (screenPos.X + Width > screen.Right)
                screenPos.X = screen.Right - Width - 10;

            Location = screenPos;

            Show(owner.FindForm());
            BringToFront();
        }
    }
}