using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Helpers;

namespace SmartRoutine.UI
{
    public partial class MainForm : BorderlessResizableForm
    {
        private readonly IBusinessLogic _logic;

        // ========== FIELDS ==========
        private readonly string _programName = "SmartRoutine";

        // UI Elements
        private Label lblTitle;
        private Button btnMinimize;
        private Button btnMaximize;
        private Button btnClose;
        private FormDragHandle formDragHandle;

        // Constants
        private const int TOP_BAR_HEIGHT = 30;
        private static readonly Size BUTTON_SIZE = new Size(30, 30);
        private static readonly Size DEFAULT_WINDOW_SIZE = new Size(1024, 768);
        private static readonly Size MINIMUM_WINDOW_SIZE = new Size(640, 360);

        // ========== CONSTRUCTOR ==========
        public MainForm(IBusinessLogic logic)
        {
            _logic = logic ?? throw new ArgumentNullException(nameof(logic));

            InitializeComponent();
            ConfigureForm();
            CreateIntegratedUI();

            // Events
            this.Resize += MainForm_Resize;
            this.Load += MainForm_Load;
        }
        // ========== CONFIGURATION ==========
        private void ConfigureForm()
        {
            this.BackColor = UIStyles.Colors.Black;
            this.MinimumSize = MINIMUM_WINDOW_SIZE;
            this.Size = DEFAULT_WINDOW_SIZE;
            this.CenterToScreen();
            this.DoubleBuffered = true;
        }

        // ========== UI CREATION METHODS ==========
        private void CreateIntegratedUI()
        {
            this.Controls.Clear();
            CreateTitleBar();
        }

        private void CreateTitleBar()
        {
            lblTitle = UIStyles.Labels.CreateTitle($"{_programName}");
            lblTitle.Location = new Point(0, 0);
            lblTitle.Size = new Size(this.ClientSize.Width, TOP_BAR_HEIGHT);
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.Padding = new Padding(10, 0, 0, 0);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            formDragHandle = new FormDragHandle(lblTitle);
            this.Controls.Add(lblTitle);

            CreateWindowButtons();
        }
        private void CreateWindowButtons()
        {
            if (lblTitle == null) return;

            // Buttons in der richtigen Reihenfolge (von rechts: Close, Maximize, Minimize)
            int rightMargin = 0;

            btnClose = CreateWindowButton("✕", "Close window", rightMargin);
            rightMargin += BUTTON_SIZE.Width;

            btnMaximize = CreateWindowButton("🗖", "Maximize window", rightMargin);
            rightMargin += BUTTON_SIZE.Width;

            btnMinimize = CreateWindowButton("🗕", "Minimize window", rightMargin);

            // Events zuweisen
            btnClose.Click += (s, e) => this.Close();
            btnMaximize.Click += ToggleMaximize;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // Tooltips erstellen
            CreateToolTips();

            btnClose.BringToFront();
            btnMaximize.BringToFront();
            btnMinimize.BringToFront();
        }

        private Button CreateWindowButton(string text, string tooltip, int offsetFromRight)
        {
            if (lblTitle == null) throw new InvalidOperationException("Title bar not initialized");

            var button = UIStyles.Buttons.CreateStandard(text, tooltip, BUTTON_SIZE);
            button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button.Location = new Point(lblTitle.Width - BUTTON_SIZE.Width - offsetFromRight, 0);
            button.TabIndex = 0; // Für Tastaturnavigation
            lblTitle.Controls.Add(button);
            return button;
        }

        private void CreateToolTips()
        {
            var toolTip = UIStyles.ToolTips.CreateToolTip();

            if (btnMinimize != null) toolTip.SetToolTip(btnMinimize, "Minimize window");
            if (btnMaximize != null) toolTip.SetToolTip(btnMaximize, "Maximize window");
            if (btnClose != null) toolTip.SetToolTip(btnClose, "Close window");
        }

        private void UpdateWindowButtonsPosition()
        {
            if (lblTitle == null) return;

            lblTitle.SuspendLayout();

            try
            {
                int rightMargin = 0;
                if (btnClose != null)
                {
                    btnClose.Location = new Point(lblTitle.Width - BUTTON_SIZE.Width - rightMargin, 0);
                    rightMargin += BUTTON_SIZE.Width;
                }
                if (btnMaximize != null)
                {
                    btnMaximize.Location = new Point(lblTitle.Width - BUTTON_SIZE.Width - rightMargin, 0);
                    rightMargin += BUTTON_SIZE.Width;
                }
                if (btnMinimize != null)
                {
                    btnMinimize.Location = new Point(lblTitle.Width - BUTTON_SIZE.Width - rightMargin, 0);
                }

                btnClose?.Invalidate();
                btnMaximize?.Invalidate();
                btnMinimize?.Invalidate();
            }
            finally
            {
                lblTitle.ResumeLayout(false);
            }
        }

        // ========== EVENT HANDLERS ==========
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _logic.Initialize();
                // Hier weitere Initialisierungen
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden: {ex.Message}", "Initialisierungsfehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
        private void MainForm_Resize(object sender, EventArgs e)
        {
            UpdateWindowButtonsPosition();
            if (btnMaximize != null)
            {
                btnMaximize.Text = this.WindowState == FormWindowState.Maximized ? "❐" : "🗖";
                btnMaximize.Refresh();
            }
        }
        private void ToggleMaximize(object sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }

        // ========== 9. DISPOSE ==========
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _logic.Cleanup();
                SleepPreventer.AllowSleep();
            }
            catch (Exception ex)
            {
                // Loggen falls möglich
                Console.WriteLine($"Fehler beim Cleanup: {ex.Message}");
            }
            finally
            {
                base.OnFormClosing(e);
            }
        }
    }

}
