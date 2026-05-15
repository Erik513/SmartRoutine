using SmartRoutine.UI.Controls;
using SmartRoutine.UI.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    public partial class SmartRoutineForm : BorderlessResizableForm
    {
        protected TitleBarControl TitleBar;
        protected Panel ContentPanel;

        public SmartRoutineForm(
            Image icon = null,
            string title = "SmartRoutine",
            bool showMinimize = true,
            bool showMaximize = true,
            bool showClose = true)
        {
            InitializeSmartRoutineForm(
                icon,
                title,
                showMinimize,
                showMaximize,
                showClose);
        }

        private void InitializeSmartRoutineForm(
            Image icon, 
            string title,
            bool showMinimize,
            bool showMaximize,
            bool showClose)
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = UIStyles.Colors.BackgroundDark;

            var rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UIStyles.Colors.BackgroundDark,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            TitleBar = new TitleBarControl(
                icon: icon,
                title: title,
                titleTextAlign: ContentAlignment.MiddleCenter,
                showMinimizeButton: showMinimize,
                showMaximizeButton: showMaximize,
                showCloseButton: showClose)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

            ContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundDark,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            rootLayout.Controls.Add(TitleBar, 0, 0);
            rootLayout.Controls.Add(ContentPanel, 0, 1);

            Controls.Add(rootLayout);
        }
    }
}