using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Forms;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace SmartRoutine.Tests.UI
{
    [TestClass]
    public class ExecutionFormTests
    {
        private static Routine CreateRoutineWithAutoStartStep()
        {
            return new Routine
            {
                Name = "Test",
                Steps = new List<RoutineStep>
                {
                    new OpenFolderStep { Name = "Ordner", Order = 0, Show = true, AutoStart = true, FolderPath = @"C:\gibt-es-nicht" }
                }
            };
        }

        [TestMethod]
        public void Constructor_WhenAutoStartStepFails_DoesNotThrow()
        {
            Func<RoutineStep, StepExecutionResult> failing =
                step => throw new InvalidOperationException("kaputt");

            using (ExecutionForm form = new ExecutionForm(CreateRoutineWithAutoStartStep(), failing))
            {
                Assert.IsNotNull(form);
            }
        }

        [TestMethod]
        public void Constructor_WhenAutoStartStepFails_TriesTheStepOnlyOnce()
        {
            int calls = 0;
            Func<RoutineStep, StepExecutionResult> failing = step =>
            {
                calls++;
                throw new InvalidOperationException("kaputt");
            };

            using (ExecutionForm form = new ExecutionForm(CreateRoutineWithAutoStartStep(), failing))
            {
                Assert.AreEqual(1, calls);
            }
        }

        private static Routine CreateManualRoutine()
        {
            return new Routine
            {
                Name = "Test",
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep { Id = "1", Name = "Eins", Order = 0, Show = true, AutoStart = false, Url = "https://a.example" },
                    new OpenUrlStep { Id = "2", Name = "Zwei", Order = 1, Show = true, AutoStart = false, Url = "https://b.example" }
                }
            };
        }

        // The form is not shown, so PerformClick (which needs a visible control) would do nothing.
        private static void Click(Button button)
        {
            typeof(Control)
                .GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(button, new object[] { EventArgs.Empty });
        }

        private static T Field<T>(ExecutionForm form, string name)
        {
            return (T)typeof(ExecutionForm)
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(form);
        }

        private static string SuccessMessage(RoutineStep step)
        {
            return (string)typeof(ExecutionForm)
                .GetMethod("GetSuccessMessage", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { step });
        }

        [TestMethod]
        public void SuccessMessage_NamesWhatWasOpened()
        {
            StringAssert.Contains(SuccessMessage(new OpenUrlStep { Url = "https://x.example" }), "https://x.example");
            StringAssert.Contains(SuccessMessage(new OpenFolderStep { FolderPath = @"C:\Daten" }), @"C:\Daten");
            StringAssert.Contains(SuccessMessage(new OpenApplicationStep { ApplicationPath = @"C:\Programme\app.exe" }), "app.exe");
            StringAssert.Contains(SuccessMessage(new OpenDocumentStep { FilePath = @"C:\Docs\brief.docx" }), "brief.docx");
        }

        [TestMethod]
        public void Constructor_ShowsFirstStepAndCounter()
        {
            using (ExecutionForm form = new ExecutionForm(CreateManualRoutine(), step => StepExecutionResult.None()))
            {
                Assert.AreEqual("1/2", Field<Label>(form, "_stepCounterLabel").Text);
                Assert.AreEqual("Eins", Field<Label>(form, "_stepNameLabel").Text);
                Assert.IsFalse(Field<Button>(form, "_previousButton").Enabled);
                Assert.IsTrue(Field<Button>(form, "_executeButton").Enabled);
            }
        }

        [TestMethod]
        public void ExecuteButton_RunsTheCurrentStepOnce()
        {
            var executed = new List<string>();

            using (ExecutionForm form = new ExecutionForm(CreateManualRoutine(), step => { executed.Add(step.Name); return StepExecutionResult.None(); }))
            {
                Click(Field<Button>(form, "_executeButton"));

                CollectionAssert.AreEqual(new[] { "Eins" }, executed);
            }
        }

        [TestMethod]
        public void ExecuteButton_WhenStepThrows_KeepsFormUsableAndStepNotExecuted()
        {
            using (ExecutionForm form = new ExecutionForm(CreateManualRoutine(), step => throw new InvalidOperationException("kaputt")))
            {
                Click(Field<Button>(form, "_executeButton"));

                var session = Field<RoutineExecutionSession>(form, "_session");

                Assert.IsFalse(session.CurrentStepWasExecuted);
                Assert.IsTrue(Field<Button>(form, "_executeButton").Enabled);
            }
        }

        [TestMethod]
        public void NextButton_GoesToNextStepAndShowsEndSymbolOnLastStep()
        {
            using (ExecutionForm form = new ExecutionForm(CreateManualRoutine(), step => StepExecutionResult.None()))
            {
                Button next = Field<Button>(form, "_nextButton");
                string forwardSymbol = next.Text;

                Click(next);

                Assert.AreEqual("2/2", Field<Label>(form, "_stepCounterLabel").Text);
                Assert.AreEqual("Zwei", Field<Label>(form, "_stepNameLabel").Text);
                Assert.AreNotEqual(forwardSymbol, next.Text);
                Assert.IsTrue(Field<Button>(form, "_previousButton").Enabled);
            }
        }

        [TestMethod]
        public void PreviousButton_GoesBackToFirstStep()
        {
            using (ExecutionForm form = new ExecutionForm(CreateManualRoutine(), step => StepExecutionResult.None()))
            {
                Click(Field<Button>(form, "_nextButton"));
                Click(Field<Button>(form, "_previousButton"));

                Assert.AreEqual("1/2", Field<Label>(form, "_stepCounterLabel").Text);
            }
        }

        [TestMethod]
        public void Constructor_RoutineWithoutSteps_DoesNotThrow()
        {
            using (ExecutionForm form = new ExecutionForm(new Routine { Name = "Leer" }, step => StepExecutionResult.None()))
            {
                Assert.IsNotNull(form);
            }
        }
    }
}
