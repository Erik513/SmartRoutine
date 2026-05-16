using System;
using System.Drawing;
using System.Windows.Forms;
using SmartRoutine.UI.Helpers;

namespace SmartRoutine.UI.Controls
{
    public partial class StyledListBoxControl : UserControl
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

        private readonly string _displayTextMember;
        private readonly bool _showEnumeration;

        public StyledListBox InnerListBox => listBox;
        public new event MouseEventHandler MouseMove
        {
            add => listBox.MouseMove += value;
            remove => listBox.MouseMove -= value;
        }

        public new event EventHandler MouseLeave
        {
            add => listBox.MouseLeave += value;
            remove => listBox.MouseLeave -= value;
        }

        public Func<object, bool> IsItemDisabled
        {
            get => listBox.IsItemDisabled;
            set => listBox.IsItemDisabled = value;
        }

        public StyledListBoxControl()
            : this(
                title: "",
                displayTextMember: null,
                showHeader: false,
                allowReorder: false,
                showEnumeration: false,
                textAlign: ContentAlignment.MiddleLeft)
        {
        }
        public StyledListBoxControl(
            string title = "",
            string displayTextMember = null,
            bool showHeader = true,
            bool allowReorder = false,
            bool showEnumeration = false,
            ContentAlignment textAlign = ContentAlignment.MiddleLeft)
        {
            _title = title;
            _displayTextMember = displayTextMember;
            _showEnumeration = showEnumeration;

            InitializeControl(showHeader, allowReorder, textAlign);

            lblTitle.Text = title;
            lblTitle.TextAlign = textAlign;
            headerPanel.Visible = showHeader && !string.IsNullOrWhiteSpace(title);

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw, true);

            UpdateStyles();
        }

        public int IndexFromPoint(Point point)
        {
            return listBox.IndexFromPoint(point);
        }

        private void InitializeControl(bool showHeader, bool allowReorder, ContentAlignment textAlign)
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Transparent;

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = _headerHeight,
                BackColor = _headerBackColor,
                Visible = showHeader
            };

            lblTitle = new Label
            {
                Text = _title,
                Dock = DockStyle.Fill,
                ForeColor = _headerForeColor,
                Font = _headerFont,
                TextAlign = textAlign,
                Padding = GetHeaderPadding(textAlign),
                BackColor = Color.Transparent
            };

            headerPanel.Controls.Add(lblTitle);

            listBox = new StyledListBox(allowReorder)
            {
                Dock = DockStyle.Fill,
                DisplayTextMember = _displayTextMember,
                ShowEnumeration = _showEnumeration
            };

            // Events weiterleiten
            listBox.SelectedIndexChanged += (s, e) => SelectedIndexChanged?.Invoke(s, e);
            listBox.ItemsReordered += (s, e) => ItemsReordered?.Invoke(s, e);

            this.Controls.Add(listBox);
            this.Controls.Add(headerPanel);
        }

        private Padding GetHeaderPadding(ContentAlignment textAlign)
        {
            switch (textAlign)
            {
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.TopLeft:
                case ContentAlignment.BottomLeft:
                    return new Padding(10, 0, 0, 0);

                case ContentAlignment.MiddleRight:
                case ContentAlignment.TopRight:
                case ContentAlignment.BottomRight:
                    return new Padding(0, 0, 10, 0);

                default:
                    return new Padding(0);
            }
        }

        // ========== Öffentliche Eigenschaften ==========

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                lblTitle.Text = value;
                headerPanel.Visible = !string.IsNullOrWhiteSpace(value);
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