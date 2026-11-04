using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using ErikwnkWFUI.Styles;
using SmartRoutine.Localization;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    /// <summary>
    /// Controls of the library whose texts (tooltips) are asked by key and follow the language.
    /// </summary>
    internal static class LocalizedControls
    {
        /// <summary>A symbol button whose tooltip is the text of <paramref name="tooltipKey"/>.</summary>
        public static Button CreateSymbolButton(UIButtonPreset preset, string tooltipKey, Size? size = null)
        {
            Button button = size.HasValue
                ? UIStyles.Buttons.CreateSymbol(preset, Loc.S(tooltipKey), size.Value)
                : UIStyles.Buttons.CreateSymbol(preset, Loc.S(tooltipKey));

            UIStyles.Localize(button, b => UIStyles.Buttons.UpdateTooltip(b, Loc.S(tooltipKey)));

            return button;
        }

        /// <summary>A toggle switch whose two tooltips (on / off) are texts by key.</summary>
        public static ToggleSwitch CreateToggle(bool isChecked, string checkedKey, string uncheckedKey)
        {
            ToggleSwitch toggle = UIStyles.ToggleSwitches.CreatePrimary(isChecked);

            UIStyles.Localize(toggle, t =>
            {
                t.ToolTipTextChecked = Loc.S(checkedKey);
                t.ToolTipTextUnchecked = Loc.S(uncheckedKey);
            });

            return toggle;
        }

        /// <summary>A yes / no toggle switch.</summary>
        public static ToggleSwitch CreateYesNoToggle(bool isChecked)
        {
            return CreateToggle(isChecked, "Common.Yes", "Common.No");
        }
    }
}
