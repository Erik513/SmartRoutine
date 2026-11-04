using ErikwnkCore;
using System;
using System.Collections.Generic;

namespace SmartRoutine.Localization
{
    /// <summary>
    /// The texts of the program in German and English. A text is asked by its key ("Area.Name"):
    /// <c>Loc.S("Routines.Title")</c>, with arguments <c>Loc.S("Editor.StepSaved", name)</c> ({0}, {1} ... in the
    /// text). The current language is the one of the library (<c>UIStyles.Language</c>), so the language box
    /// in the bar at the bottom switches the texts of the program too.
    /// <para>
    /// Texts that stay on a control (labels, tooltips, headers, rows of a table) are given as functions
    /// (<see cref="F(string)"/>) to <c>UIStyles.Localize</c> / <c>AddRow</c>, so they follow a switch. Texts
    /// that are made when they are shown (dialogs, toasts, popups) are simply asked with <see cref="S(string)"/>.
    /// </para>
    /// <para>
    /// Rules for the tables (<c>LocDe.cs</c>, <c>LocEn.cs</c>): every key is in both; the English text is written
    /// for the situation and not translated word by word; a placeholder is in both texts or in none.
    /// </para>
    /// </summary>
    public static class Loc
    {
        static Loc()
        {
            AppLocalization.Register(AppLanguage.German, LocDe.Texts);
            AppLocalization.Register(AppLanguage.English, LocEn.Texts);
        }

        /// <summary>The text for the key in the current language (English when it is missing there, the key itself when it is missing everywhere).</summary>
        public static string S(string key)
        {
            return AppLocalization.Get(key);
        }

        /// <summary>The text for the key with {0}, {1} ... replaced by the arguments.</summary>
        public static string S(string key, params object[] args)
        {
            return AppLocalization.Get(key, args);
        }

        /// <summary>A function that asks the text each time: for controls that must follow the language.</summary>
        public static Func<string> F(string key)
        {
            return () => S(key);
        }

        /// <summary>A function for a text with arguments.</summary>
        public static Func<string> F(string key, params object[] args)
        {
            return () => S(key, args);
        }

        /// <summary>Raised when the language has changed (for what cannot be bound to one control).</summary>
        public static event EventHandler LanguageChanged
        {
            add { AppLocalization.LanguageChanged += value; }
            remove { AppLocalization.LanguageChanged -= value; }
        }

        /// <summary>All keys and texts of a language (for the tests that compare the tables).</summary>
        public static IReadOnlyDictionary<string, string> Table(AppLanguage language)
        {
            return language == AppLanguage.German ? LocDe.Texts : LocEn.Texts;
        }
    }
}
