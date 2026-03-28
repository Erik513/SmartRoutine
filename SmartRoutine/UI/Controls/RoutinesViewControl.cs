using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
        private List<Routine> _testRoutines;

        //private ListBox lstRoutines;
        private StyledListBox lstRoutines;
        private Button btnNewRoutine, btnEditRoutine, btnDeleteRoutine, btnStartRoutine;
        private Label lblNoRoutines;

        public RoutinesViewControl(IRoutineService routineService)
        {
            _routineService = routineService;

            // Wichtig: Diese Einstellungen müssen hier sein
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);
            this.BackColor = Color.Black;

            CreateTestData();
            InitializeControl();
            LoadRoutines();

            lstRoutines.ItemsReordered += LstRoutines_ItemsReordered;
        }
        private void CreateTestData()
        {
            _testRoutines = new List<Routine>();

            // Routine 1: Morgens
            var morningRoutine = new Routine
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Morgenroutine",
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 0, Type = StepType.OpenUrl, Value = "https://www.wetter.de", Description = "Wetter checken" },
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 1, Type = StepType.OpenUrl, Value = "https://www.spiegel.de", Description = "Nachrichten lesen" },
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 2, Type = StepType.OpenUrl, Value = "https://mail.google.com", Description = "E-Mails prüfen" }
                }
            };
            _testRoutines.Add(morningRoutine);

            // Routine 2: Arbeit
            var workRoutine = new Routine
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Arbeitsstart",
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 0, Type = StepType.OpenUrl, Value = "https://trello.com", Description = "Trello öffnen" },
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 1, Type = StepType.OpenUrl, Value = "https://github.com", Description = "GitHub öffnen" },
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 2, Type = StepType.OpenUrl, Value = "https://slack.com", Description = "Slack öffnen" }
                }
            };
            _testRoutines.Add(workRoutine);

            // Routine 3: Feierabend
            var eveningRoutine = new Routine
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Feierabend",
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 0, Type = StepType.OpenUrl, Value = "https://www.netflix.com", Description = "Netflix öffnen" },
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 1, Type = StepType.OpenUrl, Value = "https://www.spotify.com", Description = "Spotify öffnen" },
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 2, Type = StepType.OpenUrl, Value = "https://www.youtube.com", Description = "YouTube öffnen" }
                }
            };
            _testRoutines.Add(eveningRoutine);

            // Routine 4: Wochenende
            var weekendRoutine = new Routine
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Wochenende",
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 0, Type = StepType.OpenUrl, Value = "https://www.eventim.de", Description = "Events checken" },
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 1, Type = StepType.OpenUrl, Value = "https://www.booking.com", Description = "Reise planen" }
                }
            };
            _testRoutines.Add(weekendRoutine);

            // Routine 5: Entwickeln
            var devRoutine = new Routine
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Entwicklungsstart",
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 0, Type = StepType.OpenUrl, Value = "https://stackoverflow.com", Description = "Stack Overflow" },
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 1, Type = StepType.OpenUrl, Value = "https://docs.microsoft.com", Description = "Microsoft Docs" },
                    new RoutineStep { Id = Guid.NewGuid().ToString(), Order = 2, Type = StepType.OpenUrl, Value = "https://github.com", Description = "GitHub" }
                }
            };
            _testRoutines.Add(devRoutine);
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

            lstRoutines = new StyledListBox
            {
                Location = new Point(0, 50),
                Size = new Size(400, 300),
                ItemHeightCustom = 35
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

            // Service aktivieren (JSON-Speicherung)
            //var routines = _routineService.GetAllRoutines();
            //foreach (var routine in routines)
            //{
            //    lstRoutines.Items.Add(routine);
            //}

            // Testdaten verwenden
            foreach (var routine in _testRoutines)
            {
                lstRoutines.Items.Add(routine);
            }

            bool hasRoutines = lstRoutines.Items.Count > 0;
            lstRoutines.Visible = hasRoutines;
            lblNoRoutines.Visible = !hasRoutines;

            // SelectedIndexChanged manuell aufrufen, um Buttons zu aktualisieren
            LstRoutines_SelectedIndexChanged(this, EventArgs.Empty);
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

            bool hasSelection = _selectedRoutine != null;
            btnEditRoutine.Enabled = hasSelection;
            btnDeleteRoutine.Enabled = hasSelection;
            btnStartRoutine.Enabled = hasSelection;

            if (hasSelection)
            {
                RoutineSelected?.Invoke(this, _selectedRoutine);
            }
        }
        private void LstRoutines_ItemsReordered(object sender, EventArgs e)
        {
            // Hier die neue Reihenfolge speichern
            // z.B. die _testRoutines Liste aktualisieren oder Service aufrufen

            // Beispiel: _testRoutines Liste aktualisieren
            var newOrder = new List<Routine>();
            foreach (var item in lstRoutines.Items)
            {
                if (item is Routine routine)
                {
                    newOrder.Add(routine);
                }
            }
            _testRoutines = newOrder;

            // Optional: Orders aktualisieren
            for (int i = 0; i < _testRoutines.Count; i++)
            {
                foreach (var step in _testRoutines[i].Steps)
                {
                    step.Order = step.Order; // Behält die Schritt-Reihenfolge
                }
            }
        }
    }
}