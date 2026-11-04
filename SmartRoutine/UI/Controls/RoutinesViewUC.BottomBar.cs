using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using SmartRoutine.UI.Helpers;
using System.Drawing;
using System.Windows.Forms;
using ComboBox = System.Windows.Forms.ComboBox;
using FlowLayoutPanel = System.Windows.Forms.FlowLayoutPanel;
using TableLayoutPanel = System.Windows.Forms.TableLayoutPanel;

namespace SmartRoutine.UI.Controls
{
    // The bar at the bottom of the routine list: theme button and language choice on the left, the
    // version and - while there is one - the update button on the right (only on this page, not in the editor).
    public partial class RoutinesViewUC
    {
        private const int BottomBarHeight = 36;
        private const int BarControlHeight = 30;

        /// <summary>The bar at the bottom: the main window shows the update button in it.</summary>
        public VersionPanel VersionBar { get; private set; }

        private VersionPanel CreateBottomBar()
        {
            VersionPanel bar = UIStyles.VersionPanels.CreateStandard();
            bar.Dock = DockStyle.Bottom;
            bar.Height = BottomBarHeight;

            // Dark gray like the button panel above it (the standard strip is the darkest color of the theme).
            UIStyles.Colors.Follow(bar, () => bar.BackColor = UIStyles.Colors.BackgroundDarkElevated);

            // The controls stand in the middle of the bar's height (anchored left only).
            TableLayoutPanel left = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            left.Dock = DockStyle.Left;
            left.AutoSize = true;
            left.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            left.BackColor = Color.Transparent;
            left.Margin = new Padding(0);
            left.Padding = new Padding(10, 0, 0, 0);
            left.RowStyles.Clear();
            left.ColumnStyles.Clear();
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            left.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            Button themeButton = UIStyles.Buttons.CreateThemeToggleStandard(new Size(BarControlHeight, BarControlHeight));
            themeButton.Anchor = AnchorStyles.Left;
            themeButton.Margin = new Padding(0, 0, 8, 0);

            ComboBox languageBox = UIStyles.ComboBoxes.CreateLanguagePrimary();
            languageBox.Width = 120;
            languageBox.Anchor = AnchorStyles.Left;
            languageBox.Margin = new Padding(0);
            ComboDropUp.Enable(languageBox);

            left.Controls.Add(themeButton, 0, 0);
            left.Controls.Add(languageBox, 1, 0);

            bar.Controls.Add(left);

            VersionBar = bar;

            return bar;
        }
    }
}
