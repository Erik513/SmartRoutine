using SmartRoutine.Data;
using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SmartRoutine.Tests.Data
{
    public class RoutineRepositoryTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly RoutineRepository _repository;

        public RoutineRepositoryTests()
        {
            _dbPath = Path.Combine(
                Path.GetTempPath(),
                "SmartRoutine_Test_" + Guid.NewGuid() + ".db");

            _repository = new RoutineRepository(_dbPath);
        }

        public void Dispose()
        {
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }

        [Fact]
        public void Constructor_WithEmptyPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new RoutineRepository(""));
        }

        [Fact]
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

            Assert.NotNull(result);
            Assert.Equal("routine-1", result.Id);
            Assert.Equal("Test Routine", result.Name);
            Assert.Equal(1, result.Order);
        }

        [Fact]
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

            Assert.Equal(2, result.Count);
            Assert.Equal("routine-1", result[0].Id);
            Assert.Equal("routine-2", result[1].Id);
        }

        [Fact]
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

            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            Assert.Equal(5, result.Order);
            Assert.NotNull(result.UpdatedAt);
        }

        [Fact]
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

            Assert.Null(result);
        }

        [Fact]
        public void Routine_DefaultValues_AreInitialized()
        {
            Routine routine = new Routine();

            Assert.False(string.IsNullOrWhiteSpace(routine.Id));
            Assert.Equal(string.Empty, routine.Name);
            Assert.Equal(0, routine.Order);
            Assert.NotEqual(default(DateTime), routine.CreatedAt);
            Assert.Null(routine.UpdatedAt);
            Assert.Null(routine.LastExecutionAt);
            Assert.NotNull(routine.Steps);
            Assert.Empty(routine.Steps);
        }

        [Fact]
        public void OpenUrlStep_DefaultValues_AreInitialized()
        {
            OpenUrlStep step = new OpenUrlStep();

            Assert.False(string.IsNullOrWhiteSpace(step.Id));
            Assert.Equal(StepType.OpenUrl, step.Type);
            Assert.Equal(string.Empty, step.Name);
            Assert.Equal(string.Empty, step.Description);
            Assert.True(step.Show);
            Assert.True(step.AutoStart);
            Assert.Equal(string.Empty, step.Url);
            Assert.True(step.OpenInExternalBrowser);
        }

        [Fact]
        public void OpenFolderStep_DefaultValues_AreInitialized()
        {
            OpenFolderStep step = new OpenFolderStep();

            Assert.Equal(StepType.OpenFolder, step.Type);
            Assert.Equal(string.Empty, step.FolderPath);
            Assert.True(step.OpenInNewWindow);
        }

        [Fact]
        public void OpenApplicationStep_DefaultValues_AreInitialized()
        {
            OpenApplicationStep step = new OpenApplicationStep();

            Assert.Equal(StepType.OpenApplication, step.Type);
            Assert.Equal(string.Empty, step.ApplicationPath);
            Assert.Equal(string.Empty, step.Arguments);
            Assert.False(step.RunAsAdmin);
            Assert.Equal(string.Empty, step.WorkingDirectory);
        }

        [Fact]
        public void OpenDocumentStep_DefaultValues_AreInitialized()
        {
            OpenDocumentStep step = new OpenDocumentStep();

            Assert.Equal(StepType.OpenDocument, step.Type);
            Assert.Equal(string.Empty, step.FilePath);
            Assert.True(step.OpenWithAssociatedApp);
        }

        [Fact]
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

            Assert.NotNull(loaded);
            Assert.Single(loaded.Steps);
        }

        [Fact]
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

            Assert.Single(loaded.Steps);
        }

        [Fact]
        public void GetRoutine_UnknownId_ReturnsNull()
        {
            Routine result =
                _repository.GetRoutine("does-not-exist");

            Assert.Null(result);
        }

        [Fact]
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

            Assert.NotNull(loaded);
            Assert.NotNull(loaded.Steps);
            Assert.Equal(2, loaded.Steps.Count);

            Assert.NotNull(loaded.Steps[0]);
            Assert.NotNull(loaded.Steps[1]);

            Assert.Equal(2, loaded.Steps[0].Order);
            Assert.Equal(1, loaded.Steps[1].Order);
        }
        [Fact]
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

            Assert.Equal("Updated", updated.Name);

            _repository.DeleteRoutine(routine.Id);

            Assert.Null(
                _repository.GetRoutine(routine.Id));
        }
    }
}