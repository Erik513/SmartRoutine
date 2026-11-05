using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using System;
using System.Collections.Generic;

namespace SmartRoutine.Tests
{
    [TestClass]
    public class RoutineStepFactoryTests
    {
        private static void AssertCommonFieldsCopied(RoutineStep original, RoutineStep copy)
        {
            Assert.AreNotSame(original, copy);
            Assert.AreEqual(original.GetType(), copy.GetType());
            Assert.AreEqual(original.Id, copy.Id);
            Assert.AreEqual(original.Order, copy.Order);
            Assert.AreEqual(original.Name, copy.Name);
            Assert.AreEqual(original.Description, copy.Description);
            Assert.AreEqual(original.Show, copy.Show);
            Assert.AreEqual(original.AutoStart, copy.AutoStart);
            Assert.AreEqual(original.AutoContinue, copy.AutoContinue);
        }

        private static OpenUrlStep Url() => new OpenUrlStep
        {
            Id = "u", Order = 3, Name = "Url", Description = "D", Show = false, AutoStart = false, AutoContinue = false,
            Url = "https://example.com", OpenInExternalBrowser = false
        };

        private static OpenFolderStep Folder() => new OpenFolderStep
        {
            Id = "f", Order = 1, Name = "Folder", Description = "D", Show = true, AutoStart = false, AutoContinue = true,
            FolderPath = @"C:\Daten", OpenInNewWindow = false
        };

        private static OpenApplicationStep App() => new OpenApplicationStep
        {
            Id = "a", Order = 2, Name = "App", Description = "D", Show = true, AutoStart = false, AutoContinue = false,
            ApplicationPath = @"C:\app.exe", Arguments = "-x", RunAsAdmin = true, WorkingDirectory = @"C:\"
        };

        private static OpenDocumentStep Doc() => new OpenDocumentStep
        {
            Id = "d", Order = 4, Name = "Doc", Description = "D", Show = true, AutoStart = true, AutoContinue = false,
            FilePath = @"C:\a.docx", OpenWithAssociatedApp = false
        };

        [TestMethod]
        public void CreateCopy_Null_ReturnsNull()
        {
            Assert.IsNull(RoutineStepFactory.CreateCopy(null));
            Assert.IsNull(RoutineStepFactory.CreateAutoRunCopy(null));
        }

        [TestMethod]
        public void CreateCopy_UrlStep_CopiesAllFields()
        {
            var original = Url();
            var copy = (OpenUrlStep)RoutineStepFactory.CreateCopy(original);

            AssertCommonFieldsCopied(original, copy);
            Assert.AreEqual(original.Url, copy.Url);
            Assert.AreEqual(original.OpenInExternalBrowser, copy.OpenInExternalBrowser);
        }

        [TestMethod]
        public void CreateCopy_FolderStep_CopiesAllFields()
        {
            var original = Folder();
            var copy = (OpenFolderStep)RoutineStepFactory.CreateCopy(original);

            AssertCommonFieldsCopied(original, copy);
            Assert.AreEqual(original.FolderPath, copy.FolderPath);
            Assert.AreEqual(original.OpenInNewWindow, copy.OpenInNewWindow);
        }

        [TestMethod]
        public void CreateCopy_ApplicationStep_CopiesAllFields()
        {
            var original = App();
            var copy = (OpenApplicationStep)RoutineStepFactory.CreateCopy(original);

            AssertCommonFieldsCopied(original, copy);
            Assert.AreEqual(original.ApplicationPath, copy.ApplicationPath);
            Assert.AreEqual(original.Arguments, copy.Arguments);
            Assert.AreEqual(original.RunAsAdmin, copy.RunAsAdmin);
            Assert.AreEqual(original.WorkingDirectory, copy.WorkingDirectory);
        }

        [TestMethod]
        public void CreateCopy_DocumentStep_CopiesAllFields()
        {
            var original = Doc();
            var copy = (OpenDocumentStep)RoutineStepFactory.CreateCopy(original);

            AssertCommonFieldsCopied(original, copy);
            Assert.AreEqual(original.FilePath, copy.FilePath);
            Assert.AreEqual(original.OpenWithAssociatedApp, copy.OpenWithAssociatedApp);
        }

