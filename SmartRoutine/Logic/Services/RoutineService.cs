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
            Debug.WriteLine($"=== SERVICE GetRoutine: {id} ===");

            Routine routine;
            if (_useTestData)
            {
                routine = _testRoutines?.FirstOrDefault(r => r.Id == id);
                Debug.WriteLine($"  TestData Modus, Routine gefunden: {routine != null}");
            }
            else
            {
                routine = _repository.LoadRoutines().FirstOrDefault(r => r.Id == id);
                Debug.WriteLine($"  Repository Modus, Routine gefunden: {routine != null}");
            }

            if (routine != null && routine.Steps != null && routine.Steps.Any())
            {
                Debug.WriteLine($"  Steps vor Sortierung: {string.Join(", ", routine.Steps.Select(s => s.Name))}");
                routine.Steps = routine.Steps.OrderBy(s => s.Order).ToList();
                Debug.WriteLine($"  Steps nach Sortierung: {string.Join(", ", routine.Steps.Select(s => s.Name))}");

                // Zeige Step-Typen an
                foreach (var step in routine.Steps)
                {
                    Debug.WriteLine($"    Step: {step.Name}, Typ: {step.GetType().Name}");
                    if (step is OpenUrlStep urlStep)
                        Debug.WriteLine($"      Url: {urlStep.Url}, OpenInternally: {urlStep.OpenInExternBrowser}");
                }
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
                    for (int i = 0; i < routine.Steps.Count; i++)
                    {
                        routine.Steps[i].Order = i;
                    }
                    _testRoutines[index] = routine;
                }
                else
                {
                    routine.Order = _testRoutines.Count;
                    _testRoutines.Add(routine);
                }
            }
            else
            {
                var existing = _repository.LoadRoutines().FirstOrDefault(r => r.Id == routine.Id);
                if (existing != null)
                {
                    existing.Name = routine.Name;
                    existing.UpdatedAt = DateTime.Now;
                    existing.Steps.Clear();
                    foreach (var step in routine.Steps.OrderBy(s => s.Order))
                    {
                        // Kopiere den Step mit seinem konkreten Typ
                        existing.Steps.Add(CopyStep(step));
                    }
                    _repository.UpdateRoutine(existing);
                }
                else
                {
                    var routines = _repository.LoadRoutines();
                    int maxOrder = routines.Count > 0 ? routines.Max(r => r.Order) : -1;
                    routine.Order = maxOrder + 1;
                    routine.CreatedAt = DateTime.Now;
                    _repository.AddRoutine(routine);
                }
            }
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
                        OpenInExternBrowser = urlStep.OpenInExternBrowser
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
            Debug.WriteLine($"=== SERVICE UpdateStep ===");
            Debug.WriteLine($"  RoutineId: {routineId}");
            Debug.WriteLine($"  Step.Id: {step.Id}");
            Debug.WriteLine($"  Step.Name: {step.Name}");
            Debug.WriteLine($"  Step.Type: {step.GetType().Name}");

            var routine = GetRoutine(routineId);
            if (routine == null)
            {
                Debug.WriteLine("  FEHLER: Routine nicht gefunden!");
                return;
            }

            Debug.WriteLine($"  Routine gefunden: {routine.Name}, Steps: {routine.Steps.Count}");

            var index = routine.Steps.ToList().FindIndex(s => s.Id == step.Id);
            if (index >= 0)
            {
                Debug.WriteLine($"  Step gefunden an Index {index}");
                Debug.WriteLine($"  Alter Name: {routine.Steps[index].Name}");
                Debug.WriteLine($"  Neuer Name: {step.Name}");

                routine.Steps[index] = step;

                if (!_useTestData)
                {
                    _repository.UpdateRoutine(routine);
                    Debug.WriteLine("  Repository.UpdateRoutine aufgerufen");
                }
                else
                {
                    Debug.WriteLine("  TestData Modus - keine Repository Speicherung");
                }
            }
            else
            {
                Debug.WriteLine("  FEHLER: Step nicht gefunden in Routine!");
                foreach (var s in routine.Steps)
                {
                    Debug.WriteLine($"    Vorhandener Step: {s.Id} - {s.Name}");
                }
            }
        }

        // Die alte Methode darf NICHT mehr aufgerufen werden.
        // Sie kannst du löschen oder als obsolete markieren:
        [Obsolete("Use UpdateStep(string routineId, RoutineStep step) instead")]
        public void UpdateStep(string routineId, string stepId, string name, string description, bool show, StepType type, string value)
        {
            // Alte Implementierung - wird nicht mehr verwendet
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

        public void ExecuteStep(RoutineStep step)
        {
            switch (step)
            {
                case OpenUrlStep urlStep:
                    if (urlStep.OpenInExternBrowser)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = urlStep.Url,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        // In interner WebView öffnen (Event auslösen)
                        // OnOpenUrlInWebView?.Invoke(urlStep.Url);
                    }
                    break;
                case OpenFolderStep folderStep:
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = folderStep.FolderPath,
                        UseShellExecute = true
                    });
                    break;
                case OpenApplicationStep appStep:
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = appStep.ApplicationPath,
                        Arguments = appStep.Arguments,
                        UseShellExecute = true
                    };
                    if (appStep.RunAsAdmin)
                        startInfo.Verb = "runas";
                    Process.Start(startInfo);
                    break;
                case OpenDocumentStep docStep:
                    if (System.IO.File.Exists(docStep.FilePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = docStep.FilePath,
                            UseShellExecute = true
                        });
                    }
                    break;
                default:
                    throw new NotSupportedException($"Step type {step.GetType()} not supported");
            }
        }
        public bool ValidateStep(RoutineStep step)
        {
            switch (step)
            {
                case OpenUrlStep urlStep:
                    return Uri.IsWellFormedUriString(urlStep.Url, UriKind.Absolute);
                case OpenFolderStep folderStep:
                    return !string.IsNullOrWhiteSpace(folderStep.FolderPath) &&
                           System.IO.Directory.Exists(folderStep.FolderPath);
                case OpenApplicationStep appStep:
                    return !string.IsNullOrWhiteSpace(appStep.ApplicationPath) &&
                           System.IO.File.Exists(appStep.ApplicationPath);
                case OpenDocumentStep docStep:
                    return !string.IsNullOrWhiteSpace(docStep.FilePath) &&
                           System.IO.File.Exists(docStep.FilePath);
                default:
                    return true;
            }
        }
    }
}