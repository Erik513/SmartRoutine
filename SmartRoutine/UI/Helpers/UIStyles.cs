using SmartRoutine.UI.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    public static class UIStyles
    {
        public static class Colors
        {
            public static readonly Color Black = Color.Black;

            public static readonly Color BackgroundBlack = Color.FromArgb(10, 10, 10);
            public static readonly Color BackgroundDark = Color.FromArgb(20, 20, 20);
            public static readonly Color BackgroundDarkElevated = Color.FromArgb(25, 25, 25);
            public static readonly Color BackgroundMedium = Color.FromArgb(35, 35, 35);
            public static readonly Color BackgroundMediumElevated = Color.FromArgb(40, 40, 40);
            public static readonly Color BackgroundLight = Color.FromArgb(50, 50, 50);
            public static readonly Color BackgroundLighter = Color.FromArgb(60, 60, 60);

            public static readonly Color PrimaryDarkDark = Color.FromArgb(0, 30, 60);
            public static readonly Color PrimaryDark = Color.FromArgb(0, 50, 90);
            public static readonly Color Primary = Color.FromArgb(0, 90, 158);
            public static readonly Color PrimaryLight = Color.FromArgb(0, 120, 215);

            public static readonly Color SecondaryDark = Color.FromArgb(20, 80, 140);
            public static readonly Color Secondary = Color.FromArgb(30, 100, 180);
            public static readonly Color SecondaryLight = Color.FromArgb(50, 130, 210);

            public static readonly Color GreenDark = Color.FromArgb(20, 100, 50);
            public static readonly Color Green = Color.FromArgb(30, 150, 70);
            public static readonly Color GreenLight = Color.FromArgb(40, 180, 90);
            public static readonly Color GreenLighter = Color.FromArgb(50, 210, 110);

            public static readonly Color YellowDark = Color.FromArgb(170, 125, 0);
            public static readonly Color Yellow = Color.FromArgb(200, 150, 0);
            public static readonly Color YellowLight = Color.FromArgb(230, 180, 30);
            public static readonly Color YellowLighter = Color.FromArgb(255, 210, 60);

            public static readonly Color RedDark = Color.FromArgb(150, 20, 30);
            public static readonly Color Red = Color.FromArgb(180, 40, 50);
            public static readonly Color RedLight = Color.FromArgb(210, 60, 70);

            public static readonly Color White = Color.White;
            public static readonly Color TextPrimary = Color.FromArgb(240, 240, 240);
            public static readonly Color TextPrimaryDim = Color.FromArgb(220, 220, 220);
            public static readonly Color TextSecondary = Color.FromArgb(180, 180, 180);
            public static readonly Color TextTertiary = Color.FromArgb(140, 140, 140);
            public static readonly Color TextDisabled = Color.FromArgb(100, 100, 100);
            public static readonly Color TextMuted = Color.FromArgb(120, 120, 120);

            public static readonly Color BorderDark = Color.FromArgb(50, 50, 50);
            public static readonly Color BorderMedium = Color.FromArgb(70, 70, 70);
            public static readonly Color BorderLight = Color.FromArgb(90, 90, 90);
            public static readonly Color BorderPrimary = Color.FromArgb(0, 100, 180);
            public static readonly Color BorderRed = Color.FromArgb(180, 40, 50);

            public static readonly Color HoverOverlay = Color.FromArgb(30, 30, 30, 80);
            public static readonly Color ActiveOverlay = Color.FromArgb(40, 40, 40, 120);
            public static readonly Color Selection = Color.FromArgb(0, 90, 158, 60);

            public static readonly Color Transparent = Color.Transparent;
            public static readonly Color OverlayDark = Color.FromArgb(0, 0, 0, 180);
            public static readonly Color OverlayMedium = Color.FromArgb(0, 0, 0, 120);
            public static readonly Color OverlayLight = Color.FromArgb(0, 0, 0, 60);
        }

        public static class Fonts
        {
            public static readonly Font Title = new Font("Segoe UI", 10, FontStyle.Bold);
            public static readonly Font Normal = new Font("Segoe UI", 9);
            public static readonly Font Small = new Font("Segoe UI", 8);
            public static readonly Font Monospace = new Font("Consolas", 9);
            public static readonly Font Icon = new Font("Segoe UI Symbol", 13f);
            public static readonly Font Emoji = new Font("Segoe UI Emoji", 11);
        }

        public static class Buttons
        {
            private static readonly Size DefaultButtonSize = new Size(30, 30);
            private static readonly Size DefaultIconButtonSize = new Size(32, 32);

            public static Button CreateStandard(
                string text = "",
                string tooltip = "",
                Size? size = null,
                bool isIcon = false)
            {
                return CreateStyledButton(
                    text,
                    tooltip,
                    size,
                    isIcon,
                    Colors.BackgroundMedium,
                    Colors.TextPrimary,
                    Colors.BorderDark,
                    1,
                    Colors.BackgroundLight,
                    Colors.Primary,
                    Colors.BackgroundMedium,
                    Colors.TextPrimary);
            }

            public static Button CreatePrimary(
                string text = "",
                string tooltip = "",
                Size? size = null,
                bool isIcon = false)
            {
                return CreateStyledButton(
                    text,
                    tooltip,
                    size,
                    isIcon,
                    Colors.PrimaryDark,
                    Colors.TextPrimary,
                    Colors.BorderDark,
                    0,
                    Colors.Primary,
                    Colors.PrimaryLight,
                    Colors.PrimaryDark,
                    Colors.TextPrimary);
            }

            public static Button CreateGreen(
                string text = "",
                string tooltip = "",
                Size? size = null,
                bool isIcon = false)
            {
                return CreateStyledButton(
                    text,
                    tooltip,
                    size,
                    isIcon,
                    Colors.GreenDark,
                    Colors.TextPrimary,
                    Colors.BorderDark,
                    1,
                    Colors.Green,
                    Colors.GreenLight,
                    Colors.GreenDark,
                    Colors.TextPrimary);
            }

            public static Button CreateDanger(
                string text = "",
                string tooltip = "",
                Size? size = null,
                bool isIcon = false)
            {
                return CreateStyledButton(
                    text,
                    tooltip,
                    size,
                    isIcon,
                    Colors.RedDark,
                    Colors.TextPrimary,
                    Colors.BorderDark,
                    1,
                    Colors.Red,
                    Colors.RedLight,
                    Colors.RedDark,
                    Colors.TextDisabled);
            }

            public static Button CreateBrowseInFolder(
                string tooltip = "",
                Size? size = null,
                bool isIcon = true)
            {
                return CreateStyledButton(
                    "📁",
                    tooltip,
                    size,
                    isIcon,
                    Colors.Yellow,
                    Colors.TextPrimary,
                    Colors.BorderDark,
                    1,
                    Colors.YellowLight,
                    Colors.YellowLighter,
                    Colors.Yellow,
                    Colors.TextDisabled);
            }

            public static Button CreateIconButton(string text, int size = 32)
            {
                Button button = new Button
                {
                    Text = text,
                    Size = size > 0 ? new Size(size, size) : DefaultIconButtonSize,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 255, 255, 255),
                    ForeColor = Colors.TextPrimary,
                    Font = Fonts.Emoji,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(4),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 255, 255, 255);
                button.FlatAppearance.MouseDownBackColor = Color.FromArgb(90, 255, 255, 255);

                button.Resize += OnRoundIconButtonResize;
                ApplyRoundRegion(button);

                return button;
            }

            private static Button CreateStyledButton(
                string text,
                string tooltip,
                Size? size,
                bool isIcon,
                Color backColor,
                Color foreColor,
                Color borderColor,
                int borderSize,
                Color mouseOverBackColor,
                Color mouseDownBackColor,
                Color disabledBackColor,
                Color disabledForeColor)
            {
                Button button = new Button
                {
                    Text = text ?? "",
                    Size = size ?? DefaultButtonSize,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = backColor,
                    ForeColor = foreColor,
                    Font = isIcon ? Fonts.Icon : Fonts.Normal,
                    TabStop = false,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0),
                    Padding = isIcon ? new Padding(0) : new Padding(6, 0, 6, 0),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                button.FlatAppearance.BorderSize = borderSize;
                button.FlatAppearance.BorderColor = borderColor;
                button.FlatAppearance.MouseOverBackColor = mouseOverBackColor;
                button.FlatAppearance.MouseDownBackColor = mouseDownBackColor;

                SetEnabledStyle(button, backColor, foreColor);

                AddToolTip(button, tooltip);

                return button;
            }

            private static void SetEnabledStyle(
                Button button,
                Color enabledBackColor,
                Color enabledForeColor)
            {
                if (button == null)
                    return;

                Color disabledBackColor = GetDisabledBackColor(enabledBackColor);
                Color disabledForeColor = Colors.TextDisabled;

                ApplyEnabledStyle(
                    button,
                    enabledBackColor,
                    enabledForeColor,
                    disabledBackColor,
                    disabledForeColor);

                button.EnabledChanged += delegate
                {
                    ApplyEnabledStyle(
                        button,
                        enabledBackColor,
                        enabledForeColor,
                        disabledBackColor,
                        disabledForeColor);
                };
            }

            private static Color Darken(Color color, double factor)
            {
                factor = Math.Max(0, Math.Min(1, factor));

                return Color.FromArgb(
                    color.A,
                    Math.Max(0, Math.Min(255, (int)(color.R * factor))),
                    Math.Max(0, Math.Min(255, (int)(color.G * factor))),
                    Math.Max(0, Math.Min(255, (int)(color.B * factor))));
            }

            private static Color GetDisabledBackColor(Color enabledBackColor)
            {
                return Darken(enabledBackColor, 0.65);
            }

            private static void ApplyEnabledStyle(
                Button button,
                Color enabledBackColor,
                Color enabledForeColor,
                Color disabledBackColor,
                Color disabledForeColor)
            {
                if (button == null)
                    return;

                button.BackColor = button.Enabled
                    ? enabledBackColor
                    : disabledBackColor;

                button.ForeColor = button.Enabled
                    ? enabledForeColor
                    : disabledForeColor;
            }

            private static void AddToolTip(Button button, string tooltip)
            {
                if (button == null || string.IsNullOrWhiteSpace(tooltip))
                    return;

                ToolTip toolTip = ToolTips.CreateToolTip();
                toolTip.SetToolTip(button, tooltip);

                button.Disposed += delegate
                {
                    toolTip.Dispose();
                };
            }

            private static void OnRoundIconButtonResize(object sender, EventArgs e)
            {
                Button button = sender as Button;

                if (button == null)
                    return;

                ApplyRoundRegion(button);
            }

            private static void ApplyRoundRegion(Button button)
            {
                if (button == null || button.Width <= 0 || button.Height <= 0)
                    return;

                Region oldRegion = button.Region;
                GraphicsPath path = new GraphicsPath();

                try
                {
                    path.AddEllipse(0, 0, button.Width, button.Height);
                    button.Region = new Region(path);
                }
                finally
                {
                    path.Dispose();

                    if (oldRegion != null)
                        oldRegion.Dispose();
                }
            }
        }

        public static class Labels
        {
            public static Label CreateTitle(string text = "")
            {
                return new Label
                {
                    Text = text ?? "",
                    ForeColor = Colors.TextPrimary,
                    Font = Fonts.Title,
                    BackColor = Colors.BackgroundDark,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoEllipsis = true,
                    AllowDrop = true,
                    AutoSize = false,
                    UseMnemonic = false
                };
            }

            public static Label CreateNormal(string text = "")
            {
                return new Label
                {
                    Text = text ?? "",
                    ForeColor = Colors.TextSecondary,
                    Font = Fonts.Normal,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true,
                    AllowDrop = true,
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                    UseMnemonic = false
                };
            }

            public static Label CreateMuted(string text = "")
            {
                return new Label
                {
                    Text = text ?? "",
                    ForeColor = Colors.TextMuted,
                    Font = Fonts.Small,
                    BackColor = Color.Transparent,
                    UseMnemonic = false
                };
            }
        }

        public static class TextBoxes
        {
            public static TextBox CreateStandard(string text = "", string placeholder = "")
            {
                TextBox textBox = new TextBox
                {
                    Text = text ?? "",
                    BackColor = Colors.BackgroundMedium,
                    ForeColor = Colors.TextPrimary,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = Fonts.Normal
                };

                if (!string.IsNullOrWhiteSpace(placeholder))
                    SetPlaceholder(textBox, placeholder);

                return textBox;
            }
            public static TextBox CreateBorderstyleNone(string text = "", string placeholder = "")
            {
                TextBox textBox = new TextBox
                {
                    Text = text ?? "",
                    BackColor = Colors.BackgroundLight,
                    ForeColor = Colors.TextPrimary,
                    BorderStyle = BorderStyle.None,
                    Font = Fonts.Normal
                };

                if (!string.IsNullOrWhiteSpace(placeholder))
                    SetPlaceholder(textBox, placeholder);

                return textBox;
            }

            private static void SetPlaceholder(TextBox textBox, string placeholder)
            {
                if (textBox == null)
                    return;

                string placeholderText = placeholder ?? "";

                textBox.Text = placeholderText;
                textBox.ForeColor = Colors.TextMuted;

                textBox.GotFocus += delegate
                {
                    if (textBox.Text != placeholderText)
                        return;

                    textBox.Text = "";
                    textBox.ForeColor = Colors.TextPrimary;
                };

                textBox.LostFocus += delegate
                {
                    if (!string.IsNullOrWhiteSpace(textBox.Text))
                        return;

                    textBox.Text = placeholderText;
                    textBox.ForeColor = Colors.TextMuted;
                };
            }
        }

        public static class ComboBoxes
        {
            public static ComboBox CreateStandard(ComboBoxStyle comboBoxStyle)
            {
                return new ComboBox
                {
                    BackColor = Colors.BackgroundMedium,
                    ForeColor = Colors.TextPrimary,
                    FlatStyle = FlatStyle.Flat,
                    Font = Fonts.Normal,
                    DropDownStyle = comboBoxStyle
                };
            }
        }

        public static class Panels
        {
            public static Panel CreateDark()
            {
                return CreatePanel(Colors.BackgroundDark, DockStyle.Fill);
            }

            public static Panel CreateMedium()
            {
                return CreatePanel(Colors.BackgroundMedium, DockStyle.Fill);
            }

            public static Panel CreateElevated()
            {
                return CreatePanel(Colors.BackgroundMediumElevated, DockStyle.Fill);
            }

            public static Panel CreateTransparent()
            {
                return CreatePanel(Color.Transparent, DockStyle.None);
            }

            private static Panel CreatePanel(Color backColor, DockStyle dock)
            {
                return new Panel
                {
                    Dock = dock,
                    BackColor = backColor,
                    BorderStyle = BorderStyle.None,
                    Padding = new Padding(0),
                    Margin = new Padding(0)
                };
            }
        }

        public static class CheckBoxes
        {
            public static CheckBox CreateStandard(
                string text = "",
                bool checkedState = true)
            {
                return new CheckBox
                {
                    Text = text ?? "",
                    Checked = checkedState,
                    ForeColor = Colors.TextSecondary,
                    BackColor = Color.Transparent,
                    Font = Fonts.Normal,
                    FlatStyle = FlatStyle.Flat
                };
            }

            public static CheckBox CreateCompact(bool checkedState = true)
            {
                CheckBox checkBox = CreateStandard("", checkedState);
                checkBox.Size = new Size(25, 25);

                return checkBox;
            }
        }

        public static class ToggleSwitches
        {
            public static ToggleSwitch CreateStandard(
                bool checkedState = true,
                string tooltipChecked = null,
                string tooltipUnchecked = null)
            {
                return CreateToggleSwitch(
                    checkedState,
                    new Size(45, 25),
                    tooltipChecked,
                    tooltipUnchecked);
            }

            public static ToggleSwitch CreateSmall(
                bool checkedState = true,
                string tooltipChecked = null,
                string tooltipUnchecked = null)
            {
                return CreateToggleSwitch(
                    checkedState,
                    new Size(35, 20),
                    tooltipChecked,
                    tooltipUnchecked);
            }

            public static ToggleSwitch CreateLarge(
                bool checkedState = true,
                string tooltipChecked = null,
                string tooltipUnchecked = null)
            {
                return CreateToggleSwitch(
                    checkedState,
                    new Size(55, 30),
                    tooltipChecked,
                    tooltipUnchecked);
            }

            private static ToggleSwitch CreateToggleSwitch(
                bool checkedState,
                Size size,
                string tooltipChecked,
                string tooltipUnchecked)
            {
                return new ToggleSwitch
                {
                    Checked = checkedState,
                    Size = size,
                    ToolTipTextChecked = tooltipChecked,
                    ToolTipTextUnchecked = tooltipUnchecked,
                    BackColor = Colors.Transparent
                };
            }
        }

        public static class TableLayoutPanels
        {
            public static TableLayoutPanel CreateStandard(int columnCount, int rowCount)
            {
                return CreateTableLayoutPanel(
                    columnCount,
                    rowCount,
                    Colors.BackgroundMediumElevated);
            }

            public static TableLayoutPanel CreateDark(int columnCount, int rowCount)
            {
                return CreateTableLayoutPanel(
                    columnCount,
                    rowCount,
                    Colors.BackgroundDark);
            }

            private static TableLayoutPanel CreateTableLayoutPanel(
                int columnCount,
                int rowCount,
                Color backColor)
            {
                return new TableLayoutPanel
                {
                    ColumnCount = Math.Max(0, columnCount),
                    RowCount = Math.Max(0, rowCount),
                    BackColor = backColor,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };
            }
        }

        public static class ToolTips
        {
            public static ToolTip CreateToolTip(string text = "")
            {
                return new ToolTip
                {
                    ToolTipTitle = text ?? "",
                    BackColor = Colors.BackgroundMedium,
                    ForeColor = Colors.TextPrimary,
                    AutoPopDelay = 10000,
                    InitialDelay = 1000,
                    ReshowDelay = 300,
                    UseAnimation = true,
                    UseFading = true
                };
            }
        }
    }
}