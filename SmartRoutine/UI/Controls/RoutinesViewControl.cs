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

        private StyledListBoxWithHeader lstRoutines;
        private Button btnNewRoutine, btnEditRoutine, btnDeleteRoutine, btnStartRoutine;

        public RoutinesViewControl(IRoutineService routineService)
        {
            _routineService = routineService;

            this.Dock = DockStyle.Fill;
            this.BackColor = UIStyles.Colors.BackgroundLight;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

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
            // Haupt-TableLayoutPanel (zentriert, 60% der Breite)
            var mainLayout = new TableLayoutPanel
            {
                Anchor = AnchorStyles.None,
                Size = new Size((int)(this.Width * 0.6), (int)(this.Height * 0.6)),
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2
            };

            // RowStyles: ListBox (90%) + Buttons (10%)
            mainLayout.RowStyles.Clear();
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // ========== OBERE ZEILE: StyledListBoxWithHeader ==========
            lstRoutines = new StyledListBoxWithHeader("Meine Routinen", ContentAlignment.MiddleCenter)
            {
                Dock = DockStyle.Fill,
                ItemHeightCustom = 35, 
                Height = 200
            };
            lstRoutines.SelectedIndexChanged += LstRoutines_SelectedIndexChanged;
            mainLayout.Controls.Add(lstRoutines, 0, 0);

            // ========== UNTERE ZEILE: Button Panel (4x1 Layout) ==========
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 4,
                RowCount = 1,
                AutoSize = true,           // Automatische Größenanpassung
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // Spalten gleichmäßig verteilen
            buttonPanel.ColumnStyles.Clear();
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // Zeilen gleichmäßig verteilen
            buttonPanel.RowStyles.Clear();
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Buttons
            btnNewRoutine = UIStyles.Buttons.CreatePrimary("+ Neue Routine", "", new Size(0, 0));
            btnNewRoutine.Dock = DockStyle.Fill;
            btnNewRoutine.Margin = new Padding(5, 5, 5, 5);
            btnNewRoutine.Click += (s, e) => NewRoutineClicked?.Invoke(s, EventArgs.Empty);

            btnEditRoutine = UIStyles.Buttons.CreatePrimary("✎ Bearbeiten", "", new Size(0, 0));
            btnEditRoutine.Dock = DockStyle.Fill;
            btnEditRoutine.Margin = new Padding(5, 5, 5, 5);
            btnEditRoutine.Click += (s, e) => EditRoutineClicked?.Invoke(s, _selectedRoutine);

            btnDeleteRoutine = UIStyles.Buttons.CreateDanger("🗑 Löschen", "", new Size(0, 0));
            btnDeleteRoutine.Dock = DockStyle.Fill;
            btnDeleteRoutine.Margin = new Padding(5, 5, 5, 5);
            btnDeleteRoutine.Click += (s, e) => DeleteRoutineClicked?.Invoke(s, _selectedRoutine);

            btnStartRoutine = UIStyles.Buttons.CreateGreen("▶ Starten", "", new Size(0, 0));
            btnStartRoutine.Dock = DockStyle.Fill;
            btnStartRoutine.Margin = new Padding(5, 5, 5, 5);
            btnStartRoutine.Enabled = true;
            btnStartRoutine.Click += (s, e) => StartRoutineClicked?.Invoke(s, _selectedRoutine);


            // Buttons im 2x2 Layout platzieren
            buttonPanel.Controls.Add(btnNewRoutine, 0, 0);
            buttonPanel.Controls.Add(btnEditRoutine, 1, 0);
            buttonPanel.Controls.Add(btnDeleteRoutine, 2, 0);
            buttonPanel.Controls.Add(btnStartRoutine, 3, 0);

            mainLayout.Controls.Add(buttonPanel, 0, 1);

            // Zentrieren des Haupt-Layouts
            this.Controls.Add(mainLayout);

            // Resize-Event für Zentrierung
            this.Resize += (s, e) => CenterControls(mainLayout);
        }

        private void CenterControls(TableLayoutPanel mainLayout)
        {
            int newWidth = (int)(this.Width * 0.6);
            int newHeight = (int)(this.Height * 0.6);

            mainLayout.Size = new Size(newWidth, newHeight);
            mainLayout.Location = new Point(
                (this.Width - mainLayout.Width) / 2,
                (this.Height - mainLayout.Height) / 2
            );
        }

        public void LoadRoutines()
        {
            lstRoutines.Items.Clear();

            // Service aktivieren (JSON-Speicherung)
            var routines = _routineService.GetAllRoutines();
            foreach (var routine in routines)
            {
                lstRoutines.Items.Add(routine);
            }

            // Testdaten verwenden
            //foreach (var routine in _testRoutines)
            //{
            //    lstRoutines.Items.Add(routine);
            //}

            lstRoutines.Visible = true;

            _selectedRoutine = null;
            lstRoutines.SelectedIndex = -1;

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