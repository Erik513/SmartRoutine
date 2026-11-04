using ErikwnkWFUI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SmartRoutine.Tests.UI
{
    [TestClass]
    public class AutoRunFormTests
    {
        [TestMethod]
        public void Constructor_ShowsRoutineNameInTitleAndIsCancelable()
        {
            using (AutoRunForm form = new AutoRunForm(new Routine { Name = "Morgen" }, null, null))
            {
                StringAssert.Contains(form.Text, "Morgen");
                Assert.IsTrue(form.Cancelable);
                Assert.IsFalse(form.IsCancelled);
            }
        }

        [TestMethod]
        public void Constructor_WithoutRoutine_DoesNotThrow()
        {
            using (AutoRunForm form = new AutoRunForm(null, null, null))
            {
                Assert.AreEqual(ProgressState.Running, form.State);
            }
        }

        private static Routine CreateRoutine(int count)
        {
            Routine routine = new Routine { Name = "Test" };

            for (int i = 0; i < count; i++)
                routine.Steps.Add(new OpenUrlStep { Id = "id" + i, Order = i, Name = "S" + i, Show = true, Url = "https://example.com" });

            return routine;
        }

        private static void RunUntilEnded(AutoRunForm form)
        {
            form.StepDelayMilliseconds = 1;
            form.Show();

            DateTime end = DateTime.Now.AddSeconds(10);

            while (form.State == ProgressState.Running && DateTime.Now < end)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(5);
            }
        }

        [TestMethod]
        public void Run_AllStepsSucceed_SucceedsAndMarksRoutine()
        {
            int executed = 0;
            int marked = 0;
            Routine routine = CreateRoutine(3);

            using (AutoRunForm form = new AutoRunForm(routine, s => { executed++; return StepExecutionResult.None(); }, r => marked++))
            {
                RunUntilEnded(form);

                Assert.AreEqual(ProgressState.Succeeded, form.State);
                Assert.AreEqual(3, executed);
                Assert.AreEqual(1, marked);
                Assert.IsNotNull(routine.LastExecutionAt);
            }
        }

        [TestMethod]
        public void Run_StepThrows_FailsWithReasonAndDoesNotMarkRoutine()
        {
            int executed = 0;
            int marked = 0;
            Routine routine = CreateRoutine(3);

            using (AutoRunForm form = new AutoRunForm(
                routine,
                s =>
                {
                    executed++;

                    if (s.Name == "S1")
                        throw new InvalidOperationException("kaputt");

                    return StepExecutionResult.None();
                },
                r => marked++))
            {
                RunUntilEnded(form);

                Assert.AreEqual(ProgressState.Failed, form.State);
                Assert.AreEqual(3, executed);
                Assert.AreEqual(0, marked);
                Assert.IsNull(routine.LastExecutionAt);
                StringAssert.Contains(form.Status, "kaputt");
            }
        }

        [TestMethod]
        public void Run_CallbackThrows_Fails()
        {
            using (AutoRunForm form = new AutoRunForm(
                CreateRoutine(1),
                s => StepExecutionResult.None(),
                r => { throw new Exception("db"); }))
            {
                RunUntilEnded(form);

                Assert.AreEqual(ProgressState.Failed, form.State);
                StringAssert.Contains(form.Status, "db");
            }
        }

        [TestMethod]
        public void Run_NoSteps_Fails()
        {
            using (AutoRunForm form = new AutoRunForm(CreateRoutine(0), s => StepExecutionResult.None(), null))
            {
                RunUntilEnded(form);

                Assert.AreEqual(ProgressState.Failed, form.State);
            }
        }
    }
}
