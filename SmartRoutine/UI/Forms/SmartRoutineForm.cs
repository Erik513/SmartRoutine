using SmartRoutine.UI.Controls;
using SmartRoutine.UI.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    public partial class SmartRoutineForm : BorderlessResizableForm
    {
        private const int TitleBarHeight = 30;

        protected TitleBarControl TitleBar { get; private set; }
        protected Panel ContentPanel { get; private set; }

        public SmartRoutineForm(
            Image icon = null,
            string title = "SmartRoutine",
            bool showMinimize = true,
            bool showMaximize = true,
            bool showClose = true,
            bool allowWindowSnapAndMaximize = true,
            Color? titleBarBackColor = null)
        {
            ConfigureForm();
            CreateLayout(
                icon,
                title,
                showMinimize,
                showMaximize,
                showClose,
                allowWindowSnapAndMaximize,
                titleBarBackColor);
        }

        private void ConfigureForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = UIStyles.Colors.BackgroundDark;

            if (Properties.Resources.AppIcon != null)
                Icon = Properties.Resources.AppIcon;
        }

        private void CreateLayout(
            Image icon,
            string title,
            bool showMinimize,
            bool showMaximize,
            bool showClose,
            bool allowWindowSnapAndMaximize,
            Color? titleBarBackColor)
        {
            TableLayoutPanel rootLayout = CreateRootLayout();

            TitleBar = CreateTitleBar(
                icon,
                title,
                showMinimize,
                showMaximize,
                showClose,
                allowWindowSnapAndMaximize,
                titleBarBackColor);

            ContentPanel = CreateContentPanel();

            rootLayout.Controls.Add(TitleBar, 0, 0);
            rootLayout.Controls.Add(ContentPanel, 0, 1);

            Controls.Add(rootLayout);
        }

        private TableLayoutPanel CreateRootLayout()
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UIStyles.Colors.BackgroundDark,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, TitleBarHeight));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            return layout;
        }

        private TitleBarControl CreateTitleBar(
            Image icon,
            string title,
            bool showMinimize,
            bool showMaximize,
            bool showClose,
            bool allowWindowSnapAndMaximize,
            Color? titleBarBackColor)
        {
            return new TitleBarControl(
                icon: icon,
                title: title,
                titleTextAlign: ContentAlignment.MiddleLeft,
                showMinimizeButton: showMinimize,
                showMaximizeButton: showMaximize,
                showCloseButton: showClose,
                allowWindowSnapAndMaximize: allowWindowSnapAndMaximize,
                backColor: titleBarBackColor ?? UIStyles.Colors.BackgroundBlack)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
        }

        private Panel CreateContentPanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundDark,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
        }
    }
}