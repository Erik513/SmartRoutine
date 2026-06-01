using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CustomWFUI;

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

        private TableLayoutPanel mainLayout;
        private TableLayoutPanel buttonPanel;

        private StyledListBoxControl lstRoutines;
        private Button btnNewRoutine, btnEditRoutine, btnDeleteRoutine, btnStartRoutine;

        private readonly ToolTip _routineToolTip = UIStyles.ToolTips.CreateToolTip();
        private int _lastHoveredRoutineIndex = -1;

        private const string DefaultRoutineNamePrefix = "Meine Routine ";

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
            lstRoutines = new StyledListBoxControl(
                displayTextMember: "Name",
                allowReorder: true,
                showEnumeration: false,
                headerTitle: "Meine Routinen",
                headerTextAlign: ContentAlignment.MiddleCenter)
            {
                Dock = DockStyle.Fill,
                ItemHeightCustom = 35,
                Visible = true
            };

            lstRoutines.SelectedIndexChanged += LstRoutines_SelectedIndexChanged;
            lstRoutines.ItemsReordered += LstRoutines_ItemsReordered;
            lstRoutines.MouseMove += LstRoutines_MouseMove;
            lstRoutines.MouseLeave += LstRoutines_MouseLeave;
        }
        private void InitializeButtonPanel()
        {
            buttonPanel = UIStyles.TableLayoutPanels.CreateDark(4, 1);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.Padding = new Padding(0);

            buttonPanel.ColumnStyles.Clear();
            buttonPanel.RowStyles.Clear();

            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            btnNewRoutine = UIStyles.Buttons.CreateGreen("+", "Neue Routine erstellen", new Size(30, 30), true);
            btnNewRoutine.Dock = DockStyle.Fill;
            btnNewRoutine.Margin = new Padding(5);
            btnNewRoutine.Click += BtnNewRoutine_Click;

            btnEditRoutine = UIStyles.Buttons.CreatePrimary("✎", "Routine bearbeiten", new Size(30, 30), true);
            btnEditRoutine.Dock = DockStyle.Fill;
            btnEditRoutine.Margin = new Padding(5);
            btnEditRoutine.Click += BtnEditRoutine_Click;

            btnDeleteRoutine = UIStyles.Buttons.CreateDanger("🗑", "Routine löschen", new Size(30, 30), true);
            btnDeleteRoutine.Dock = DockStyle.Fill;
            btnDeleteRoutine.Margin = new Padding(5);
            btnDeleteRoutine.Click += BtnDeleteRoutine_Click;

            btnStartRoutine = UIStyles.Buttons.CreateGreen("▶", "Routine starten", new Size(30, 30), true);
            btnStartRoutine.Dock = DockStyle.Fill;
            btnStartRoutine.Margin = new Padding(5);
            btnStartRoutine.Click += BtnStartRoutine_Click;

            buttonPanel.Controls.Add(btnNewRoutine, 0, 0);
            buttonPanel.Controls.Add(btnEditRoutine, 1, 0);
            buttonPanel.Controls.Add(btnDeleteRoutine, 2, 0);
            buttonPanel.Controls.Add(btnStartRoutine, 3, 0);
        }

        private void BtnNewRoutine_Click(object sender, EventArgs e)
        {
            int nextNumber = GetNextRoutineNumber();
            string newRoutineName = $"Meine Routine {nextNumber}";

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

        private void BtnDeleteRoutine_Click(object sender, EventArgs e)
        {
            if (_selectedRoutine == null)
                return;

            DeleteRoutineClicked?.Invoke(this, _selectedRoutine);
        }

        private void BtnStartRoutine_Click(object sender, EventArgs e)
        {
            if (_selectedRoutine == null)
                return;

            StartRoutineClicked?.Invoke(this, _selectedRoutine);
        }


        private void BuildLayout()
        {
            mainLayout.Controls.Add(lstRoutines, 0, 0);
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            Controls.Add(mainLayout);

            Resize += (s, e) => CenterControls(mainLayout);

            CenterControls(mainLayout);
        }


        private int GetNextRoutineNumber()
        {
            var routines = _routineService.GetAllRoutines();

            int maxNumber = routines
                .Where(r => r.Name.StartsWith(DefaultRoutineNamePrefix))
                .Select(r =>
                {
                    string numberPart = r.Name.Substring("Meine Routine ".Length);
                    return int.TryParse(numberPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return maxNumber + 1;
        }



        private void CenterControls(TableLayoutPanel layout)
        {
            int newWidth = Math.Max(300, Width - 160);
            int newHeight = Math.Max(300, Height - 160);

            layout.Size = new Size(newWidth, newHeight);

            layout.Location = new Point(
                (Width - layout.Width) / 2,
                (Height - layout.Height) / 2);
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
                string text = $"Zuletzt gestartet: {SmartRoutine.UI.Helpers.DateTimeHelper.GetRelativeTime(routine.LastExecutionAt)}";
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