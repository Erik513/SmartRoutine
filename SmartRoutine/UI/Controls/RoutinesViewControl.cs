using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
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
        public event EventHandler<Routine> NewRoutineClicked;
        public event EventHandler<Routine> EditRoutineClicked;
        public event EventHandler<Routine> DeleteRoutineClicked;
        public event EventHandler<Routine> StartRoutineClicked;

        private readonly IRoutineService _routineService;
        private Routine _selectedRoutine;

        private StyledListBoxControl lstRoutines;
        private Button btnNewRoutine, btnEditRoutine, btnDeleteRoutine, btnStartRoutine;

        private ToolTip _routineToolTip = UIStyles.ToolTips.CreateToolTip();
        private int _lastHoveredRoutineIndex = -1;

        public RoutinesViewControl(IRoutineService routineService)
        {
            _routineService = routineService;

            this.Dock = DockStyle.Fill;
            this.BackColor = UIStyles.Colors.BackgroundLight;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            InitializeControl();
            LoadRoutines();
        }

        private void InitializeControl()
        {
            // Haupt-TableLayoutPanel (zentriert, 60% der Breite)
            var mainLayout = new TableLayoutPanel
            {
                Anchor = AnchorStyles.None,
                Size = new Size((int)(this.Width * 0.85), (int)(this.Height * 0.85)),
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2
            };

            // RowStyles: ListBox (90%) + Buttons (10%)
            mainLayout.RowStyles.Clear();
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // ========== OBERE ZEILE: StyledListBoxControl ==========
            lstRoutines = new StyledListBoxControl(title: "Meine Routinen", showHeader: true, allowReorder: true, ContentAlignment.MiddleCenter)
            {
                Dock = DockStyle.Fill,
                ItemHeightCustom = 35,
                Visible = true
            };
            lstRoutines.SelectedIndexChanged += LstRoutines_SelectedIndexChanged;
            lstRoutines.ItemsReordered += LstRoutines_ItemsReordered;
            lstRoutines.MouseMove += LstRoutines_MouseMove;
            lstRoutines.MouseLeave += LstRoutines_MouseLeave;
            mainLayout.Controls.Add(lstRoutines, 0, 0);

            // ========== UNTERE ZEILE: Button Panel (4x1 Layout) ==========
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 4,
                RowCount = 1,
                AutoSize = true,
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
            btnNewRoutine.Click += (s, e) =>
            {
                // Finde die nächste verfügbare Nummer
                int nextNumber = GetNextRoutineNumber();
                string newRoutineName = $"Meine Routine {nextNumber}";

                // Neue Routine erstellen
                _routineService.CreateRoutine(newRoutineName);
                var newRoutine = _routineService.GetAllRoutines().LastOrDefault();
                if (newRoutine != null)
                {
                    newRoutine.IsNew = true;
                    NewRoutineClicked?.Invoke(s, newRoutine);
                }
            };

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
            btnStartRoutine.Click += (s, e) =>
            {
                if (_selectedRoutine != null)
                {
                    StartRoutineClicked?.Invoke(s, _selectedRoutine);
                }
            };

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

        private int GetNextRoutineNumber()
        {
            var routines = _routineService.GetAllRoutines();

            int maxNumber = routines
                .Where(r => r.Name.StartsWith("Meine Routine "))
                .Select(r =>
                {
                    string numberPart = r.Name.Substring("Meine Routine ".Length);
                    return int.TryParse(numberPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return maxNumber + 1;
        }

        private void CenterControls(TableLayoutPanel mainLayout)
        {
            int newWidth = this.Width - 160;
            int newHeight = this.Height - 160;

            mainLayout.Size = new Size(newWidth, newHeight);
            mainLayout.Location = new Point(
                (this.Width - mainLayout.Width) / 2,
                (this.Height - mainLayout.Height) / 2
            );
        }

        public void LoadRoutines()
        {
            string selectedId = _selectedRoutine?.Id;

            lstRoutines.Items.Clear();
            var routines = _routineService.GetAllRoutines();

            foreach (var routine in routines)
                lstRoutines.Items.Add(routine);

            if (!string.IsNullOrEmpty(selectedId))
                SelectRoutineById(selectedId);
            else
                lstRoutines.SelectedIndex = -1;

            _selectedRoutine = lstRoutines.SelectedItem as Routine;
            UpdateButtonStates();
        }

        public void SelectRoutine(Routine routine)
        {
            if (routine == null) return;
            SelectRoutineById(routine.Id);
        }

        private void LstRoutines_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedRoutine = lstRoutines.SelectedItem as Routine;
            UpdateButtonStates();

            if (_selectedRoutine != null)
                RoutineSelected?.Invoke(this, _selectedRoutine);
        }
        private void LstRoutines_ItemsReordered(object sender, EventArgs e)
        {
            string selectedId = _selectedRoutine?.Id;

            var newOrder = new List<Routine>();

            for (int i = 0; i < lstRoutines.Items.Count; i++)
            {
                if (lstRoutines.Items[i] is Routine routine)
                {
                    routine.Order = i;
                    newOrder.Add(routine);
                }
            }

            _routineService.ReorderRoutines(newOrder);
            LoadRoutines();

            // Falls LoadRoutines die Auswahl nicht wiederherstellen konnte
            if (!string.IsNullOrEmpty(selectedId) && lstRoutines.SelectedIndex == -1)
                SelectRoutineById(selectedId);
        }

        private void LstRoutines_MouseMove(object sender, MouseEventArgs e)
        {
            int index = lstRoutines.IndexFromPoint(e.Location);

            if (index == _lastHoveredRoutineIndex)
                return;

            _lastHoveredRoutineIndex = index;

            if (index < 0 || index >= lstRoutines.Items.Count)
            {
                _routineToolTip.SetToolTip(lstRoutines.InnerListBox, "");
                return;
            }

            if (lstRoutines.Items[index] is Routine routine)
            {
                string text = $"Zuletzt gestartet: {DateTimeHelper.GetRelativeTime(routine.LastExecutionAt)}";
                _routineToolTip.SetToolTip(lstRoutines.InnerListBox, text);
            }
        }

        private void LstRoutines_MouseLeave(object sender, EventArgs e)
        {
            _lastHoveredRoutineIndex = -1;
            _routineToolTip.SetToolTip(lstRoutines.InnerListBox, "");
        }

        private void SelectRoutineById(string routineId)
        {
            if (string.IsNullOrEmpty(routineId)) return;

            for (int i = 0; i < lstRoutines.Items.Count; i++)
            {
                if (lstRoutines.Items[i] is Routine r && r.Id == routineId)
                {
                    lstRoutines.SelectedIndex = i;
                    return;
                }
            }
        }
        private void UpdateButtonStates()
        {
            bool hasSelection = _selectedRoutine != null;
            btnEditRoutine.Enabled = hasSelection;
            btnDeleteRoutine.Enabled = hasSelection;
            btnStartRoutine.Enabled = hasSelection;
        }
    }
}