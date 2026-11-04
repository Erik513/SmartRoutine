using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using ErikwnkWFUI.Forms;
using ErikwnkWFUI.Styles;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TableLayoutPanel = System.Windows.Forms.TableLayoutPanel;

namespace SmartRoutine.UI.Controls
{
    public partial class RoutinesViewUC : UserControl
    {
        public event EventHandler<Routine> RoutineSelected;
        public event EventHandler<Routine> NewRoutineClicked;
        public event EventHandler<Routine> EditRoutineClicked;
        public event EventHandler<Routine> DeleteRoutineClicked;
        public event EventHandler<Routine> StartRoutineClicked;
        public event EventHandler<Routine> StartRoutineAutoClicked;

        private readonly IRoutineService _routineService;
        private Routine _selectedRoutine;

        private TableLayoutPanel mainLayout;
        private TableLayoutPanel buttonPanel;

        private TitledListBox lstRoutines;
        private Button btnNewRoutine, btnEditRoutine, btnDeleteRoutine, btnStartRoutine, btnStartRoutineAuto;

        private InfoPopupForm _routineInfoPopup = UIStyles.InfoPopups.CreateStandard();
        private int _lastHoveredRoutineIndex = -1;

        private const string DefaultRoutineNamePrefix = "Meine Routine ";

        public RoutinesViewUC()
        {
            DoubleBuffered = true;

            // The popup also shows while another program is in front.
            _routineInfoPopup.ShowAlways = true;
        }
        public RoutinesViewUC(IRoutineService routineService) : this()
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
            InitializeMainLayout();
            InitializeRoutineList();
            InitializeButtonPanel();
            BuildLayout();
        }

        private void InitializeMainLayout()
        {
            mainLayout = UIStyles.TableLayoutPanels.CreateStandard(1, 2);

            mainLayout.Anchor = AnchorStyles.None;
            mainLayout.BackColor = Color.Transparent;
            mainLayout.Size = new Size(
                (int)(Width * 0.85),
                (int)(Height * 0.85));

            mainLayout.RowStyles.Clear();
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            mainLayout.ColumnStyles.Clear();
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        }

