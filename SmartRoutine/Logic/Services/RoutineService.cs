using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
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

        public RoutineService(IRoutineRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Routine> GetAllRoutines()
        {
            var routines = _repository.LoadRoutines()
                .OrderBy(r => r.Order)
                .ToList();

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
            var routine = _repository.GetRoutine(id);

            if (routine != null && routine.Steps != null && routine.Steps.Any())
            {
                routine.Steps = routine.Steps
                    .OrderBy(s => s.Order)
                    .ToList();
            }

            return routine;
        }

        public Routine CreateRoutine(string name)
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

        public void DeleteRoutine(string id)
        {
            _repository.DeleteRoutine(id);
        }

        public void ReorderRoutines(List<Routine> reorderedRoutines)
        {
            if (reorderedRoutines == null) return;

            for (int i = 0; i < reorderedRoutines.Count; i++)
            {
                reorderedRoutines[i].Order = i; // nur Routine.Order
            }

            foreach (var routine in reorderedRoutines)
            {
                _repository.UpdateRoutine(routine);
            }
        }

        public void SaveRoutine(Routine routine)
        {
            if (routine == null) return;

            NormalizeStepOrders(routine);

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

        private void NormalizeStepOrders(Routine routine)
        {
            if (routine.Steps == null)
                routine.Steps = new List<RoutineStep>();

            var orderedSteps = routine.Steps.OrderBy(s => s.Order).ToList();

            for (int i = 0; i < orderedSteps.Count; i++)
                orderedSteps[i].Order = i;

            routine.Steps = orderedSteps;
        }

        private RoutineStep CopyStep(RoutineStep original)
        {
            return RoutineStepFactory.CreateCopy(original);
        }

        // Steps =================================================================================

        public void AddStep(string routineId, RoutineStep step)
        {
            var routine = GetRoutine(routineId);
            if (routine == null || step == null) return;

            int maxOrder = routine.Steps.Any() ? routine.Steps.Max(s => s.Order) : -1;
            step.Order = maxOrder + 1;

            if (string.IsNullOrWhiteSpace(step.Id))
                step.Id = Guid.NewGuid().ToString();

            routine.Steps.Add(step);

            _repository.UpdateRoutine(routine);
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

                _repository.UpdateRoutine(routine);
            }
        }

        public void RemoveStep(string routineId, string stepId)
        {
            var routine = GetRoutine(routineId);
            if (routine == null) return;

            var step = routine.Steps.FirstOrDefault(s => s.Id == stepId);
            if (step == null) return;

            routine.Steps.Remove(step);

            var orderedSteps = routine.Steps.OrderBy(s => s.Order).ToList();

            for (int i = 0; i < orderedSteps.Count; i++)
            {
                orderedSteps[i].Order = i;
            }

            routine.Steps = orderedSteps;
            routine.UpdatedAt = DateTime.Now;

            _repository.UpdateRoutine(routine);
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

            _repository.UpdateRoutine(routine);
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

            if (step == null)
            {
                errorMessage = "Der Schritt darf nicht null sein.";
                return false;
            }

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