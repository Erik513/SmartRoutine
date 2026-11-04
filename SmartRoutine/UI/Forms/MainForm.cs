using SmartRoutine.Data.Models;
using SmartRoutine.Localization;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;
using ErikwnkWFUI;
using ErikwnkWFUI.Forms;
using ErikwnkWFUI.Styles;
using ErikwnkCore;
using MessageBox = ErikwnkWFUI.Forms.MessageBox;
using MessageBoxButtons = ErikwnkWFUI.Forms.MessageBoxButtons;
using MessageBoxIcon = ErikwnkWFUI.Forms.MessageBoxIcon;
using ComboBox = System.Windows.Forms.ComboBox;

namespace SmartRoutine.UI.Forms
{
    public partial class MainForm : AppForm
    {
        private static readonly Size DefaultWindowSize = new Size(1024, 768);
        private static readonly Size MinimumWindowSize = new Size(800, 600);


        private readonly IRoutineService _routineService;

        private RoutinesViewUC _routinesView;
        private RoutineEditorUC _editorView;
        private bool _updateCheckStarted;

        public MainForm(IRoutineService routineService)
            : base(FormOptions.CreateStandard(
                "SmartRoutine",
                ContentAlignment.MiddleLeft,
                null,
                Properties.Resources.IconLogo))
            {
            if (routineService == null)
                throw new ArgumentNullException(nameof(routineService));

            _routineService = routineService;

            ConfigureForm();
            CreateViews();
            WireEvents();
            AddViewsToContentPanel();

            ShowRoutinesView();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CleanupBeforeClose();
            DisposeUpdateResources();
            base.OnFormClosing(e);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // OnShown feuert auch erneut, wenn das Form nach ExecuteFullRoutine's
            // Hide()/Show()-Wechsel wieder sichtbar wird - der Update-Check soll
            // aber nur einmal pro Programmstart laufen.
            if (_updateCheckStarted)
                return;

            _updateCheckStarted = true;

            CheckForUpdatesAsync();
        }

        private void ConfigureForm()
        {
            MinimumSize = MinimumWindowSize;
            Size = DefaultWindowSize;
            CenterToScreen();
        }

        private void CreateViews()
        {
            UrlValidationService urlValidationService = new UrlValidationService();

            _routinesView = new RoutinesViewUC(_routineService)
            {
                Dock = DockStyle.Fill
            };

            _editorView = new RoutineEditorUC(_routineService, urlValidationService)
            {
                Dock = DockStyle.Fill
            };
        }

        private void WireEvents()
        {
            _routinesView.NewRoutineClicked += OnNewRoutineClicked;
            _routinesView.EditRoutineClicked += OnEditRoutineClicked;
            _routinesView.DeleteRoutineClicked += OnDeleteRoutineClicked;
            _routinesView.StartRoutineClicked += OnStartRoutineClicked;
            _routinesView.StartRoutineAutoClicked += OnStartRoutineAutoClicked;

            _editorView.BackToRoutinesClicked += OnBackToRoutinesClicked;
            _editorView.SaveChanges += OnSaveChanges;
        }

        private void AddViewsToContentPanel()
        {
            ContentPanel.Controls.Clear();
            ContentPanel.Padding = new Padding(1);

            ContentPanel.Controls.Add(_routinesView);
            ContentPanel.Controls.Add(_editorView);
        }

        private void ShowRoutinesView()
        {
            _editorView.Hide();
            _routinesView.Show();
            _routinesView.LoadRoutines();
        }

        private void ShowEditorView(Routine routine, bool isNewRoutine)
        {
            _routinesView.Hide();
            _editorView.Show();
            _editorView.LoadRoutine(routine, isNewRoutine);
        }

        private void SaveRoutine(Routine routine)
        {
            if (routine == null)
                return;

            _routineService.SaveRoutine(routine);
            _routinesView.LoadRoutines();
        }

