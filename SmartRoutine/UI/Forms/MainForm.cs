using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Controls;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    public partial class MainForm : SmartRoutineForm
    {

        // ========== FIELDS ==========
        private readonly IRoutineService _routineService;
        private RoutinesViewControl _routinesView;
        private RoutineEditorViewControl _editorView;
        private Routine _currentRoutine;

        // Constants
        private static readonly Size DEFAULT_WINDOW_SIZE = new Size(1024, 768);
        private static readonly Size MINIMUM_WINDOW_SIZE = new Size(800, 600);

        // ========== CONSTRUCTOR ==========
        public MainForm(IRoutineService routineService)
            : base(
                icon: Properties.Resources.IconLogo,
                title: "SmartRoutine",
                showMinimize: true,
                showMaximize: true,
                showClose: true, 
                allowWindowSnapAndMaximize: true,
                titleBarBackColor: default)
        {
            _routineService = routineService
                ?? throw new ArgumentNullException(nameof(routineService));

            ConfigureForm();
            CreateIntegratedUI();
            ShowRoutinesView();
        }
        // ========== CONFIGURATION ==========
        private void ConfigureForm()
        {
            this.BackColor = UIStyles.Colors.BackgroundDark;
            this.MinimumSize = MINIMUM_WINDOW_SIZE;
            this.Size = DEFAULT_WINDOW_SIZE;
            this.CenterToScreen();
        }

        // ========== UI CREATION METHODS ==========
        private void CreateIntegratedUI()
        {
            ContentPanel.Controls.Clear();
            ContentPanel.Padding = new Padding(5);

            _routinesView = new RoutinesViewControl(_routineService);
            _routinesView.Dock = DockStyle.Fill;
            _routinesView.NewRoutineClicked += (s, routine) => ShowEditorView(routine, isNewRoutine: true);
            _routinesView.EditRoutineClicked += (s, routine) => ShowEditorView(routine, isNewRoutine: false);
            _routinesView.DeleteRoutineClicked += (s, routine) => DeleteRoutine(routine);
            _routinesView.StartRoutineClicked += (s, routine) =>
            {
                ExecuteFullRoutine(routine);
            };

            var urlValidationService = new UrlValidationService();

            _editorView = new RoutineEditorViewControl(_routineService, urlValidationService);
            _editorView.Dock = DockStyle.Fill;
            _editorView.BackToRoutinesClicked += (s, e) => ShowRoutinesView();
            _editorView.SaveChanges += (s, routine) => SaveRoutine(routine);

            ContentPanel.Controls.Add(_routinesView);
            ContentPanel.Controls.Add(_editorView);
        }

        private void ExecuteFullRoutine(Routine routine)
        {
            if (routine == null || routine.Steps.Count == 0)
            {
                CustomMessageBox.Show(
                    "Diese Routine enthält keine Schritte.",
                    "Info",
                    CustomMessageBoxButtons.OK,
                    CustomMessageBoxIcon.Info,
                    FindForm());
                return;
            }
            var session = new RoutineExecutionSession(routine);

            if (!session.HasExecutableSteps)
            {
                CustomMessageBox.Show(
                    "Alle Schritte dieser Routine sind derzeit deaktiviert.",
                    "Routine nicht ausführbar",
                    CustomMessageBoxButtons.OK,
                    CustomMessageBoxIcon.Info,
                    FindForm());

                return;
            }

            routine.LastExecutionAt = DateTime.Now;
            _routineService.SaveRoutine(routine);

            var executionForm = new ExecutionForm(routine, step =>
            {
                return _routineService.ExecuteStep(step);
            });
            executionForm.ShowDialog(this);
        }

        private void SaveRoutine(Routine routine)
        {
            if (routine == null) return;

            _routineService.SaveRoutine(routine);

            // Frische Version mit garantiert korrekten Orders holen
            if (_currentRoutine != null && _currentRoutine.Id == routine.Id)
            {
                _currentRoutine = _routineService.GetRoutine(routine.Id);
            }

            _routinesView.LoadRoutines();
        }

        // =======================================================================================================================================

        private void ShowRoutinesView()
        {
            _routinesView.Show();
            _editorView.Hide();
            _routinesView.LoadRoutines();
        }

        private void ShowEditorView(Routine routine, bool isNewRoutine = false)
        {
            _currentRoutine = routine;
            _routinesView.Hide();
            _editorView.Show();
            _editorView.LoadRoutine(routine, isNewRoutine);
        }

        private void DeleteRoutine(Routine routine)
        {
            if (routine == null) return;

            if (CustomMessageBox.Show(
                    $"Routine '{routine.Name}' wirklich löschen?",
                    "Bestätigen",
                    CustomMessageBoxButtons.YesNo,
                    CustomMessageBoxIcon.Warning,
                    FindForm()) == DialogResult.Yes)
            {
                _routineService.DeleteRoutine(routine.Id);
                _routinesView.LoadRoutines();
            }
        }


        // ========== EVENT HANDLER ==========
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                SleepPreventer.AllowSleep();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Cleanup: {ex.Message}");
            }
            finally
            {
                base.OnFormClosing(e);
            }
        }
    }

}
