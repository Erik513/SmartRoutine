using System.Collections.Generic;

namespace SmartRoutine.Localization
{
    // German texts. Same keys as LocEn.
    internal static class LocDe
    {
        internal static readonly Dictionary<string, string> Texts = new Dictionary<string, string>
        {
            // ---- Common
            ["Common.Confirm"] = "Bestätigen",
            ["Common.Info"] = "Info",

            // ---- Time (relative time of the last run)
            ["Time.Never"] = "Noch nie gestartet",
            ["Time.JustNow"] = "Gerade eben",
            ["Time.MinuteAgo"] = "Vor 1 Minute",
            ["Time.MinutesAgo"] = "Vor {0} Minuten",
            ["Time.HourAgo"] = "Vor 1 Stunde",
            ["Time.HoursAgo"] = "Vor {0} Stunden",
            ["Time.DayAgo"] = "Vor 1 Tag",
            ["Time.DaysAgo"] = "Vor {0} Tagen",
            ["Time.DateFormat"] = "dd.MM.yyyy HH:mm",

            // ---- Routine list (main page)
            ["Routines.Title"] = "Meine Routinen",
            ["Routines.DefaultName"] = "Meine Routine {0}",
            ["Routines.Button.New"] = "Neue Routine erstellen",
            ["Routines.Button.Edit"] = "Routine bearbeiten",
            ["Routines.Button.Delete"] = "Routine löschen",
            ["Routines.Button.Start"] = "Routine starten",
            ["Routines.Button.StartAuto"] = "Routine mit Auto-Ausführung starten",
            ["Routines.Popup.LastRun"] = "Letzte Ausführung",
            ["Routines.Popup.Steps"] = "Schritte ({0})",
            ["Routines.Popup.DisabledSteps"] = "Ausgeblendete Schritte ({0})",
            ["Routines.Popup.None"] = "Keine",

            // ---- Main window (dialogs)
            ["Main.DeleteRoutine.Text"] = "Routine '{0}' wirklich löschen?",
            ["Main.AutoRun.Title"] = "Automatische Ausführung starten",
            ["Main.AutoRun.Text"] = "Möchten Sie die Routine '{0}' automatisch ausführen?\n\nAlle aktivierten Schritte werden nacheinander gestartet.\nInterne URL-Schritte werden dabei einmalig im externen Browser geöffnet.",
            ["Main.NoSteps.Text"] = "Diese Routine enthält keine Schritte.",
            ["Main.AllDisabled.Title"] = "Routine nicht ausführbar",
            ["Main.AllDisabled.Text"] = "Alle Schritte dieser Routine sind derzeit deaktiviert.",
        };
    }
}
