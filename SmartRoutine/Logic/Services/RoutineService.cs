using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.TestData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SmartRoutine.Logic.Services
{
    public class RoutineService : IRoutineService
    {
        private readonly IRoutineRepository _repository;
        private List<Routine> _testRoutines;
        private readonly bool _useTestData;

        public bool UseTestData => _useTestData;

        public RoutineService(IRoutineRepository repository = null, bool useTestData = false)
        {
            _repository = repository;
            _useTestData = useTestData;

            if (_useTestData)
            {
                LoadTestData();
            }
            else if (_repository == null)
            {
                throw new ArgumentNullException(
                    nameof(repository),
                    "Repository is required when test data is disabled.");
            }
        }

        private void LoadTestData()
        {
            var testData = new TestDataFactory();
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

            if (routine != null && routine.Steps != null && routine.Steps.Any())
            {
                routine.Steps = routine.Steps.OrderBy(s => s.Order).ToList();
            }

            return routine;
        }

        public Routine CreateRoutine(string name)
        {
            if (_useTestData)
            {
                int maxOrder = _testRoutines.Count > 0 ? _testRoutines.Max(r => r.Order) : -1;

                var newRoutine = new Routine
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Order = maxOrder + 1,
                    CreatedAt = DateTime.Now,
                    Steps = new List<RoutineStep>(),
                };

                _testRoutines.Add(newRoutine);
                return newRoutine;
            }
            else
            {
                var routines = _repository.LoadRoutines();
                int maxOrder = routines.Count > 0 ? routines.Max(r => r.Order) : -1;

                var routine = new Routine
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Order = maxOrder + 1,
                    CreatedAt = DateTime.Now,
                    Steps = new List<RoutineStep>()
                };

                _repository.AddRoutine(routine);
                return routine;
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

            NormalizeStepOrders(routine);

            if (_useTestData)
                SaveRoutineInMemory(routine);
            else
                SaveRoutineInRepository(routine);
        }

        private void NormalizeStepOrders(Routine routine)
        {
            if (routine.Steps == null)
                routine.Steps = new List<RoutineStep>();

            var orderedSteps = routine.Steps.OrderBy(s => s.Order).ToList();

            for (int i = 0; i < orderedSteps.Count; i++)
                orderedSteps[i].Order = i;

            routine.Steps = orderedSteps;
        }

        private void SaveRoutineInMemory(Routine routine)
        {
            var index = _testRoutines.FindIndex(r => r.Id == routine.Id);

            if (index >= 0)
            {
                routine.UpdatedAt = DateTime.Now;
                _testRoutines[index] = routine;
                return;
            }

            int maxOrder = _testRoutines.Count > 0 ? _testRoutines.Max(r => r.Order) : -1;

            routine.Order = maxOrder + 1;
            routine.CreatedAt = DateTime.Now;

            _testRoutines.Add(routine);
        }

        private void SaveRoutineInRepository(Routine routine)
        {
            var existing = _repository.GetRoutine(routine.Id);

            if (existing != null)
            {
                existing.Name = routine.Name;
                existing.UpdatedAt = DateTime.Now;
                existing.LastExecutionAt = routine.LastExecutionAt;
                existing.Steps = routine.Steps.Select(CopyStep).ToList();

                _repository.UpdateRoutine(existing);
                return;
            }

            var routines = _repository.LoadRoutines();
            int maxOrder = routines.Count > 0 ? routines.Max(r => r.Order) : -1;

            routine.Order = maxOrder + 1;
            routine.CreatedAt = DateTime.Now;

            _repository.AddRoutine(routine);
        }


        private RoutineStep CopyStep(RoutineStep original)
        {
            switch (original)
            {
                case OpenUrlStep urlStep:
                    return new OpenUrlStep
                    {
                        Id = urlStep.Id,
                        Order = urlStep.Order,
                        Name = urlStep.Name,
                        Description = urlStep.Description,
                        Show = urlStep.Show,
                        AutoStart = urlStep.AutoStart,
                        Url = urlStep.Url,
                        OpenInExternalBrowser = urlStep.OpenInExternalBrowser
                    };
                case OpenFolderStep folderStep:
                    return new OpenFolderStep
                    {
                        Id = folderStep.Id,
                        Order = folderStep.Order,
                        Name = folderStep.Name,
                        Description = folderStep.Description,
                        Show = folderStep.Show,
                        AutoStart = folderStep.AutoStart,
                        FolderPath = folderStep.FolderPath,
                        OpenInNewWindow = folderStep.OpenInNewWindow
                    };
                case OpenApplicationStep appStep:
                    return new OpenApplicationStep
                    {
                        Id = appStep.Id,
                        Order = appStep.Order,
                        Name = appStep.Name,
                        Description = appStep.Description,
                        Show = appStep.Show,
                        AutoStart = appStep.AutoStart,
                        ApplicationPath = appStep.ApplicationPath,
                        Arguments = appStep.Arguments,
                        RunAsAdmin = appStep.RunAsAdmin,
                        WorkingDirectory = appStep.WorkingDirectory
                    };
                case OpenDocumentStep docStep:  // ← Neu
                    return new OpenDocumentStep
                    {
                        Id = docStep.Id,
                        Order = docStep.Order,
                        Name = docStep.Name,
                        Description = docStep.Description,
                        Show = docStep.Show,
                        AutoStart = docStep.AutoStart,
                        FilePath = docStep.FilePath,
                        OpenWithAssociatedApp = docStep.OpenWithAssociatedApp
                    };
                default:
                    throw new NotSupportedException($"Step type {original.GetType()} not supported");
            }
        }
        
        // Steps =================================================================================

        public void AddStep(string routineId, RoutineStep step)
        {
            var routine = GetRoutine(routineId);
            if (routine == null) return;

            int maxOrder = routine.Steps.Any() ? routine.Steps.Max(s => s.Order) : -1;
            step.Order = maxOrder + 1;
            step.Id = Guid.NewGuid().ToString();

            routine.Steps.Add(step);

            if (!_useTestData)
            {
                _repository.UpdateRoutine(routine);  // LiteDB speichert alles korrekt!
            }
        }

        public void UpdateStep(string routineId, RoutineStep step)
        {
            var routine = GetRoutine(routineId);
            if (routine == null)
            {
                return;
            }

            var index = routine.Steps.ToList().FindIndex(s => s.Id == step.Id);
            if (index >= 0)
            {
                routine.Steps[index] = step;

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

            // VALIDIERUNG: Prüfe ob Indizes gültig sind
            if (oldIndex < 0 || oldIndex >= steps.Count) return;
            if (newIndex < 0 || newIndex >= steps.Count) return;
            if (oldIndex == newIndex) return;

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

        public StepExecutionResult ExecuteStep(RoutineStep step)
        {
            if (step == null)
                return StepExecutionResult.None();

            switch (step)
            {
                case OpenUrlStep urlStep:
                    return ExecuteOpenUrl(urlStep);

                case OpenFolderStep folderStep:
                    ExecuteOpenFolder(folderStep);
                    return StepExecutionResult.None();

                case OpenApplicationStep appStep:
                    ExecuteOpenApplication(appStep);
                    return StepExecutionResult.None();

                case OpenDocumentStep docStep:
                    ExecuteOpenDocument(docStep);
                    return StepExecutionResult.None();

                default:
                    throw new NotSupportedException($"Step type {step.GetType()} not supported");
            }
        }

        private StepExecutionResult ExecuteOpenUrl(OpenUrlStep step)
        {
            if (step.OpenInExternalBrowser)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = step.Url,
                    UseShellExecute = true
                });

                return StepExecutionResult.None();
            }

            return StepExecutionResult.OpenInternalUrl(step.Url);
        }

        private void ExecuteOpenFolder(OpenFolderStep step)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = step.FolderPath,
                UseShellExecute = true
            });
        }

        private void ExecuteOpenApplication(OpenApplicationStep step)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = step.ApplicationPath,
                Arguments = step.Arguments ?? string.Empty,
                UseShellExecute = true
            };
            if (step.RunAsAdmin)
                startInfo.Verb = "runas";
            Process.Start(startInfo);
        }

        private void ExecuteOpenDocument(OpenDocumentStep step)
        {
            if (System.IO.File.Exists(step.FilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = step.FilePath,
                    UseShellExecute = true
                });
            }
        }

        public bool ValidateStep(
            RoutineStep step,
            out string errorMessage)
        {
            errorMessage = null;

            switch (step)
            {
                case OpenUrlStep urlStep:

                    if (string.IsNullOrWhiteSpace(urlStep.Url))
                    {
                        errorMessage = "Die URL ist leer.";
                        return false;
                    }

                    if (!Uri.IsWellFormedUriString(urlStep.Url, UriKind.Absolute))
                    {
                        errorMessage = $"Die URL '{urlStep.Url}' ist ungültig.";
                        return false;
                    }

                    return true;

                case OpenFolderStep folderStep:

                    if (string.IsNullOrWhiteSpace(folderStep.FolderPath))
                    {
                        errorMessage = "Der Ordnerpfad ist leer.";
                        return false;
                    }

                    if (!Directory.Exists(folderStep.FolderPath))
                    {
                        errorMessage =
                            $"Der Ordner existiert nicht:\n{folderStep.FolderPath}";

                        return false;
                    }

                    return true;

                case OpenApplicationStep appStep:

                    if (string.IsNullOrWhiteSpace(appStep.ApplicationPath))
                    {
                        errorMessage = "Der Programmpfad ist leer.";
                        return false;
                    }

                    if (!File.Exists(appStep.ApplicationPath))
                    {
                        errorMessage =
                            $"Die Anwendung existiert nicht:\n{appStep.ApplicationPath}";

                        return false;
                    }

                    return true;

                case OpenDocumentStep docStep:

                    if (string.IsNullOrWhiteSpace(docStep.FilePath))
                    {
                        errorMessage = "Der Dateipfad ist leer.";
                        return false;
                    }

                    if (!File.Exists(docStep.FilePath))
                    {
                        errorMessage =
                            $"Die Datei existiert nicht:\n{docStep.FilePath}";

                        return false;
                    }

                    return true;

                default:
                    return true;
            }
        }
    }
}