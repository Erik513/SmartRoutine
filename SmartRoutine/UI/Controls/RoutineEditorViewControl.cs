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
        private TableLayoutPanel leftBtnsTlp;
        private Button btnAddStep, btnDeleteStep;

        // Rechte Seite
        private TableLayoutPanel rightTlp;
        private Label lblStepNameTitle;
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

            // Titel (lblStepNameTitle)
            lblStepNameTitle = UIStyles.Labels.CreateTitle();
            lblStepNameTitle.Dock = DockStyle.Fill;

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
                lblStepNameTitle, lblStepName, txtStepName,
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
            if (routine != null)
            {
                _originalRoutine = DeepCopy(routine);
                _currentRoutine = DeepCopy(routine);
                txtRoutineName.Text = _currentRoutine.Name;
                RefreshStepsList();
            }
            else
            {
                _originalRoutine = null;
                _currentRoutine = new Routine { Name = "", Steps = new List<RoutineStep>() };
                txtRoutineName.Text = "";
                RefreshStepsList();
            }

            ClearEditor();
            btnDeleteStep.Enabled = false;
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
                    Type = s.Type,
                    Value = s.Value,
                    Description = s.Description,
                    UserDescription = s.UserDescription
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
                    orig.Description != curr.Description ||
                    orig.UserDescription != curr.UserDescription)
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
                lstSteps.Items.Add(step);
            }
            lstSteps.Visible = true;

            bool hasSteps = lstSteps.Items.Count > 0;
            SetStepButtonsEnabled(hasSteps);
        }

        private void ClearEditor()
        {
            lblStepNameTitle.Text = "Neuen Schritt erstellen";
            txtStepName.Text = "";
            txtStepDescription.Text = "";
            txtUrl.Text = "";
            cmbStepType.SelectedIndex = -1;
            stepTypeContentTlp.Visible = false;
            _editingStep = null;
            SetEditorEnabled(false);
        }

        private void LoadStepToEditor(RoutineStep step)
        {
            lblStepNameTitle.Text = $"{step.Description} bearbeiten";
            txtStepName.Text = step.Description;
            txtStepDescription.Text = step.UserDescription;

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
        private void SetStepButtonsEnabled(bool enabled)
        {
            btnDeleteStep.Enabled = enabled;
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
            // Prüfen ob ein Item ausgewählt ist und ob es ein RoutineStep ist
            if (lstSteps.SelectedItem is RoutineStep step)
            {
                // Rechte Seite sichtbar machen
                rightTlp.Visible = true;
                LoadStepToEditor(step);
                btnDeleteStep.Enabled = true;
            }
            else
            {
                // Kein Schritt ausgewählt -> Rechte Seite unsichtbar
                rightTlp.Visible = false;
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
            rightTlp.Visible = true;
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
            string userDescription = txtStepDescription.Text.Trim();

            // Validierung
            if (string.IsNullOrWhiteSpace(stepName))
            {
                MessageBox.Show("Bitte gib einen Namen für den Schritt ein.", "Hinweis",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStepName.Focus();
                return;
            }

            // Schritt-Typ aus ComboBox holen
            StepType selectedType = StepType.OpenUrl;
            string stepUrl = "";

            if (cmbStepType.SelectedIndex != -1)
            {
                var selectedItem = (KeyValuePair<StepType, string>)cmbStepType.SelectedItem;
                if (!string.IsNullOrEmpty(selectedItem.Value))
                {
                    selectedType = selectedItem.Key;
                }
            }

            // Validierung je nach Typ
            if (selectedType == StepType.OpenUrl)
            {
                stepUrl = txtUrl.Text.Trim();

                // Validiere mit Service
                var validationResult = _urlValidationService.ValidateAndRepairUrl(stepUrl, false);

                if (!validationResult.IsValid)
                {
                    MessageBox.Show(validationResult.ErrorMessage, "Ungültige URL",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUrl.Focus();
                    return;
                }

                // Verwende die reparierte URL
                stepUrl = validationResult.RepairedUrl;
            }

            // Schritt speichern oder aktualisieren
            if (_editingStep != null)
            {
                // Vorhandenen Schritt aktualisieren
                _editingStep.Description = stepName;
                _editingStep.UserDescription = userDescription;
                _editingStep.Type = selectedType;
                if (selectedType == StepType.OpenUrl)
                {
                    _editingStep.Value = stepUrl;
                }
            }
            else
            {
                // Neuen Schritt erstellen
                var newStep = new RoutineStep
                {
                    Id = Guid.NewGuid().ToString(),
                    Order = _currentRoutine.Steps.Count,
                    Type = selectedType,
                    Value = selectedType == StepType.OpenUrl ? stepUrl : "",
                    Description = stepName,
                    UserDescription = userDescription
                };
                _currentRoutine.Steps.Add(newStep);
            }

            // UI aktualisieren
            RefreshStepsList();
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = false;
            lstSteps.SelectedIndex = -1;

            // Routine als geändert markieren
            _currentRoutine.UpdatedAt = DateTime.Now;
        }

        private void BtnCancelStep_Click(object sender, EventArgs e)
        {
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = lstSteps.SelectedItem != null;
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