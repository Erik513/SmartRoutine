using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SmartRoutine.Logic.Services
{
    public class RoutineService : IRoutineService
    {
        private readonly IRoutineRepository _repository;
        private List<Routine> _testRoutines;
        private bool _useTestData = AppSettings.UseTestData;
        private TestData _testDataService;

        public bool UseTestData
        {
            get => _useTestData;
            set
            {
                _useTestData = value;
                if (_useTestData)
                {
                    LoadTestData();
                }
            }
        }
        public RoutineService(IRoutineRepository repository = null, bool useTestData = true)
        {
            _repository = repository;
            _useTestData = useTestData;

            if (_useTestData)
            {
                LoadTestData();
            }

            // Nur wenn kein Testdaten-Modus, dann muss Repository existieren
            if (!_useTestData && _repository == null)
            {
                throw new ArgumentNullException(nameof(repository), "Repository is required when not using test data");
            }
        }

        private void LoadTestData()
        {
            var testData = new TestData();
            _testRoutines = testData.GetTestRoutines();
        }

        public List<Routine> GetAllRoutines()
        {
            List<Routine> routines;

            if (_useTestData)
            {
                routines = _testRoutines;
            }
            else
            {
                routines = _repository.LoadRoutines();
            }

            // Routinen nach Order sortieren
            routines = routines.OrderBy(r => r.Order).ToList();

            // UND: Steps in jeder Routine sortieren
            foreach (var routine in routines)
            {
                if (routine.Steps != null && routine.Steps.Any())
                {
                    routine.Steps = routine.Steps.OrderBy(s => s.Order).ToList();
                }
            }

            return routines;
        }

        public Routine GetRoutine(string id)
        {
            Routine routine;

            if (_useTestData)
            {
                routine = _testRoutines?.FirstOrDefault(r => r.Id == id);
            }
            else
            {
                routine = _repository.LoadRoutines().FirstOrDefault(r => r.Id == id);
            }

            // ===== CRITICAL: Steps IMMER nach Order sortieren =====
            if (routine != null && routine.Steps != null && routine.Steps.Any())
            {
                routine.Steps = routine.Steps.OrderBy(s => s.Order).ToList();
            }

            return routine;
        }

        public void CreateRoutine(string name)
        {
            if (_useTestData)
            {
                var newRoutine = new Routine
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Order = _testRoutines.Count,
                    CreatedAt = DateTime.Now,
                    Steps = new List<RoutineStep>(),
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
            if (routine == null) return;

            if (_useTestData)
            {
                var index = _testRoutines.FindIndex(r => r.Id == routine.Id);
                if (index >= 0)
                {
                    var existing = _testRoutines[index];

                    // Nur Name und UpdatedAt aktualisieren
                    existing.Name = routine.Name;
                    existing.UpdatedAt = routine.UpdatedAt;

                    // Steps ersetzen (mit korrekten Orders)
                    var sortedSteps = routine.Steps.OrderBy(s => s.Order).ToList();
                    for (int i = 0; i < sortedSteps.Count; i++)
                    {
                        sortedSteps[i].Order = i;
                    }
                    existing.Steps = sortedSteps;

                    // WICHTIG: Die Order der Routine selbst NICHT ändern!
                    // existing.Order bleibt wie es ist
                }
            }
            else
            {
                var existing = _repository.GetRoutine(routine.Id);
                if (existing != null)
                {
                    existing.Name = routine.Name;
                    existing.UpdatedAt = routine.UpdatedAt;

                    var sortedSteps = routine.Steps.OrderBy(s => s.Order).ToList();
                    for (int i = 0; i < sortedSteps.Count; i++)
                    {
                        sortedSteps[i].Order = i;
                    }
                    existing.Steps = sortedSteps;

                    _repository.UpdateRoutine(existing);
                }
            }
        }

        public void DeleteRoutine(string id)
        {
            if (_useTestData)
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
                reorderedRoutines[i].Order = i; // nur Routine.Order
            }

            if (_useTestData)
            {
                _testRoutines = reorderedRoutines.ToList(); // neue Liste
            }
            else
            {
                foreach (var routine in reorderedRoutines)
                {
                    _repository.UpdateRoutine(routine);
                }
            }
        }

        public void SaveRoutine(Routine routine)
        {
            if (routine == null) return;

            if (_useTestData)
            {
                var index = _testRoutines.FindIndex(r => r.Id == routine.Id);
                if (index >= 0)
                {
                    // Orders explizit korrigieren
                    for (int i = 0; i < routine.Steps.Count; i++)
                    {
                        routine.Steps[i].Order = i;
                    }
                    _testRoutines[index] = routine;
                }
            }
            else
            {
                // Für echte Repository später ähnlich
                var existing = _repository.LoadRoutines().FirstOrDefault(r => r.Id == routine.Id);
                if (existing != null)
                {
                    existing.Name = routine.Name;
                    existing.UpdatedAt = DateTime.Now;

                    // Steps komplett ersetzen
                    existing.Steps.Clear();
                    foreach (var step in routine.Steps.OrderBy(s => s.Order))
                    {
                        existing.Steps.Add(new RoutineStep
                        {
                            Id = step.Id,
                            Order = step.Order,
                            Name = step.Name,
                            Description = step.Description,
                            Show = step.Show,
                            Type = step.Type,
                            Value = step.Value
                        });
                    }
                    _repository.UpdateRoutine(existing);
                }
            }
        }
        // Steps =================================================================================

        public void AddStep(string routineId, string name, string description, bool show, StepType type, string value)
        {
            var routine = GetRoutine(routineId);
            if (routine == null) return;

            int maxOrder = routine.Steps.Any() ? routine.Steps.Max(s => s.Order) : -1;

            var step = new RoutineStep
            {
                Id = Guid.NewGuid().ToString(),
                Order = maxOrder + 1,
                Name = name,
                Description = description,
                Show = show,
                Type = type,
                Value = value
            };

            routine.Steps.Add(step);

            if (!_useTestData)
            {
                _repository.UpdateRoutine(routine);
            }
        }

        public void UpdateStep(string routineId, string stepId, string name, string description, bool show, StepType type, string value)
        {
            var routine = GetRoutine(routineId);
            var step = routine?.Steps.FirstOrDefault(s => s.Id == stepId);

            if (step != null)
            {
                step.Name = name;
                step.Description = description;
                step.Show = show;
                step.Type = type;
                step.Value = value;

                if (!_useTestData)
                {
                    _repository.UpdateRoutine(routine);
                }
            }
        }

        public void RemoveStep(string routineId, string stepId)
        {
            var routine = GetRoutine(routineId);
            if (routine == null) return;

            var step = routine.Steps.FirstOrDefault(s => s.Id == stepId);
            if (step == null) return;

            // Schritt entfernen
            routine.Steps.Remove(step);

            // WICHTIG: Erst SORTIEREN, dann Orders neu vergeben
            var orderedSteps = routine.Steps.OrderBy(s => s.Order).ToList();
            for (int i = 0; i < orderedSteps.Count; i++)
            {
                orderedSteps[i].Order = i;
            }

            // Die sortierte Liste wieder zuweisen
            routine.Steps = orderedSteps;

            // UpdatedAt aktualisieren
            routine.UpdatedAt = DateTime.Now;

            // Speichern
            if (!_useTestData)
            {
                _repository.UpdateRoutine(routine);
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

            if (!_useTestData)
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