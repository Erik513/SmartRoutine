using SmartRoutine.UI.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Controls
{
    public partial class StyledPropertyTable : UserControl
    {
        private const int LabelColumnWidth = 130;
        private const int RowHeight = 42;
        private const int SectionHeight = 32;
        private const int LabelLeftPadding = 10;

        private readonly TableLayoutPanel _layout;

        public StyledPropertyTable()
        {
            ConfigureControl();

            _layout = CreateLayoutPanel();

            Controls.Add(_layout);
        }

        public void ClearRows()
        {
            _layout.Controls.Clear();
            _layout.RowStyles.Clear();
            _layout.RowCount = 0;
        }

        public void AddRow(string labelText, Control editorControl)
        {
            if (editorControl == null)
                return;

            int row = AddRowStyle(RowHeight);

            Label label = CreateRowLabel(labelText);

            ConfigureEditorControl(editorControl);

            _layout.Controls.Add(label, 0, row);
            _layout.Controls.Add(editorControl, 1, row);
        }

        public void AddSection(string title)
        {
            int row = AddRowStyle(SectionHeight);

            Label label = CreateSectionLabel(title);

            _layout.Controls.Add(label, 0, row);
            _layout.SetColumnSpan(label, 2);
        }

        private void ConfigureControl()
        {
            Dock = DockStyle.Fill;
            BackColor = UIStyles.Colors.BackgroundMedium;
            Margin = new Padding(0);
            Padding = new Padding(0);
        }

        private TableLayoutPanel CreateLayoutPanel()
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 0,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelColumnWidth));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            return layout;
        }

        private int AddRowStyle(int height)
        {
            int row = _layout.RowCount;

            _layout.RowCount++;
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));

            return row;
        }

        private Label CreateRowLabel(string labelText)
        {
            Label label = UIStyles.Labels.CreateNormal(labelText ?? "");

            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Padding = new Padding(LabelLeftPadding, 0, 0, 0);
            label.Margin = new Padding(0);

            return label;
        }

        private Label CreateSectionLabel(string title)
        {
            Label label = UIStyles.Labels.CreateTitle(title ?? "");

            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Padding = new Padding(LabelLeftPadding, 0, 0, 0);
            label.Margin = new Padding(0);
            label.BackColor = UIStyles.Colors.BackgroundDark;

            return label;
        }

        private void ConfigureEditorControl(Control editorControl)
        {
            if (editorControl is ToggleSwitch)
            {
                editorControl.Dock = DockStyle.None;
                editorControl.Anchor = AnchorStyles.Left;
                editorControl.Margin = new Padding(6, 8, 10, 6);
                return;
            }

            editorControl.Dock = DockStyle.Fill;
            editorControl.Margin = new Padding(6, 6, 10, 6);
        }
    }
}