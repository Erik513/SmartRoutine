using CustomWFUI;
using CustomWFUI.Forms;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    public partial class AutoRunForm : StyledForm
    {
        private readonly Routine _routine;
        private readonly Func<RoutineStep, StepExecutionResult> _onStepExecute;
        private readonly Action<Routine> _onRoutineExecuted;

        private List<RoutineStep> _steps;
        private int _currentIndex;

        private Label _statusLabel;
        private Label _stepLabel;
        private ProgressBar _progressBar;
        private Button _cancelButton;

        private bool _cancelRequested;

        private const int StepDelayMilliseconds = 1000;

        public AutoRunForm(
            Routine routine,
            Func<RoutineStep, StepExecutionResult> onStepExecute,
            Action<Routine> onRoutineExecuted)
            : base(StyledFormOptions.CreateDialog(
                routine != null ? $"Auto-Ausführung: {routine.Name}" : "Auto-Ausführung",
                ContentAlignment.MiddleLeft,
                UIStyles.Colors.BackgroundDarkElevated))
        {
            _routine = routine;
            _onStepExecute = onStepExecute;
            _onRoutineExecuted = onRoutineExecuted;

            InitializeForm();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BeginInvoke(new Action(RunSteps));
        }

        private void InitializeForm()
        {
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(500, 220);
            MinimumSize = Size;
            MaximumSize = Size;
            BackColor = UIStyles.Colors.BackgroundDark;
            TopMost = true;

            TableLayoutPanel layout = UIStyles.TableLayoutPanels.CreateStandard(1, 4);
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(20);

            layout.RowStyles.Clear();
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            _statusLabel = UIStyles.Labels.CreateTitle("Routine wird ausgeführt...");
            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.TextAlign = ContentAlignment.MiddleCenter;

            _stepLabel = UIStyles.Labels.CreateNormal("");
            _stepLabel.Dock = DockStyle.Fill;
            _stepLabel.TextAlign = ContentAlignment.MiddleCenter;
            _stepLabel.AutoEllipsis = true;

            _progressBar = new ProgressBar();
            _progressBar.Dock = DockStyle.Fill;
            _progressBar.Minimum = 0;

            _cancelButton = UIStyles.Buttons.CreateStandard(
                "✖",
                "Abbrechen",
                new Size(80, 30),
                true);

            _cancelButton.Dock = DockStyle.Fill;
            _cancelButton.Click += OnCancelButtonClick;

            TableLayoutPanel buttonPanel =
            UIStyles.TableLayoutPanels.CreateStandard(2, 1);

            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.BackColor = Color.Transparent;

            buttonPanel.RowStyles.Clear();
            buttonPanel.ColumnStyles.Clear();

            buttonPanel.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            buttonPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            buttonPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 120));

            buttonPanel.Controls.Add(_cancelButton, 1, 0);

            layout.Controls.Add(_statusLabel, 0, 0);
            layout.Controls.Add(_stepLabel, 0, 1);
            layout.Controls.Add(_progressBar, 0, 2);
            layout.Controls.Add(buttonPanel, 0, 3);

            ContentPanel.Controls.Clear();
            ContentPanel.Controls.Add(layout);
        }
        private void RunSteps()
        {
            _steps = RoutineStepFactory.CreateAutoRunSteps(_routine);
            _currentIndex = 0;

            _progressBar.Maximum = _steps.Count;
            _progressBar.Value = 0;

            if (_steps.Count == 0)
            {
                _statusLabel.Text = "Keine ausführbaren Schritte.";
                return;
            }

            RunNextStep();
        }

        private void RunNextStep()
        {
            if (_cancelRequested)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            if (_currentIndex >= _steps.Count)
            {
                MarkRoutineExecuted();
                _statusLabel.Text = "Fertig.";
                _stepLabel.Text = "Alle Schritte wurden ausgeführt.";
                _cancelButton.Text = "Schließen";
                _cancelButton.Enabled = true;
                _cancelButton.Click -= OnCancelButtonClick;
                _cancelButton.Click += (s, e) =>
                {
                    DialogResult = DialogResult.OK;
                    Close();
                };

                return;
            }

            RoutineStep step = _steps[_currentIndex];

            _stepLabel.Text =
                $"{_currentIndex + 1}/{_steps.Count}: {step.Name}";

            try
            {
                if (_onStepExecute != null)
                    _onStepExecute(step);
            }
            catch
            {
                // Optional später: Fehlerliste anzeigen
            }

            _currentIndex++;
            _progressBar.Value = Math.Min(_currentIndex, _progressBar.Maximum);

            Timer timer = new Timer();
            timer.Interval = StepDelayMilliseconds;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
                RunNextStep();
            };
            timer.Start();
        }

        private void MarkRoutineExecuted()
        {
            if (_routine == null)
                return;

            _routine.LastExecutionAt = DateTime.Now;

            if (_onRoutineExecuted != null)
                _onRoutineExecuted(_routine);
        }
        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            _cancelRequested = true;
            _cancelButton.Enabled = false;
            _statusLabel.Text = "Wird abgebrochen...";
        }

        private void CloseAfterDelay()
        {
            Timer timer = new Timer();
            timer.Interval = 700;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();

                DialogResult = DialogResult.OK;
                Close();
            };
            timer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancelRequested = true;

            base.OnFormClosing(e);
        }
    }
}