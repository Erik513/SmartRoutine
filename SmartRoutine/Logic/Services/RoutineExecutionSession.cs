using SmartRoutine.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmartRoutine.Logic.Services
{
    public class RoutineExecutionSession
    {
        private readonly List<RoutineStep> _steps;
        private readonly HashSet<string> _executedStepIds = new HashSet<string>();

        public int CurrentIndex { get; private set; } = 0;

        public RoutineStep CurrentStep => HasExecutableSteps ? _steps[CurrentIndex] : null;

        public int StepCount => _steps.Count;

        public bool HasExecutableSteps => _steps.Count > 0;

        public bool CanGoPrevious => CurrentIndex > 0;

        public bool CanGoNext => CurrentIndex < _steps.Count - 1;

        public bool CurrentStepWasExecuted =>
            CurrentStep != null && _executedStepIds.Contains(CurrentStep.Id);

        public bool ShouldAutoExecuteCurrentStep =>
            CurrentStep != null && CurrentStep.AutoStart && !CurrentStepWasExecuted;

        public bool CanExecuteCurrentStepManually =>
            CurrentStep != null && (!CurrentStep.AutoStart || CurrentStepWasExecuted);

        public string StepCounterText => $"{CurrentIndex + 1}/{_steps.Count}";

        public RoutineExecutionSession(Routine routine, RoutineStep specificStep = null)
        {
            if (specificStep != null)
            {
                _steps = new List<RoutineStep> { specificStep };
            }
            else
            {
                _steps = routine?.Steps?
                    .Where(s => s.Show)
                    .OrderBy(s => s.Order)
                    .ToList() ?? new List<RoutineStep>();
            }
        }

        public void GoToStep(int index)
        {
            if (index < 0 || index >= _steps.Count)
                return;

            CurrentIndex = index;
        }

        public void GoNext()
        {
            if (CanGoNext)
                CurrentIndex++;
        }

        public void GoPrevious()
        {
            if (CanGoPrevious)
                CurrentIndex--;
        }

        public void MarkCurrentStepExecuted()
        {
            _executedStepIds.Add(CurrentStep.Id);
        }
    }
}