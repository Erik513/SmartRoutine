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

            // ---- Common (more)
            ["Common.Save"] = "Speichern",
            ["Common.Cancel"] = "Abbrechen",
            ["Common.Yes"] = "Ja",
            ["Common.No"] = "Nein",

            // ---- Step types (list of actions)
            ["StepType.OpenUrl"] = "Webseite öffnen",
            ["StepType.OpenFolder"] = "Ordner öffnen",
            ["StepType.OpenApplication"] = "Programm starten",
            ["StepType.OpenDocument"] = "Dokument öffnen",

            // ---- Editor: layout
            ["Editor.RoutineName"] = "Routinenname",
            ["Editor.StepsTitle"] = "Meine Schritte",
            ["Editor.StepEdit"] = "Schritt bearbeiten",
            ["Editor.StepNew"] = "Neuen Schritt erstellen",
            ["Editor.LastRun"] = "Zuletzt gestartet: {0}",
            ["Editor.Section.StepInfo"] = "Schrittinformationen",
            ["Editor.Section.Options"] = "Optionen",
            ["Editor.Row.Name"] = "Name",
            ["Editor.Row.Description"] = "Beschreibung",
            ["Editor.Row.Action"] = "Aktion",
            ["Editor.Row.Automation"] = "Automatisierung",
            ["Editor.Label.Start"] = "Start",
            ["Editor.Label.Continue"] = "Weiter",
            ["Editor.Toggle.StepEnabled"] = "Schritt ist aktiviert",
            ["Editor.Toggle.StepDisabled"] = "Schritt ist deaktiviert",
            ["Editor.Toggle.AutoStartOn"] = "Automatisch starten",
            ["Editor.Toggle.AutoStartOff"] = "Nicht automatisch starten",
            ["Editor.Toggle.ContinueOn"] = "Automatisch zum nächsten Schritt wechseln",
            ["Editor.Toggle.ContinueOff"] = "Nach Ausführung anhalten",
            ["Editor.Button.Execute"] = "Schritt ausführen",
            ["Editor.Button.Duplicate"] = "Schritt duplizieren",
            ["Editor.Button.Add"] = "Schritt hinzufügen",
            ["Editor.Button.Delete"] = "Schritt löschen",
            ["Editor.Button.Back"] = "Zurück zur Hauptansicht",

            // ---- Editor: dialogs and messages
            ["Editor.SaveChanges.Title"] = "Änderungen speichern",
            ["Editor.SaveChanges.Step"] = "Möchten Sie die Änderungen am aktuellen Schritt speichern?",
            ["Editor.SaveChanges.BeforeRun"] = "Möchten Sie die Änderungen vor der Ausführung speichern?",
            ["Editor.DeleteStep.Text"] = "Schritt '{0}' wirklich löschen?",
            ["Editor.CopySuffix"] = " Kopie",
            ["Editor.Toast.Duplicated"] = "✓ Schritt '{0}' dupliziert",
            ["Editor.Toast.Deleted"] = "✓ Schritt '{0}' gelöscht",
            ["Editor.Toast.Saved"] = "✓ Schritt '{0}' gespeichert",
            ["Editor.Validation.Title"] = "Validierung",
            ["Editor.Validation.ErrorTitle"] = "Fehler bei Validierung",
            ["Editor.Validation.RoutineNameEmpty"] = "Bitte geben Sie einen Namen für die Routine ein.",
            ["Editor.Validation.RoutineNameExists"] = "Eine Routine mit dem Namen '{0}' existiert bereits.\nBitte wählen Sie einen anderen Namen.",
            ["Editor.Validation.StepNameEmpty"] = "Bitte geben Sie einen Namen für den Schritt ein.",
            ["Editor.Validation.StepTypeEmpty"] = "Bitte wählen Sie einen Aktionstyp aus.",
            ["Editor.CannotRun.Title"] = "Ausführung nicht möglich",

            // ---- Step editors
            ["StepEditor.Url.Row.Url"] = "URL",
            ["StepEditor.Url.Row.OpenIn"] = "Öffnen in",
            ["StepEditor.Url.Toggle.External"] = "Externer Browser",
            ["StepEditor.Url.Toggle.InApp"] = "In App öffnen",
            ["StepEditor.Url.Validation.Empty"] = "Bitte geben Sie eine URL ein.",
            ["StepEditor.Folder.Row.Path"] = "Ordnerpfad",
            ["StepEditor.Folder.Row.NewWindow"] = "Neues Fenster",
            ["StepEditor.Folder.Browse"] = "Ordner auswählen",
            ["StepEditor.Folder.Validation.Empty"] = "Bitte geben Sie einen Ordnerpfad ein.",
            ["StepEditor.App.Row.Path"] = "Programmpfad",
            ["StepEditor.App.Row.Arguments"] = "Argumente",
            ["StepEditor.App.Row.RunAsAdmin"] = "Als Admin",
            ["StepEditor.App.Browse"] = "Programm auswählen",
            ["StepEditor.App.Filter"] = "Anwendungen (*.exe)|*.exe|Alle Dateien (*.*)|*.*",
            ["StepEditor.App.Validation.Empty"] = "Bitte geben Sie einen Programmpfad ein.",
            ["StepEditor.Document.Row.Path"] = "Dateipfad",
            ["StepEditor.Document.Browse"] = "Dokument auswählen",
            ["StepEditor.Document.Filter"] = "Alle Dateien (*.*)|*.*",
            ["StepEditor.Document.Validation.Empty"] = "Bitte wählen Sie eine Datei aus.",

            // ---- Execution window (step by step)
            ["Execution.Title"] = "Routine: {0}",
            ["Execution.TitleNoRoutine"] = "Routine",
            ["Execution.Button.Previous"] = "Vorheriger Schritt",
            ["Execution.Button.Next"] = "Nächster Schritt",
            ["Execution.Button.Finish"] = "Routine beenden",
            ["Execution.Button.Run"] = "Schritt ausführen",
            ["Execution.Error.Title"] = "Ausführung fehlgeschlagen",
            ["Execution.Error.Text"] = "Der Schritt '{0}' konnte nicht ausgeführt werden:\n\n{1}",
            ["Execution.Countdown"] = "Weiter in {0} Sekunden...",
            ["Execution.CountdownOne"] = "Weiter in 1 Sekunde...",
            ["Execution.Info.Title"] = "Beschreibung",
            ["Execution.Info.None"] = "Keine Beschreibung",
            ["Execution.Done.Url"] = "✓ URL geöffnet: {0}",
            ["Execution.Done.Folder"] = "✓ Ordner geöffnet: {0}",
            ["Execution.Done.Application"] = "✓ Gestartet: {0}",
            ["Execution.Done.Document"] = "✓ Geöffnet: {0}",
            ["Execution.Done.Other"] = "✓ {0} ausgeführt",
            ["Execution.WebViewFailed"] = "WebView konnte nicht geöffnet werden.",

            // ---- Automatic run
            ["AutoRun.Title"] = "Auto-Ausführung: {0}",
            ["AutoRun.TitleNoRoutine"] = "Auto-Ausführung",
            ["AutoRun.Status.Running"] = "Routine wird ausgeführt...",
            ["AutoRun.Status.NoSteps"] = "Keine ausführbaren Schritte.",
            ["AutoRun.Status.Done"] = "Alle Schritte wurden ausgeführt.",
            ["AutoRun.Status.Failed"] = "Nicht ausgeführt:\n{0}",
            ["AutoRun.Status.SaveFailed"] = "Die Routine wurde ausgeführt, aber nicht als ausgeführt gespeichert:\n{0}",
            ["AutoRun.UnnamedStep"] = "(ohne Namen)",
        };
    }
}
