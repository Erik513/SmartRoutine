using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IUrlValidationService _urlValidationService;
        private Routine _originalRoutine;
        private Routine _currentRoutine;
        private RoutineStep _editingStep;
        private ToolTip _errorToolTip = UIStyles.ToolTips.CreateToolTip();
        private bool _isRefreshing = false;
        private bool _suppressSelectionEvent = false;

        // UI Controls
        private TableLayoutPanel mainTlp;

        // Inhalt (links + rechts)
        private TableLayoutPanel contentTlp;

        // Linke Seite
        private TableLayoutPanel leftTlp;
        private TableLayoutPanel leftRoutineTitleTlp;
        private Label lblRoutineName;
        private TextBox txtRoutineName;
        private StyledListBoxWithHeader lstSteps;
        //private ListBox lstSteps;

        private TableLayoutPanel leftBtnsTlp;
        private Button btnAddStep, btnDeleteStep;

        // Rechte Seite
        private TableLayoutPanel rightTlp;
        private TableLayoutPanel rightTitleTlp;
        private Label lblStepNameTitle;
        private ToggleSwitch tglStepEnabled;
        private Label lblStepName;
        private TextBox txtStepName;
        private Label lblStepDescription;
        private TextBox txtStepDescription;
        private Label lblStepType;
        private ComboBox cmbStepType;

        private TableLayoutPanel rightBtnsTlp;
        private Button btnSaveStep, btnCancelStep;

        // Controls based on StepType
        // StepTypeContent
        private Panel stepTypeContentPanel;
        private TableLayoutPanel stepTypeContentTlp;
        // OpenURL
        private Label lblUrl;
        private TextBox txtUrl;
        private ComboBox cmbOpenUrl;
        // More ...

        // Footer
        private Button btnBack;

        public RoutineEditorViewControl(IRoutineService routineService, IUrlValidationService urlValidationService)
        {
            _routineService = routineService;
            _urlValidationService = urlValidationService;
            _originalRoutine = null;
            _currentRoutine = null;
            _editingStep = null;

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Black;

            this.Load += (s, e) =>
            {
                lstSteps.Visible = true;
                lstSteps.Invalidate();
                lstSteps.Update();
            };

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            InitializeControl();
        }

        private void InitializeControl()
        {
            // Hauptstruktur
            mainTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 2);
            mainTlp.Dock = DockStyle.Fill;
            mainTlp.RowStyles.Clear();
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Inhalt
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Footer

            // ========== INHALT (Linke + Rechte Seite) ==========
            contentTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            contentTlp.Dock = DockStyle.Fill;
            contentTlp.ColumnStyles.Clear();
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));  // Linke Seite 40%
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));  // Rechte Seite 60%

            // ----- LINKE SEITE -----
            leftTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 3);
            leftTlp.Dock = DockStyle.Fill;
            leftTlp.RowStyles.Clear();
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // Oben
            leftRoutineTitleTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            leftRoutineTitleTlp.Dock = DockStyle.Fill;
            leftRoutineTitleTlp.ColumnStyles.Clear();
            leftRoutineTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); 
            leftRoutineTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            lblRoutineName = UIStyles.Labels.CreateTitle("Routinenname:");
            lblRoutineName.Dock = DockStyle.Fill;

            txtRoutineName = UIStyles.TextBoxes.CreateStandard();
            txtRoutineName.Dock = DockStyle.Fill;

            leftRoutineTitleTlp.Controls.Add(lblRoutineName, 0, 0);
            leftRoutineTitleTlp.Controls.Add(txtRoutineName, 1, 0);

            // Mitte
            lstSteps = new StyledListBoxWithHeader("Schritte", ContentAlignment.MiddleCenter)
            {
                Dock = DockStyle.Fill,
                ItemHeightCustom = 35,
                Visible = true,
            };
            lstSteps.SelectedIndexChanged += LstSteps_SelectedIndexChanged;
            lstSteps.ItemsReordered += LstSteps_ItemsReordered;

            // Unten
            leftBtnsTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            leftBtnsTlp.Dock = DockStyle.Fill;

            btnAddStep = UIStyles.Buttons.CreatePrimary("+ Schritt hinzufügen", "", new Size(155, 35));
            btnAddStep.Click += BtnAddStep_Click;

            btnDeleteStep = UIStyles.Buttons.CreateDanger("🗑 Löschen", "", new Size(155, 35));
            btnDeleteStep.Click += BtnDeleteStep_Click;
            btnDeleteStep.Enabled = false;

            leftBtnsTlp.ColumnStyles.Clear();
            leftBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btnAddStep.Width));
            leftBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btnDeleteStep.Width));

            leftBtnsTlp.Controls.Add(btnAddStep, 0, 0);
            leftBtnsTlp.Controls.Add(btnDeleteStep, 1, 0);

            leftTlp.Controls.AddRange(new Control[] {
                leftRoutineTitleTlp, lstSteps, leftBtnsTlp
            });

            // ----- RECHTE SEITE -----
            rightTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 9);
            rightTlp.Dock = DockStyle.Fill;
            rightTlp.RowStyles.Clear();
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // lblStepNameTitle
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // lblStepName
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // txtStepName
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // lblStepDescription
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // txtStepDescription
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // lblStepType
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // cmbStepType
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // stepTypeContentTlp
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // btnSaveStep, btnCancelStep
            rightTlp.Visible = false;

            // rightTitleTlp
            rightTitleTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            rightTitleTlp.Dock = DockStyle.Fill;
            rightTitleTlp.ColumnStyles.Clear();
            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // lblStepNameTitle
            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60)); // chkStepEnabled

            // Titel (lblStepNameTitle)
            lblStepNameTitle = UIStyles.Labels.CreateTitle();
            lblStepNameTitle.Dock = DockStyle.Fill;

            // chkStepEnabled
            tglStepEnabled = UIStyles.ToggleSwitches.CreateStandard(true, "Schritt ist aktiviert", "Schritt ist deaktiviert");
            tglStepEnabled.Location = new Point(0, 0);
            tglStepEnabled.Anchor = AnchorStyles.None;

            rightTitleTlp.Controls.Add(lblStepNameTitle, 0, 0);
            rightTitleTlp.Controls.Add(tglStepEnabled, 1, 0);

            // lblStepName
            lblStepName = UIStyles.Labels.CreateNormal("Name:");
            lblStepName.Dock = DockStyle.Fill;

            // txtStepName
            txtStepName = UIStyles.TextBoxes.CreateStandard();
            txtStepName.Dock = DockStyle.Fill;

            // lblStepDescription
            lblStepDescription = UIStyles.Labels.CreateNormal("Beschreibung:");
            lblStepDescription.Dock = DockStyle.Fill;

            // txtStepDescription
            txtStepDescription = UIStyles.TextBoxes.CreateStandard();
            txtStepDescription.Dock = DockStyle.Fill;

            // lblStepType
            lblStepType = UIStyles.Labels.CreateNormal("Aktion:");
            lblStepType.Dock = DockStyle.Fill;
            // cmbStepType
            cmbStepType = UIStyles.ComboBoxes.CreateStandard(ComboBoxStyle.DropDownList);
            cmbStepType.Dock = DockStyle.Fill;
            var stepTypes = StepTypeHelper.GetStepTypeListWithEmpty();
            cmbStepType.DataSource = stepTypes;
            cmbStepType.DisplayMember = "Value";
            cmbStepType.ValueMember = "Key";
            cmbStepType.SelectedIndex = -1;
            cmbStepType.SelectedIndexChanged += CmbStepType_SelectedIndexChanged;

            // ==================================================================================================================
            // stepTypeContent
            stepTypeContentPanel = UIStyles.Panels.CreateMedium();
            stepTypeContentPanel.Dock = DockStyle.Fill;

            stepTypeContentTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 1);
            stepTypeContentTlp.Dock = DockStyle.Fill;

            stepTypeContentPanel.Controls.Add(stepTypeContentTlp);
            // OpenURL Controls vorbereiten
            lblUrl = UIStyles.Labels.CreateNormal("Webseite:");
            lblUrl.Dock = DockStyle.Fill;

            txtUrl = UIStyles.TextBoxes.CreateStandard();
            txtUrl.Dock = DockStyle.Fill;
            txtUrl.TextChanged += TxtUrl_TextChanged;  // Für Live-Validierung
            txtUrl.LostFocus += TxtUrl_LostFocus;       // Für finale Validierung

            // Drag & Drop für txtUrl aktivieren
            DragDropHelper.EnableTextDragDrop(txtUrl, (droppedText) =>
            {
                // Optional: Bereinige den gedroppten Text
                string cleanedText = droppedText.Trim();

                // Setze den Text in die TextBox
                txtUrl.Text = cleanedText;

                // Führe die Validierung aus
                TxtUrl_TextChanged(txtUrl, EventArgs.Empty);

                // Setze den Cursor ans Ende
                txtUrl.SelectionStart = txtUrl.Text.Length;
            });

            // ==================================================================================================================

            // btnSaveStep, btnCancelStep
            rightBtnsTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            rightBtnsTlp.Dock = DockStyle.Fill;

            btnSaveStep = UIStyles.Buttons.CreatePrimary("💾 Schritt speichern", "", new Size(155, 35));
            btnSaveStep.Click += BtnSaveStep_Click;

            btnCancelStep = UIStyles.Buttons.CreateStandard("✖ Abbrechen", "", new Size(155, 35));
            btnCancelStep.Click += BtnCancelStep_Click;

            rightBtnsTlp.ColumnStyles.Clear();
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btnSaveStep.Width));
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btnCancelStep.Width));
            rightBtnsTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            rightBtnsTlp.Controls.Add(btnSaveStep, 0, 0);
            rightBtnsTlp.Controls.Add(btnCancelStep, 1, 0);

            rightTlp.Controls.AddRange(new Control[] {
                rightTitleTlp, lblStepName, txtStepName,
                lblStepDescription, txtStepDescription,
                lblStepType, cmbStepType, stepTypeContentPanel, 
                rightBtnsTlp
            });

            // ========== FOOTER ==========
            var footerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundDark,
                Padding = new Padding(20, 10, 20, 10)
            };

            btnBack = UIStyles.Buttons.CreateStandard("← Zurück", "", new Size(100, 35));
            btnBack.Location = new Point(footerPanel.Width - 120, 12);
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Click += BtnBack_Click;

            footerPanel.Controls.AddRange(new Control[] { btnBack });

            // Hinzufügen
            contentTlp.Controls.Add(leftTlp, 0, 0);
            contentTlp.Controls.Add(rightTlp, 1, 0);

            mainTlp.Controls.Add(contentTlp, 0, 0);
            mainTlp.Controls.Add(footerPanel, 0, 1);

            this.Controls.Add(mainTlp);

            // Initial Zustand
            SetEditorEnabled(false);
        }

        // ========== PUBLIC METHODS ==========
        public void LoadRoutine(Routine routine)
        {
            _originalRoutine = DeepCopy(routine ?? new Routine());
            _currentRoutine = DeepCopy(routine ?? new Routine());

            txtRoutineName.Text = _currentRoutine.Name;

            lstSteps.SelectedIndex = -1;
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = false;

            RefreshStepsList(silent: true);
            ClearEditor();

            if (_currentRoutine.Steps.Count == 0)
                rightTlp.Visible = false;

            if (routine?.IsNew == true)
            {
                this.BeginInvoke(new Action(() =>
                {
                    txtRoutineName.Focus();
                    txtRoutineName.SelectAll();
                }));

                routine.IsNew = false;
            }
        }
        private void TxtUrl_TextChanged(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                // Leeres Feld - normale Farbe
                txtUrl.ForeColor = UIStyles.Colors.TextPrimary;
                _errorToolTip.SetToolTip(txtUrl, "");
                return;
            }

            // Live-Validierung während der Eingabe
            var result = _urlValidationService.ValidateAndRepairUrl(url, false);

            if (result.IsValid)
            {
                // Gültige URL - normale Farbe
                txtUrl.ForeColor = UIStyles.Colors.TextPrimary;
                _errorToolTip.SetToolTip(txtUrl, "");

                // Wenn die URL repariert wurde, im Hintergrund merken (aber nicht überschreiben während der Eingabe)
                if (result.RepairedUrl != url && result.RepairedUrl != txtUrl.Tag as string)
                {
                    txtUrl.Tag = result.RepairedUrl; // Reparierte URL als Tag speichern
                }
            }
            else
            {
                // Ungültige URL - rote Farbe und Fehlermeldung im ToolTip
                txtUrl.ForeColor = UIStyles.Colors.Red;
                _errorToolTip.SetToolTip(txtUrl, result.ErrorMessage);
            }
        }
        private void TxtUrl_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUrl.Text))
                return;

            var result = _urlValidationService.ValidateAndRepairUrl(txtUrl.Text, false);

            if (result.IsValid)
            {
                // Verwende die reparierte URL wenn vorhanden
                if (result.RepairedUrl != txtUrl.Text)
                {
                    txtUrl.Text = result.RepairedUrl;
                }
                txtUrl.ForeColor = UIStyles.Colors.TextPrimary;
                _errorToolTip.SetToolTip(txtUrl, "");
            }
            else
            {
                txtUrl.ForeColor = UIStyles.Colors.Red;
                _errorToolTip.SetToolTip(txtUrl, result.ErrorMessage);
            }
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
                    Name = s.Name,
                    Description = s.Description,
                    Show = s.Show,
                    Type = s.Type,
                    Value = s.Value
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
                    orig.Name != curr.Name ||
                    orig.Description != curr.Description)
                    return true;
            }

            return false;
        }

        private void ClearEditor()
        {
            lblStepNameTitle.Text = "Neuen Schritt erstellen";
            txtStepName.Text = "";
            txtStepDescription.Text = "";
            txtUrl.Text = "";
            tglStepEnabled.Checked = true;
            cmbStepType.SelectedIndex = -1;
            stepTypeContentTlp.Visible = false;
            _editingStep = null;
            SetEditorEnabled(false);
            rightTlp.Visible = false;
        }

        private void LoadStepToEditor(RoutineStep step)
        {
            lblStepNameTitle.Text = $"{step.Name} bearbeiten";
            txtStepName.Text = step.Name;
            txtStepDescription.Text = step.Description;
            tglStepEnabled.Checked = step.Show;

            // Je nach StepType den entsprechenden Content laden
            var stepTypes = (List<KeyValuePair<StepType, string>>)cmbStepType.DataSource;
            for (int i = 0; i < stepTypes.Count; i++)
            {
                if (stepTypes[i].Key == step.Type && !string.IsNullOrEmpty(stepTypes[i].Value))
                {
                    cmbStepType.SelectedIndex = i;
                    break;
                }
            }

            if (step.Type == StepType.OpenUrl)
            {
                txtUrl.Text = step.Value;
                stepTypeContentTlp.Visible = true;
            }
            else
            {
                stepTypeContentTlp.Visible = false;
            }

            _editingStep = step;
            SetEditorEnabled(true);
        }

        private void SetEditorEnabled(bool enabled)
        {
            foreach (Control control in rightTlp.Controls)
            {
                control.Enabled = enabled;
            }
            // stepTypeContentTlp Controls manuell aktivieren/deaktivieren
            foreach (Control control in stepTypeContentTlp.Controls)
            {
                control.Enabled = enabled;
            }
        }

        // ========== STEP EVENT HANDLER ==========
        private void LstSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isRefreshing || !(lstSteps.SelectedItem is RoutineStep step))
            {
                rightTlp.Visible = false;
                ClearEditor();
                btnDeleteStep.Enabled = false;
                return;
            }

            // Aktuellen Schritt speichern, bevor neuer geladen wird
            if (_editingStep != null && rightTlp.Visible)
            {
                if (!ValidateCurrentStep()) return;
                SaveCurrentStep(refreshList: false);
            }

            rightTlp.Visible = true;
            LoadStepToEditor(step);
            btnDeleteStep.Enabled = true;
        }
        private void LstSteps_ItemsReordered(object sender, EventArgs e)
        {
            if (_isRefreshing) return;

            var reordered = lstSteps.Items.Cast<RoutineStep>().ToList();

            for (int i = 0; i < reordered.Count; i++)
                reordered[i].Order = i;

            _currentRoutine.Steps = reordered;
            _routineService.UpdateRoutine(_currentRoutine);
        }
        private void BtnAddStep_Click(object sender, EventArgs e)
        {
            if (_editingStep != null && rightTlp.Visible)
            {
                if (!ValidateCurrentStep()) return;
                SaveCurrentStep(refreshList: false);
            }

            ClearEditor();
            rightTlp.Visible = true;
            SetEditorEnabled(true);
            txtStepName.Focus();
        }

        private void BtnDeleteStep_Click(object sender, EventArgs e)
        {
            if (!(lstSteps.SelectedItem is RoutineStep stepToDelete)) return;

            if (MessageBox.Show($"Schritt '{stepToDelete.Name}' wirklich löschen?",
                    "Bestätigen", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes)
                return;

            _routineService.RemoveStep(_currentRoutine.Id, stepToDelete.Id);
            _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

            RefreshStepsList(silent: true);

            lstSteps.SelectedIndex = -1;
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = _currentRoutine.Steps.Count > 0;

            SaveChanges?.Invoke(this, _currentRoutine);
        }



        // ========== STEP LIST METHODS ==========
        private void RefreshStepsList(bool silent = false)
        {
            if (_currentRoutine == null) return;

            _isRefreshing = true;
            lstSteps.BeginUpdate();
            lstSteps.Items.Clear();

            var orderedSteps = _currentRoutine.Steps.OrderBy(s => s.Order).ToList();
            foreach (var step in orderedSteps)
                lstSteps.Items.Add(step);

            lstSteps.EndUpdate();
            _isRefreshing = false;

            if (lstSteps.Items.Count > 0)
                lstSteps.SelectedIndex = -1;

            if (!silent)
                Debug.WriteLine($"RefreshStepsList: {orderedSteps.Count} Schritte geladen");
        }


        private void BtnSaveStep_Click(object sender, EventArgs e)
        {
            if (!ValidateCurrentStep()) return;
            SaveCurrentStep();
            lstSteps.SelectedIndex = -1;
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = false;
        }

        private void BtnCancelStep_Click(object sender, EventArgs e)
        {
            lstSteps.SelectedIndex = -1;
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = lstSteps.Items.Count > 0 && lstSteps.SelectedIndex >= 0;
        }
        private void CmbStepType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Alten Inhalt löschen
            stepTypeContentTlp.Controls.Clear();
            stepTypeContentTlp.RowStyles.Clear();
            stepTypeContentTlp.ColumnStyles.Clear();

            // Prüfen ob etwas ausgewählt ist
            if (cmbStepType.SelectedIndex == -1)
            {
                stepTypeContentTlp.Visible = false;
                return;
            }

            // Ausgewählten StepType holen
            var selectedItem = (KeyValuePair<StepType, string>)cmbStepType.SelectedItem;
            StepType selectedType = selectedItem.Key;

            // Prüfen ob es der leere Eintrag ist (leerer String)
            if (string.IsNullOrEmpty(selectedItem.Value))
            {
                stepTypeContentTlp.Visible = false;
                return;
            }

            // Je nach ausgewähltem Typ den Content aufbauen
            switch (selectedType)
            {
                case StepType.OpenUrl:
                    stepTypeContentTlp.ColumnCount = 1;
                    stepTypeContentTlp.RowCount = 2;

                    stepTypeContentTlp.RowStyles.Clear();
                    stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

                    // Label und TextBox hinzufügen
                    stepTypeContentTlp.Controls.Add(lblUrl, 0, 0);
                    stepTypeContentTlp.Controls.Add(txtUrl, 0, 1);

                    stepTypeContentTlp.Visible = true;
                    break;

                default:
                    stepTypeContentTlp.Visible = false;
                    break;
            }
        }

        // ========== BACK BUTTON ==========
        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (_editingStep != null && rightTlp.Visible)
            {
                if (!ValidateCurrentStep()) return;
                SaveCurrentStep(refreshList: false);
            }

            if (!ValidateRoutineName()) return;

            if (HasChanges())
            {
                SaveCurrentRoutine();
                SaveChanges?.Invoke(this, _currentRoutine);
            }

            BackToRoutinesClicked?.Invoke(this, EventArgs.Empty);
        }
        private bool ValidateRoutineName()
        {
            string routineName = txtRoutineName.Text.Trim();

            // Prüfen ob Name leer ist
            if (string.IsNullOrWhiteSpace(routineName))
            {
                MessageBox.Show("Bitte geben Sie einen Namen für die Routine ein.", "Validierung",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoutineName.Focus();
                return false;
            }

            // Prüfen ob Name bereits existiert
            var existingRoutines = _routineService.GetAllRoutines();
            bool nameExists = existingRoutines.Any(r =>
                r.Name.Equals(routineName, StringComparison.OrdinalIgnoreCase) &&
                r.Id != _currentRoutine.Id);

            if (nameExists)
            {
                MessageBox.Show($"Eine Routine mit dem Namen '{routineName}' existiert bereits.\nBitte wählen Sie einen anderen Namen.",
                    "Validierung",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoutineName.Focus();
                txtRoutineName.SelectAll();
                return false;
            }

            return true;
        }
        private bool ValidateCurrentStep()
        {
            if (string.IsNullOrWhiteSpace(txtStepName.Text))
            {
                MessageBox.Show("Bitte geben Sie einen Namen für den Schritt ein.", "Validierung",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStepName.Focus();
                return false;
            }

            if (cmbStepType.SelectedIndex == -1)
            {
                MessageBox.Show("Bitte wählen Sie einen Aktionstyp aus.", "Validierung",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbStepType.Focus();
                return false;
            }

            var selectedItem = (KeyValuePair<StepType, string>)cmbStepType.SelectedItem;
            if (selectedItem.Key == StepType.OpenUrl && string.IsNullOrWhiteSpace(txtUrl.Text))
            {
                MessageBox.Show("Bitte geben Sie eine URL ein.", "Validierung",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrl.Focus();
                return false;
            }

            return true;
        }

        private void SaveCurrentStep(bool refreshList = true)
        {
            string name = txtStepName.Text.Trim();
            string description = txtStepDescription.Text.Trim();
            bool show = tglStepEnabled.Checked;

            if (cmbStepType.SelectedIndex == -1)
            {
                MessageBox.Show("Bitte wählen Sie einen Aktionstyp aus.", "Validierung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = (KeyValuePair<StepType, string>)cmbStepType.SelectedItem;
            StepType type = selectedItem.Key;

            string value = type == StepType.OpenUrl
                ? _urlValidationService.ValidateAndRepairUrl(txtUrl.Text.Trim(), false).RepairedUrl
                : "";

            if (_editingStep != null)
            {
                _routineService.UpdateStep(_currentRoutine.Id, _editingStep.Id, name, description, show, type, value);

                // Lokale Kopie aktualisieren
                _editingStep.Name = name;
                _editingStep.Description = description;
                _editingStep.Show = show;
                _editingStep.Type = type;
                _editingStep.Value = value;
            }
            else
            {
                _routineService.AddStep(_currentRoutine.Id, name, description, show, type, value);
                _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

                if (_currentRoutine.Steps.Any())
                    _editingStep = _currentRoutine.Steps.OrderBy(s => s.Order).LastOrDefault();
            }
            if (refreshList)
                RefreshStepsList(silent: true);
        }

        private void SaveCurrentRoutine()
        {
            _currentRoutine.Name = txtRoutineName.Text.Trim();
            _currentRoutine.UpdatedAt = DateTime.Now;
            _routineService.UpdateRoutine(_currentRoutine);
        }
    }
}