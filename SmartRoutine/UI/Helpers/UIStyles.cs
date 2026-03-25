using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SmartRoutine.UI.Helpers
{
    public static class UIStyles
    {
        // Farbpalette für Dark Theme
        public static class Colors
        {
            // ===== HINTERGRUNDFARBEN (Dunkle Grautöne) =====


            public static Color Black = Color.Black;                            // Schwarz
            // Haupt-Hintergrund (tiefstes Schwarz)
            public static Color BackgroundBlack = Color.FromArgb(10, 10, 10);   
            // Dunkle Ebenen
            public static Color BackgroundDark = Color.FromArgb(20, 20, 20);
            public static Color BackgroundDarkElevated = Color.FromArgb(25, 25, 25);
            // Mittlere Ebenen
            public static Color BackgroundMedium = Color.FromArgb(35, 35, 35);
            public static Color BackgroundMediumElevated = Color.FromArgb(40, 40, 40);
            // Helle Ebenen (für Hover/Active States)
            public static Color BackgroundLight = Color.FromArgb(50, 50, 50);
            public static Color BackgroundLighter = Color.FromArgb(60, 60, 60);


            // ===== AKZENTFARBEN (Dunkelblau-Töne) =====

            // Primäres Blau (Hauptakzent)
            public static Color PrimaryDarkDark = Color.FromArgb(0, 30, 60);
            public static Color PrimaryDark = Color.FromArgb(0, 50, 90);        // Dunkleres Blau
            public static Color Primary = Color.FromArgb(0, 90, 158);           // Dunkelblau
            public static Color PrimaryLight = Color.FromArgb(0, 120, 215);     // Helleres Blau

            // Sekundäres Blau (für weniger wichtige Elemente)
            public static Color SecondaryDark = Color.FromArgb(20, 80, 140);
            public static Color Secondary = Color.FromArgb(30, 100, 180);
            public static Color SecondaryLight = Color.FromArgb(50, 130, 210);


            // ===== FUNKTIONSFARBEN =====

            // Erfolg/Positive Aktionen
            public static Color Success = Color.FromArgb(30, 150, 70);      // Dunkles Grün
            public static Color SuccessLight = Color.FromArgb(40, 180, 90);

            // Warnungen
            public static Color Warning = Color.FromArgb(200, 150, 0);      // Dunkles Gelb/Gold
            public static Color WarningLight = Color.FromArgb(230, 180, 30);

            // Fehler/Negative Aktionen
            public static Color Error = Color.FromArgb(180, 40, 50);        // Dunkles Rot
            public static Color ErrorLight = Color.FromArgb(210, 60, 70);


            // ===== TEXTFARBEN =====

            // Primärer Text (am hellsten)
            public static Color White = Color.White;
            public static Color TextPrimary = Color.FromArgb(240, 240, 240); // Fast weiß
            public static Color TextPrimaryDim = Color.FromArgb(220, 220, 220);

            // Sekundärer Text (für weniger wichtige Texte)
            public static Color TextSecondary = Color.FromArgb(180, 180, 180);
            public static Color TextTertiary = Color.FromArgb(140, 140, 140);

            // Deaktivierter Text
            public static Color TextDisabled = Color.FromArgb(100, 100, 100);


            // ===== RAHMEN & LINIEN =====

            // Rahmen für dunkle Hintergründe
            public static Color BorderDark = Color.FromArgb(50, 50, 50);
            public static Color BorderMedium = Color.FromArgb(70, 70, 70);
            public static Color BorderLight = Color.FromArgb(90, 90, 90);

            // Spezielle Rahmen
            public static Color BorderPrimary = Color.FromArgb(0, 100, 180); // Blauer Rahmen
            public static Color BorderError = Color.FromArgb(180, 40, 50);   // Roter Rahmen


            // ===== ÜBERLAGERUNGEN & EFFEKTE =====

            // Hover-Effekte
            public static Color HoverOverlay = Color.FromArgb(30, 30, 30, 80);
            public static Color ActiveOverlay = Color.FromArgb(40, 40, 40, 120);

            // Auswahl-Highlight
            public static Color Selection = Color.FromArgb(0, 90, 158, 60); // Transparentes Blau

            // Schatten/Transparenz
            public static Color Transparent = Color.Transparent;
            public static Color OverlayDark = Color.FromArgb(0, 0, 0, 180);
            public static Color OverlayMedium = Color.FromArgb(0, 0, 0, 120);
            public static Color OverlayLight = Color.FromArgb(0, 0, 0, 60);
            public static Color TextMuted = Color.FromArgb(120, 120, 120);
        }

        // Schriftarten
        public static class Fonts
        {
            public static Font Title = new Font("Segoe UI", 10, FontStyle.Bold);
            public static Font Normal = new Font("Segoe UI", 9);
            public static Font Small = new Font("Segoe UI", 8);
            public static Font Monospace = new Font("Consolas", 9);
            public static Font Icon = new Font("Segoe UI Symbol", 10);
        }

        // Button-Styles
        public static class Buttons
        {
            // Standard-Button
            public static Button CreateStandard(string text = "", string tooltip = "", Size? size = null)
            {
                var button = new Button
                {
                    Text = text,
                    Size = size ?? new Size(30, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Colors.BackgroundMedium,
                    ForeColor = Colors.TextPrimary,
                    Font = Fonts.Normal,
                    TabStop = false,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Colors.BorderDark;
                button.FlatAppearance.MouseOverBackColor = Colors.BackgroundLight;
                button.FlatAppearance.MouseDownBackColor = Colors.Primary;
                AddToolTip(button, tooltip);

                return button;
            }

            // Primary-Button (hervorstehend)
            public static Button CreatePrimary(string text = "", string tooltip = "", Size? size = null)
            {
                var button = new Button
                {
                    Text = text,
                    Size = size ?? new Size(30, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Colors.PrimaryDark,
                    ForeColor = Colors.TextPrimary,
                    Font = Fonts.Normal,
                    TabStop = false,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Colors.BorderDark;
                button.FlatAppearance.MouseOverBackColor = Colors.Primary;
                button.FlatAppearance.MouseDownBackColor = Colors.PrimaryLight;
                AddToolTip(button, tooltip);

                return button;
            }

            // Danger-Button (für Close/Actions)
            public static Button CreateDanger(string text = "", string tooltip = "", Size? size = null)
            {
                var button = new Button
                {
                    Text = text,
                    Size = size ?? new Size(30, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Colors.Warning,
                    ForeColor = Colors.Error,
                    Font = Fonts.Icon,
                    TabStop = false,
                    Cursor = Cursors.Hand
                };
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Colors.Error;
                button.FlatAppearance.MouseOverBackColor = Colors.ErrorLight;
                button.FlatAppearance.MouseDownBackColor = Colors.Error;
                AddToolTip(button, tooltip);

                return button;
            }
            

            private static void AddToolTip(Button button, string tooltip)
            {
                // Tooltip hinzufügen
                if (!string.IsNullOrEmpty(tooltip))
                {
                    var toolTip = ToolTips.CreateToolTip();
                    toolTip.SetToolTip(button, tooltip);
                }
            }

        }

        // Label-Styles
        public static class Labels
        {
            public static Label CreateTitle(string text)
            {
                return new Label
                {
                    Text = text,
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

            public static Label CreateNormal(string text)
            {
                return new Label
                {
                    Text = text,
                    ForeColor = Colors.TextSecondary,
                    Font = Fonts.Normal,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoEllipsis = true,
                    AllowDrop = true,
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                    UseMnemonic = false
                };
            }

            public static Label CreateMuted(string text)
            {
                return new Label
                {
                    Text = text,
                    ForeColor = Colors.TextMuted,
                    Font = Fonts.Small,
                    BackColor = Color.Transparent,
                    UseMnemonic = false
                };
            }
        }

        // TextBox-Styles
        public static class TextBoxes
        {
            public static TextBox CreateStandard(string text = "", string placeholder = "")
            {
                var textBox = new TextBox
                {
                    Text = text,
                    BackColor = Colors.BackgroundMedium,
                    ForeColor = Colors.TextPrimary,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = Fonts.Normal
                };

                if (!string.IsNullOrEmpty(placeholder))
                {
                    SetPlaceholder(textBox, placeholder);
                }

                return textBox;
            }

            private static void SetPlaceholder(TextBox textBox, string placeholder)
            {
                textBox.Text = placeholder;
                textBox.ForeColor = Colors.TextMuted;

                textBox.GotFocus += (s, e) =>
                {
                    if (textBox.Text == placeholder)
                    {
                        textBox.Text = "";
                        textBox.ForeColor = Colors.TextPrimary;
                    }
                };

                textBox.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        textBox.Text = placeholder;
                        textBox.ForeColor = Colors.TextMuted;
                    }
                };
            }
        }

        // ComboBox-Styles
        public static class ComboBoxes
        {
            public static ComboBox CreateStandard()
            {
                return new ComboBox
                {
                    BackColor = Colors.BackgroundMedium,
                    ForeColor = Colors.TextPrimary,
                    FlatStyle = FlatStyle.Flat,
                    Font = Fonts.Normal,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
            }
        }

        // Panel-Styles
        public static class Panels
        {
            public static Panel CreateDark()
            {
                return new Panel
                {
                    BackColor = Colors.PrimaryDarkDark,
                    BorderStyle = BorderStyle.None,
                    Padding = new Padding(0),
                    Margin = new Padding(0)
                };
            }

            public static Panel CreateMedium()
            {
                return new Panel
                {
                    BackColor = Colors.BackgroundMedium,
                    BorderStyle = BorderStyle.FixedSingle
                };
            }
        }

        // CheckBox-Styles
        public static class CheckBoxes
        {
            public static CheckBox CreateStandard(string text)
            {
                return new CheckBox
                {
                    Text = text,
                    ForeColor = Colors.TextSecondary,
                    BackColor = Color.Transparent,
                    Font = Fonts.Normal,
                    FlatStyle = FlatStyle.Flat
                };
            }
        }

        // ToolTip-Styles
        public static class ToolTips
        {
            public static ToolTip CreateToolTip(string text = "")
            {
                return new ToolTip
                {
                    ToolTipTitle = text,
                    BackColor = Colors.BackgroundMedium,
                    ForeColor = Colors.TextPrimary,
                    AutoPopDelay = 5000,
                    InitialDelay = 500,
                    ReshowDelay = 100,
                    UseAnimation = true,
                    UseFading = true
                };
            }
        }
    }
}
