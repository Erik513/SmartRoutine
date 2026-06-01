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

namespace SmartRoutine.UI.Forms
{
    public partial class MainForm : StyledForm
    {
        private static readonly Size DefaultWindowSize = new Size(1024, 768);
        private static readonly Size MinimumWindowSize = new Size(800, 600);

        private readonly IRoutineService _routineService;

        private RoutinesViewControl _routinesView;
        private RoutineEditorViewControl _editorView;
        private Routine _currentRoutine;

        public MainForm(IRoutineService routineService)
            : base(StyledFormOptions.CreateStandard(
                "SmartRoutine",
                ContentAlignment.MiddleLeft,
                UIColors.BackgroundBlack,
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

            _routinesView = new RoutinesViewControl(_routineService)
            {
                Dock = DockStyle.Fill
            };

            _editorView = new RoutineEditorViewControl(_routineService, urlValidationService)
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

            _editorView.BackToRoutinesClicked += OnBackToRoutinesClicked;
            _editorView.SaveChanges += OnSaveChanges;
        }

        private void AddViewsToContentPanel()
        {
            ContentPanel.Controls.Clear();
            ContentPanel.Padding = new Padding(5);

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

            DialogResult result = CustomWFUI.Forms.CustomMessageBox.Show(
                $"Routine '{routine.Name}' wirklich löschen?",
                "Bestätigen",
                CustomWFUI.Forms.CustomMessageBoxButtons.YesNo,
                CustomWFUI.Forms.CustomMessageBoxIcon.Warning,
                this);

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

            ExecutionForm executionForm = new ExecutionForm(
                routine,
                ExecuteRoutineStep);

            try
            {
                executionForm.ShowDialog(this);
            }
            finally
            {
                executionForm.Dispose();
            }
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
            CustomWFUI.Forms.CustomMessageBox.Show(
                message,
                title,
                CustomWFUI.Forms.CustomMessageBoxButtons.OK,
                CustomWFUI.Forms.CustomMessageBoxIcon.Info,
                this);
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

        private void OnBackToRoutinesClicked(object sender, EventArgs e)
        {
            ShowRoutinesView();
        }

        private void OnSaveChanges(object sender, Routine routine)
        {
            SaveRoutine(routine);
        }
    }
}