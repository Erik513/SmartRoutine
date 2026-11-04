using System.Collections.Generic;

namespace SmartRoutine.Localization
{
    // English texts. Same keys as LocDe; written for the situation, not word by word.
    internal static class LocEn
    {
        internal static readonly Dictionary<string, string> Texts = new Dictionary<string, string>
        {
            // ---- Common
            ["Common.Confirm"] = "Confirm",
            ["Common.Info"] = "Info",

            // ---- Time (relative time of the last run)
            ["Time.Never"] = "Never run",
            ["Time.JustNow"] = "Just now",
            ["Time.MinuteAgo"] = "1 minute ago",
            ["Time.MinutesAgo"] = "{0} minutes ago",
            ["Time.HourAgo"] = "1 hour ago",
            ["Time.HoursAgo"] = "{0} hours ago",
            ["Time.DayAgo"] = "1 day ago",
            ["Time.DaysAgo"] = "{0} days ago",
            ["Time.DateFormat"] = "MMM d, yyyy h:mm tt",

            // ---- Routine list (main page)
            ["Routines.Title"] = "My routines",
            ["Routines.DefaultName"] = "My routine {0}",
            ["Routines.Button.New"] = "New routine",
            ["Routines.Button.Edit"] = "Edit routine",
            ["Routines.Button.Delete"] = "Delete routine",
            ["Routines.Button.Start"] = "Start routine",
            ["Routines.Button.StartAuto"] = "Run routine automatically",
            ["Routines.Popup.LastRun"] = "Last run",
            ["Routines.Popup.Steps"] = "Steps ({0})",
            ["Routines.Popup.DisabledSteps"] = "Disabled steps ({0})",
            ["Routines.Popup.None"] = "None",

            // ---- Main window (dialogs)
            ["Main.DeleteRoutine.Text"] = "Delete routine '{0}'?",
            ["Main.AutoRun.Title"] = "Run automatically",
            ["Main.AutoRun.Text"] = "Run routine '{0}' automatically?\n\nAll enabled steps are started one after the other.\nSteps that open a URL inside the app will open it once in your external browser instead.",
            ["Main.NoSteps.Text"] = "This routine has no steps.",
            ["Main.AllDisabled.Title"] = "Can't run routine",
            ["Main.AllDisabled.Text"] = "All steps of this routine are currently disabled.",
        };
    }
}
