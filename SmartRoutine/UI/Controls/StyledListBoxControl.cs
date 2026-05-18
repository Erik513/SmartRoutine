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


        private int _headerHeight = 30;
        private Color _headerBackColor = UIStyles.Colors.BackgroundDark;
        private Color _headerForeColor = UIStyles.Colors.TextPrimary;
        private Font _headerFont = UIStyles.Fonts.Title;

        // Events der inneren ListBox nach außen weiterleiten
        public event EventHandler SelectedIndexChanged;
        public event EventHandler ItemsReordered;

        private readonly string _displayTextMember;
        private readonly bool _allowReorder;
        private readonly bool _showEnumeration;
        private string _headerTitle;
        private readonly ContentAlignment _headerTextAlign;

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

        public Func<object, Image> IconProvider
        {
            get => listBox.IconProvider;
            set => listBox.IconProvider = value;
        }

        public StyledListBoxControl() { }

        public StyledListBoxControl(
            string displayTextMember = null,
            bool allowReorder = false,
            bool showEnumeration = false,
            string headerTitle = null,
            ContentAlignment headerTextAlign = ContentAlignment.MiddleLeft) : this()
        {

            _displayTextMember = displayTextMember;
            _allowReorder = allowReorder;
            _showEnumeration = showEnumeration;
            _headerTitle = headerTitle ?? "";
            _headerTextAlign = headerTextAlign;
            InitializeControl();


            SetStyle(ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ResizeRedraw, true);

            UpdateStyles();
        }

        public int IndexFromPoint(Point point)
        {
            return listBox.IndexFromPoint(point);
        }

        private void InitializeControl()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Transparent;

            bool showHeader = !string.IsNullOrWhiteSpace(_headerTitle);

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = _headerHeight,
                BackColor = _headerBackColor,
                Visible = showHeader
            };

            lblTitle = new Label
            {
                Text = _headerTitle,
                Dock = DockStyle.Fill,
                ForeColor = _headerForeColor,
                Font = _headerFont,
                TextAlign = _headerTextAlign,
                Padding = GetHeaderPadding(_headerTextAlign),
                BackColor = Color.Transparent
            };

            headerPanel.Controls.Add(lblTitle);

            listBox = new StyledListBox
            {
                Dock = DockStyle.Fill,
                DisplayTextMember = _displayTextMember,
                ShowEnumeration = _showEnumeration,
                AllowReorder = _allowReorder
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
            get => _headerTitle;
            set
            {
                _headerTitle = value;
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