using System;
using System.Drawing;
using System.Windows.Forms;
using SmartRoutine.UI.Helpers;

namespace SmartRoutine.UI.Controls
{
    public partial class StyledListBoxWithHeader : UserControl
    {
        private Panel headerPanel;
        private Label lblTitle;
        private StyledListBox listBox;

        private string _title = "";
        private int _headerHeight = 30;
        private Color _headerBackColor = UIStyles.Colors.BackgroundDark;
        private Color _headerForeColor = UIStyles.Colors.TextPrimary;
        private Font _headerFont = UIStyles.Fonts.Title;

        // Events der inneren ListBox nach außen weiterleiten
        public event EventHandler SelectedIndexChanged;
        public event EventHandler ItemsReordered;

        public StyledListBoxWithHeader()
        {
            InitializeControl();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();
        }
        public StyledListBoxWithHeader(string title, ContentAlignment textAlign = ContentAlignment.MiddleLeft) : this()
        {
            _title = title;
            lblTitle.Text = title;
            lblTitle.TextAlign = textAlign;

            // Padding anpassen bei Linksbündig vs. Zentriert
            if (textAlign == ContentAlignment.MiddleLeft)
            {
                lblTitle.Padding = new Padding(10, 0, 0, 0);
            }
            else if (textAlign == ContentAlignment.MiddleCenter)
            {
                lblTitle.Padding = new Padding(0, 0, 0, 0);
            }
            else if (textAlign == ContentAlignment.MiddleRight)
            {
                lblTitle.Padding = new Padding(0, 0, 10, 0);
            }
        }

        private void InitializeControl()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Transparent;

            // Header Panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = _headerHeight,
                BackColor = _headerBackColor
            };

            lblTitle = new Label
            {
                Text = _title,
                Dock = DockStyle.Fill,
                ForeColor = _headerForeColor,
                Font = _headerFont,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.Transparent
            };

            headerPanel.Controls.Add(lblTitle);

            // ListBox
            listBox = new StyledListBox
            {
                Dock = DockStyle.Fill,
                MinimumSize = new Size(0, 50)
            };

            // Events weiterleiten
            listBox.SelectedIndexChanged += (s, e) => SelectedIndexChanged?.Invoke(s, e);
            listBox.ItemsReordered += (s, e) => ItemsReordered?.Invoke(s, e);

            this.Controls.Add(listBox);
            this.Controls.Add(headerPanel);
        }

        // ========== Öffentliche Eigenschaften ==========

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                lblTitle.Text = value;
                headerPanel.Visible = !string.IsNullOrEmpty(value);
            }
        }

        public int HeaderHeight
        {
            get => _headerHeight;
            set
            {
                _headerHeight = value;
                headerPanel.Height = value;
            }
        }

        public Color HeaderBackColor
        {
            get => _headerBackColor;
            set
            {
                _headerBackColor = value;
                headerPanel.BackColor = value;
            }
        }

        public Color HeaderForeColor
        {
            get => _headerForeColor;
            set
            {
                _headerForeColor = value;
                lblTitle.ForeColor = value;
            }
        }

        public Font HeaderFont
        {
            get => _headerFont;
            set
            {
                _headerFont = value;
                lblTitle.Font = value;
            }
        }

        // ListBox Eigenschaften durchreichen
        public ListBox.ObjectCollection Items => listBox.Items;

        public object SelectedItem
        {
            get => listBox.SelectedItem;
            set => listBox.SelectedItem = value;
        }

        public int SelectedIndex
        {
            get => listBox.SelectedIndex;
            set => listBox.SelectedIndex = value;
        }

        public int ItemHeightCustom
        {
            get => listBox.ItemHeightCustom;
            set => listBox.ItemHeightCustom = value;
        }

        public Color DragIndicatorColor
        {
            get => listBox.DragIndicatorColor;
            set => listBox.DragIndicatorColor = value;
        }

        // ListBox Methoden durchreichen
        public void ClearSelected() => listBox.ClearSelected();

        public void MoveItem(int fromIndex, int toIndex) => listBox.MoveItem(fromIndex, toIndex);

        public void BeginUpdate() => listBox.BeginUpdate();

        public void EndUpdate() => listBox.EndUpdate();
    }
}