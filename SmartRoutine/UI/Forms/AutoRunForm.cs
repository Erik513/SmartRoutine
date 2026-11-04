using ErikwnkWFUI;
using ErikwnkWFUI.Forms;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    /// <summary>
    /// Runs all steps of a routine one after the other and shows how far it has come
    /// (a <see cref="ProgressForm"/> of the library; the user can cancel).
    /// </summary>
    public partial class AutoRunForm : ProgressForm
    {
        private const int StepDelayMilliseconds = 1000;

        private readonly Routine _routine;
        private readonly Func<RoutineStep, StepExecutionResult> _onStepExecute;
        private readonly Action<Routine> _onRoutineExecuted;
        private readonly List<string> _failedSteps = new List<string>();

        private List<RoutineStep> _steps;
        private int _currentIndex;
        private Timer _stepTimer;

        public AutoRunForm(
            Routine routine,
            Func<RoutineStep, StepExecutionResult> onStepExecute,
            Action<Routine> onRoutineExecuted)
            : base(
                FormOptions.CreateDialog(
                    routine != null ? $"Auto-Ausführung: {routine.Name}" : "Auto-Ausführung",
                    ContentAlignment.MiddleLeft,
                    UIStyles.Colors.BackgroundDarkElevated),
                "Routine wird ausgeführt...",
                true)
        {
            _routine = routine;
            _onStepExecute = onStepExecute;
            _onRoutineExecuted = onRoutineExecuted;

            BarColors = UIStyles.Colors.Sets.Green;
            CancelButtonText = "Abbrechen";
            TopMost = true;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BeginInvoke(new Action(RunSteps));
        }

        private void RunSteps()
        {
            _steps = RoutineStepFactory.CreateAutoRunSteps(_routine);
            _currentIndex = 0;

            if (_steps.Count == 0)
            {
                Fail("Keine ausführbaren Schritte.");
                return;
            }

            RunNextStep();
        }

        private void RunNextStep()
        {
            if (IsDisposed)
                return;

            if (IsCancelled)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            if (_currentIndex >= _steps.Count)
            {
                Finish();
                return;
            }

            RoutineStep step = _steps[_currentIndex];

            Status = $"{_currentIndex + 1}/{_steps.Count}: {step.Name}";

            try
            {
                if (_onStepExecute != null)
                    _onStepExecute(step);
            }
            catch
            {
                // The run goes on with the next step; the failed ones are listed at the end.
                _failedSteps.Add(step.Name);
            }

            _currentIndex++;
            Report(_currentIndex * 100 / _steps.Count);

            StopStepTimer();

            _stepTimer = new Timer();
            _stepTimer.Interval = StepDelayMilliseconds;
            _stepTimer.Tick += (s, e) =>
            {
                StopStepTimer();
                RunNextStep();
            };
            _stepTimer.Start();
        }

        private void Finish()
        {
            MarkRoutineExecuted();

            if (_failedSteps.Count == 0)
                Succeed("Alle Schritte wurden ausgeführt.");
            else
                Fail("Nicht ausgeführt: " + string.Join(", ", _failedSteps));
        }

        private void MarkRoutineExecuted()
        {
            if (_routine == null)
                return;

            _routine.LastExecutionAt = DateTime.Now;

            if (_onRoutineExecuted != null)
                _onRoutineExecuted(_routine);
        }

        private void StopStepTimer()
        {
            if (_stepTimer == null)
                return;

            _stepTimer.Stop();
            _stepTimer.Dispose();
            _stepTimer = null;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopStepTimer();

            base.OnFormClosing(e);
        }
    }
}
