using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartRoutine.Logic
{
    public class RoutineService : IRoutineService
    {
        private readonly IRoutineRepository _repository;

        public RoutineService(IRoutineRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Routine> GetAllRoutines() => _repository.LoadRoutines();

        public Routine GetRoutine(string id) => _repository.LoadRoutines().FirstOrDefault(r => r.Id == id);

        public void CreateRoutine(string name)
        {
            var routine = new Routine
            {
                Name = name,
                CreatedAt = DateTime.Now
            };
            _repository.AddRoutine(routine);
        }

        public void UpdateRoutine(Routine routine) => _repository.UpdateRoutine(routine);

        public void DeleteRoutine(string id) => _repository.DeleteRoutine(id);

        public void AddStep(string routineId, StepType type, string value, string description)
        {
            var routine = GetRoutine(routineId);
            if (routine == null) return;

            var step = new RoutineStep
            {
                Order = routine.Steps.Count,
                Type = type,
                Value = value,
                Description = description
            };

            routine.Steps.Add(step);
            _repository.UpdateRoutine(routine);
        }

        public void UpdateStep(string routineId, string stepId, string value, string description)
        {
            var routine = GetRoutine(routineId);
            var step = routine?.Steps.FirstOrDefault(s => s.Id == stepId);

            if (step != null)
            {
                step.Value = value;
                step.Description = description;
                _repository.UpdateRoutine(routine);
            }
        }

        public void RemoveStep(string routineId, string stepId)
        {
            var routine = GetRoutine(routineId);
            var step = routine?.Steps.FirstOrDefault(s => s.Id == stepId);

            if (step != null)
            {
                routine.Steps.Remove(step);

                // Orders aktualisieren
                for (int i = 0; i < routine.Steps.Count; i++)
                    routine.Steps[i].Order = i;

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
            _repository.UpdateRoutine(routine);
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
