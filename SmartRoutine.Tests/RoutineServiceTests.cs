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
            return new RoutineService(repo, false); // false = KEINE TestDataFactory!
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

            var step = new OpenUrlStep { Name = "Step 1", Description = "Description", Url = "https://example.com", OpenInExternalBrowser = true };
            service.AddStep(routine.Id, step);

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual(1, updatedRoutine.Steps.Count);
            Assert.AreEqual("Step 1", updatedRoutine.Steps[0].Name);
            Assert.AreEqual("https://example.com", ((OpenUrlStep)updatedRoutine.Steps[0]).Url);
        }

        [TestMethod]
        public void AddStep_WithMultipleSteps_SetsCorrectOrders()
        {
            
        }

        [TestMethod]
        public void AddStep_WithDifferentTypes_AddsCorrectly()
        {
            var service = CreateService();
            service.CreateRoutine("Test");
            var routine = service.GetAllRoutines().First();

            var urlStep = new OpenUrlStep { Name = "URL Step", Url = "https://example.com", OpenInExternalBrowser = true };
            var folderStep = new OpenFolderStep { Name = "Folder Step", FolderPath = @"C:\Test", OpenInNewWindow = true };
            var appStep = new OpenApplicationStep { Name = "App Step", ApplicationPath = "notepad.exe", Arguments = "test.txt", RunAsAdmin = false };

            service.AddStep(routine.Id, urlStep);
            service.AddStep(routine.Id, folderStep);
            service.AddStep(routine.Id, appStep);

            var updatedRoutine = service.GetRoutine(routine.Id);
            Assert.AreEqual(3, updatedRoutine.Steps.Count);
            Assert.AreEqual(StepType.OpenUrl, updatedRoutine.Steps[0].Type);
            Assert.AreEqual(StepType.OpenFolder, updatedRoutine.Steps[1].Type);
            Assert.AreEqual(StepType.OpenApplication, updatedRoutine.Steps[2].Type);
            Assert.AreEqual("https://example.com", ((OpenUrlStep)updatedRoutine.Steps[0]).Url);
        }

        [TestMethod]
        public void AddStep_WithNonExistentRoutine_DoesNothing()
        {

        }


        // UPDATE STEP

        [TestMethod] // Main-Function
        public void UpdateStep_ModifiesStep()
        {
            var service = CreateService();
            service.CreateRoutine("Test Routine");
            var routine = service.GetAllRoutines().First();

            var originalStep = new OpenUrlStep { Id = "step1", Name = "Original", Url = "oldurl", OpenInExternalBrowser = true };
            service.AddStep(routine.Id, originalStep);

            var updatedStep = new OpenUrlStep
            {
                Id = originalStep.Id,
                Name = "Updated",
                Description = "New Desc",
                Show = false,
                Url = "newurl",
                OpenInExternalBrowser = false
            };
            service.UpdateStep(routine.Id, updatedStep);

            var resultStep = service.GetRoutine(routine.Id).Steps.First() as OpenUrlStep;
            Assert.AreEqual("Updated", resultStep.Name);
            Assert.AreEqual("New Desc", resultStep.Description);
            Assert.IsFalse(resultStep.Show);
            Assert.AreEqual("newurl", resultStep.Url);
            Assert.IsFalse(resultStep.OpenInExternalBrowser);
        }

        [TestMethod]
        public void UpdateStep_WithNonExistentStep_DoesNothing()
        {

        }

        [TestMethod]
        public void UpdateStep_WithNonExistentRoutine_DoesNothing()
        {
            
        }


        // REMOVE STEP

        [TestMethod] // Main-Function
        public void RemoveStep_RemovesStepFromRoutine()
        {
            
        }

        [TestMethod]
        public void RemoveStep_ReordersRemainingStepsCorrectly()
        {
            
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
            
        }


        // REORDER STEPS

        [TestMethod] // Main-Function
        public void ReorderSteps_ChangesStepOrder()
        {
           
        }

        [TestMethod]
        public void ReorderSteps_WithInvalidIndices_DoesNothing()
        {
            
        }

        [TestMethod]
        public void ValidateStep_WithNullValue_ReturnsFalse()
        {

        }
    }
}