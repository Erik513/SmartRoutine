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

namespace SmartRoutine.UI
{
    public partial class MainForm : BorderlessResizableForm
    {

        // ========== FIELDS ==========
        private readonly IRoutineService _routineService;
        private RoutinesViewControl _routinesView;
        private RoutineEditorViewControl _editorView;
        private Routine _currentRoutine;
        private const bool DEVELOPER_MODE = true;

        // Constants
        private static readonly Size DEFAULT_WINDOW_SIZE = new Size(1024, 768);
        private static readonly Size MINIMUM_WINDOW_SIZE = new Size(800, 600);

        // ========== CONSTRUCTOR ==========
        public MainForm(IRoutineService routineService)
        {
            _routineService = routineService ?? throw new ArgumentNullException(nameof(routineService));

            ConfigureForm();
            CreateIntegratedUI();
            ShowRoutinesView();

            this.Resize += MainForm_Resize;
            this.Load += MainForm_Load;
        }
        // ========== CONFIGURATION ==========
        private void ConfigureForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = UIStyles.Colors.BackgroundDark;
            this.MinimumSize = MINIMUM_WINDOW_SIZE;
            this.Size = DEFAULT_WINDOW_SIZE;
            this.CenterToScreen();
        }

        // ========== UI CREATION METHODS ==========
        private void CreateIntegratedUI()
        {
            this.Controls.Clear();

            // TitleBar
            var titleBar = new TitleBarControl("SmartRoutine");
            this.Controls.Add(titleBar);

            // Content Panel
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5, 5 + titleBar.Height, 5, 5)
            };
            this.Controls.Add(contentPanel);

            // UserControls
            var testDataService = new TestDataService();
            _routinesView = new RoutinesViewControl(_routineService, testDataService);
            _routinesView.NewRoutineClicked += (s, e) => ShowEditorView(null);
            _routinesView.EditRoutineClicked += (s, routine) => ShowEditorView(routine);
            _routinesView.DeleteRoutineClicked += (s, routine) => DeleteRoutine(routine);
            _routinesView.StartRoutineClicked += (s, routine) => StartRoutine(routine);
            contentPanel.Controls.Add(_routinesView);

            var urlValidationService = new UrlValidationService();
            _editorView = new RoutineEditorViewControl(_routineService, urlValidationService);
            _editorView.BackToRoutinesClicked += (s, e) => ShowRoutinesView();
            _editorView.SaveChanges += (s, routine) => SaveRoutine(routine);
            contentPanel.Controls.Add(_editorView);
        }

        private void SaveRoutine(Routine routine)
        {
            if (routine == null) return;

            var existing = _routineService.GetRoutine(routine.Id);
            if (existing == null)
            {
                _routineService.CreateRoutine(routine.Name);
                var newRoutine = _routineService.GetAllRoutines().LastOrDefault();
                if (newRoutine != null)
                {
                    foreach (var step in routine.Steps)
                    {
                        _routineService.AddStep(newRoutine.Id, step.Type, step.Value, step.Description, step.UserDescription);
                    }
                }
            }
            else
            {
                existing.Name = routine.Name;
                existing.Steps = routine.Steps;
                existing.UpdatedAt = DateTime.Now;
                _routineService.UpdateRoutine(existing);
            }

            _routinesView.LoadRoutines();
        }

        // =======================================================================================================================================

        private void ShowRoutinesView()
        {
            _routinesView.Visible = true;
            _editorView.Visible = false;
            _routinesView.LoadRoutines();
        }

        private void ShowEditorView(Routine routine)
        {
            _currentRoutine = routine;
            _routinesView.Visible = false;
            _editorView.Visible = true;
            _editorView.LoadRoutine(routine);
        }

        private void DeleteRoutine(Routine routine)
        {
            if (routine == null) return;

            if (MessageBox.Show($"Routine '{routine.Name}' wirklich löschen?", "Bestätigen",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _routineService.DeleteRoutine(routine.Id);
                _routinesView.LoadRoutines();
            }
        }

        private void StartRoutine(Routine routine)
        {
            MessageBox.Show("Start-Funktion wird später implementiert.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        // ========== EVENT HANDLER ==========
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Keine Testdaten mehr nötig, da sie direkt in RoutinesViewControl erstellt werden
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden: {ex.Message}", "Initialisierungsfehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            this.SuspendLayout();
            try
            {
                if (this.Controls[0] is TitleBarControl titleBar)
                {
                    titleBar.UpdateMaximizeButton(this.WindowState == FormWindowState.Maximized);
                }
            }
            finally
            {
                this.ResumeLayout(false);
                this.PerformLayout();
            }
        }

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
