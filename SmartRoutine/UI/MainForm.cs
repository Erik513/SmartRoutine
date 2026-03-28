using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
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
            this.BackColor = UIStyles.Colors.BackgroundBlack;
            this.MinimumSize = MINIMUM_WINDOW_SIZE;
            this.Size = DEFAULT_WINDOW_SIZE;
            this.CenterToScreen();
            this.DoubleBuffered = true;
        }

        // ========== UI CREATION METHODS ==========
        private void CreateIntegratedUI()
        {
            this.Controls.Clear();

            // TitleBar
            var titleBar = new TitleBarControl("SmartRoutine");
            this.Controls.Add(titleBar);

            // Content Panel - mit Anchor statt Dock
            var contentPanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Red, // Testfarbe
                Location = new Point(0, titleBar.Height),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - titleBar.Height)
            };
            this.Controls.Add(contentPanel);

            // UserControls
            _routinesView = new RoutinesViewControl(_routineService);
            _routinesView.Dock = DockStyle.Fill;
            _routinesView.NewRoutineClicked += (s, e) => ShowEditorView(null);
            _routinesView.EditRoutineClicked += (s, routine) => ShowEditorView(routine);
            _routinesView.DeleteRoutineClicked += (s, routine) => DeleteRoutine(routine);
            _routinesView.StartRoutineClicked += (s, routine) => StartRoutine(routine);
            contentPanel.Controls.Add(_routinesView);

            _editorView = new RoutineEditorViewControl(_routineService);
            _editorView.Dock = DockStyle.Fill;
            _editorView.BackToRoutinesClicked += (s, e) => ShowRoutinesView();
            contentPanel.Controls.Add(_editorView);

            // Resize-Event für manuelle Positionierung
            this.Resize += (s, e) => UpdateContentPanelPosition(contentPanel, titleBar);
        }

        private void UpdateContentPanelPosition(Panel contentPanel, TitleBarControl titleBar)
        {
            if (contentPanel != null && titleBar != null)
            {
                contentPanel.Location = new Point(0, titleBar.Height);
                contentPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - titleBar.Height);
            }
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
            //_editorView.LoadRoutine(routine);
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
            if (this.Controls[0] is TitleBarControl titleBar)
            {
                titleBar.UpdateMaximizeButton(this.WindowState == FormWindowState.Maximized);
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