        private void DeleteRoutine(Routine routine)
        {
            if (routine == null)
                return;

            DialogResult result = MessageBox.Show(
                Loc.S("Main.DeleteRoutine.Text", routine.Name),
                Loc.S("Common.Confirm"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                this,
                FormSize.Small);

            if (result != DialogResult.Yes)
                return;

            _routineService.DeleteRoutine(routine.Id);
            _routinesView.LoadRoutines();
        }

        private void ExecuteFullRoutine(Routine routine)
        {
            if (!CanExecuteRoutine(routine))
                return;

            bool previousShowInTaskbar = ShowInTaskbar;
            bool previousVisible = Visible;

            ExecutionForm form = new ExecutionForm(routine, ExecuteRoutineStep);

            form.Shown += (s, e) =>
            {
                Hide();

                form.ShowInTaskbar = true;
                form.Activate();
                form.BringToFront();
            };

            form.BeforeCloseRequested += (s, e) =>
                RestoreAfterExecution(previousShowInTaskbar, previousVisible);

            bool finished;

            try
            {
                form.ShowDialog();
            }
            finally
            {
                finished = form.IsFinished;
                form.Dispose();

                RestoreAfterExecution(previousShowInTaskbar, previousVisible);
            }

            // Only a routine that was gone through to its end counts as executed.
            if (finished)
            {
                MarkRoutineAsExecuted(routine);
                _routinesView.LoadRoutines();
            }
        }

        // Brings the main window back the way it was before the execution form hid it.
        private void RestoreAfterExecution(bool showInTaskbar, bool wasVisible)
        {
            ShowInTaskbar = showInTaskbar;

            if (wasVisible && !Visible)
                Show();

            Activate();
            RefreshAfterHiddenExecutionForm();
        }

        private void RefreshAfterHiddenExecutionForm()
        {
            // Form wurde während der Routinenausführung per Hide()/Show() ausgeblendet.
            // Die selbstgezeichnete, gecachte Titelleiste (TitleBarControl) bekommt dabei
            // keinen automatischen Repaint, deshalb hier das ganze Form explizit invalidieren.
            Refresh();
        }
        private void ExecuteFullRoutineAuto(Routine routine)
        {
            if (!CanExecuteRoutine(routine))
                return;

            DialogResult result = MessageBox.Show(
                Loc.S("Main.AutoRun.Text", routine.Name),
                Loc.S("Main.AutoRun.Title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                this,
                FormSize.Medium);

            if (result != DialogResult.Yes)
                return;

            AutoRunForm form = new AutoRunForm(
                routine,
                ExecuteRoutineStep,
                OnRoutineAutoRunCompleted);

            try
            {
                form.ShowDialog(this);
            }
            finally
            {
                form.Dispose();
            }
        }

        private void OnRoutineAutoRunCompleted(Routine routine)
        {
            if (routine == null)
                return;

            MarkRoutineAsExecuted(routine);

            _routinesView.LoadRoutines();
        }

        private bool CanExecuteRoutine(Routine routine)
        {
            if (routine == null || routine.Steps.Count == 0)
            {
                ShowInfoMessage(
                    Loc.S("Main.NoSteps.Text"),
                    Loc.S("Common.Info"));

                return false;
            }

            RoutineExecutionSession session = new RoutineExecutionSession(routine);

            if (!session.HasExecutableSteps)
            {
                ShowInfoMessage(
                    Loc.S("Main.AllDisabled.Text"),
                    Loc.S("Main.AllDisabled.Title"));

                return false;
            }

            return true;
        }

        private void MarkRoutineAsExecuted(Routine routine)
        {
            routine.LastExecutionAt = DateTime.Now;
            _routineService.SaveRoutine(routine);
        }

        private StepExecutionResult ExecuteRoutineStep(RoutineStep step)
        {
            return _routineService.ExecuteStep(step);
        }

        private void ShowInfoMessage(string message, string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Info,
                this,
                FormSize.Small);
        }

        private void CleanupBeforeClose()
        {
            try
            {
                SleepPreventer.AllowSleep();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Cleanup: {ex.Message}");
            }
        }

        private void OnNewRoutineClicked(object sender, Routine routine)
        {
            ShowEditorView(routine, true);
        }

        private void OnEditRoutineClicked(object sender, Routine routine)
        {
            ShowEditorView(routine, false);
        }

        private void OnDeleteRoutineClicked(object sender, Routine routine)
        {
            DeleteRoutine(routine);
        }

        private void OnStartRoutineClicked(object sender, Routine routine)
        {
            ExecuteFullRoutine(routine);
        }
        private void OnStartRoutineAutoClicked(object sender, Routine routine)
        {
            ExecuteFullRoutineAuto(routine);
        }

        private void OnBackToRoutinesClicked(object sender, EventArgs e)
        {
            ShowRoutinesView();
        }

        private void OnSaveChanges(object sender, Routine routine)
        {
            SaveRoutine(routine);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            RefreshComboBoxes(this);
        }

        private void RefreshComboBoxes(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                ComboBox comboBox = control as ComboBox;

                if (comboBox != null)
                {
                    comboBox.Invalidate();
                    comboBox.Refresh();
                }

                RefreshComboBoxes(control);
            }
        }
    }
}