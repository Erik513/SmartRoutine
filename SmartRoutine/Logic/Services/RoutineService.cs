using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SmartRoutine.Logic.Services
{
    public class RoutineService : IRoutineService
    {
        private readonly IRoutineRepository _repository;
        private List<Routine> _testRoutines;
        private readonly ITestDataService _testDataService;

        public RoutineService(IRoutineRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _testDataService = new TestDataService();
        }

        public List<Routine> GetAllRoutines()
        {
            if (AppSettings.UseTestData)
            {
                // Beim ersten Aufruf: Frische Testdaten laden
                if (_testRoutines == null)
                {
                    _testRoutines = _testDataService.GetTestRoutines();
                }
                return _testRoutines;
            }
            else
            {
                return _repository.LoadRoutines().OrderBy(r => r.Order).ToList();
            }
        }

        public Routine GetRoutine(string id)
        {
            return GetAllRoutines().FirstOrDefault(r => r.Id == id);
        }

        public void CreateRoutine(string name)
        {
            if (AppSettings.UseTestData)
            {
                // In die Test-Liste einfügen
                var newRoutine = new Routine
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Order = _testRoutines.Count,
                    CreatedAt = DateTime.Now,
                    Steps = new List<RoutineStep>()
                };
                _testRoutines.Add(newRoutine);
            }
            else
            {
                var routines = _repository.LoadRoutines();
                int maxOrder = routines.Count > 0 ? routines.Max(r => r.Order) : -1;
                var routine = new Routine
                {
                    Name = name,
                    Order = maxOrder + 1,
                    CreatedAt = DateTime.Now
                };
                _repository.AddRoutine(routine);
            }
        }

        public void UpdateRoutine(Routine routine)
        {
            if (AppSettings.UseTestData)
            {
                var existing = _testRoutines.FirstOrDefault(r => r.Id == routine.Id);
                if (existing != null)
                {
                    existing.Name = routine.Name;
                    existing.Steps = routine.Steps;
                    existing.UpdatedAt = DateTime.Now;
                }
            }
            else
            {
                _repository.UpdateRoutine(routine);
            }
        }

        public void DeleteRoutine(string id)
        {
            if (AppSettings.UseTestData)
            {
                _testRoutines.RemoveAll(r => r.Id == id);
            }
            else
            {
                _repository.DeleteRoutine(id);
            }
        }

        public void ReorderRoutines(List<Routine> reorderedRoutines)
        {
            if (reorderedRoutines == null) return;

            for (int i = 0; i < reorderedRoutines.Count; i++)
            {
                reorderedRoutines[i].Order = i;
            }

            if (AppSettings.UseTestData)
            {
                _testRoutines = reorderedRoutines;
            }
            else
            {
                foreach (var routine in reorderedRoutines)
                {
                    _repository.UpdateRoutine(routine);
                }
            }
        }

        // Steps =================================================================================

        public void AddStep(string routineId, StepType type, string value, string description, string userDescription = null)
        {
            var routine = GetRoutine(routineId);
            if (routine == null) return;

            var step = new RoutineStep
            {
                Id = Guid.NewGuid().ToString(),
                Order = routine.Steps.Count,
                Type = type,
                Value = value,
                Description = description,
                UserDescription = userDescription ?? description
            };

            routine.Steps.Add(step);

            if (!AppSettings.UseTestData)
            {
                _repository.UpdateRoutine(routine);
            }
        }

        public void UpdateStep(string routineId, string stepId, string value, string description, string userDescription = null)
        {
            var routine = GetRoutine(routineId);
            var step = routine?.Steps.FirstOrDefault(s => s.Id == stepId);

            if (step != null)
            {
                step.Value = value;
                step.Description = description;
                step.UserDescription = userDescription ?? description;

                if (!AppSettings.UseTestData)
                {
                    _repository.UpdateRoutine(routine);
                }
            }
        }

        public void RemoveStep(string routineId, string stepId)
        {
            var routine = GetRoutine(routineId);
            var step = routine?.Steps.FirstOrDefault(s => s.Id == stepId);

            if (step != null)
            {
                routine.Steps.Remove(step);
                for (int i = 0; i < routine.Steps.Count; i++)
                    routine.Steps[i].Order = i;

                if (!AppSettings.UseTestData)
                {
                    _repository.UpdateRoutine(routine);
                }
            }
        }

        public void ReorderSteps(string routineId, int oldIndex, int newIndex)
        {
            var routine = GetRoutine(routineId);
            if (routine == null) return;

            var steps = routine.Steps.ToList();
            var step = steps[oldIndex];
            steps.RemoveAt(oldIndex);
            steps.Insert(newIndex, step);

            for (int i = 0; i < steps.Count; i++)
                steps[i].Order = i;

            routine.Steps = steps;

            if (!AppSettings.UseTestData)
            {
                _repository.UpdateRoutine(routine);
            }
        }

        public void ExecuteStep(RoutineStep step)
        {
            switch (step.Type)
            {
                case StepType.OpenUrl:
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = step.Value,
                        UseShellExecute = true
                    });
                    break;
            }
        }

        public bool ValidateStep(RoutineStep step)
        {
            switch (step.Type)
            {
                case StepType.OpenUrl:
                    return Uri.IsWellFormedUriString(step.Value, UriKind.Absolute);
                default:
                    return true;
            }
        }
    }
}