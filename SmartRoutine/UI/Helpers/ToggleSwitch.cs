using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    public class ToggleSwitch : Control
    {
        private bool _checked;
        private bool _isHovered;
        private bool _isPressed;
        private ToolTip _toolTip;
        private string _toolTipTextChecked = "";
        private string _toolTipTextUnchecked = "";

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    UpdateToolTip();
                    Invalidate();
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // Tooltip Text für eingeschalteten Zustand
        public string ToolTipTextChecked
        {
            get => _toolTipTextChecked;
            set
            {
                _toolTipTextChecked = value;
                UpdateToolTip();
            }
        }

        // Tooltip Text für ausgeschalteten Zustand
        public string ToolTipTextUnchecked
        {
            get => _toolTipTextUnchecked;
            set
            {
                _toolTipTextUnchecked = value;
                UpdateToolTip();
            }
        }

        // Einfacher Tooltip (für beide Zustände gleich)
        public string ToolTipText
        {
            set
            {
                _toolTipTextChecked = value;
                _toolTipTextUnchecked = value;
                UpdateToolTip();
            }
        }

        public ToggleSwitch()
        {
            this.Size = new Size(45, 25);
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;

            _toolTip = new ToolTip();
            _toolTip.InitialDelay = 500;
            _toolTip.ReshowDelay = 100;
            _toolTip.AutoPopDelay = 5000;
        }

        private void UpdateToolTip()
        {
            if (_toolTip == null) return;

            string tooltipText = _checked ? _toolTipTextChecked : _toolTipTextUnchecked;

            if (!string.IsNullOrEmpty(tooltipText))
            {
                _toolTip.SetToolTip(this, tooltipText);
            }
            else
            {
                _toolTip.SetToolTip(this, "");
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = false;
                Checked = !Checked;
                Invalidate();
            }
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Maße
            int toggleWidth = this.Width - 4;
            int toggleHeight = this.Height - 4;
            int knobSize = toggleHeight - 2;
            int radius = toggleHeight / 2;

            // Positionen
            int x = 2;
            int y = 2;
            int knobX = Checked ? x + toggleWidth - knobSize - 2 : x + 2;

            // Hintergrundfarbe
            Color backColor;
            if (!this.Enabled)
                backColor = UIStyles.Colors.BackgroundDark;
            else if (Checked)
                backColor = UIStyles.Colors.Primary;
            else
                backColor = UIStyles.Colors.BackgroundMedium;

            // Hintergrund zeichnen (abgerundetes Rechteck)
            using (var path = GetRoundedRectangle(new Rectangle(x, y, toggleWidth, toggleHeight), radius))
            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Rahmen
            Color borderColor;
            if (!this.Enabled)
                borderColor = UIStyles.Colors.BorderDark;
            else if (_isHovered && !Checked)
                borderColor = UIStyles.Colors.Primary;
            else
                borderColor = UIStyles.Colors.BorderMedium;

            using (var path = GetRoundedRectangle(new Rectangle(x, y, toggleWidth, toggleHeight), radius))
            using (var pen = new Pen(borderColor, 1))
            {
                e.Graphics.DrawPath(pen, path);
            }

            // Knopf (der runde Schieber)
            Color knobColor;
            if (!this.Enabled)
                knobColor = UIStyles.Colors.TextDisabled;
            else if (_isPressed)
                knobColor = UIStyles.Colors.PrimaryLight;
            else if (_isHovered)
                knobColor = UIStyles.Colors.TextPrimary;
            else
                knobColor = UIStyles.Colors.White;

            using (var brush = new SolidBrush(knobColor))
            {
                e.Graphics.FillEllipse(brush, knobX, y + 2, knobSize, knobSize);
            }

            // Knopf-Rahmen
            using (var pen = new Pen(UIStyles.Colors.BorderMedium, 1))
            {
                e.Graphics.DrawEllipse(pen, knobX, y + 2, knobSize, knobSize);
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}