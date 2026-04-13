using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartRoutine.Data;
using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartRoutine.Tests
{ 
    // In-Memory Repository für Tests
    public class InMemoryRepository : IRoutineRepository
    {
        private List<Routine> _routines = new List<Routine>();

        public Routine GetRoutine(string id) => _routines.FirstOrDefault(r => r.Id == id);
        public List<Routine> LoadRoutines() => _routines.OrderBy(r => r.Order).ToList();
        
        public void AddRoutine(Routine routine) => _routines.Add(routine);
        public void UpdateRoutine(Routine routine)
        {
            var index = _routines.FindIndex(r => r.Id == routine.Id);
            if (index >= 0) _routines[index] = routine;
        }
        public void DeleteRoutine(string id) => _routines.RemoveAll(r => r.Id == id);
        public void SaveRoutines(List<Routine> routines) => _routines = routines ?? new List<Routine>();
    }

    [TestClass]
    public class RoutineServiceTests
    {
        private RoutineService CreateService()
        {
            var repo = new InMemoryRepository();
            return new RoutineService(repo, false); // false = KEINE TestData!
        }

        [TestMethod]
        public void Test_ServiceCanBeCreated()
        {
            var service = CreateService();
            Assert.IsNotNull(service);
        }

        // ROUTINES =============================================================================================

        // GET ROUTINE

        [TestMethod] // Main-Function
        public void GetRoutine_ReturnsCorrectRoutine()
        {
            var service = CreateService();
            service.CreateRoutine("Find Me");
            var createdRoutine = service.GetAllRoutines().First();

            var foundRoutine = service.GetRoutine(createdRoutine.Id);

            Assert.IsNotNull(foundRoutine);
            Assert.AreEqual("Find Me", foundRoutine.Name);
        }

        [TestMethod]
        public void GetRoutine_WithNonExistentId_ReturnsNull()
        {
            var service = CreateService();

            var result = service.GetRoutine("non-existent-id");

            Assert.IsNull(result);
        }


        // GET ALL ROUTINES

        [TestMethod] // Main-Function
        public void GetAllRoutines_ReturnsSortedRoutines()
        {
            var service = CreateService();
            service.CreateRoutine("Routine B");
            service.CreateRoutine("Routine A");
            service.CreateRoutine("Routine C");

            var routines = service.GetAllRoutines();

            Assert.AreEqual(3, routines.Count);
            Assert.AreEqual("Routine B", routines[0].Name);
            Assert.AreEqual("Routine A", routines[1].Name);
            Assert.AreEqual("Routine C", routines[2].Name);
        }

        [TestMethod]
        public void GetAllRoutines_WhenEmpty_ReturnsEmptyList()
        {
            var service = CreateService();

            var routines = service.GetAllRoutines();

            Assert.IsNotNull(routines);
            Assert.AreEqual(0, routines.Count);
        }


        // CREATE ROUTINE

        [TestMethod] // Main-Function
        public void CreateRoutine_AddsNewRoutine()
        {
            var service = CreateService();

            service.CreateRoutine("Test Routine");
            var routines = service.GetAllRoutines();

            Assert.AreEqual(1, routines.Count);
            Assert.AreEqual("Test Routine", routines[0].Name);
        }

        [TestMethod]
        public void CreateRoutine_WithEmptyName_AddsRoutine()
        {
            var service = CreateService();

            service.CreateRoutine("");
            var routines = service.GetAllRoutines();

            Assert.AreEqual(1, routines.Count);
            Assert.AreEqual("", routines[0].Name);
        }

        [TestMethod]
        public void CreateRoutine_CallsAddRoutine_WhenUsingMock()
        {
            var mockRepo = new Mock<IRoutineRepository>();
            mockRepo.Setup(r => r.LoadRoutines()).Returns(new List<Routine>());

            var service = new RoutineService(mockRepo.Object, false);

            service.CreateRoutine("Test Routine");

            mockRepo.Verify(r => r.AddRoutine(It.IsAny<Routine>()), Times.Once);
        }


        // UPDATE ROUTINE

        [TestMethod] // Main-Function
        public void UpdateRoutine_UpdatesNameAndSteps()
        {
            var service = CreateService();
            service.CreateRoutine("Original Name");
            var routine = service.GetAllRoutines().First();
            service.AddStep(routine.Id, "Old Step", "", true, StepType.OpenUrl, "oldurl");

            routine.Name = "Updated Name";
            routine.Steps = new List<RoutineStep>
            {
                new RoutineStep { Id = "new1", Name = "New Step", Order = 0, Type = StepType.OpenUrl, Value = "newurl" }
            };
            service.UpdateRoutine(routine);

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual("Updated Name", updatedRoutine.Name);
            Assert.AreEqual(1, updatedRoutine.Steps.Count);
            Assert.AreEqual("New Step", updatedRoutine.Steps[0].Name);
        }

        [TestMethod]
        public void UpdateRoutine_WithNull_DoesNothing()
        {
            var service = CreateService();
            service.CreateRoutine("Original");

            service.UpdateRoutine(null);
            var routines = service.GetAllRoutines();

            Assert.AreEqual(1, routines.Count);
            Assert.AreEqual("Original", routines[0].Name);
        }

        [TestMethod]
        public void UpdateRoutine_WithNonExistentRoutine_DoesNothing()
        {
            var service = CreateService();
            service.CreateRoutine("Original");
            var nonExistent = new Routine { Id = "non-existent", Name = "Ghost" };

            service.UpdateRoutine(nonExistent);
            var routines = service.GetAllRoutines();

            Assert.AreEqual(1, routines.Count);
            Assert.AreEqual("Original", routines[0].Name);
            Assert.IsNull(service.GetRoutine("non-existent"));
        }


        // SAVE ROUTINE

        [TestMethod] // Main-Function
        public void SaveRoutine_WithNewRoutine_AddsRoutine()
        {
            var service = CreateService();
            var newRoutine = new Routine
            {
                Id = Guid.NewGuid().ToString(),
                Name = "New Routine",
                Steps = new List<RoutineStep>()
            };

            service.SaveRoutine(newRoutine);
            var routines = service.GetAllRoutines();

            Assert.AreEqual(1, routines.Count);
            Assert.AreEqual("New Routine", routines[0].Name);
        }

        [TestMethod]
        public void SaveRoutine_WithNewRoutine_SetsCorrectOrder()
        {
            // Testet ob neue Routine die richtige Order bekommt
            var service = CreateService();

            var newRoutine1 = new Routine { Id = Guid.NewGuid().ToString(), Name = "First", Steps = new List<RoutineStep>() };
            var newRoutine2 = new Routine { Id = Guid.NewGuid().ToString(), Name = "Second", Steps = new List<RoutineStep>() };

            service.SaveRoutine(newRoutine1);
            service.SaveRoutine(newRoutine2);

            var routines = service.GetAllRoutines();
            Assert.AreEqual(0, routines[0].Order);
            Assert.AreEqual(1, routines[1].Order);
        }

        [TestMethod]
        public void SaveRoutine_WithExistingRoutine_UpdatesRoutine()
        {
            var service = CreateService();
            service.CreateRoutine("Original");
            var routine = service.GetAllRoutines().First();
            routine.Name = "Updated";

            service.SaveRoutine(routine);
            var updated = service.GetRoutine(routine.Id);

            Assert.AreEqual("Updated", updated.Name);
        }

        [TestMethod]
        public void SaveRoutine_WithNull_DoesNothing()
        {
            var service = CreateService();

            service.SaveRoutine(null);
            var routines = service.GetAllRoutines();

            Assert.AreEqual(0, routines.Count);
        }


        // REORDER ROUTINES

        [TestMethod] // Main-Function
        public void ReorderRoutines_WithValidList_UpdatesOrders()
        {
            var service = CreateService();
            service.CreateRoutine("Routine A");
            service.CreateRoutine("Routine B");
            service.CreateRoutine("Routine C");
            var routines = service.GetAllRoutines().ToList();

            // Reverse order
            var reversed = routines.OrderByDescending(r => r.Order).ToList();

            service.ReorderRoutines(reversed);
            var reordered = service.GetAllRoutines();

            Assert.AreEqual(0, reordered[0].Order);
            Assert.AreEqual(1, reordered[1].Order);
            Assert.AreEqual(2, reordered[2].Order);
        }

        [TestMethod]
        public void ReorderRoutines_WithNullList_DoesNothing()
        {
            var service = CreateService();
            service.CreateRoutine("Routine A");
            service.CreateRoutine("Routine B");
            var before = service.GetAllRoutines().ToList();

            service.ReorderRoutines(null);
            var after = service.GetAllRoutines().ToList();

            Assert.AreEqual(before.Count, after.Count);
            Assert.AreEqual(before[0].Name, after[0].Name);
            Assert.AreEqual(before[1].Name, after[1].Name);
        }

        [TestMethod]
        public void ReorderRoutines_WithEmptyList_DoesNothing()
        {
            var service = CreateService();
            service.CreateRoutine("Routine A");
            var before = service.GetAllRoutines().ToList();

            service.ReorderRoutines(new List<Routine>());
            var after = service.GetAllRoutines().ToList();

            Assert.AreEqual(before.Count, after.Count);
        }


        // DELETE ROUTINE

        [TestMethod] // Main-Function
        public void DeleteRoutine_RemovesRoutine()
        {
            var service = CreateService();
            service.CreateRoutine("To Delete");
            var routine = service.GetAllRoutines().First();

            service.DeleteRoutine(routine.Id);
            var routines = service.GetAllRoutines();

            Assert.AreEqual(0, routines.Count);
        }

        [TestMethod]
        public void DeleteRoutine_CallsDeleteRoutine_WhenUsingMock()
        {
            var mockRepo = new Mock<IRoutineRepository>();
            var routine = new Routine { Id = "123", Name = "Test" };
            mockRepo.Setup(r => r.LoadRoutines()).Returns(new List<Routine> { routine });

            var service = new RoutineService(mockRepo.Object, false);

            service.DeleteRoutine("123");

            mockRepo.Verify(r => r.DeleteRoutine("123"), Times.Once);
        }


        //STEPS =============================================================================================


        // ADD STEP

        [TestMethod] // Main-Function
        public void AddStep_AddsStepToRoutine()
        {
            var service = CreateService();
            service.CreateRoutine("Test Routine");
            var routine = service.GetAllRoutines().First();

            service.AddStep(routine.Id, "Step 1", "Description", true, StepType.OpenUrl, "https://example.com");

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual(1, updatedRoutine.Steps.Count);
            Assert.AreEqual("Step 1", updatedRoutine.Steps[0].Name);
        }

        [TestMethod]
        public void AddStep_WithMultipleSteps_SetsCorrectOrders()
        {
            var service = CreateService();
            service.CreateRoutine("Test");
            var routine = service.GetAllRoutines().First();

            service.AddStep(routine.Id, "First", "", true, StepType.OpenUrl, "");
            service.AddStep(routine.Id, "Second", "", true, StepType.OpenUrl, "");
            service.AddStep(routine.Id, "Third", "", true, StepType.OpenUrl, "");

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual(0, updatedRoutine.Steps[0].Order);
            Assert.AreEqual(1, updatedRoutine.Steps[1].Order);
            Assert.AreEqual(2, updatedRoutine.Steps[2].Order);
        }

        [TestMethod]
        public void AddStep_WithDifferentTypes_AddsCorrectly()
        {
            // Testet verschiedene Step-Typen (wenn mehr vorhanden)
            var service = CreateService();
            service.CreateRoutine("Test");
            var routine = service.GetAllRoutines().First();

            service.AddStep(routine.Id, "URL Step", "", true, StepType.OpenUrl, "https://example.com");
            // service.AddStep(routine.Id, "Folder Step", "", true, StepType.OpenFolder, @"C:\Test");
            // service.AddStep(routine.Id, "App Step", "", true, StepType.OpenApplication, "notepad.exe");

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual(StepType.OpenUrl, updatedRoutine.Steps[0].Type);
            Assert.AreEqual("https://example.com", updatedRoutine.Steps[0].Value);
        }

        [TestMethod]
        public void AddStep_WithNonExistentRoutine_DoesNothing()
        {
            var service = CreateService();

            service.AddStep("non-existent-id", "Step", "", true, StepType.OpenUrl, "");
            var routines = service.GetAllRoutines();

            Assert.AreEqual(0, routines.Count);
        }


        // UPDATE STEP

        [TestMethod] // Main-Function
        public void UpdateStep_ModifiesStep()
        {
            var service = CreateService();
            service.CreateRoutine("Test Routine");
            var routine = service.GetAllRoutines().First();
            service.AddStep(routine.Id, "Original", "", true, StepType.OpenUrl, "oldurl");

            var step = service.GetRoutine(routine.Id).Steps.First();

            service.UpdateStep(routine.Id, step.Id, "Updated", "New Desc", false, StepType.OpenUrl, "newurl");

            var updatedStep = service.GetRoutine(routine.Id).Steps.First();
            Assert.AreEqual("Updated", updatedStep.Name);
            Assert.AreEqual("New Desc", updatedStep.Description);
            Assert.IsFalse(updatedStep.Show);
            Assert.AreEqual("newurl", updatedStep.Value);
        }

        [TestMethod]
        public void UpdateStep_WithNonExistentStep_DoesNothing()
        {
            var service = CreateService();
            service.CreateRoutine("Test");
            var routine = service.GetAllRoutines().First();
            service.AddStep(routine.Id, "Original", "", true, StepType.OpenUrl, "oldurl");

            service.UpdateStep(routine.Id, "non-existent-id", "Updated", "", false, StepType.OpenUrl, "newurl");

            var step = service.GetRoutine(routine.Id).Steps.First();
            Assert.AreEqual("Original", step.Name);
            Assert.AreEqual("oldurl", step.Value);
        }

        [TestMethod]
        public void UpdateStep_WithNonExistentRoutine_DoesNothing()
        {
            var service = CreateService();

            // (sollte keine Exception werfen)
            service.UpdateStep("non-existent-id", "step-id", "Updated", "", false, StepType.OpenUrl, "newurl");

            var routines = service.GetAllRoutines();
            Assert.AreEqual(0, routines.Count);
        }


        // REMOVE STEP

        [TestMethod] // Main-Function
        public void RemoveStep_RemovesStepFromRoutine()
        {
            var service = CreateService();
            service.CreateRoutine("Test Routine");
            var routine = service.GetAllRoutines().First();
            service.AddStep(routine.Id, "Step 1", "", true, StepType.OpenUrl, "");
            service.AddStep(routine.Id, "Step 2", "", true, StepType.OpenUrl, "");

            routine = service.GetRoutine(routine.Id);
            var stepToRemove = routine.Steps.First(s => s.Name == "Step 1");

            service.RemoveStep(routine.Id, stepToRemove.Id);

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual(1, updatedRoutine.Steps.Count);
            Assert.AreEqual("Step 2", updatedRoutine.Steps[0].Name);
        }

        [TestMethod]
        public void RemoveStep_ReordersRemainingStepsCorrectly()
        {
            // Testet ob nach dem Löschen die Orders korrekt aktualisiert werden
            var service = CreateService();
            service.CreateRoutine("Test");
            var routine = service.GetAllRoutines().First();
            service.AddStep(routine.Id, "First", "", true, StepType.OpenUrl, "");
            service.AddStep(routine.Id, "Second", "", true, StepType.OpenUrl, "");
            service.AddStep(routine.Id, "Third", "", true, StepType.OpenUrl, "");

            var stepToRemove = service.GetRoutine(routine.Id).Steps.First(s => s.Name == "Second");
            service.RemoveStep(routine.Id, stepToRemove.Id);

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual(0, updatedRoutine.Steps[0].Order);
            Assert.AreEqual(1, updatedRoutine.Steps[1].Order);
            Assert.AreEqual("First", updatedRoutine.Steps[0].Name);
            Assert.AreEqual("Third", updatedRoutine.Steps[1].Name);
        }

        [TestMethod]
        public void RemoveStep_WithNonExistentRoutine_DoesNothing()
        {
            var service = CreateService();

            // (sollte keine Exception werfen)
            service.RemoveStep("non-existent-id", "step-id");

            var routines = service.GetAllRoutines();
            Assert.AreEqual(0, routines.Count);
        }

        [TestMethod]
        public void RemoveStep_WithNonExistentStep_DoesNothing()
        {
            var service = CreateService();
            service.CreateRoutine("Test");
            var routine = service.GetAllRoutines().First();
            service.AddStep(routine.Id, "Step 1", "", true, StepType.OpenUrl, "");

            service.RemoveStep(routine.Id, "non-existent-id");

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual(1, updatedRoutine.Steps.Count);
            Assert.AreEqual("Step 1", updatedRoutine.Steps[0].Name);
        }


        // REORDER STEPS

        [TestMethod] // Main-Function
        public void ReorderSteps_ChangesStepOrder()
        {
            var service = CreateService();
            service.CreateRoutine("Reorder Test");
            var routine = service.GetAllRoutines().First();
            service.AddStep(routine.Id, "First", "", true, StepType.OpenUrl, "");
            service.AddStep(routine.Id, "Second", "", true, StepType.OpenUrl, "");
            service.AddStep(routine.Id, "Third", "", true, StepType.OpenUrl, "");

            service.ReorderSteps(routine.Id, 2, 0);

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual("Third", updatedRoutine.Steps[0].Name);
            Assert.AreEqual("First", updatedRoutine.Steps[1].Name);
            Assert.AreEqual("Second", updatedRoutine.Steps[2].Name);
        }

        [TestMethod]
        public void ReorderSteps_WithInvalidIndices_DoesNothing()
        {
            var service = CreateService();
            service.CreateRoutine("Test");
            var routine = service.GetAllRoutines().First();
            service.AddStep(routine.Id, "First", "", true, StepType.OpenUrl, "");
            service.AddStep(routine.Id, "Second", "", true, StepType.OpenUrl, "");

            service.ReorderSteps(routine.Id, 99, 0); // Ungültiger oldIndex

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual("First", updatedRoutine.Steps[0].Name);
            Assert.AreEqual("Second", updatedRoutine.Steps[1].Name);
        }


        // VALIDATE STEP

        [TestMethod] // Main-Function
        public void ValidateStep_ValidUrl_ReturnsTrue()
        {
            var service = CreateService();
            var step = new RoutineStep
            {
                Type = StepType.OpenUrl,
                Value = "https://www.google.com"
            };

            var result = service.ValidateStep(step);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidateStep_InvalidUrl_ReturnsFalse()
        {
            var service = CreateService();
            var step = new RoutineStep
            {
                Type = StepType.OpenUrl,
                Value = "not-a-url"
            };

            var result = service.ValidateStep(step);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateStep_WithWhitespaceUrl_ReturnsFalse()
        {
            var service = CreateService();
            var step = new RoutineStep
            {
                Type = StepType.OpenUrl,
                Value = "   "
            };

            var result = service.ValidateStep(step);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateStep_WithNullValue_ReturnsFalse()
        {
            var service = CreateService();
            var step = new RoutineStep
            {
                Type = StepType.OpenUrl,
                Value = null
            };

            var result = service.ValidateStep(step);
            Assert.IsFalse(result);
        }
    }
}