        [TestMethod]
        public void CreateCopy_ChangingTheCopy_DoesNotChangeTheOriginal()
        {
            var original = Url();
            var copy = (OpenUrlStep)RoutineStepFactory.CreateCopy(original);

            copy.Name = "changed";
            copy.Url = "https://other.example";

            Assert.AreEqual("Url", original.Name);
            Assert.AreEqual("https://example.com", original.Url);
        }

        [TestMethod]
        public void CreateAutoRunCopy_ForcesAutoStartAndAutoContinueForEveryType()
        {
            foreach (RoutineStep original in new RoutineStep[] { Url(), Folder(), App(), Doc() })
            {
                var copy = RoutineStepFactory.CreateAutoRunCopy(original);

                Assert.IsTrue(copy.AutoStart, original.GetType().Name);
                Assert.IsTrue(copy.AutoContinue, original.GetType().Name);
                Assert.AreEqual(original.Name, copy.Name);
            }
        }

        [TestMethod]
        public void CreateAutoRunCopy_UrlStep_OpensInExternalBrowser()
        {
            var copy = (OpenUrlStep)RoutineStepFactory.CreateAutoRunCopy(Url());

            Assert.IsTrue(copy.OpenInExternalBrowser);
        }

        [TestMethod]
        public void CreateAutoRunCopy_KeepsOtherSettings()
        {
            var copy = (OpenApplicationStep)RoutineStepFactory.CreateAutoRunCopy(App());

            Assert.IsTrue(copy.RunAsAdmin);
            Assert.AreEqual("-x", copy.Arguments);
        }

        [TestMethod]
        public void CreateAutoRunSteps_TakesOnlyShownStepsInOrder()
        {
            var routine = new Routine
            {
                Steps = new List<RoutineStep>
                {
                    new OpenFolderStep { Id = "3", Order = 3, Show = true },
                    new OpenFolderStep { Id = "hidden", Order = 0, Show = false },
                    new OpenFolderStep { Id = "1", Order = 1, Show = true }
                }
            };

            var steps = RoutineStepFactory.CreateAutoRunSteps(routine);

            Assert.AreEqual(2, steps.Count);
            Assert.AreEqual("1", steps[0].Id);
            Assert.AreEqual("3", steps[1].Id);
        }

        [TestMethod]
        public void CreateAutoRunSteps_NullRoutineOrSteps_ReturnsEmptyList()
        {
            Assert.AreEqual(0, RoutineStepFactory.CreateAutoRunSteps(null).Count);
            Assert.AreEqual(0, RoutineStepFactory.CreateAutoRunSteps(new Routine { Steps = null }).Count);
        }
    }

    [TestClass]
    public class RoutineCloneServiceTests
    {
        [TestMethod]
        public void DeepCopy_Null_ReturnsNull()
        {
            Assert.IsNull(RoutineCloneService.DeepCopy(null));
        }

        [TestMethod]
        public void DeepCopy_CopiesRoutineFieldsAndSteps()
        {
            var created = new DateTime(2024, 1, 2);
            var original = new Routine
            {
                Id = "r", Name = "N", Order = 4, CreatedAt = created,
                UpdatedAt = new DateTime(2024, 2, 2), LastExecutionAt = new DateTime(2024, 3, 3),
                Steps = new List<RoutineStep> { new OpenUrlStep { Id = "s", Name = "S", Url = "https://a.example" } }
            };

            var copy = RoutineCloneService.DeepCopy(original);

            Assert.AreNotSame(original, copy);
            Assert.AreEqual("r", copy.Id);
            Assert.AreEqual("N", copy.Name);
            Assert.AreEqual(4, copy.Order);
            Assert.AreEqual(created, copy.CreatedAt);
            Assert.AreEqual(original.UpdatedAt, copy.UpdatedAt);
            Assert.AreEqual(original.LastExecutionAt, copy.LastExecutionAt);
            Assert.AreEqual(1, copy.Steps.Count);
            Assert.AreNotSame(original.Steps[0], copy.Steps[0]);
        }

        [TestMethod]
        public void DeepCopy_ChangingTheCopy_DoesNotChangeTheOriginal()
        {
            var original = new Routine
            {
                Name = "N",
                Steps = new List<RoutineStep> { new OpenUrlStep { Name = "S" } }
            };

            var copy = RoutineCloneService.DeepCopy(original);
            copy.Name = "X";
            copy.Steps[0].Name = "Y";
            copy.Steps.Add(new OpenUrlStep());

            Assert.AreEqual("N", original.Name);
            Assert.AreEqual("S", original.Steps[0].Name);
            Assert.AreEqual(1, original.Steps.Count);
        }

