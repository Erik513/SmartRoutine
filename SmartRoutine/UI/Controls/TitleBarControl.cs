using System;
using System.Drawing;
using System.Windows.Forms;
using SmartRoutine.UI.Helpers;

namespace SmartRoutine.UI.Controls
{
    public class TitleBarControl : Panel
    {
        private Label lblTitle;
        private PictureBox picIcon;
        private Button btnMinimize;
        private Button btnMaximize;
        private Button btnClose;
        private FormDragHandle titleBarDragHandle;
        private FormDragHandle titleLabelDragHandle;
        private FormDragHandle iconDragHandle;
        private Form parentForm;

        private const int TITLE_BAR_HEIGHT = 30;
        private static readonly Size BUTTON_SIZE = new Size(30, 30);
        private const int ICON_SIZE = TITLE_BAR_HEIGHT;
        private const int ICON_LEFT_MARGIN = 0;

        public string Title
        {
            get => lblTitle.Text;
            set => lblTitle.Text = value;
        }
        
        public Image IconImage
        {
            get => picIcon.Image;
            set
            {
                picIcon.Image = value;
                picIcon.Visible = value != null;
                UpdateLayout();
            }
        }
        public ContentAlignment TitleTextAlign
        {
            get => lblTitle.TextAlign;
            set => lblTitle.TextAlign = value;
        }
        public bool ShowMinimizeButton
        {
            get => btnMinimize.Visible;
            set
            {
                btnMinimize.Visible = value;
                UpdateLayout();
            }
        }
        public bool ShowMaximizeButton
        {
            get => btnMaximize.Visible;
            set
            {
                btnMaximize.Visible = value;
                UpdateLayout();
            }
        }
        public bool ShowCloseButton
        {
            get => btnClose.Visible;
            set
            {
                btnClose.Visible = value;
                UpdateLayout();
            }
        }
        private readonly bool _allowWindowSnapAndMaximize;
        public TitleBarControl(
            Image icon = null,
            string title = "",
            ContentAlignment titleTextAlign = ContentAlignment.MiddleCenter,
            bool showMinimizeButton = true,
            bool showMaximizeButton = true,
            bool showCloseButton = true,
            bool allowWindowSnapAndMaximize = true)
        {
            _allowWindowSnapAndMaximize = allowWindowSnapAndMaximize;
            Height = TITLE_BAR_HEIGHT;
            Dock = DockStyle.Top;
            BackColor = UIStyles.Colors.BackgroundBlack;
            Visible = true;

            lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundBlack,
                ForeColor = UIStyles.Colors.TextPrimary,
                Font = UIStyles.Fonts.Title,
                TextAlign = titleTextAlign,
                Padding = new Padding(10, 0, 10, 0)
            };

            picIcon = new PictureBox
            {
                Image = icon,
                Size = new Size(ICON_SIZE, ICON_SIZE),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Visible = icon != null
            };

            btnClose = UIStyles.Buttons.CreateStandard("✕", "Close window", BUTTON_SIZE);
            btnMaximize = UIStyles.Buttons.CreateStandard("🗖", "Maximize window", BUTTON_SIZE);
            btnMinimize = UIStyles.Buttons.CreateStandard("🗕", "Minimize window", BUTTON_SIZE);

            btnMinimize.Visible = showMinimizeButton;
            btnMaximize.Visible = showMaximizeButton;
            btnClose.Visible = showCloseButton;

            Controls.Add(lblTitle);
            Controls.Add(picIcon);
            Controls.Add(btnMinimize);
            Controls.Add(btnMaximize);
            Controls.Add(btnClose);

            lblTitle.SendToBack();
            picIcon.BringToFront();
            btnMinimize.BringToFront();
            btnMaximize.BringToFront();
            btnClose.BringToFront();

            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnMinimize, "Minimize window");
            toolTip.SetToolTip(btnMaximize, "Maximize window");
            toolTip.SetToolTip(btnClose, "Close window");

            CreateDragHandles();

            Resize += (s, e) => UpdateLayout();
            ParentChanged += OnParentChanged;
            HandleCreated += TitleBarControl_HandleCreated;

            UpdateLayout();

