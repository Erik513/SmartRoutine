using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Helpers;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartRoutine.UI.Controls
{
    public partial class RoutineEditorViewControl : UserControl
    {
        public event EventHandler BackToRoutinesClicked;
        public event EventHandler<Routine> SaveChanges;

        private readonly IRoutineService _routineService;
        private Routine _originalRoutine;
        private Routine _currentRoutine;
        private RoutineStep _editingStep;

        // UI Controls
        private TextBox txtRoutineName;
        private Button btnBack;

        // Linke Seite
        private StyledListBoxWithHeader lstSteps;
        private Button btnAddStep, btnDeleteStep;
        private Panel leftPanel;
        private Panel rightPanel;

        // Rechte Seite
        private TextBox txtStepName;
        private TextBox txtStepDescription;
        private ComboBox cmbStepType;
        private TextBox txtUrl;
        private Button btnSaveStep, btnCancelStep;
        private Label lblEditorTitle;

        public RoutineEditorViewControl(IRoutineService routineService)
        {
            _routineService = routineService;
            _originalRoutine = null;
            _currentRoutine = null;
            _editingStep = null;

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Black;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            InitializeControl();
        }

        private void InitializeControl()
        {
            // Haupt-TableLayout für die gesamte Steuerung
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = UIStyles.Colors.BackgroundMediumElevated
            };
            mainLayout.RowStyles.Clear();
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Inhalt

            // ========== HEADER ==========
            var headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundDark,
                Padding = new Padding(20, 10, 20, 10)
            };

            var lblRoutineName = UIStyles.Labels.CreateNormal("Routine-Name:");
            lblRoutineName.Location = new Point(20, 18);
            lblRoutineName.Size = new Size(90, 25);

            txtRoutineName = new TextBox
            {
                Location = new Point(115, 18),
                Size = new Size(300, 25),
                BackColor = UIStyles.Colors.BackgroundMedium,
                ForeColor = UIStyles.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UIStyles.Fonts.Normal
            };

            btnBack = UIStyles.Buttons.CreateStandard("← Zurück", "", new Size(100, 35));
            btnBack.Location = new Point(headerPanel.Width - 120, 12);
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Click += BtnBack_Click;

            headerPanel.Controls.AddRange(new Control[] { lblRoutineName, txtRoutineName, btnBack });

            // ========== INHALT (Linke + Rechte Seite) ==========
            var stepsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UIStyles.Colors.BackgroundMediumElevated
            };
            stepsLayout.ColumnStyles.Clear();
            stepsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));  // Linke Seite 40%
            stepsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));  // Rechte Seite 60%

            // ----- LINKE SEITE -----
            leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundDark,
                Padding = new Padding(10)
            };

            var lblStepsTitle = UIStyles.Labels.CreateTitle("Schritte");
            lblStepsTitle.Location = new Point(10, 10);
            lblStepsTitle.Size = new Size(200, 30);

            lstSteps = new StyledListBoxWithHeader("Schritte", ContentAlignment.MiddleCenter)
            {
                Location = new Point(10, 50),
                Width = leftPanel.Width - 20,
                Height = 350,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ItemHeightCustom = 35,
                Visible = true
            };
            lstSteps.SelectedIndexChanged += LstSteps_SelectedIndexChanged;
            lstSteps.ItemsReordered += LstSteps_ItemsReordered;

            btnAddStep = UIStyles.Buttons.CreatePrimary("+ Schritt hinzufügen", "", new Size(155, 35));
            btnAddStep.Location = new Point(10, 410);
            btnAddStep.Click += BtnAddStep_Click;

            btnDeleteStep = UIStyles.Buttons.CreateStandard("🗑 Löschen", "", new Size(155, 35));
            btnDeleteStep.Location = new Point(175, 410);
            btnDeleteStep.Click += BtnDeleteStep_Click;
            btnDeleteStep.Enabled = false;

            leftPanel.Controls.AddRange(new Control[] { lblStepsTitle, lstSteps, btnAddStep, btnDeleteStep });

            // ----- RECHTE SEITE -----
            rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundDark,
                Padding = new Padding(10)
            };

            var editorLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = UIStyles.Colors.BackgroundMediumElevated
            }
            ;
            editorLayout.ColumnStyles.Clear();
            editorLayout.RowStyles.Add(new ColumnStyle(SizeType.AutoSize, 100));

            // Title
            lblEditorTitle = UIStyles.Labels.CreateTitle("Neuen Schritt erstellen");
            lblEditorTitle.Dock = DockStyle.Fill;

            // Schritt-Name
            var lblStepName = UIStyles.Labels.CreateNormal("Schritt-Name:");
            lblStepName.Dock = DockStyle.Fill;

            txtStepName = new TextBox
            {
                Location = new Point(110, 50),
                Size = new Size(350, 25),
                BackColor = UIStyles.Colors.BackgroundMedium,
                ForeColor = UIStyles.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UIStyles.Fonts.Normal
            };
            txtStepName.Dock = DockStyle.Fill;

            // Beschreibung
            var lblStepDescription = UIStyles.Labels.CreateNormal("Beschreibung:");
            lblStepDescription.Dock = DockStyle.Fill;

            txtStepDescription = new TextBox
            {
                Location = new Point(110, 90),
                Size = new Size(350, 60),
                Multiline = true,
                BackColor = UIStyles.Colors.BackgroundMedium,
                ForeColor = UIStyles.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UIStyles.Fonts.Normal
            };
            txtStepDescription.Dock = DockStyle.Fill;

            // Step Typ
            var lblStepType = UIStyles.Labels.CreateNormal("Typ:");
            lblStepType.Dock = DockStyle.Fill;

            cmbStepType = new ComboBox
            {
                Location = new Point(110, 165),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = UIStyles.Colors.BackgroundMedium,
                ForeColor = UIStyles.Colors.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            cmbStepType.Dock = DockStyle.Fill;
            cmbStepType.Items.AddRange(new object[] { "🌐 Website öffnen" });
            cmbStepType.SelectedIndex = 0;

            // URL Feld
            var lblUrl = UIStyles.Labels.CreateNormal("URL:");
            lblUrl.Dock = DockStyle.Fill;

            txtUrl = new TextBox
            {
                Location = new Point(110, 205),
                Size = new Size(350, 25),
                BackColor = UIStyles.Colors.BackgroundMedium,
                ForeColor = UIStyles.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UIStyles.Fonts.Normal
            };
            txtUrl.Dock = DockStyle.Fill;

            // Editor Buttons
            btnSaveStep = UIStyles.Buttons.CreatePrimary("💾 Schritt speichern", "", new Size(150, 35));
            btnSaveStep.Dock = DockStyle.Fill;
            btnSaveStep.Click += BtnSaveStep_Click;

            btnCancelStep = UIStyles.Buttons.CreateStandard("✖ Abbrechen", "", new Size(150, 35));
            btnCancelStep.Dock = DockStyle.Fill;
            btnCancelStep.Click += BtnCancelStep_Click;

            editorLayout.Controls.AddRange(new Control[] {
                lblEditorTitle, lblStepName, txtStepName,
                lblStepDescription, txtStepDescription,
                lblStepType, cmbStepType,
                lblUrl, txtUrl,
                btnSaveStep, btnCancelStep
            });

            stepsLayout.Controls.Add(leftPanel, 0, 0);
            stepsLayout.Controls.Add(rightPanel, 1, 0);

            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(stepsLayout, 0, 1);

            this.Controls.Add(mainLayout);

            // Initial Zustand
            SetEditorEnabled(false);
            btnDeleteStep.Enabled = false;
        }

        // ========== PUBLIC METHODS ==========
        public void LoadRoutine(Routine routine)
        {
            if (routine != null)
            {
                _originalRoutine = DeepCopy(routine);
                _currentRoutine = DeepCopy(routine);
                txtRoutineName.Text = _currentRoutine.Name;  // Name sollte jetzt angezeigt werden

                RefreshStepsList();  // Steps sollten jetzt angezeigt werden
            }
            else
            {
                _originalRoutine = null;
                _currentRoutine = new Routine { Name = "", Steps = new System.Collections.Generic.List<RoutineStep>() };
                txtRoutineName.Text = "";
            }

            ClearEditor();
            btnDeleteStep.Enabled = false;
        }

        private Routine DeepCopy(Routine original)
        {
            if (original == null) return null;

            return new Routine
            {
                Id = original.Id,
                Name = original.Name,
                CreatedAt = original.CreatedAt,
                UpdatedAt = original.UpdatedAt,
                Steps = original.Steps.Select(s => new RoutineStep
                {
                    Id = s.Id,
                    Order = s.Order,
                    Type = s.Type,
                    Value = s.Value,
                    Description = s.Description
                }).ToList()
            };
        }

        private bool HasChanges()
        {
            if (_originalRoutine == null)
            {
                return !string.IsNullOrWhiteSpace(txtRoutineName.Text) || _currentRoutine.Steps.Count > 0;
            }

            if (_originalRoutine.Name != txtRoutineName.Text)
                return true;

            if (_originalRoutine.Steps.Count != _currentRoutine.Steps.Count)
                return true;

            for (int i = 0; i < _originalRoutine.Steps.Count; i++)
            {
                var orig = _originalRoutine.Steps[i];
                var curr = _currentRoutine.Steps[i];

                if (orig.Order != curr.Order ||
                    orig.Type != curr.Type ||
                    orig.Value != curr.Value ||
                    orig.Description != curr.Description)
                    return true;
            }

            return false;
        }

        // ========== STEP LIST METHODS ==========
        private void RefreshStepsList()
        {
            if (_currentRoutine == null) return;

            lstSteps.Items.Clear();

            var steps = _currentRoutine.Steps.OrderBy(s => s.Order).ToList();
            foreach (var step in steps)
            {
                lstSteps.Items.Add(step.Description);
                Console.WriteLine($"Added step: {step.Description}");
            }

            // "Keine Schritte" Label anzeigen/ausblenden
            bool hasSteps = lstSteps.Items.Count > 0;
            lstSteps.Visible = hasSteps;

            Console.WriteLine($"Steps in ListBox: {lstSteps.Items.Count}, hasSteps: {hasSteps}");

            SetStepButtonsEnabled(hasSteps);
        }

        private void ClearEditor()
        {
            lblEditorTitle.Text = "Neuen Schritt erstellen";
            txtStepName.Text = "";
            txtStepDescription.Text = "";
            txtUrl.Text = "";
            _editingStep = null;
            SetEditorEnabled(false);
        }

        private void LoadStepToEditor(RoutineStep step)
        {
            lblEditorTitle.Text = "Schritt bearbeiten";
            txtStepName.Text = step.Description;
            txtStepDescription.Text = step.Description;
            txtUrl.Text = step.Value;
            _editingStep = step;
            SetEditorEnabled(true);
        }

        private void SetStepButtonsEnabled(bool enabled)
        {
            btnDeleteStep.Enabled = enabled;
        }

        private void SetEditorEnabled(bool enabled)
        {
            txtStepName.Enabled = enabled;
            txtStepDescription.Enabled = enabled;
            cmbStepType.Enabled = enabled;
            txtUrl.Enabled = enabled;
            btnSaveStep.Enabled = enabled;
            btnCancelStep.Enabled = enabled;
        }

        // ========== STEP EVENT HANDLER ==========
        private void LstSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSteps.SelectedItem is RoutineStep step)
            {
                LoadStepToEditor(step);
                btnDeleteStep.Enabled = true;
            }
            else
            {
                ClearEditor();
                btnDeleteStep.Enabled = false;
            }
        }

        private void LstSteps_ItemsReordered(object sender, EventArgs e)
        {
            var newOrder = new System.Collections.Generic.List<RoutineStep>();
            for (int i = 0; i < lstSteps.Items.Count; i++)
            {
                var step = lstSteps.Items[i] as RoutineStep;
                if (step != null)
                {
                    step.Order = i;
                    newOrder.Add(step);
                }
            }
            _currentRoutine.Steps = newOrder;
        }

        private void BtnAddStep_Click(object sender, EventArgs e)
        {
            ClearEditor();
            _editingStep = null;
            SetEditorEnabled(true);
            txtStepName.Focus();
        }

        private void BtnDeleteStep_Click(object sender, EventArgs e)
        {
            if (lstSteps.SelectedItem is RoutineStep step)
            {
                if (MessageBox.Show($"Schritt '{step.Description}' löschen?", "Bestätigen",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _currentRoutine.Steps.Remove(step);
                    RefreshStepsList();
                    ClearEditor();
                    btnDeleteStep.Enabled = false;
                    SaveChanges.Invoke(this, _currentRoutine);
                }
            }
        }

        private void BtnSaveStep_Click(object sender, EventArgs e)
        {
            string stepName = txtStepName.Text.Trim();
            string stepUrl = txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(stepName))
            {
                MessageBox.Show("Bitte gib einen Namen für den Schritt ein.", "Hinweis",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStepName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(stepUrl))
            {
                MessageBox.Show("Bitte gib eine URL ein.", "Hinweis",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrl.Focus();
                return;
            }

            string description = txtStepDescription.Text.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                description = stepName;
            }

            if (_editingStep != null)
            {
                _editingStep.Description = stepName;
                _editingStep.Value = stepUrl;
                _editingStep.Type = StepType.OpenUrl;
            }
            else
            {
                var newStep = new RoutineStep
                {
                    Id = Guid.NewGuid().ToString(),
                    Order = _currentRoutine.Steps.Count,
                    Type = StepType.OpenUrl,
                    Value = stepUrl,
                    Description = stepName
                };
                _currentRoutine.Steps.Add(newStep);
            }

            RefreshStepsList();
            ClearEditor();
            btnDeleteStep.Enabled = false;

            if (_currentRoutine.Steps.Count > 0)
            {
                lstSteps.SelectedIndex = _currentRoutine.Steps.Count - 1;
            }
        }

        private void BtnCancelStep_Click(object sender, EventArgs e)
        {
            ClearEditor();
            btnDeleteStep.Enabled = lstSteps.SelectedItem != null;
        }

        // ========== BACK BUTTON ==========
        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (HasChanges())
            {
                DialogResult result = MessageBox.Show(
                    "Möchten Sie die Änderungen speichern?",
                    "Änderungen speichern",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveCurrentRoutine();
                    SaveChanges?.Invoke(this, _currentRoutine);
                    BackToRoutinesClicked?.Invoke(sender, e);
                }
                else if (result == DialogResult.No)
                {
                    BackToRoutinesClicked?.Invoke(sender, e);
                }
            }
            else
            {
                BackToRoutinesClicked?.Invoke(sender, e);
            }
        }

        private void SaveCurrentRoutine()
        {
            if (_currentRoutine == null) return;
            _currentRoutine.Name = txtRoutineName.Text.Trim();
            _currentRoutine.UpdatedAt = DateTime.Now;
        }
    }
}