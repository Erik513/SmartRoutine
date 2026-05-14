using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartRoutine.UI.Controls
{
    public partial class StyledPropertyTable : UserControl
    {
        private readonly TableLayoutPanel _layout;

        public StyledPropertyTable()
        {
            Dock = DockStyle.Fill;
            BackColor = UIStyles.Colors.BackgroundMedium;

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 0,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

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
            int row = _layout.RowCount++;
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            var label = UIStyles.Labels.CreateNormal(labelText);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Padding = new Padding(10, 0, 0, 0);
            label.Margin = new Padding(0);

            if (editorControl is ToggleSwitch)
            {
                editorControl.Dock = DockStyle.None;
                editorControl.Anchor = AnchorStyles.Left;
                editorControl.Margin = new Padding(6, 8, 10, 6);
            }
            else
            {
                editorControl.Dock = DockStyle.Fill;
                editorControl.Margin = new Padding(6, 6, 10, 6);
            }

            _layout.Controls.Add(label, 0, row);
            _layout.Controls.Add(editorControl, 1, row);
        }

        public void AddSection(string title)
        {
            int row = _layout.RowCount++;
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            var label = UIStyles.Labels.CreateTitle(title);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Padding = new Padding(10, 0, 0, 0);
            label.Margin = new Padding(0);
            label.BackColor = UIStyles.Colors.BackgroundDark;

            _layout.Controls.Add(label, 0, row);
            _layout.SetColumnSpan(label, 2);
        }
    }
}