        [TestMethod]
        public void DeepCopy_NullSteps_GivesEmptyList()
        {
            var copy = RoutineCloneService.DeepCopy(new Routine { Steps = null });

            Assert.IsNotNull(copy.Steps);
            Assert.AreEqual(0, copy.Steps.Count);
        }
    }

    [TestClass]
    public class RoutineExecutionSessionTests
    {
        private static Routine Routine3()
        {
            return new Routine
            {
                Steps = new List<RoutineStep>
                {
                    new OpenFolderStep { Id = "c", Order = 2, Show = true, AutoStart = false },
                    new OpenFolderStep { Id = "hidden", Order = 1, Show = false },
                    new OpenFolderStep { Id = "a", Order = 0, Show = true, AutoStart = true },
                    new OpenFolderStep { Id = "b", Order = 1, Show = true, AutoStart = false }
                }
            };
        }

        [TestMethod]
        public void Constructor_UsesShownStepsInOrder()
        {
            var session = new RoutineExecutionSession(Routine3());

            Assert.AreEqual(3, session.StepCount);
            Assert.AreEqual("a", session.CurrentStep.Id);
            Assert.AreEqual("1/3", session.StepCounterText);
        }

        [TestMethod]
        public void Constructor_SpecificStep_IsTheOnlyStep_EvenWhenHidden()
        {
            var step = new OpenFolderStep { Id = "x", Show = false };

            var session = new RoutineExecutionSession(Routine3(), step);

            Assert.AreEqual(1, session.StepCount);
            Assert.AreSame(step, session.CurrentStep);
        }

        [TestMethod]
        public void NullRoutineOrNoShownSteps_HasNoExecutableSteps()
        {
            var empty = new RoutineExecutionSession(null);
            var allHidden = new RoutineExecutionSession(new Routine
            {
                Steps = new List<RoutineStep> { new OpenFolderStep { Show = false } }
            });

            Assert.IsFalse(empty.HasExecutableSteps);
            Assert.IsNull(empty.CurrentStep);
            Assert.IsFalse(allHidden.HasExecutableSteps);
            Assert.IsFalse(empty.CanGoNext);
            Assert.IsFalse(empty.CanGoPrevious);
            Assert.IsFalse(empty.ShouldAutoExecuteCurrentStep);
            Assert.IsFalse(empty.CanExecuteCurrentStepManually);
        }

        [TestMethod]
        public void GoNextAndPrevious_StayInsideTheRange()
        {
            var session = new RoutineExecutionSession(Routine3());

            session.GoPrevious();
            Assert.AreEqual(0, session.CurrentIndex);

            session.GoNext();
            session.GoNext();
            session.GoNext();

            Assert.AreEqual(2, session.CurrentIndex);
            Assert.IsFalse(session.CanGoNext);
            Assert.IsTrue(session.CanGoPrevious);
        }

        [TestMethod]
        public void AutoStartStep_RunsAutomaticallyOnceThenOnlyManually()
        {
            var session = new RoutineExecutionSession(Routine3());

            Assert.IsTrue(session.ShouldAutoExecuteCurrentStep);
            Assert.IsFalse(session.CanExecuteCurrentStepManually);

            session.MarkCurrentStepExecuted();

            Assert.IsTrue(session.CurrentStepWasExecuted);
            Assert.IsFalse(session.ShouldAutoExecuteCurrentStep);
            Assert.IsTrue(session.CanExecuteCurrentStepManually);
        }

        [TestMethod]
        public void ManualStep_CanBeExecutedByHand_AndIsNotAutoExecuted()
        {
            var session = new RoutineExecutionSession(Routine3());
            session.GoNext();

            Assert.IsFalse(session.ShouldAutoExecuteCurrentStep);
            Assert.IsTrue(session.CanExecuteCurrentStepManually);
        }

        [TestMethod]
        public void MarkCurrentStepExecuted_WithoutSteps_DoesNotThrow()
        {
            var session = new RoutineExecutionSession(null);

            session.MarkCurrentStepExecuted();

            Assert.IsFalse(session.CurrentStepWasExecuted);
        }
    }
}
