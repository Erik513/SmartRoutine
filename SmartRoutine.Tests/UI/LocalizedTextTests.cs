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
    }
}
