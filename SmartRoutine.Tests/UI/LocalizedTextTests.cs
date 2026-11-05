using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using ErikwnkWFUI.Styles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Controls;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SmartRoutine.Tests.UI
{
    // The texts follow the language (the language box in the bar at the bottom switches it).
    [TestClass]
    public class LocalizedTextTests
    {
        private static readonly DateTime Now = new DateTime(2026, 11, 5, 12, 0, 0);

        [TestCleanup]
        public void BackToGerman()
        {
            UIStyles.Language = UILanguage.German;
        }

        // Relative time

        [TestMethod]
        public void RelativeTime_German()
        {
            UIStyles.Language = UILanguage.German;

            Assert.AreEqual("Noch nie gestartet", DateTimeHelper.GetRelativeTime(null, Now));
            Assert.AreEqual("Gerade eben", DateTimeHelper.GetRelativeTime(Now.AddSeconds(-30), Now));
            Assert.AreEqual("Vor 1 Minute", DateTimeHelper.GetRelativeTime(Now.AddMinutes(-1), Now));
            Assert.AreEqual("Vor 5 Minuten", DateTimeHelper.GetRelativeTime(Now.AddMinutes(-5), Now));
            Assert.AreEqual("Vor 1 Stunde", DateTimeHelper.GetRelativeTime(Now.AddHours(-1), Now));
            Assert.AreEqual("Vor 3 Stunden", DateTimeHelper.GetRelativeTime(Now.AddHours(-3), Now));
            Assert.AreEqual("Vor 1 Tag", DateTimeHelper.GetRelativeTime(Now.AddDays(-1), Now));
            Assert.AreEqual("Vor 2 Tagen", DateTimeHelper.GetRelativeTime(Now.AddDays(-2), Now));
            Assert.AreEqual("01.11.2026 12:00", DateTimeHelper.GetRelativeTime(Now.AddDays(-4), Now.AddDays(3)));
        }

        [TestMethod]
        public void RelativeTime_English()
        {
            UIStyles.Language = UILanguage.English;

            Assert.AreEqual("Never run", DateTimeHelper.GetRelativeTime(null, Now));
            Assert.AreEqual("Just now", DateTimeHelper.GetRelativeTime(Now.AddSeconds(-30), Now));
            Assert.AreEqual("1 minute ago", DateTimeHelper.GetRelativeTime(Now.AddMinutes(-1), Now));
            Assert.AreEqual("5 minutes ago", DateTimeHelper.GetRelativeTime(Now.AddMinutes(-5), Now));
            Assert.AreEqual("1 hour ago", DateTimeHelper.GetRelativeTime(Now.AddHours(-1), Now));
            Assert.AreEqual("3 hours ago", DateTimeHelper.GetRelativeTime(Now.AddHours(-3), Now));
            Assert.AreEqual("1 day ago", DateTimeHelper.GetRelativeTime(Now.AddDays(-1), Now));
            Assert.AreEqual("2 days ago", DateTimeHelper.GetRelativeTime(Now.AddDays(-2), Now));
            Assert.AreEqual("Nov 1, 2026 12:00 PM", DateTimeHelper.GetRelativeTime(Now.AddDays(-4), Now.AddDays(3)));
        }

        [TestMethod]
        public void RelativeTime_OlderThanAWeek_ShowsTheDate()
        {
            UIStyles.Language = UILanguage.German;

            Assert.AreEqual("20.10.2026 08:30", DateTimeHelper.GetRelativeTime(new DateTime(2026, 10, 20, 8, 30, 0), Now));
        }

        // Routine list

        private static RoutinesViewUC CreateView(out Mock<IRoutineService> service, params string[] names)
        {
            service = new Mock<IRoutineService>();
            var routines = names.Select((n, i) => new Routine { Id = "id" + i, Name = n, Order = i }).ToList();
            service.Setup(s => s.GetAllRoutines()).Returns(routines);

            return new RoutinesViewUC(service.Object);
        }

        private static T Field<T>(object owner, string name)
        {
            return (T)owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);
        }

        [TestMethod]
        public void RoutineList_TitleFollowsTheLanguage()
        {
            using (RoutinesViewUC view = CreateView(out _))
            {
                var list = Field<TitledListBox>(view, "lstRoutines");

                Assert.AreEqual("Meine Routinen", list.Title);

                UIStyles.Language = UILanguage.English;

                Assert.AreEqual("My routines", list.Title);

                UIStyles.Language = UILanguage.German;

                Assert.AreEqual("Meine Routinen", list.Title);
            }
        }

        [TestMethod]
        public void NewRoutine_GetsADefaultNameInTheCurrentLanguage()
        {
            UIStyles.Language = UILanguage.English;

            using (RoutinesViewUC view = CreateView(out Mock<IRoutineService> service))
            {
                service.Setup(s => s.CreateRoutine(It.IsAny<string>())).Returns(new Routine { Id = "n" });

                Field<Button>(view, "btnNewRoutine").PerformClick();

                service.Verify(s => s.CreateRoutine("My routine 1"), Times.Once);
            }
        }

        [TestMethod]
        public void NewRoutine_NumberingCountsTheNamesOfBothLanguages()
        {
            UIStyles.Language = UILanguage.English;

            using (RoutinesViewUC view = CreateView(out Mock<IRoutineService> service, "Meine Routine 3", "My routine 5", "Something else"))
            {
                service.Setup(s => s.CreateRoutine(It.IsAny<string>())).Returns(new Routine { Id = "n" });

                Field<Button>(view, "btnNewRoutine").PerformClick();

                service.Verify(s => s.CreateRoutine("My routine 6"), Times.Once);
            }
        }

        // Editor

        private static RoutineEditorUC CreateEditor()
        {
            return new RoutineEditorUC(new Mock<IRoutineService>().Object, new Mock<IUrlValidationService>().Object);
        }

        [TestMethod]
        public void Editor_ActionList_FollowsTheLanguageAndKeepsTheSelection()
        {
            using (RoutineEditorUC editor = CreateEditor())
            {
                var combo = Field<System.Windows.Forms.ComboBox>(editor, "cmbStepType");

                Assert.AreEqual(4, combo.Items.Count);
                Assert.AreEqual("Webseite öffnen", ((StepTypeOption)combo.Items[0]).DisplayName);

                combo.SelectedIndex = 2;

                UIStyles.Language = UILanguage.English;

                Assert.AreEqual(4, combo.Items.Count);
                Assert.AreEqual(2, combo.SelectedIndex);
                Assert.AreEqual("Start program", ((StepTypeOption)combo.SelectedItem).DisplayName);
                Assert.AreEqual(StepType.OpenApplication, ((StepTypeOption)combo.SelectedItem).Type);
            }
        }

        [TestMethod]
        public void Editor_ActionList_WithoutSelection_StaysWithoutSelection()
        {
            using (RoutineEditorUC editor = CreateEditor())
            {
                var combo = Field<System.Windows.Forms.ComboBox>(editor, "cmbStepType");

                UIStyles.Language = UILanguage.English;

                Assert.AreEqual(-1, combo.SelectedIndex);
                Assert.AreEqual("Open website", ((StepTypeOption)combo.Items[0]).DisplayName);
            }
        }

        [TestMethod]
        public void Editor_LastRunText_FollowsTheLanguage()
        {
            using (RoutineEditorUC editor = CreateEditor())
            {
                var label = Field<Label>(editor, "lblLastExecution");

                editor.LoadRoutine(new Routine { Id = "r", Name = "R", LastExecutionAt = DateTime.Now.AddMinutes(-5) });

                Assert.AreEqual("Zuletzt gestartet: Vor 5 Minuten", label.Text);

                UIStyles.Language = UILanguage.English;

                Assert.AreEqual("Last run: 5 minutes ago", label.Text);
            }
        }

        [TestMethod]
        public void Editor_LastRunText_IsEmptyForARoutineThatWasNeverRun()
        {
            using (RoutineEditorUC editor = CreateEditor())
            {
                editor.LoadRoutine(new Routine { Id = "r", Name = "R" });

                Assert.AreEqual("", Field<Label>(editor, "lblLastExecution").Text);
            }
        }

        [TestMethod]
        public void Editor_StepListTitleAndButtons_FollowTheLanguage()
        {
            using (RoutineEditorUC editor = CreateEditor())
            {
                var list = Field<TitledListBox>(editor, "lstSteps");

                Assert.AreEqual("Meine Schritte", list.Title);

                UIStyles.Language = UILanguage.English;

                Assert.AreEqual("My steps", list.Title);
            }
        }

        [TestMethod]
        public void Editor_DuplicateStep_GetsTheCopySuffixOfTheCurrentLanguage()
        {
            var method = typeof(RoutineEditorUC).GetMethod("InsertDuplicateAfter", BindingFlags.NonPublic | BindingFlags.Static);
            var original = new OpenUrlStep { Id = "a", Name = "Open news", Url = "https://news.example" };

            UIStyles.Language = UILanguage.English;

            var copy = (RoutineStep)method.Invoke(null, new object[] { new List<RoutineStep> { original }, original });

            Assert.AreEqual("Open news copy", copy.Name);
        }

        // Execution window and automatic run

        private static Routine ManualRoutine()
        {
            return new Routine
            {
                Name = "Morning",
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep { Id = "1", Name = "One", Order = 0, Show = true, AutoStart = false, Url = "https://a.example" },
                    new OpenUrlStep { Id = "2", Name = "Two", Order = 1, Show = true, AutoStart = false, Url = "https://b.example" }
                }
            };
        }

        private static string SuccessMessage(RoutineStep step)
        {
            return (string)typeof(SmartRoutine.UI.Forms.ExecutionForm)
                .GetMethod("GetSuccessMessage", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { step });
        }

        [TestMethod]
        public void Execution_SuccessMessages_AreWrittenForTheLanguage()
        {
            UIStyles.Language = UILanguage.German;
            Assert.AreEqual("✓ Ordner geöffnet: C:/Daten", SuccessMessage(new OpenFolderStep { FolderPath = "C:/Daten" }));
            Assert.AreEqual("✓ Gestartet: app.exe", SuccessMessage(new OpenApplicationStep { ApplicationPath = "C:/x/app.exe" }));

            UIStyles.Language = UILanguage.English;
            Assert.AreEqual("✓ Opened folder: C:/Daten", SuccessMessage(new OpenFolderStep { FolderPath = "C:/Daten" }));
            Assert.AreEqual("✓ Started: app.exe", SuccessMessage(new OpenApplicationStep { ApplicationPath = "C:/x/app.exe" }));
            Assert.AreEqual("✓ Opened URL: https://x.example", SuccessMessage(new OpenUrlStep { Url = "https://x.example" }));
            Assert.AreEqual("✓ Opened: brief.docx", SuccessMessage(new OpenDocumentStep { FilePath = "C:/brief.docx" }));
            Assert.AreEqual("✓ Other completed", SuccessMessage(new UnknownStep { Name = "Other" }));
        }

        private class UnknownStep : RoutineStep
        {
            public override StepType Type => StepType.OpenUrl;
        }

        [TestMethod]
        public void Execution_WindowTitleAndButtons_AreEnglishWhenTheLanguageIs()
        {
            UIStyles.Language = UILanguage.English;

            using (var form = new SmartRoutine.UI.Forms.ExecutionForm(ManualRoutine(), step => SmartRoutine.Logic.Services.StepExecutionResult.None()))
            {
                StringAssert.Contains(form.Text, "Routine: Morning");
            }
        }

        [TestMethod]
        public void AutoRun_TitleAndStatus_AreEnglishWhenTheLanguageIs()
        {
            UIStyles.Language = UILanguage.English;

            using (var form = new SmartRoutine.UI.Forms.AutoRunForm(ManualRoutine(), null, null))
            {
                StringAssert.Contains(form.Text, "Auto run: Morning");
                Assert.AreEqual("Running routine...", form.Status);
                Assert.AreEqual("Cancel", form.CancelButtonText);
            }
        }

        [TestMethod]
        public void AutoRun_Result_IsEnglishWhenTheLanguageIs()
        {
            UIStyles.Language = UILanguage.English;
            var routine = ManualRoutine();

            using (var form = new SmartRoutine.UI.Forms.AutoRunForm(routine, step => throw new InvalidOperationException("broken"), null))
            {
                form.StepDelayMilliseconds = 1;
                form.Show();

                DateTime end = DateTime.Now.AddSeconds(10);

                while (form.State == ErikwnkWFUI.Forms.ProgressState.Running && DateTime.Now < end)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(5);
                }

                StringAssert.StartsWith(form.Status, "These steps couldn't be run:");
                StringAssert.Contains(form.Status, "One (broken)");
            }
        }

        // Messages of the services

        private static string Validation(RoutineStep step)
        {
            var service = new SmartRoutine.Logic.Services.RoutineService(new InMemoryRepository());
            service.ValidateStep(step, out string message);

            return message;
        }

        [TestMethod]
        public void StepValidation_Messages_FollowTheLanguage()
        {
            UIStyles.Language = UILanguage.German;
            Assert.AreEqual("Die URL ist leer.", Validation(new OpenUrlStep { Url = "" }));
            Assert.AreEqual("Der Ordnerpfad ist leer.", Validation(new OpenFolderStep { FolderPath = "" }));

            UIStyles.Language = UILanguage.English;
            Assert.AreEqual("The URL is empty.", Validation(new OpenUrlStep { Url = "" }));
            Assert.AreEqual("The folder path is empty.", Validation(new OpenFolderStep { FolderPath = "" }));
            Assert.AreEqual("The program path is empty.", Validation(new OpenApplicationStep { ApplicationPath = "" }));
            Assert.AreEqual("The file path is empty.", Validation(new OpenDocumentStep { FilePath = "" }));
            Assert.AreEqual("The step is missing.", Validation(null));
            Assert.AreEqual("The URL 'nonsense' isn't valid.", Validation(new OpenUrlStep { Url = "nonsense" }));
        }

        [TestMethod]
        public void StepValidation_MissingPaths_NameThePathInEnglish()
        {
            UIStyles.Language = UILanguage.English;
            string message = Validation(new OpenFolderStep { FolderPath = "C:/does-not-exist-12345" });

            StringAssert.StartsWith(message, "The folder doesn't exist:");
            StringAssert.Contains(message, "C:/does-not-exist-12345");
        }

        [TestMethod]
        public void UrlValidation_Messages_FollowTheLanguage()
        {
            var service = new SmartRoutine.Logic.Services.UrlValidationService();

            UIStyles.Language = UILanguage.German;
            StringAssert.Contains(service.ValidateAndRepairUrl("foo bar").ErrorMessage, "Leerzeichen");
            Assert.AreEqual("Die URL darf nicht leer sein.", service.ValidateAndRepairUrl("").ErrorMessage);

            UIStyles.Language = UILanguage.English;
            Assert.AreEqual("URLs can't contain spaces.", service.ValidateAndRepairUrl("foo bar").ErrorMessage);
            Assert.AreEqual("The URL can't be empty.", service.ValidateAndRepairUrl("").ErrorMessage);
            StringAssert.StartsWith(service.ValidateAndRepairUrl("abc").ErrorMessage, "The text you entered isn't a valid URL.");
            StringAssert.Contains(service.ValidateAndRepairUrl("abc").ErrorMessage, "Entered: abc");
        }

        [TestMethod]
        public void MissingDocument_ExceptionMessage_IsEnglishWhenTheLanguageIs()
        {
            UIStyles.Language = UILanguage.English;
            var service = new SmartRoutine.Logic.Services.RoutineService(new InMemoryRepository());

            var exception = Assert.ThrowsExactly<System.IO.FileNotFoundException>(
                () => service.ExecuteStep(new OpenDocumentStep { FilePath = "C:/missing/brief.docx" }));

            StringAssert.StartsWith(exception.Message, "The file doesn't exist:");
        }

        // Walk through: after a switch to English no German text of the program is left on the screen.

        private static void CollectTexts(Control control, HashSet<string> texts)
        {
            if (!string.IsNullOrWhiteSpace(control.Text))
                texts.Add(control.Text.Trim());

            foreach (Control child in control.Controls)
                CollectTexts(child, texts);
        }

        // Texts that are German on purpose in the English table too (names, symbols) are not a leftover.
        private static HashSet<string> GermanOnlyTexts()
        {
            var german = SmartRoutine.Localization.Loc.Table(ErikwnkCore.AppLanguage.German);
            var english = SmartRoutine.Localization.Loc.Table(ErikwnkCore.AppLanguage.English);

            return new HashSet<string>(german
                .Where(p => english.ContainsKey(p.Key) && english[p.Key] != p.Value && !p.Value.Contains("{"))
                .Select(p => p.Value.Trim()));
        }

        [TestMethod]
        public void SwitchToEnglish_LeavesNoGermanTextOnTheEditor()
        {
            var routine = new Routine { Id = "r", Name = "R" };
            routine.Steps.Add(new OpenUrlStep { Id = "1", Name = "A", Order = 0, Url = "https://a.example" });
            routine.Steps.Add(new OpenFolderStep { Id = "2", Name = "B", Order = 1 });
            routine.Steps.Add(new OpenApplicationStep { Id = "3", Name = "C", Order = 2 });
            routine.Steps.Add(new OpenDocumentStep { Id = "4", Name = "D", Order = 3 });

            using (RoutineEditorUC editor = CreateEditor())
            {
                editor.LoadRoutine(routine);

                var list = Field<TitledListBox>(editor, "lstSteps");
                var german = GermanOnlyTexts();

                for (int i = 0; i < 4; i++)
                {
                    UIStyles.Language = UILanguage.German;
                    list.SelectedIndex = i;

                    // The walk finds the texts at all (otherwise "no German text left" would mean nothing).
                    var germanTexts = new HashSet<string>();
                    CollectTexts(editor, germanTexts);
                    Assert.IsTrue(germanTexts.Any(german.Contains), "No German text found in German mode (step " + i + ").");

                    UIStyles.Language = UILanguage.English;

                    var texts = new HashSet<string>();
                    CollectTexts(editor, texts);

                    var leftovers = texts.Where(german.Contains).ToList();

                    Assert.AreEqual(0, leftovers.Count, "German texts after the switch (step " + i + "): " + string.Join(" | ", leftovers));
                }
            }
        }

        [TestMethod]
        public void SwitchToEnglish_LeavesNoGermanTextOnTheRoutineList()
        {
            using (RoutinesViewUC view = CreateView(out _, "Meine Routine 1"))
            {
                UIStyles.Language = UILanguage.English;

                var texts = new HashSet<string>();
                CollectTexts(view, texts);

                var leftovers = texts.Where(GermanOnlyTexts().Contains).ToList();

                Assert.AreEqual(0, leftovers.Count, "German texts after the switch: " + string.Join(" | ", leftovers));
            }
        }
    }
}
