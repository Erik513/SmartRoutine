using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Helpers;

namespace SmartRoutine.UI.Controls
{
    public partial class RoutinesViewControl : UserControl
    {
        public event EventHandler<Routine> RoutineSelected;
        public event EventHandler NewRoutineClicked;
        public event EventHandler<Routine> EditRoutineClicked;
        public event EventHandler<Routine> DeleteRoutineClicked;
        public event EventHandler<Routine> StartRoutineClicked;

        private readonly IRoutineService _routineService;
        private Routine _selectedRoutine;

        private ListBox lstRoutines;
        private Button btnNewRoutine, btnEditRoutine, btnDeleteRoutine, btnStartRoutine;
        private Label lblNoRoutines;

        public RoutinesViewControl(IRoutineService routineService)
        {
            _routineService = routineService;

            // Wichtig: Diese Einstellungen müssen hier sein
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);
            this.BackColor = Color.Black;

            InitializeControl();
            LoadRoutines();
        }

        private void InitializeControl()
        {
            // Inneres Panel für den Inhalt (weißer/heller Hintergrund)
            var innerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundMediumElevated
            };

            this.Controls.Add(innerPanel);

            // Zentrierter Container
            var centerContainer = new Panel
            {
                Anchor = AnchorStyles.None,
                Size = new Size(400, 500)
            };

            var lblHeader = UIStyles.Labels.CreateTitle("Meine Routinen");
            lblHeader.Location = new Point(0, 0);
            lblHeader.Size = new Size(400, 40);
            lblHeader.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblHeader.TextAlign = ContentAlignment.MiddleCenter;

            lstRoutines = new ListBox
            {
                Location = new Point(0, 50),
                Size = new Size(400, 300),
                BackColor = UIStyles.Colors.BackgroundMedium,
                ForeColor = UIStyles.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UIStyles.Fonts.Normal
            };
            lstRoutines.SelectedIndexChanged += LstRoutines_SelectedIndexChanged;

            lblNoRoutines = UIStyles.Labels.CreateMuted("Keine Routinen vorhanden.\nKlicke auf 'Neue Routine' um eine zu erstellen.");
            lblNoRoutines.Location = new Point(0, 50);
            lblNoRoutines.Size = new Size(400, 80);
            lblNoRoutines.TextAlign = ContentAlignment.MiddleCenter;
            lblNoRoutines.Visible = false;

            btnNewRoutine = UIStyles.Buttons.CreatePrimary("+ Neue Routine", "", new Size(190, 40));
            btnNewRoutine.Location = new Point(0, 360);
            btnNewRoutine.Click += (s, e) => NewRoutineClicked?.Invoke(s, EventArgs.Empty);

            btnEditRoutine = UIStyles.Buttons.CreateStandard("✎ Bearbeiten", "", new Size(190, 40));
            btnEditRoutine.Location = new Point(210, 360);
            btnEditRoutine.Click += (s, e) => EditRoutineClicked?.Invoke(s, _selectedRoutine);

            btnDeleteRoutine = UIStyles.Buttons.CreateStandard("🗑 Löschen", "", new Size(190, 40));
            btnDeleteRoutine.Location = new Point(0, 410);
            btnDeleteRoutine.Click += (s, e) => DeleteRoutineClicked?.Invoke(s, _selectedRoutine);

            btnStartRoutine = UIStyles.Buttons.CreatePrimary("▶ Starten", "", new Size(190, 40));
            btnStartRoutine.Location = new Point(210, 410);
            btnStartRoutine.Enabled = false;
            btnStartRoutine.Click += (s, e) => StartRoutineClicked?.Invoke(s, _selectedRoutine);

            centerContainer.Controls.AddRange(new Control[] {
                lblHeader, lstRoutines, lblNoRoutines,
                btnNewRoutine, btnEditRoutine, btnDeleteRoutine, btnStartRoutine
            });

            innerPanel.Controls.Add(centerContainer);
            this.Resize += (s, e) => CenterContainer(centerContainer);
        }

        private void CenterContainer(Panel container)
        {
            container.Location = new Point(
                (this.ClientSize.Width - container.Width) / 2,
                (this.ClientSize.Height - container.Height) / 2
            );
        }

        public void LoadRoutines()
        {
            lstRoutines.Items.Clear();
            var routines = _routineService.GetAllRoutines();

            foreach (var routine in routines)
            {
                lstRoutines.Items.Add(routine);
            }

            bool hasRoutines = lstRoutines.Items.Count > 0;
            lstRoutines.Visible = hasRoutines;
            lblNoRoutines.Visible = !hasRoutines;
            btnEditRoutine.Enabled = hasRoutines;
            btnDeleteRoutine.Enabled = hasRoutines;
            btnStartRoutine.Enabled = hasRoutines;
        }

        public void SelectRoutine(Routine routine)
        {
            if (routine == null) return;

            for (int i = 0; i < lstRoutines.Items.Count; i++)
            {
                var r = lstRoutines.Items[i] as Routine;
                if (r != null && r.Id == routine.Id)
                {
                    lstRoutines.SelectedIndex = i;
                    break;
                }
            }
        }

        private void LstRoutines_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedRoutine = lstRoutines.SelectedItem as Routine;
            btnEditRoutine.Enabled = _selectedRoutine != null;
            btnDeleteRoutine.Enabled = _selectedRoutine != null;
            btnStartRoutine.Enabled = _selectedRoutine != null;

            if (_selectedRoutine != null)
            {
                RoutineSelected?.Invoke(this, _selectedRoutine);
            }
        }
    }
}