            RunWhenHandleReady(UpdateLayout);
        }
        private void CreateDragHandles()
        {
            DisposeDragHandle();

            titleBarDragHandle = new FormDragHandle(this, _allowWindowSnapAndMaximize);
            titleLabelDragHandle = new FormDragHandle(lblTitle, _allowWindowSnapAndMaximize);

            if (picIcon != null)
                iconDragHandle = new FormDragHandle(picIcon, _allowWindowSnapAndMaximize);
        }

        private void RunWhenHandleReady(Action action)
        {
            if (IsHandleCreated)
            {
                BeginInvoke(action);
                return;
            }

            EventHandler handler = null;

            handler = (s, e) =>
            {
                HandleCreated -= handler;
                BeginInvoke(action);
            };

            HandleCreated += handler;
        }

        private void UpdateLayout()
        {
            if (btnClose == null || btnMaximize == null || btnMinimize == null || picIcon == null)
                return;

            int right = Width;

            if (btnClose.Visible)
            {
                btnClose.Location = new Point(right - BUTTON_SIZE.Width, 0);
                right -= BUTTON_SIZE.Width;
                btnClose.BringToFront();
            }

            if (btnMaximize.Visible)
            {
                btnMaximize.Location = new Point(right - BUTTON_SIZE.Width, 0);
                right -= BUTTON_SIZE.Width;
                btnMaximize.BringToFront();
            }

            if (btnMinimize.Visible)
            {
                btnMinimize.Location = new Point(right - BUTTON_SIZE.Width, 0);
                right -= BUTTON_SIZE.Width;
                btnMinimize.BringToFront();
            }

            if (picIcon.Visible)
            {
                picIcon.Location = new Point(
                ICON_LEFT_MARGIN,
                0);

                picIcon.BringToFront();
            }
        }

        private void OnParentChanged(object sender, EventArgs e)
        {
            AttachToParentFormWhenReady();
        }

        private void AttachToParentFormWhenReady()
        {
            var form = FindForm();

            if (form == null)
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(AttachToParentFormWhenReady));
                }
                else
                {
                    HandleCreated += TitleBarControl_HandleCreated;
                }

                return;
            }

            parentForm = form;

            btnMinimize.Click -= BtnMinimize_Click;
            btnMaximize.Click -= BtnMaximize_Click;
            btnClose.Click -= BtnClose_Click;

            btnMinimize.Click += BtnMinimize_Click;
            btnMaximize.Click += BtnMaximize_Click;
            btnClose.Click += BtnClose_Click;

            parentForm.Resize -= ParentForm_Resize;
            parentForm.Resize += ParentForm_Resize;

            UpdateMaximizeButton(parentForm.WindowState == FormWindowState.Maximized);
        }

        private void TitleBarControl_HandleCreated(object sender, EventArgs e)
        {
            HandleCreated -= TitleBarControl_HandleCreated;
            BeginInvoke(new Action(AttachToParentFormWhenReady));
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            if (parentForm != null)
                parentForm.WindowState = FormWindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, EventArgs e)
        {
            ToggleMaximize();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            parentForm?.Close();
        }

        private void ParentForm_Resize(object sender, EventArgs e)
        {
            var form = parentForm ?? FindForm();

            if (form == null)
                return;

            parentForm = form;

            UpdateMaximizeButton(form.WindowState == FormWindowState.Maximized);
        }

        private void ToggleMaximize()
        {
            if (parentForm == null)
                return;

            parentForm.WindowState = parentForm.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }

        public void UpdateMaximizeButton(bool isMaximized)
        {
            if (btnMaximize != null)
                btnMaximize.Text = isMaximized ? "❐" : "🗖";
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                BeginInvoke(new Action(() =>
                {
                    UpdateLayout();
                }));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeDragHandle();

                if (parentForm != null)
                    parentForm.Resize -= ParentForm_Resize;
            }

            base.Dispose(disposing);
        }
        public void DisposeDragHandle()
        {
            titleBarDragHandle?.Dispose();
            titleLabelDragHandle?.Dispose();
            iconDragHandle?.Dispose();

            titleBarDragHandle = null;
            titleLabelDragHandle = null;
            iconDragHandle = null;
        }
    }
}