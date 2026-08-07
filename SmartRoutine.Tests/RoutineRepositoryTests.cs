using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Data;
using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartRoutine.Tests.Data
{
    [TestClass]
    public class RoutineRepositoryTests
    {
        private string _dbPath;
        private RoutineRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _dbPath = Path.Combine(
                Path.GetTempPath(),
                "SmartRoutine_Test_" + Guid.NewGuid() + ".db");

            _repository = new RoutineRepository(_dbPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }

        [TestMethod]
        public void Constructor_WithEmptyPath_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() =>
                new RoutineRepository(""));
        }

        [TestMethod]
        public void AddRoutine_And_GetRoutine_ReturnsRoutine()
        {
            Routine routine = new Routine
            {
                Id = "routine-1",
                Name = "Test Routine",
                Order = 1
            };

            _repository.AddRoutine(routine);

            Routine result = _repository.GetRoutine("routine-1");

            Assert.IsNotNull(result);
            Assert.AreEqual("routine-1", result.Id);
            Assert.AreEqual("Test Routine", result.Name);
            Assert.AreEqual(1, result.Order);
        }

        [TestMethod]
        public void LoadRoutines_ReturnsRoutinesOrderedByOrder()
        {
            Routine routine2 = new Routine
            {
                Id = "routine-2",
                Name = "Second",
                Order = 2
            };

            Routine routine1 = new Routine
            {
                Id = "routine-1",
                Name = "First",
                Order = 1
            };

            _repository.AddRoutine(routine2);
            _repository.AddRoutine(routine1);

            List<Routine> result = _repository.LoadRoutines();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("routine-1", result[0].Id);
            Assert.AreEqual("routine-2", result[1].Id);
        }

        [TestMethod]
        public void UpdateRoutine_UpdatesExistingRoutine()
        {
            Routine routine = new Routine
            {
                Id = "routine-1",
                Name = "Old Name",
                Order = 1
            };

            _repository.AddRoutine(routine);

            routine.Name = "New Name";
            routine.Order = 5;
            routine.UpdatedAt = DateTime.Now;

            _repository.UpdateRoutine(routine);

            Routine result = _repository.GetRoutine("routine-1");

            Assert.IsNotNull(result);
            Assert.AreEqual("New Name", result.Name);
            Assert.AreEqual(5, result.Order);
            Assert.IsNotNull(result.UpdatedAt);
        }

        [TestMethod]
        public void DeleteRoutine_RemovesRoutine()
        {
            Routine routine = new Routine
            {
                Id = "routine-1",
                Name = "Delete Me"
            };

            _repository.AddRoutine(routine);

            _repository.DeleteRoutine("routine-1");

            Routine result = _repository.GetRoutine("routine-1");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Routine_DefaultValues_AreInitialized()
        {
            Routine routine = new Routine();

            Assert.IsFalse(string.IsNullOrWhiteSpace(routine.Id));
            Assert.AreEqual(string.Empty, routine.Name);
            Assert.AreEqual(0, routine.Order);
            Assert.AreNotEqual(default(DateTime), routine.CreatedAt);
            Assert.IsNull(routine.UpdatedAt);
            Assert.IsNull(routine.LastExecutionAt);
            Assert.IsNotNull(routine.Steps);
            Assert.AreEqual(0, routine.Steps.Count);
        }

        [TestMethod]
        public void OpenUrlStep_DefaultValues_AreInitialized()
        {
            OpenUrlStep step = new OpenUrlStep();

            Assert.IsFalse(string.IsNullOrWhiteSpace(step.Id));
            Assert.AreEqual(StepType.OpenUrl, step.Type);
            Assert.AreEqual(string.Empty, step.Name);
            Assert.AreEqual(string.Empty, step.Description);
            Assert.IsTrue(step.Show);
            Assert.IsTrue(step.AutoStart);
            Assert.AreEqual(string.Empty, step.Url);
            Assert.IsTrue(step.OpenInExternalBrowser);
        }

        [TestMethod]
        public void OpenFolderStep_DefaultValues_AreInitialized()
        {
            OpenFolderStep step = new OpenFolderStep();

            Assert.AreEqual(StepType.OpenFolder, step.Type);
            Assert.AreEqual(string.Empty, step.FolderPath);
            Assert.IsTrue(step.OpenInNewWindow);
        }

        [TestMethod]
        public void OpenApplicationStep_DefaultValues_AreInitialized()
        {
            OpenApplicationStep step = new OpenApplicationStep();

            Assert.AreEqual(StepType.OpenApplication, step.Type);
            Assert.AreEqual(string.Empty, step.ApplicationPath);
            Assert.AreEqual(string.Empty, step.Arguments);
            Assert.IsFalse(step.RunAsAdmin);
            Assert.AreEqual(string.Empty, step.WorkingDirectory);
        }

        [TestMethod]
        public void OpenDocumentStep_DefaultValues_AreInitialized()
        {
            OpenDocumentStep step = new OpenDocumentStep();

            Assert.AreEqual(StepType.OpenDocument, step.Type);
            Assert.AreEqual(string.Empty, step.FilePath);
            Assert.IsTrue(step.OpenWithAssociatedApp);
        }

        [TestMethod]
        public void AddRoutine_WithOpenUrlStep_PersistsStep()
        {
            Routine routine = new Routine
            {
                Id = "routine-1",
                Name = "Test"
            };

            routine.Steps.Add(
                new OpenUrlStep
                {
                    Name = "Google",
                    Url = "https://google.de"
                });

            _repository.AddRoutine(routine);

            Routine loaded =
                _repository.GetRoutine("routine-1");

            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.Steps.Count);
        }

        [TestMethod]
        public void UpdateRoutine_UpdatesSteps()
        {
            Routine routine = new Routine
            {
                Id = "routine-1",
                Name = "Test"
            };

            _repository.AddRoutine(routine);

            routine.Steps.Add(
                new OpenFolderStep
                {
                    FolderPath = @"C:\Temp"
                });

            _repository.UpdateRoutine(routine);

            Routine loaded =
                _repository.GetRoutine("routine-1");

            Assert.AreEqual(1, loaded.Steps.Count);
        }

        [TestMethod]
        public void GetRoutine_UnknownId_ReturnsNull()
        {
            Routine result =
                _repository.GetRoutine("does-not-exist");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RoutineStep_Order_IsPreserved()
        {
            Routine routine = new Routine
            {
                Id = "routine-step-order",
                Name = "Step Order Test"
            };

            routine.Steps.Add(
                new OpenUrlStep
                {
                    Name = "Second",
                    Order = 2,
                    Url = "https://example.com"
                });

            routine.Steps.Add(
                new OpenFolderStep
                {
                    Name = "First",
                    Order = 1,
                    FolderPath = @"C:\Temp"
                });

            _repository.AddRoutine(routine);

            Routine loaded = _repository.GetRoutine("routine-step-order");

            Assert.IsNotNull(loaded);
            Assert.IsNotNull(loaded.Steps);
            Assert.AreEqual(2, loaded.Steps.Count);

            Assert.IsNotNull(loaded.Steps[0]);
            Assert.IsNotNull(loaded.Steps[1]);

            Assert.AreEqual(2, loaded.Steps[0].Order);
            Assert.AreEqual(1, loaded.Steps[1].Order);
        }

        [TestMethod]
        public void CompleteCrudWorkflow_Works()
        {
            Routine routine = new Routine
            {
                Name = "Workflow"
            };

            _repository.AddRoutine(routine);

            Routine loaded =
                _repository.GetRoutine(routine.Id);

            loaded.Name = "Updated";

            _repository.UpdateRoutine(loaded);

            Routine updated =
                _repository.GetRoutine(routine.Id);

            Assert.AreEqual("Updated", updated.Name);

            _repository.DeleteRoutine(routine.Id);

            Assert.IsNull(
                _repository.GetRoutine(routine.Id));
        }
    }
}
