using ErikwnkWFUI;
using ErikwnkWFUI.Forms;
using SmartRoutine.Data.Models;
using SmartRoutine.Localization;
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
        private int _stepDelayMilliseconds = 1000;

        /// <summary>Pause between two steps in milliseconds (default 1000).</summary>
        public int StepDelayMilliseconds
        {
            get { return _stepDelayMilliseconds; }
            set { _stepDelayMilliseconds = value; }
        }

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
                    routine != null ? Loc.S("AutoRun.Title", routine.Name) : Loc.S("AutoRun.TitleNoRoutine"),
                    ContentAlignment.MiddleLeft,
                    UIStyles.Colors.BackgroundDarkElevated),
                Loc.S("AutoRun.Status.Running"),
                true)
        {
            _routine = routine;
            _onStepExecute = onStepExecute;
            _onRoutineExecuted = onRoutineExecuted;

            BarColors = UIStyles.Colors.Sets.Green;
            CancelButtonText = Loc.S("Common.Cancel");
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
                Fail(Loc.S("AutoRun.Status.NoSteps"));
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
            catch (Exception ex)
            {
                // The run goes on with the next step; the failed ones are listed at the end.
                string name = string.IsNullOrWhiteSpace(step.Name) ? Loc.S("AutoRun.UnnamedStep") : step.Name;
                _failedSteps.Add($"{name} ({ex.Message})");
            }

            _currentIndex++;
            Report(_currentIndex * 100 / _steps.Count);

            StopStepTimer();

            _stepTimer = new Timer();
            _stepTimer.Interval = Math.Max(1, _stepDelayMilliseconds);
            _stepTimer.Tick += (s, e) =>
            {
                StopStepTimer();
                RunNextStep();
            };
            _stepTimer.Start();
        }

        private void Finish()
        {
            if (_failedSteps.Count > 0)
            {
                // A routine with failed steps does not count as executed.
                Fail(Loc.S("AutoRun.Status.Failed", string.Join("\n", _failedSteps)));
                return;
            }

            try
            {
                MarkRoutineExecuted();
            }
            catch (Exception ex)
            {
                Fail(Loc.S("AutoRun.Status.SaveFailed", ex.Message));
                return;
            }

            Succeed(Loc.S("AutoRun.Status.Done"));
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
