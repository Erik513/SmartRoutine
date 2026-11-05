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

            // ---- Common (more)
            ["Common.Save"] = "Save",
            ["Common.Cancel"] = "Cancel",
            ["Common.Yes"] = "Yes",
            ["Common.No"] = "No",

            // ---- Step types (list of actions)
            ["StepType.OpenUrl"] = "Open website",
            ["StepType.OpenFolder"] = "Open folder",
            ["StepType.OpenApplication"] = "Start program",
            ["StepType.OpenDocument"] = "Open document",

            // ---- Editor: layout
            ["Editor.RoutineName"] = "Routine name",
            ["Editor.StepsTitle"] = "My steps",
            ["Editor.StepEdit"] = "Edit step",
            ["Editor.StepNew"] = "New step",
            ["Editor.LastRun"] = "Last run: {0}",
            ["Editor.Section.StepInfo"] = "Step details",
            ["Editor.Section.Options"] = "Options",
            ["Editor.Row.Name"] = "Name",
            ["Editor.Row.Description"] = "Description",
            ["Editor.Row.Action"] = "Action",
            ["Editor.Row.Automation"] = "Automation",
            ["Editor.Label.Start"] = "Start",
            ["Editor.Label.Continue"] = "Continue",
            ["Editor.Toggle.StepEnabled"] = "Step is enabled",
            ["Editor.Toggle.StepDisabled"] = "Step is disabled",
            ["Editor.Toggle.AutoStartOn"] = "Start automatically",
            ["Editor.Toggle.AutoStartOff"] = "Don't start automatically",
            ["Editor.Toggle.ContinueOn"] = "Move on to the next step automatically",
            ["Editor.Toggle.ContinueOff"] = "Stop after running",
            ["Editor.Button.Execute"] = "Run step",
            ["Editor.Button.Duplicate"] = "Duplicate step",
            ["Editor.Button.Add"] = "Add step",
            ["Editor.Button.Delete"] = "Delete step",
            ["Editor.Button.Back"] = "Back to routines",

            // ---- Editor: dialogs and messages
            ["Editor.SaveChanges.Title"] = "Save changes",
            ["Editor.SaveChanges.Step"] = "Save the changes to this step?",
            ["Editor.SaveChanges.BeforeRun"] = "Save your changes before running the step?",
            ["Editor.DeleteStep.Text"] = "Delete step '{0}'?",
            ["Editor.CopySuffix"] = " copy",
            ["Editor.Toast.Duplicated"] = "✓ Step '{0}' duplicated",
            ["Editor.Toast.Deleted"] = "✓ Step '{0}' deleted",
            ["Editor.Toast.Saved"] = "✓ Step '{0}' saved",
            ["Editor.Validation.Title"] = "Check your input",
            ["Editor.Validation.ErrorTitle"] = "Check your input",
            ["Editor.Validation.RoutineNameEmpty"] = "Please enter a name for the routine.",
            ["Editor.Validation.RoutineNameExists"] = "A routine named '{0}' already exists.\nPlease choose a different name.",
            ["Editor.Validation.StepNameEmpty"] = "Please enter a name for the step.",
            ["Editor.Validation.StepTypeEmpty"] = "Please choose an action.",
            ["Editor.CannotRun.Title"] = "Can't run step",

            // ---- Step editors
            ["StepEditor.Url.Row.Url"] = "URL",
            ["StepEditor.Url.Row.OpenIn"] = "Open in",
            ["StepEditor.Url.Toggle.External"] = "External browser",
            ["StepEditor.Url.Toggle.InApp"] = "Inside the app",
            ["StepEditor.Url.Validation.Empty"] = "Please enter a URL.",
            ["StepEditor.Folder.Row.Path"] = "Folder path",
            ["StepEditor.Folder.Row.NewWindow"] = "New window",
            ["StepEditor.Folder.Browse"] = "Choose folder",
            ["StepEditor.Folder.Validation.Empty"] = "Please enter a folder path.",
            ["StepEditor.App.Row.Path"] = "Program path",
            ["StepEditor.App.Row.Arguments"] = "Arguments",
            ["StepEditor.App.Row.RunAsAdmin"] = "Run as admin",
            ["StepEditor.App.Browse"] = "Choose program",
            ["StepEditor.App.Filter"] = "Applications (*.exe)|*.exe|All files (*.*)|*.*",
            ["StepEditor.App.Validation.Empty"] = "Please enter a program path.",
            ["StepEditor.Document.Row.Path"] = "File path",
            ["StepEditor.Document.Browse"] = "Choose document",
            ["StepEditor.Document.Filter"] = "All files (*.*)|*.*",
            ["StepEditor.Document.Validation.Empty"] = "Please choose a file.",

            // ---- Execution window (step by step)
            ["Execution.Title"] = "Routine: {0}",
            ["Execution.TitleNoRoutine"] = "Routine",
            ["Execution.Button.Previous"] = "Previous step",
            ["Execution.Button.Next"] = "Next step",
            ["Execution.Button.Finish"] = "Finish routine",
            ["Execution.Button.Run"] = "Run step",
            ["Execution.Error.Title"] = "Couldn't run step",
            ["Execution.Error.Text"] = "Couldn't run step '{0}':\n\n{1}",
            ["Execution.Countdown"] = "Continuing in {0} seconds...",
            ["Execution.CountdownOne"] = "Continuing in 1 second...",
            ["Execution.Info.Title"] = "Description",
            ["Execution.Info.None"] = "No description",
            ["Execution.Done.Url"] = "✓ Opened URL: {0}",
            ["Execution.Done.Folder"] = "✓ Opened folder: {0}",
            ["Execution.Done.Application"] = "✓ Started: {0}",
            ["Execution.Done.Document"] = "✓ Opened: {0}",
            ["Execution.Done.Other"] = "✓ {0} completed",
            ["Execution.WebViewFailed"] = "Couldn't open the built-in browser.",

            // ---- Automatic run
            ["AutoRun.Title"] = "Auto run: {0}",
            ["AutoRun.TitleNoRoutine"] = "Auto run",
            ["AutoRun.Status.Running"] = "Running routine...",
            ["AutoRun.Status.NoSteps"] = "There are no steps to run.",
            ["AutoRun.Status.Done"] = "All steps completed.",
            ["AutoRun.Status.Failed"] = "These steps couldn't be run:\n{0}",
            ["AutoRun.Status.SaveFailed"] = "The routine ran, but it couldn't be marked as run:\n{0}",
            ["AutoRun.UnnamedStep"] = "(unnamed)",

            // ---- Check of the steps before a run
            ["Main.PreCheck.Title"] = "Problems in this routine",
            ["Main.PreCheck.Text"] = "Some steps of this routine can't be run:\n\n{0}\n\nStart anyway?",
            ["Main.PreCheck.More"] = "… and {0} more",

            // ---- Program start
            ["Program.AlreadyRunning"] = "SmartRoutine is already running.",
            ["Program.StartFailed"] = "SmartRoutine couldn't start.",
            ["Program.UnexpectedError"] = "An unexpected error occurred.",
            ["Program.LogHint"] = "Details are saved in:\n{0}",

            // ---- Validation of a step (RoutineService)
            ["Validate.Step.Null"] = "The step is missing.",
            ["Validate.Url.Empty"] = "The URL is empty.",
            ["Validate.Url.Invalid"] = "The URL '{0}' isn't valid.",
            ["Validate.Folder.Empty"] = "The folder path is empty.",
            ["Validate.Folder.Missing"] = "The folder doesn't exist:\n{0}",
            ["Validate.App.Empty"] = "The program path is empty.",
            ["Validate.App.Missing"] = "The program doesn't exist:\n{0}",
            ["Validate.Document.Empty"] = "The file path is empty.",
            ["Validate.Document.Missing"] = "The file doesn't exist:\n{0}",

            // ---- Validation of a URL (UrlValidationService)
            ["Url.Error.Spaces"] = "URLs can't contain spaces.",
            ["Url.Error.Email"] = "This looks like an email address. Use the format \"mailto:email@example.com\".",
            ["Url.Error.WindowsPath"] = "This looks like a Windows file path. Use the format \"file:///C:/path/to/file\".",
            ["Url.Error.Other"] = "The text you entered isn't a valid URL.\n\nEntered: {0}\n\nExamples of valid URLs:\n• https://example.com\n• http://192.168.1.1\n• mailto:user@example.com\n• file:///C:/Users/Name/file.txt",
            ["Url.Error.Empty"] = "The URL can't be empty.",
        };
    }
}