        private void InitializeRoutineList()
        {
            lstRoutines = UIStyles.TitledListBoxes.CreateStandard(
                headerTitle: "Meine Routinen",
                displayMember: "Name",
                allowReorder: true,
                showEnumeration: false,
                headerTextAlign: ContentAlignment.MiddleCenter);
            lstRoutines.Dock = DockStyle.Fill;

            lstRoutines.SelectedIndexChanged += LstRoutines_SelectedIndexChanged;
            lstRoutines.ItemsReordered += LstRoutines_ItemsReordered;
            lstRoutines.MouseMove += LstRoutines_MouseMove;
            lstRoutines.MouseLeave += LstRoutines_MouseLeave;
            lstRoutines.MouseDoubleClick += LstRoutines_MouseDoubleClick;
        }
        private void InitializeButtonPanel()
        {
            buttonPanel = UIStyles.TableLayoutPanels.CreateDark(6, 1);
            buttonPanel.BackColor = UIStyles.Colors.BackgroundDarkElevated;
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.Padding = new Padding(0);

            buttonPanel.ColumnStyles.Clear();
            buttonPanel.RowStyles.Clear();

            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));

            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            btnNewRoutine = CreateRoutineButton(UIStyles.Buttons.Presets.Add, "Neue Routine erstellen", new Padding(5), BtnNewRoutine_Click);
            btnEditRoutine = CreateRoutineButton(UIStyles.Buttons.Presets.Edit, "Routine bearbeiten", new Padding(5), BtnEditRoutine_Click);
            btnDeleteRoutine = CreateRoutineButton(UIStyles.Buttons.Presets.Delete, "Routine löschen", new Padding(5), BtnDeleteRoutine_Click);
            btnStartRoutine = CreateRoutineButton(UIStyles.Buttons.Presets.Play, "Routine starten", new Padding(5, 5, 0, 5), BtnStartRoutine_Click);
            btnStartRoutineAuto = CreateRoutineButton(UIStyles.Buttons.Presets.Lightning, "Routine mit Auto-Ausführung starten", new Padding(0, 5, 5, 5), BtnStartRoutineAuto_Click);

            buttonPanel.Controls.Add(btnNewRoutine, 0, 0);
            buttonPanel.Controls.Add(btnEditRoutine, 1, 0);
            buttonPanel.Controls.Add(btnDeleteRoutine, 2, 0);
            buttonPanel.Controls.Add(btnStartRoutine, 4, 0);
            buttonPanel.Controls.Add(btnStartRoutineAuto, 5, 0);
        }

        private static Button CreateRoutineButton(UIButtonPreset preset, string tooltip, Padding margin, EventHandler onClick)
        {
            Button button = UIStyles.Buttons.CreateSymbol(preset, tooltip);
            button.Dock = DockStyle.Fill;
            button.Margin = margin;
            button.Click += onClick;

            return button;
        }

        private void BtnNewRoutine_Click(object sender, EventArgs e)
        {
            int nextNumber = GetNextRoutineNumber(_routineService.GetAllRoutines());
            string newRoutineName = DefaultRoutineNamePrefix + nextNumber;

            var newRoutine = _routineService.CreateRoutine(newRoutineName);

            LoadRoutines();
            SelectRoutine(newRoutine);

            NewRoutineClicked?.Invoke(this, newRoutine);
        }

        private void BtnEditRoutine_Click(object sender, EventArgs e)
        {
            if (_selectedRoutine == null)
                return;

            EditRoutineClicked?.Invoke(this, _selectedRoutine);
        }

        // A double click on a routine opens it in the editor, like the edit button. Only a click on
        // an item counts (not on the empty room below the items); the first click has selected it.
        private void LstRoutines_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            int index = lstRoutines.IndexFromPoint(e.Location);

            if (index < 0 || index >= lstRoutines.Items.Count)
                return;

            lstRoutines.SelectedIndex = index;
            BtnEditRoutine_Click(sender, e);
        }

        private void BtnDeleteRoutine_Click(object sender, EventArgs e)
        {
            if (_selectedRoutine == null)
                return;

            DeleteRoutineClicked?.Invoke(this, _selectedRoutine);
        }

        private void BtnStartRoutine_Click(object sender, EventArgs e)
        {
            StartSelectedRoutine(StartRoutineClicked);
        }

        private void BtnStartRoutineAuto_Click(object sender, EventArgs e)
        {
            StartSelectedRoutine(StartRoutineAutoClicked);
        }

        // Clears the selection and raises the given start event for the routine that was selected.
        private void StartSelectedRoutine(EventHandler<Routine> startEvent)
        {
            if (_selectedRoutine == null)
                return;

            Routine routineToStart = _selectedRoutine;

            ClearRoutineSelection();

            startEvent?.Invoke(this, routineToStart);
        }

        private void ClearRoutineSelection()
        {
            lstRoutines.ClearSelected();
            _selectedRoutine = null;

            btnEditRoutine.Enabled = false;
            btnDeleteRoutine.Enabled = false;
            btnStartRoutine.Enabled = false;
            btnStartRoutineAuto.Enabled = false;

            Refresh();
        }

        private void BuildLayout()
        {
            mainLayout.Controls.Add(lstRoutines, 0, 0);
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            Controls.Add(mainLayout);
            Controls.Add(CreateBottomBar());

            Resize += (s, e) => CenterControls(mainLayout);

            CenterControls(mainLayout);
        }


        // The number for the next default name: one more than the highest "Meine Routine <n>" in use.
        private static int GetNextRoutineNumber(IEnumerable<Routine> routines)
        {
            int maxNumber = routines
                .Where(r => r.Name != null && r.Name.StartsWith(DefaultRoutineNamePrefix))
                .Select(r =>
                {
                    string numberPart = r.Name.Substring(DefaultRoutineNamePrefix.Length);
                    return int.TryParse(numberPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return maxNumber + 1;
        }

        private void CenterControls(TableLayoutPanel layout)
        {
            int newWidth = Math.Max(300, Width - 160);
            int newHeight = Math.Max(300, Height - BottomBarHeight - 160);

            layout.Size = new Size(newWidth, newHeight);

            layout.Location = new Point(
                (Width - layout.Width) / 2,
                (Height - BottomBarHeight - layout.Height) / 2);
        }

        public void LoadRoutines()
        {
            string selectedId = _selectedRoutine?.Id;

            ReloadRoutineItems();

            RestoreSelection(selectedId);

            _selectedRoutine = lstRoutines.SelectedItem as Routine;

            UpdateButtonStates();
        }
        private void ReloadRoutineItems()
        {
            lstRoutines.Items.Clear();

            var routines = _routineService.GetAllRoutines();

            foreach (var routine in routines)
            {
                lstRoutines.Items.Add(routine);
            }
        }
        private void RestoreSelection(string selectedId)
        {
            if (!string.IsNullOrEmpty(selectedId))
            {
                SelectRoutineById(selectedId);
                return;
            }

            lstRoutines.SelectedIndex = -1;
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

            var reorderedRoutines = GetReorderedRoutinesFromList();

            _routineService.ReorderRoutines(reorderedRoutines);

            LoadRoutines();

            RestoreSelectionAfterReorder(selectedId);
        }

        private List<Routine> GetReorderedRoutinesFromList()
        {
            var reorderedRoutines = new List<Routine>();

            for (int i = 0; i < lstRoutines.Items.Count; i++)
            {
                if (lstRoutines.Items[i] is Routine routine)
                {
                    routine.Order = i;
                    reorderedRoutines.Add(routine);
                }
            }

            return reorderedRoutines;
        }
        private void RestoreSelectionAfterReorder(string selectedId)
        {
            if (string.IsNullOrEmpty(selectedId))
                return;

            if (lstRoutines.SelectedIndex != -1)
                return;

            SelectRoutineById(selectedId);
        }

        private void HideRoutineInfoPopup()
        {
            if (_routineInfoPopup == null)
                return;

            _routineInfoPopup.CancelPendingShow();
            _routineInfoPopup.Hide();
        }

        private static string BuildStepListText(List<RoutineStep> steps)
        {
            if (steps == null || steps.Count == 0)
                return "Keine";

            return string.Join(
                Environment.NewLine,
                steps.Select(s => $"[{s.Order + 1}] {s.Name}"));
        }

        private void LstRoutines_MouseMove(object sender, MouseEventArgs e)
        {
            int index = lstRoutines.IndexFromPoint(e.Location);

            if (index < 0 || index >= lstRoutines.Items.Count)
            {
                _lastHoveredRoutineIndex = -1;
                HideRoutineInfoPopup();
                return;
            }

            if (index == _lastHoveredRoutineIndex)
                return;

            _lastHoveredRoutineIndex = index;

            HideRoutineInfoPopup();

            Routine routine = lstRoutines.Items[index] as Routine;

            if (routine == null)
                return;

            ShowRoutineInfoPopupDelayed(routine);
        }

        private void ShowRoutineInfoPopupDelayed(Routine routine)
        {
            List<RoutineStep> visibleSteps = routine.Steps
                .Where(s => s.Show)
                .OrderBy(s => s.Order)
                .ToList();

            List<RoutineStep> hiddenSteps = routine.Steps
                .Where(s => !s.Show)
                .OrderBy(s => s.Order)
                .ToList();

            _routineInfoPopup.ShowAtMouseDelayed(
                lstRoutines,
                Control.MousePosition,
                new InfoPopupSection(
                    "Letzte Ausführung",
                    DateTimeHelper.GetRelativeTime(routine.LastExecutionAt)),
                new InfoPopupSection(
                    $"Schritte ({visibleSteps.Count})",
                    BuildStepListText(visibleSteps)),
                new InfoPopupSection(
                    $"Ausgeblendete Schritte ({hiddenSteps.Count})",
                    BuildStepListText(hiddenSteps)));
        }
        private void LstRoutines_MouseLeave(object sender, EventArgs e)
        {
            _lastHoveredRoutineIndex = -1;
            HideRoutineInfoPopup();
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
            btnStartRoutineAuto.Enabled = hasSelection;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_routineInfoPopup != null)
                {
                    _routineInfoPopup.Dispose();
                    _routineInfoPopup = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}