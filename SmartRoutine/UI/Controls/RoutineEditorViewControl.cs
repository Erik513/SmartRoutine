using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Forms;
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
        // Für Tests: Wenn true, werden MessageBoxen automatisch mit "Ja" beantwortet
        public bool AutoConfirmDialogs { get; set; } = false;

        public event EventHandler BackToRoutinesClicked;
        public event EventHandler<Routine> SaveChanges;

        private readonly IRoutineService _routineService;
        private readonly IUrlValidationService _urlValidationService;
        private Routine _originalRoutine;
        private Routine _currentRoutine;
        private RoutineStep _editingStep;
        private ToolTip _errorToolTip = UIStyles.ToolTips.CreateToolTip();
        private bool _isRefreshing = false;

        // UI Controls
        private TableLayoutPanel mainTlp;

        // Inhalt (links + rechts)
        private TableLayoutPanel contentTlp;

        // Linke Seite
        private TableLayoutPanel leftTlp;
        private TableLayoutPanel leftRoutineTitleTlp;
        private Label lblRoutineName;
        private TextBox txtRoutineName;
        private StyledListBoxControl lstSteps;

        // Rechte Seite
        private TableLayoutPanel rightTlp;
        private TableLayoutPanel rightTitleTlp;
        private Button btnExecuteStep;
        private Label lblStepNameTitle;
        private ToggleSwitch tglStepEnabled;

        private StyledPropertyTable editorBaseTable;
        private TextBox txtStepName;
        private TextBox txtStepDescription;
        private ComboBox cmbStepType;
        private ToggleSwitch tglAutoStart;

        private StyledPropertyTable editorOptionsTable;
        
        private Panel rightFillPanel;
        
        private TableLayoutPanel rightBtnsTlp;
        private Button btnSaveStep, btnCancelStep;

        // Controls based on StepType
        
        // OpenURL Controls
        private TextBox txtUrl;
        private ToggleSwitch tglOpenInExternBrowser;

        // OpenFolder Controls
        private TextBox txtFolderPath;
        private ToggleSwitch tglOpenInNewWindow;

        // OpenApplication Controls
        private TextBox txtAppPath;
        private TextBox txtAppArguments;
        private ToggleSwitch tglRunAsAdmin;

        // OpenDocument Controls
        private TextBox txtDocumentPath;
        private Button btnBrowseDocument;

        // Footer
        private Button btnAddStep, btnDeleteStep;
        private Label lblLastExecution;
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
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            // ========== INHALT ==========
            contentTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            contentTlp.Dock = DockStyle.Fill;
            contentTlp.ColumnStyles.Clear();
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            // ========== LINKE SEITE ==========
            leftTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 2);
            leftTlp.Dock = DockStyle.Fill;
            leftTlp.RowStyles.Clear();
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

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

            lstSteps = new StyledListBoxControl(
                displayTextMember: "Name",
                allowReorder: true,
                showEnumeration: true,
                headerTitle: "Meine Schritte",
                headerTextAlign: ContentAlignment.MiddleCenter)
            {
                Dock = DockStyle.Fill,
                ItemHeightCustom = 35,
                Visible = true
            };

            lstSteps.IsItemDisabled = item => item is RoutineStep step && !step.Show;
            lstSteps.SelectedIndexChanged += LstSteps_SelectedIndexChanged;
            lstSteps.ItemsReordered += LstSteps_ItemsReordered;

            leftTlp.Controls.Add(leftRoutineTitleTlp, 0, 0);
            leftTlp.Controls.Add(lstSteps, 0, 1);

            // ----- RECHTE SEITE -----
            rightTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 5);
            rightTlp.Dock = DockStyle.Fill;
            rightTlp.RowStyles.Clear();
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // Titel
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Basis-Tabelle
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // Optionen-Tabelle
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Füllbereich
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));   // Buttons
            rightTlp.Visible = false;

            // ========== ROW 1: TITLE ==========
            rightTitleTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            rightTitleTlp.Dock = DockStyle.Fill;
            rightTitleTlp.ColumnStyles.Clear();

            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));

            tglStepEnabled = UIStyles.ToggleSwitches.CreateStandard(
                true,
                "Schritt ist aktiviert",
                "Schritt ist deaktiviert");

            tglStepEnabled.Anchor = AnchorStyles.None;

            rightTitleTlp.Controls.Add(tglStepEnabled, 1, 0);

            // ========== EDITOR CONTROLS ==========
            lblStepNameTitle = UIStyles.Labels.CreateTitle();

            txtStepName = UIStyles.TextBoxes.CreateStandard();
            txtStepName.Dock = DockStyle.Fill;

            txtStepDescription = UIStyles.TextBoxes.CreateStandard();
            txtStepDescription.Dock = DockStyle.Fill;

            cmbStepType = UIStyles.ComboBoxes.CreateStandard(ComboBoxStyle.DropDownList);
            cmbStepType.Dock = DockStyle.Fill;

            var stepTypes = StepTypeHelper.GetStepTypeListWithEmpty();
            cmbStepType.DataSource = stepTypes;
            cmbStepType.DisplayMember = "Value";
            cmbStepType.ValueMember = "Key";
            cmbStepType.SelectedIndex = -1;
            cmbStepType.SelectedIndexChanged += CmbStepType_SelectedIndexChanged;

            tglAutoStart = UIStyles.ToggleSwitches.CreateStandard(
                true,
                "Autostart An",
                "Autostart Aus");

            tglAutoStart.Anchor = AnchorStyles.Left;

            // ========== ROW 2: BASE TABLE ==========
            editorBaseTable = new StyledPropertyTable
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };
            BuildBaseEditorTable();

            // ========== ROW 3: OPTIONS TABLE ==========
            editorOptionsTable = new StyledPropertyTable
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };

            BuildOptionsEditorTable();

            // ========== ROW 4: FILL PANEL ==========
            rightFillPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundMediumElevated,
                Margin = new Padding(0)
            };

            // ========== ROW 5: BUTTONS ==========
            rightBtnsTlp = UIStyles.TableLayoutPanels.CreateStandard(4, 1);
            rightBtnsTlp.Dock = DockStyle.Fill;
            rightBtnsTlp.ColumnStyles.Clear();

            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Spacer links
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155)); // Execute
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155)); // Save
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155)); // Cancel

            // Execute
            btnExecuteStep = UIStyles.Buttons.CreateGreen(
                "▶ Ausführen",
                "",
                new Size(155, 35));

            btnExecuteStep.Click += BtnExecuteStep_Click;

            // Save
            btnSaveStep = UIStyles.Buttons.CreatePrimary(
                "💾 Speichern",
                "",
                new Size(155, 35));

            btnSaveStep.Click += BtnSaveStep_Click;

            // Cancel
            btnCancelStep = UIStyles.Buttons.CreatePrimary(
                "✖ Abbrechen",
                "",
                new Size(155, 35));

            btnCancelStep.Click += BtnCancelStep_Click;

            // Hinzufügen
            rightBtnsTlp.Controls.Add(btnExecuteStep, 1, 0);
            rightBtnsTlp.Controls.Add(btnSaveStep, 2, 0);
            rightBtnsTlp.Controls.Add(btnCancelStep, 3, 0);

            // ========== RIGHT TLP ZUSAMMENBAU ==========
            rightTlp.Controls.Add(rightTitleTlp, 0, 0);
            rightTlp.Controls.Add(editorBaseTable, 0, 1);
            rightTlp.Controls.Add(editorOptionsTable, 0, 2);
            rightTlp.Controls.Add(rightFillPanel, 0, 3);
            rightTlp.Controls.Add(rightBtnsTlp, 0, 4);

            // ========== FOOTER ==========
            var footerPanel = UIStyles.Panels.CreateDark();
            footerPanel.Dock = DockStyle.Fill;

            var footerTlp = UIStyles.TableLayoutPanels.CreateDark(4, 1);
            footerTlp.Dock = DockStyle.Fill;
            footerTlp.Padding = new Padding(0);
            footerTlp.ColumnStyles.Clear();
            footerTlp.RowStyles.Clear();

            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); // Add
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165)); // Delete
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // LastExecution
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Back

            footerTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            btnAddStep = UIStyles.Buttons.CreatePrimary("+ Schritt hinzufügen", "", new Size(155, 35));
            btnAddStep.Dock = DockStyle.Fill;
            btnAddStep.Margin = new Padding(10, 10, 5, 10);
            btnAddStep.Click += BtnAddStep_Click;

            btnDeleteStep = UIStyles.Buttons.CreateDanger("🗑 Löschen", "", new Size(155, 35));
            btnDeleteStep.Dock = DockStyle.Fill;
            btnDeleteStep.Margin = new Padding(5, 10, 10, 10);
            btnDeleteStep.Click += BtnDeleteStep_Click;
            btnDeleteStep.Enabled = false;

            lblLastExecution = UIStyles.Labels.CreateMuted();
            lblLastExecution.Dock = DockStyle.Fill;
            lblLastExecution.Margin = new Padding(15, 0, 10, 0);
            lblLastExecution.TextAlign = ContentAlignment.MiddleLeft;

            btnBack = UIStyles.Buttons.CreateStandard("← Zurück", "", new Size(100, 35));
            btnBack.Dock = DockStyle.Fill;
            btnBack.Margin = new Padding(10, 10, 10, 10);
            btnBack.Click += BtnBack_Click;

            footerTlp.Controls.Add(btnAddStep, 0, 0);
            footerTlp.Controls.Add(btnDeleteStep, 1, 0);
            footerTlp.Controls.Add(lblLastExecution, 2, 0);
            footerTlp.Controls.Add(btnBack, 3, 0);

            footerPanel.Controls.Add(footerTlp);

            // ========== ZUSAMMENBAU ==========
            contentTlp.Controls.Add(leftTlp, 0, 0);
            contentTlp.Controls.Add(rightTlp, 1, 0);

            mainTlp.Controls.Add(contentTlp, 0, 0);
            mainTlp.Controls.Add(footerPanel, 0, 1);

            this.Controls.Add(mainTlp);

            SetEditorEnabled(false);
        }

        private void BuildBaseEditorTable()
        {
            if (editorBaseTable == null)
                return;

            editorBaseTable.ClearRows();

            editorBaseTable.AddSection(lblStepNameTitle.Text);
            editorBaseTable.AddRow("Name", txtStepName);
            editorBaseTable.AddRow("Beschreibung", txtStepDescription);
            editorBaseTable.AddRow("Aktion", cmbStepType);
            editorBaseTable.AddRow("Autostart", tglAutoStart);
        }

        private void BuildOptionsEditorTable()
        {
            if (editorOptionsTable == null)
                return;

            editorOptionsTable.ClearRows();

            if (!(cmbStepType.SelectedValue is StepType selectedType))
                return;

            editorOptionsTable.AddSection("Optionen");

            switch (selectedType)
            {
                case StepType.OpenUrl:
                    CreateOpenUrlControls();
                    break;

                case StepType.OpenFolder:
                    CreateOpenFolderControls();
                    break;

                case StepType.OpenApplication:
                    CreateOpenApplicationControls();
                    break;

                case StepType.OpenDocument:
                    CreateOpenDocumentControls();
                    break;
            }
        }


        // ========== PUBLIC METHODS ==========
        public void LoadRoutine(Routine routine, bool isNewRoutine = false)
        {
            _originalRoutine = DeepCopy(routine ?? new Routine());
            _currentRoutine = DeepCopy(routine ?? new Routine());

            txtRoutineName.Text = _currentRoutine.Name;
            lblLastExecution.Text =
                _currentRoutine.LastExecutionAt.HasValue
                    ? $"Zuletzt gestartet: {DateTimeHelper.GetRelativeTime(_currentRoutine.LastExecutionAt)}"
                    : "";

            lstSteps.SelectedIndex = -1;
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = false;

            RefreshStepsList(silent: true);
            ClearEditor();

            if (_currentRoutine.Steps.Count == 0)
                rightTlp.Visible = false;
            
            if (isNewRoutine)
            {
                BeginInvoke(new Action(() =>
                {
                    txtRoutineName.Focus();
                    txtRoutineName.SelectAll();
                }));
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

            var copy = new Routine
            {
                Id = original.Id,
                Name = original.Name,
                Order = original.Order,
                CreatedAt = original.CreatedAt,
                UpdatedAt = original.UpdatedAt,
                LastExecutionAt = original.LastExecutionAt,
                Steps = new List<RoutineStep>()
            };

            foreach (var step in original.Steps)
            {
                switch (step)
                {
                    case OpenUrlStep urlStep:
                        copy.Steps.Add(new OpenUrlStep
                        {
                            Id = urlStep.Id,
                            Order = urlStep.Order,
                            Name = urlStep.Name,
                            Description = urlStep.Description,
                            Show = urlStep.Show,
                            AutoStart = urlStep.AutoStart,
                            Url = urlStep.Url,
                            OpenInExternalBrowser = urlStep.OpenInExternalBrowser
                        });
                        break;
                    case OpenFolderStep folderStep:
                        copy.Steps.Add(new OpenFolderStep
                        {
                            Id = folderStep.Id,
                            Order = folderStep.Order,
                            Name = folderStep.Name,
                            Description = folderStep.Description,
                            Show = folderStep.Show,
                            AutoStart = folderStep.AutoStart,
                            FolderPath = folderStep.FolderPath,
                            OpenInNewWindow = folderStep.OpenInNewWindow
                        });
                        break;
                    case OpenApplicationStep appStep:
                        copy.Steps.Add(new OpenApplicationStep
                        {
                            Id = appStep.Id,
                            Order = appStep.Order,
                            Name = appStep.Name,
                            Description = appStep.Description,
                            Show = appStep.Show,
                            AutoStart = appStep.AutoStart,
                            ApplicationPath = appStep.ApplicationPath,
                            Arguments = appStep.Arguments,
                            RunAsAdmin = appStep.RunAsAdmin,
                            WorkingDirectory = appStep.WorkingDirectory
                        });
                        break;
                    case OpenDocumentStep docStep:
                        copy.Steps.Add(new OpenDocumentStep
                        {
                            Id = docStep.Id,
                            Order = docStep.Order,
                            Name = docStep.Name,
                            Description = docStep.Description,
                            Show = docStep.Show,
                            AutoStart = docStep.AutoStart,
                            FilePath = docStep.FilePath,
                            OpenWithAssociatedApp = docStep.OpenWithAssociatedApp
                        });
                        break;
                }
            }

            return copy;
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

                if (orig.Order != curr.Order || orig.Name != curr.Name || orig.Description != curr.Description || orig.AutoStart != curr.AutoStart)
                    return true;

                // Step-spezifische Vergleiche
                switch (orig)
                {
                    case OpenUrlStep origUrl when curr is OpenUrlStep currUrl:
                        if (origUrl.Url != currUrl.Url || origUrl.OpenInExternalBrowser != currUrl.OpenInExternalBrowser)
                            return true;
                        break;
                    case OpenFolderStep origFolder when curr is OpenFolderStep currFolder:
                        if (origFolder.FolderPath != currFolder.FolderPath || origFolder.OpenInNewWindow != currFolder.OpenInNewWindow)
                            return true;
                        break;
                    case OpenApplicationStep origApp when curr is OpenApplicationStep currApp:
                        if (origApp.ApplicationPath != currApp.ApplicationPath ||
                            origApp.Arguments != currApp.Arguments ||
                            origApp.RunAsAdmin != currApp.RunAsAdmin)
                            return true;
                        break;
                    case OpenDocumentStep origDoc when curr is OpenDocumentStep currDoc:
                        if (origDoc.FilePath != currDoc.FilePath)
                            return true;
                        break;
                }
            }

            return false;
        }
        
        private void OpenEditorForStep(RoutineStep step = null)
        {
            CloseEditor();
            if (step != null)
            {
                LoadStepToEditor(step);
            }
            else
            {
                ClearEditor();
            }

            rightTlp.Visible = true;
            SetEditorEnabled(true);
        }


        private void CloseEditor()
        {
            rightTlp.Visible = false;
            _editingStep = null;
        }

        private void ClearEditor()
        {
            txtStepName.Text = "";
            txtStepDescription.Text = "";
            tglStepEnabled.Checked = true;
            tglAutoStart.Checked = true;

            BeginInvoke(new Action(() =>
            {
                cmbStepType.SelectedIndex = -1;
            }));

            lblStepNameTitle.Text = "Neuen Schritt erstellen";
            BuildBaseEditorTable();
            BuildOptionsEditorTable();

            _editingStep = null;
            SetEditorEnabled(false);
        }

        private void LoadStepToEditor(RoutineStep step)
        {
            lblStepNameTitle.Text = $"Schritt bearbeiten";
            BuildBaseEditorTable();
            txtStepName.Text = step.Name;
            txtStepDescription.Text = step.Description;
            tglStepEnabled.Checked = step.Show;
            tglAutoStart.Checked = step.AutoStart;

            // StepType auswählen
            for (int i = 0; i < cmbStepType.Items.Count; i++)
            {
                var item = (KeyValuePair<StepType, string>)cmbStepType.Items[i];
                if (item.Key == step.Type)
                {
                    cmbStepType.SelectedIndex = i;
                    break;
                }
            }
            BuildOptionsEditorTable();

            // Step-spezifische Werte laden
            switch (step)
            {
                case OpenUrlStep urlStep:
                    if (txtUrl != null) txtUrl.Text = urlStep.Url;
                    if (tglOpenInExternBrowser != null) tglOpenInExternBrowser.Checked = urlStep.OpenInExternalBrowser;
                    break;
                case OpenFolderStep folderStep:
                    if (txtFolderPath != null) txtFolderPath.Text = folderStep.FolderPath;
                    if (tglOpenInNewWindow != null) tglOpenInNewWindow.Checked = folderStep.OpenInNewWindow;
                    break;
                case OpenApplicationStep appStep:
                    if (txtAppPath != null) txtAppPath.Text = appStep.ApplicationPath;
                    if (txtAppArguments != null) txtAppArguments.Text = appStep.Arguments;
                    if (tglRunAsAdmin != null) tglRunAsAdmin.Checked = appStep.RunAsAdmin;
                    break;
                case OpenDocumentStep docStep:
                    if (txtDocumentPath != null) txtDocumentPath.Text = docStep.FilePath;
                    break;
            }

            _editingStep = step;
            SetEditorEnabled(true);

            this.BeginInvoke(new Action(() =>
            {
                cmbStepType.PerformLayout();
                cmbStepType.Invalidate();
                cmbStepType.Update();
            }));
        }

        private void SetEditorEnabled(bool enabled)
        {
            if (rightTitleTlp != null)
                rightTitleTlp.Enabled = enabled;

            if (editorBaseTable != null)
                editorBaseTable.Enabled = enabled;

            if (editorOptionsTable != null)
                editorOptionsTable.Enabled = enabled;

            if (rightBtnsTlp != null)
                rightBtnsTlp.Enabled = enabled;
        }

        // ========== STEP EVENT HANDLER ==========
        private void LstSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isRefreshing) return;

            if (!(lstSteps.SelectedItem is RoutineStep step))
            {
                rightTlp.Visible = false;
                ClearEditor();
                btnDeleteStep.Enabled = false;
                return;
            }

            CloseEditor();
            OpenEditorForStep(step);
            btnDeleteStep.Enabled = true;
        }

        private void LstSteps_ItemsReordered(object sender, EventArgs e)
        {
            if (_isRefreshing) return;

            var reorderedSteps = lstSteps.Items.Cast<RoutineStep>().ToList();

            for (int i = 0; i < reorderedSteps.Count; i++)
            {
                reorderedSteps[i].Order = i;
            }
            _currentRoutine.Steps = reorderedSteps;
            _routineService.SaveRoutine(_currentRoutine);
            lstSteps.Invalidate();
            lstSteps.Update();
        }
        private void BtnAddStep_Click(object sender, EventArgs e)
        {
            lstSteps.ClearSelected();
            btnDeleteStep.Enabled = false;
            CloseEditor();
            OpenEditorForStep(null);
            txtStepName.Focus();
        }

        private void BtnDeleteStep_Click(object sender, EventArgs e)
        {
            if (!(lstSteps.SelectedItem is RoutineStep stepToDelete)) return;

            bool shouldDelete = AutoConfirmDialogs;

            if (!shouldDelete)
            {
                shouldDelete = CustomMessageBox.Show(
                    $"Schritt '{stepToDelete.Name}' wirklich löschen?",
                    "Bestätigen",
                    CustomMessageBoxButtons.YesNo,
                    CustomMessageBoxIcon.Question,
                    FindForm()) == DialogResult.Yes;
            }

            if (!shouldDelete) return;

            _routineService.RemoveStep(_currentRoutine.Id, stepToDelete.Id);
            _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

            RefreshStepsList(silent: true);

            lstSteps.ClearSelected();
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = false;

            SaveChanges?.Invoke(this, _currentRoutine);

            var parentForm = this.FindForm();
            ToastForm.ShowToast($"✓ Schritt '{stepToDelete.Name}' gelöscht", parentForm);
        }


        // ========== STEP LIST METHODS ==========
        private void RefreshStepsList(bool silent = false)
        {
            if (_currentRoutine == null) return;

            _isRefreshing = true;
            lstSteps.BeginUpdate();
            lstSteps.Items.Clear();

            // Stelle sicher, dass die Steps aus der aktuellen Routine kommen
            var orderedSteps = _currentRoutine.Steps.OrderBy(s => s.Order).ToList();

            Debug.WriteLine($"RefreshStepsList: {orderedSteps.Count} Schritte werden geladen");
            foreach (var step in orderedSteps)
            {
                Debug.WriteLine($"  - {step.Name} (Order: {step.Order})");
                lstSteps.Items.Add(step);
            }

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
        private void BtnExecuteStep_Click(object sender, EventArgs e)
        {
            if (_editingStep == null)
            {
                var result = AskToSaveChanges();

                if (result == DialogResult.Yes)
                {
                    if (!ValidateCurrentStep(true)) return;

                    SaveCurrentStep(refreshList: true);

                    if (_editingStep == null) return;
                }
                else
                {
                    return;
                }
            }
            else if (HasUnsavedChanges())
            {
                var result = AskToSaveChanges();

                if (result == DialogResult.Yes)
                {
                    if (!ValidateCurrentStep(true)) return;
                    SaveCurrentStep(refreshList: false);
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            bool shouldOpenExecutionForm =
                _editingStep is OpenUrlStep urlStep && !urlStep.OpenInExternalBrowser;

            if (shouldOpenExecutionForm)
            {
                var executionForm = new ExecutionForm(_currentRoutine, _editingStep, step =>
                {
                    return _routineService.ExecuteStep(step);
                });

                executionForm.ShowDialog(this);
            }
            else
            {
                _routineService.ExecuteStep(_editingStep);
            }
        }

        private bool HasUnsavedChanges()
        {
            if (_editingStep == null) return false;

            if (_editingStep.Name != txtStepName.Text.Trim()) return true;
            if (_editingStep.Description != txtStepDescription.Text.Trim()) return true;
            if (_editingStep.Show != tglStepEnabled.Checked) return true;
            if (_editingStep.AutoStart != tglAutoStart.Checked) return true;

            var currentType = (StepType)cmbStepType.SelectedValue;
            if (_editingStep.Type != currentType) return true;

            switch (_editingStep)
            {
                case OpenUrlStep urlStep:
                    if (urlStep.Url != txtUrl?.Text) return true;
                    if (urlStep.OpenInExternalBrowser != tglOpenInExternBrowser?.Checked) return true;
                    break;
                case OpenFolderStep folderStep:
                    if (folderStep.FolderPath != txtFolderPath?.Text) return true;
                    if (folderStep.OpenInNewWindow != tglOpenInNewWindow?.Checked) return true;
                    break;
                case OpenApplicationStep appStep:
                    if (appStep.ApplicationPath != txtAppPath?.Text) return true;
                    if (appStep.Arguments != txtAppArguments?.Text) return true;
                    if (appStep.RunAsAdmin != tglRunAsAdmin?.Checked) return true;
                    break;
                case OpenDocumentStep docStep:
                    if (docStep.FilePath != txtDocumentPath?.Text) return true;
                    break;
            }

            return false;
        }

        private DialogResult AskToSaveChanges()
        {
            return CustomMessageBox.Show(
                "Möchten Sie die Änderungen vor der Ausführung speichern?",
                "Änderungen speichern",
                CustomMessageBoxButtons.YesNoCancel,
                CustomMessageBoxIcon.Question,
                FindForm());
        }

        private void CmbStepType_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildOptionsEditorTable();
        }

        private void CreateOpenUrlControls()
        {
            txtUrl = UIStyles.TextBoxes.CreateStandard();
            txtUrl.Name = "txtUrl";

            txtUrl.TextChanged += TxtUrl_TextChanged;
            txtUrl.LostFocus += TxtUrl_LostFocus;

            DragDropHelper.EnableTextDragDrop(txtUrl, droppedText =>
            {
                string cleanedText = droppedText.Trim();

                txtUrl.Text = cleanedText;
                TxtUrl_TextChanged(txtUrl, EventArgs.Empty);
                txtUrl.SelectionStart = txtUrl.Text.Length;
            });

            tglOpenInExternBrowser = UIStyles.ToggleSwitches.CreateStandard(
                false,
                "Externer Browser",
                "In App öffnen");

            tglOpenInExternBrowser.Name = "tglOpenInternally";
            tglOpenInExternBrowser.Anchor = AnchorStyles.Left;

            editorOptionsTable.AddRow("URL", txtUrl);
            editorOptionsTable.AddRow("Öffnen in", tglOpenInExternBrowser);
        }

        private void CreateOpenFolderControls()
        {
            txtFolderPath = UIStyles.TextBoxes.CreateStandard();
            txtFolderPath.Name = "txtFolderPath";

            var folderPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            folderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            folderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));

            txtFolderPath.Dock = DockStyle.Fill;

            var btnBrowse = UIStyles.Buttons.CreateStandard("Durchsuchen...");
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.Margin = new Padding(5, 0, 0, 0);

            btnBrowse.Click += (s, e) =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                        txtFolderPath.Text = dialog.SelectedPath;
                }
            };

            folderPanel.Controls.Add(txtFolderPath, 0, 0);
            folderPanel.Controls.Add(btnBrowse, 1, 0);

            tglOpenInNewWindow = UIStyles.ToggleSwitches.CreateStandard(false, "Ja", "Nein");
            tglOpenInNewWindow.Name = "tglOpenInNewWindow";
            tglOpenInNewWindow.Anchor = AnchorStyles.Left;

            editorOptionsTable.AddRow("Ordnerpfad", folderPanel);
            editorOptionsTable.AddRow("Neues Fenster", tglOpenInNewWindow);
        }


        private void CreateOpenApplicationControls()
        {
            txtAppPath = UIStyles.TextBoxes.CreateStandard();
            txtAppPath.Name = "txtAppPath";

            var appPathPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            appPathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            appPathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));

            txtAppPath.Dock = DockStyle.Fill;

            var btnBrowse = UIStyles.Buttons.CreateStandard("Durchsuchen...");
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.Margin = new Padding(5, 0, 0, 0);

            btnBrowse.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Anwendungen (*.exe)|*.exe|Alle Dateien (*.*)|*.*";

                    if (dialog.ShowDialog() == DialogResult.OK)
                        txtAppPath.Text = dialog.FileName;
                }
            };

            appPathPanel.Controls.Add(txtAppPath, 0, 0);
            appPathPanel.Controls.Add(btnBrowse, 1, 0);

            txtAppArguments = UIStyles.TextBoxes.CreateStandard();
            txtAppArguments.Name = "txtAppArguments";

            tglRunAsAdmin = UIStyles.ToggleSwitches.CreateStandard(false, "Ja", "Nein");
            tglRunAsAdmin.Name = "tglRunAsAdmin";
            tglRunAsAdmin.Anchor = AnchorStyles.Left;

            editorOptionsTable.AddRow("Programmpfad", appPathPanel);
            editorOptionsTable.AddRow("Argumente", txtAppArguments);
            editorOptionsTable.AddRow("Als Admin", tglRunAsAdmin);
        }

        private void CreateOpenDocumentControls()
        {
            txtDocumentPath = UIStyles.TextBoxes.CreateStandard();
            txtDocumentPath.Name = "txtDocumentPath";

            var documentPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            documentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            documentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));

            txtDocumentPath.Dock = DockStyle.Fill;

            btnBrowseDocument = UIStyles.Buttons.CreateStandard("Durchsuchen...");
            btnBrowseDocument.Dock = DockStyle.Fill;
            btnBrowseDocument.Margin = new Padding(5, 0, 0, 0);

            btnBrowseDocument.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Alle Dateien (*.*)|*.*";
                    dialog.Title = "Dokument auswählen";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        txtDocumentPath.Text = dialog.FileName;
                    }
                }
            };

            documentPanel.Controls.Add(txtDocumentPath, 0, 0);
            documentPanel.Controls.Add(btnBrowseDocument, 1, 0);

            editorOptionsTable.AddRow("Dateipfad", documentPanel);
        }
        // ========== BACK BUTTON ==========
        private void BtnBack_Click(object sender, EventArgs e)
        {
            CloseEditor();

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
                if (!AutoConfirmDialogs)
                {
                    CustomMessageBox.Show(
                        "Bitte geben Sie einen Namen für die Routine ein.",
                        "Validierung",
                        CustomMessageBoxButtons.OK,
                        CustomMessageBoxIcon.Warning,
                        FindForm());
                }

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
                if (!AutoConfirmDialogs)
                {
                    CustomMessageBox.Show(
                        $"Eine Routine mit dem Namen '{routineName}' existiert bereits.\nBitte wählen Sie einen anderen Namen.",
                        "Validierung",
                        CustomMessageBoxButtons.OK,
                        CustomMessageBoxIcon.Warning,
                        FindForm());
                }

                txtRoutineName.Focus();
                txtRoutineName.SelectAll();
                return false;
            }

            return true;
        }
        private bool ValidateCurrentStep(bool showMessageBox = true)
        {
            if (string.IsNullOrWhiteSpace(txtStepName.Text))
            {
                if (showMessageBox && !AutoConfirmDialogs)
                {
                    CustomMessageBox.Show(
                        "Bitte geben Sie einen Namen für den Schritt ein.",
                        "Validierung",
                        CustomMessageBoxButtons.OK,
                        CustomMessageBoxIcon.Warning,
                        FindForm());
                }

                txtStepName.Focus();
                return false;
            }

            if (cmbStepType.SelectedIndex == -1)
            {
                if (showMessageBox && !AutoConfirmDialogs)
                {
                    CustomMessageBox.Show(
                        "Bitte wählen Sie einen Aktionstyp aus.",
                        "Validierung",
                        CustomMessageBoxButtons.OK,
                        CustomMessageBoxIcon.Warning,
                        FindForm());
                }

                cmbStepType.Focus();
                return false;
            }

            var selectedType = (StepType)cmbStepType.SelectedValue;

            switch (selectedType)
            {
                case StepType.OpenUrl:
                    if (string.IsNullOrWhiteSpace(txtUrl?.Text))
                    {
                        if (showMessageBox && !AutoConfirmDialogs)
                        {
                            CustomMessageBox.Show(
                                "Bitte geben Sie eine URL ein.",
                                "Validierung",
                                CustomMessageBoxButtons.OK,
                                CustomMessageBoxIcon.Warning,
                                FindForm());
                        }

                        txtUrl?.Focus();
                        return false;
                    }

                    var urlResult = _urlValidationService.ValidateAndRepairUrl(txtUrl.Text, false);

                    if (!urlResult.IsValid)
                    {
                        if (showMessageBox && !AutoConfirmDialogs)
                        {
                            CustomMessageBox.Show(
                                urlResult.ErrorMessage,
                                "Ungültige URL",
                                CustomMessageBoxButtons.OK,
                                CustomMessageBoxIcon.Warning,
                                FindForm());
                        }

                        txtUrl.Focus();
                        return false;
                    }
                    break;

                case StepType.OpenFolder:
                    if (string.IsNullOrWhiteSpace(txtFolderPath?.Text))
                    {
                        if (showMessageBox && !AutoConfirmDialogs)
                        {
                            CustomMessageBox.Show(
                                "Bitte geben Sie einen Ordnerpfad ein.",
                                "Validierung",
                                CustomMessageBoxButtons.OK,
                                CustomMessageBoxIcon.Warning,
                                FindForm());
                        }

                        txtFolderPath?.Focus();
                        return false;
                    }
                    break;

                case StepType.OpenApplication:
                    if (string.IsNullOrWhiteSpace(txtAppPath?.Text))
                    {
                        if (showMessageBox && !AutoConfirmDialogs)
                        {
                            CustomMessageBox.Show(
                                "Bitte geben Sie einen Programmpfad ein.",
                                "Validierung",
                                CustomMessageBoxButtons.OK,
                                CustomMessageBoxIcon.Warning,
                                FindForm());
                        }

                        txtAppPath?.Focus();
                        return false;
                    }
                    break;

                case StepType.OpenDocument:
                    if (string.IsNullOrWhiteSpace(txtDocumentPath?.Text))
                    {
                        if (showMessageBox && !AutoConfirmDialogs)
                        {
                            CustomMessageBox.Show(
                                "Bitte wählen Sie eine Datei aus.",
                                "Validierung",
                                CustomMessageBoxButtons.OK,
                                CustomMessageBoxIcon.Warning,
                                FindForm());
                        }

                        txtDocumentPath?.Focus();
                        return false;
                    }

                    if (!System.IO.File.Exists(txtDocumentPath?.Text))
                    {
                        if (showMessageBox && !AutoConfirmDialogs)
                        {
                            CustomMessageBox.Show(
                                "Die ausgewählte Datei existiert nicht.",
                                "Validierung",
                                CustomMessageBoxButtons.OK,
                                CustomMessageBoxIcon.Warning,
                                FindForm());
                        }

                        txtDocumentPath?.Focus();
                        return false;
                    }
                    break;
            }

            return true;
        }
        private void SaveCurrentStep(bool refreshList = true)
        {
            if (!ValidateCurrentStep(false)) return;

            string name = txtStepName.Text.Trim();
            string description = txtStepDescription.Text.Trim();
            bool show = tglStepEnabled.Checked;

            if (cmbStepType.SelectedIndex == -1) return;

            var selectedType = (StepType)cmbStepType.SelectedValue;
            RoutineStep step;

            switch (selectedType)
            {
                case StepType.OpenUrl:
                    var urlResult = _urlValidationService.ValidateAndRepairUrl(txtUrl?.Text, false);

                    if (!urlResult.IsValid)
                        return;

                    step = new OpenUrlStep
                    {
                        Url = urlResult.RepairedUrl,
                        OpenInExternalBrowser = tglOpenInExternBrowser?.Checked ?? true
                    };
                    break;
                case StepType.OpenFolder:
                    step = new OpenFolderStep
                    {
                        FolderPath = txtFolderPath?.Text ?? "",
                        OpenInNewWindow = tglOpenInNewWindow?.Checked ?? true
                    };
                    break;
                case StepType.OpenApplication:
                    step = new OpenApplicationStep
                    {
                        ApplicationPath = txtAppPath?.Text ?? "",
                        Arguments = txtAppArguments?.Text ?? "",
                        RunAsAdmin = tglRunAsAdmin?.Checked ?? false
                    };
                    break;
                case StepType.OpenDocument:
                    step = new OpenDocumentStep
                    {
                        FilePath = txtDocumentPath?.Text ?? "",
                        OpenWithAssociatedApp = true
                    };
                    break;
                default:
                    throw new NotSupportedException();
            }

            step.Name = name;
            step.Description = description;
            step.Show = show;
            step.AutoStart = tglAutoStart.Checked;

            if (_editingStep != null)
            {
                step.Id = _editingStep.Id;
                step.Order = _editingStep.Order;
                _routineService.UpdateStep(_currentRoutine.Id, step);
                _editingStep = step;
                _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);
            }
            else
            {
                _routineService.AddStep(_currentRoutine.Id, step);
                _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

                if (_originalRoutine != null)
                    _currentRoutine.Order = _originalRoutine.Order;

                _editingStep = _currentRoutine.Steps.LastOrDefault();
            }

            if (refreshList)
            {
                RefreshStepsList(silent: false);
                var parentForm = this.FindForm();
                ToastForm.ShowToast($"✓ Schritt '{step.Name}' gespeichert", parentForm);
            }
        }
        private void SaveCurrentRoutine()
        {
            _currentRoutine.Name = txtRoutineName.Text.Trim();
            _currentRoutine.UpdatedAt = DateTime.Now;
            _routineService.SaveRoutine(_currentRoutine);
        }
    }
}