using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Forms;
using System;
using System.Collections.Generic;

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
    }
}
