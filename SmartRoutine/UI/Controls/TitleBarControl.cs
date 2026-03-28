using System;
using System.Drawing;
using System.Windows.Forms;
using SmartRoutine.UI.Helpers;

namespace SmartRoutine.UI.Controls
{
    public class TitleBarControl : Panel
    {
        private Label lblTitle;
        private Button btnMinimize;
        private Button btnMaximize;
        private Button btnClose;
        private FormDragHandle formDragHandle;
        private Form parentForm;

        private const int TITLE_BAR_HEIGHT = 30;
        private static readonly Size BUTTON_SIZE = new Size(30, 30);

        public string Title
        {
            get => lblTitle.Text;
            set => lblTitle.Text = value;
        }

        public TitleBarControl()
        {
            this.Height = TITLE_BAR_HEIGHT;
            this.Dock = DockStyle.Top;
            this.BackColor = UIStyles.Colors.BackgroundDark;
            this.Visible = true;

            // Title Label
            lblTitle = new Label
            {
                Text = "SmartRoutine",
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundDark,
                ForeColor = UIStyles.Colors.TextPrimary,
                Font = UIStyles.Fonts.Title,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Buttons mit UIStyles
            btnClose = UIStyles.Buttons.CreateStandard("✕", "Close window", BUTTON_SIZE);
            btnMaximize = UIStyles.Buttons.CreateStandard("🗖", "Maximize window", BUTTON_SIZE);
            btnMinimize = UIStyles.Buttons.CreateStandard("🗕", "Minimize window", BUTTON_SIZE);

            // Controls hinzufügen
            this.Controls.Add(lblTitle);
            this.Controls.Add(btnMinimize);
            this.Controls.Add(btnMaximize);
            this.Controls.Add(btnClose);

            // Positionen setzen
            UpdateButtonsPosition();

            // Buttons in den Vordergrund
            btnMinimize.BringToFront();
            btnMaximize.BringToFront();
            btnClose.BringToFront();

            // ToolTips
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnMinimize, "Minimize window");
            toolTip.SetToolTip(btnMaximize, "Maximize window");
            toolTip.SetToolTip(btnClose, "Close window");

            // Drag Handle auf dem LABEL
            formDragHandle = new FormDragHandle(lblTitle);

            // Events
            this.Resize += (s, e) => UpdateButtonsPosition();
            this.ParentChanged += OnParentChanged;
        }
        public TitleBarControl(string title) : this()
        {
            Title = title;
        }

        private void UpdateButtonsPosition()
        {
            btnMinimize.Location = new Point(this.Width - BUTTON_SIZE.Width * 3, 0);
            btnMaximize.Location = new Point(this.Width - BUTTON_SIZE.Width * 2, 0);
            btnClose.Location = new Point(this.Width - BUTTON_SIZE.Width, 0);
        }
        private void OnParentChanged(object sender, EventArgs e)
        {
            parentForm = this.FindForm();
            if (parentForm != null)
            {
                // Buttons mit den Form-Aktionen verknüpfen
                btnMinimize.Click += (s, ev) => parentForm.WindowState = FormWindowState.Minimized;
                btnMaximize.Click += (s, ev) => ToggleMaximize();
                btnClose.Click += (s, ev) => parentForm.Close();

                // Drag Handle
                formDragHandle = new FormDragHandle(this);

                // Maximize-Button-Text initial setzen
                UpdateMaximizeButton(parentForm.WindowState == FormWindowState.Maximized);

                // Auf Resize der Parent-Form reagieren
                parentForm.Resize += (s, ev) => UpdateMaximizeButton(parentForm.WindowState == FormWindowState.Maximized);
            }
        }
        private void ToggleMaximize()
        {
            if (parentForm != null)
            {
                parentForm.WindowState = parentForm.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (btnMinimize != null && this.Width > 0)
            {
                btnMinimize.Location = new Point(this.Width - BUTTON_SIZE.Width * 3, 0);
                btnMaximize.Location = new Point(this.Width - BUTTON_SIZE.Width * 2, 0);
                btnClose.Location = new Point(this.Width - BUTTON_SIZE.Width, 0);
            }
        }

        public void UpdateMaximizeButton(bool isMaximized)
        {
            if (btnMaximize != null)
            {
                btnMaximize.Text = isMaximized ? "❐" : "🗖";
            }
        }

        public void DisposeDragHandle()
        {
            formDragHandle?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeDragHandle();
            }
            base.Dispose(disposing);
        }
    }
}