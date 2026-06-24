using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;
using CustomWFUI;
using CustomWFUI.Forms;
using CustomWFUI.Styles;
using CustomWFUI.Helpers;

namespace SmartRoutine.UI.Forms
{
    public partial class MainForm : StyledForm
    {
        private static readonly Size DefaultWindowSize = new Size(1024, 768);
        private static readonly Size MinimumWindowSize = new Size(800, 600);

        private readonly IRoutineService _routineService;

        private RoutinesViewUC _routinesView;
        private RoutineEditorUC _editorView;
        private Routine _currentRoutine;

        public MainForm(IRoutineService routineService)
            : base(StyledFormOptions.CreateStandard(
                "SmartRoutine",
                ContentAlignment.MiddleLeft,
                UIColors.BackgroundDarkElevated,
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
            base.OnFormClosing(e);
        }

        private void ConfigureForm()
        {
            BackColor = UIStyles.Colors.BackgroundDark;
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
            _currentRoutine = routine;

            _routinesView.Hide();
            _editorView.Show();
            _editorView.LoadRoutine(routine, isNewRoutine);
        }

        private void SaveRoutine(Routine routine)
        {
            if (routine == null)
                return;

            _routineService.SaveRoutine(routine);
            RefreshCurrentRoutineIfNeeded(routine);
            _routinesView.LoadRoutines();
        }

        private void RefreshCurrentRoutineIfNeeded(Routine routine)
        {
            if (_currentRoutine == null)
                return;

            if (_currentRoutine.Id != routine.Id)
                return;

            _currentRoutine = _routineService.GetRoutine(routine.Id);
        }

        private void DeleteRoutine(Routine routine)
        {
            if (routine == null)
                return;

            DialogResult result = CustomMessageBox.Show(
                $"Routine '{routine.Name}' wirklich löschen?",
                "Bestätigen",
                CustomMessageBoxButtons.YesNo,
                CustomMessageBoxIcon.Warning,
                this,
                CustomMessageBoxSize.Small);

            if (result != DialogResult.Yes)
                return;

            _routineService.DeleteRoutine(routine.Id);
            _routinesView.LoadRoutines();
        }

        private void ExecuteFullRoutine(Routine routine)
        {
            if (!CanExecuteRoutine(routine))
                return;

            MarkRoutineAsExecuted(routine);

            FormWindowState previousWindowState = WindowState;

            ExecutionForm form = new ExecutionForm(routine, ExecuteRoutineStep);

            try
            {
                WindowState = FormWindowState.Minimized;
                form.ShowDialog(this);
            }
            finally
            {
                form.Dispose();

                WindowState = previousWindowState;

                Show();
                Activate();
                _routinesView.Refresh();
            }
        }
        private void ExecuteFullRoutineAuto(Routine routine)
        {
            if (!CanExecuteRoutine(routine))
                return;

            DialogResult result = CustomMessageBox.Show(
                $"Möchten Sie die Routine '{routine.Name}' automatisch ausführen?\n\n" +
                "Alle aktivierten Schritte werden nacheinander gestartet.\n" +
                "Interne URL-Schritte werden dabei einmalig im externen Browser geöffnet.",
                "Automatische Ausführung starten",
                CustomMessageBoxButtons.YesNo,
                CustomMessageBoxIcon.Question,
                this,
                CustomMessageBoxSize.Medium);

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

            routine.LastExecutionAt = DateTime.Now;

            _routineService.SaveRoutine(routine);

            _routinesView.LoadRoutines();
        }

        private bool CanExecuteRoutine(Routine routine)
        {
            if (routine == null || routine.Steps.Count == 0)
            {
                ShowInfoMessage(
                    "Diese Routine enthält keine Schritte.",
                    "Info");

                return false;
            }

            RoutineExecutionSession session = new RoutineExecutionSession(routine);

            if (!session.HasExecutableSteps)
            {
                ShowInfoMessage(
                    "Alle Schritte dieser Routine sind derzeit deaktiviert.",
                    "Routine nicht ausführbar");

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
            CustomMessageBox.Show(
                message,
                title,
                CustomMessageBoxButtons.OK,
                CustomMessageBoxIcon.Info,
                this,
                CustomMessageBoxSize.Small);
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