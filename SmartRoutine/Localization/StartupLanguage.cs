using ErikwnkWFUI.Styles;
using System.Globalization;

namespace SmartRoutine.Localization
{
    /// <summary>
    /// The language the program starts in the first time: the language of Windows when the program has
    /// it (German), English otherwise. What the user chooses later in the bar at the bottom is kept by
    /// the library and wins over this.
    /// </summary>
    public static class StartupLanguage
    {
        public static UILanguage For(CultureInfo culture)
        {
            return culture != null && culture.TwoLetterISOLanguageName == "de"
                ? UILanguage.German
                : UILanguage.English;
        }

        /// <summary>The language for the user interface language of Windows.</summary>
        public static UILanguage FromWindows()
        {
            return For(CultureInfo.CurrentUICulture);
        }
    }
}
