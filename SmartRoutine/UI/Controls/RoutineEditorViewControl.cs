using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Forms;
//using SmartRoutine.UI.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CustomWFUI;
using CustomWFUI.Controls;
using SmartRoutine.UI.Helpers;
using CustomWFUI.Forms;
using CustomWFUI.Helpers;

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
        private bool _isLoadingStep = false;

        // UI Controls
        private TableLayoutPanel mainTlp;

        // Inhalt (links + rechts)
        private TableLayoutPanel contentTlp;

        // Linke Seite
        private TableLayoutPanel leftTlp;
        private StyledPropertyTable routineInfoTable;
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
            this.BackColor = UIStyles.Colors.BackgroundDark;

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
            InitializeMainLayout();
            InitializeLeftPanel();
            InitializeRightPanel();
            InitializeFooter();
            BuildMainLayout();

            SetEditorEnabled(false);
        }
        private void InitializeMainLayout()
        {
            mainTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 2);
            mainTlp.Dock = DockStyle.Fill;

            mainTlp.RowStyles.Clear();
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            contentTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            contentTlp.Dock = DockStyle.Fill;

            contentTlp.ColumnStyles.Clear();
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        }
        private void InitializeLeftPanel()
        {
            leftTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 2);
            leftTlp.Dock = DockStyle.Fill;

            leftTlp.RowStyles.Clear();
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            routineInfoTable = new StyledPropertyTable();

            txtRoutineName = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtRoutineName.Dock = DockStyle.Fill;
            txtRoutineName.MaxLength = 30;

            routineInfoTable.AddRow(
                "Routinenname",
                txtRoutineName);

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
            lstSteps.IconProvider = GetStepIcon;

            leftTlp.Controls.Add(routineInfoTable, 0, 0);
            leftTlp.Controls.Add(lstSteps, 0, 1);
        }
        private void InitializeRightPanel()
        {
            rightTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 5);
            rightTlp.Dock = DockStyle.Fill;
            rightTlp.Margin = new Padding(8, 0, 0, 0);
            rightTlp.Visible = false;

            rightTlp.RowStyles.Clear();
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            InitializeRightTitlePanel();
            InitializeEditorControls();
            InitializeEditorTables();
            InitializeRightFillPanel();
            InitializeRightButtons();

            rightTlp.Controls.Add(rightTitleTlp, 0, 0);
            rightTlp.Controls.Add(editorBaseTable, 0, 1);
            rightTlp.Controls.Add(editorOptionsTable, 0, 2);
            rightTlp.Controls.Add(rightFillPanel, 0, 3);
            rightTlp.Controls.Add(rightBtnsTlp, 0, 4);
        }
        private void InitializeRightTitlePanel()
        {
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
        }

        private void InitializeEditorControls()
        {
            lblStepNameTitle = UIStyles.Labels.CreateTitle();

            txtStepName = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtStepName.Dock = DockStyle.Fill;
            txtStepName.MaxLength = 40;

            txtStepDescription = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtStepDescription.Dock = DockStyle.Fill;

            cmbStepType = UIStyles.ComboBoxes.CreateStandard(ComboBoxStyle.DropDownList);
            cmbStepType.Dock = DockStyle.Fill;

            cmbStepType.DisplayMember = "DisplayName";
            cmbStepType.ValueMember = "Type";
            cmbStepType.DataSource = StepTypeHelper.GetStepTypeOptions();

            cmbStepType.SelectedIndex = -1;
            cmbStepType.SelectedIndexChanged += CmbStepType_SelectedIndexChanged;

            tglAutoStart = UIStyles.ToggleSwitches.CreateStandard(
                true,
                "Autostart An",
                "Autostart Aus");

            tglAutoStart.Anchor = AnchorStyles.Left;
        }

        private void InitializeEditorTables()
        {
            editorBaseTable = new StyledPropertyTable
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            BuildBaseEditorTable();

            editorOptionsTable = new StyledPropertyTable
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            BuildOptionsEditorTable();
        }
        private void InitializeRightFillPanel()
        {
            rightFillPanel = UIStyles.Panels.CreateElevated();
            rightFillPanel.Dock = DockStyle.Fill;
            rightFillPanel.Margin = new Padding(0);
        }
        private void InitializeRightButtons()
        {
            rightBtnsTlp = UIStyles.TableLayoutPanels.CreateStandard(4, 1);
            rightBtnsTlp.Dock = DockStyle.Fill;
            rightBtnsTlp.Margin = new Padding(5);
            rightBtnsTlp.Padding = new Padding(0);

            rightBtnsTlp.ColumnStyles.Clear();
            rightBtnsTlp.RowStyles.Clear();

            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            rightBtnsTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            btnExecuteStep = UIStyles.Buttons.CreateGreen("▶", "Ausführen", new Size(100, 35), true);
            btnExecuteStep.Dock = DockStyle.Fill;
            btnExecuteStep.Margin = new Padding(5);
            btnExecuteStep.Click += BtnExecuteStep_Click;

            btnSaveStep = UIStyles.Buttons.CreatePrimary("💾", "Speichern", new Size(100, 35), true);
            btnSaveStep.Dock = DockStyle.Fill;
            btnSaveStep.Margin = new Padding(5);
            btnSaveStep.Click += BtnSaveStep_Click;

            btnCancelStep = UIStyles.Buttons.CreatePrimary("✖", "Abbrechen", new Size(100, 35), true);
            btnCancelStep.Dock = DockStyle.Fill;
            btnCancelStep.Margin = new Padding(5);
            btnCancelStep.Click += BtnCancelStep_Click;

            rightBtnsTlp.Controls.Add(btnExecuteStep, 1, 0);
            rightBtnsTlp.Controls.Add(btnSaveStep, 2, 0);
            rightBtnsTlp.Controls.Add(btnCancelStep, 3, 0);
        }
        private void InitializeFooter()
        {
            var footerPanel = UIStyles.Panels.CreateDark();
            footerPanel.Dock = DockStyle.Fill;

            var footerTlp = UIStyles.TableLayoutPanels.CreateDark(4, 1);
            footerTlp.BackColor = UIStyles.Colors.BackgroundDarkElevated;
            footerTlp.Dock = DockStyle.Fill;
            footerTlp.Padding = new Padding(0);

            footerTlp.ColumnStyles.Clear();
            footerTlp.RowStyles.Clear();

            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            footerTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            InitializeFooterButtons();

            lblLastExecution = UIStyles.Labels.CreateMuted();
            lblLastExecution.Dock = DockStyle.Fill;
            lblLastExecution.Margin = new Padding(15, 0, 10, 0);
            lblLastExecution.TextAlign = ContentAlignment.MiddleLeft;

            footerTlp.Controls.Add(btnAddStep, 0, 0);
            footerTlp.Controls.Add(btnDeleteStep, 1, 0);
            footerTlp.Controls.Add(lblLastExecution, 2, 0);
            footerTlp.Controls.Add(btnBack, 3, 0);

            footerPanel.Controls.Add(footerTlp);

            mainTlp.Controls.Add(footerPanel, 0, 1);
        }
        private void InitializeFooterButtons()
        {
            btnAddStep = UIStyles.Buttons.CreateGreen(
                "+",
                "Schritt hinzufügen",
                new Size(155, 35),
                true);

            btnAddStep.Dock = DockStyle.Fill;
            btnAddStep.Margin = new Padding(10, 10, 5, 10);
            btnAddStep.Click += BtnAddStep_Click;

            btnDeleteStep = UIStyles.Buttons.CreateDanger(
                "🗑",
                "Schritt löschen",
                new Size(155, 35),
                true);

            btnDeleteStep.Dock = DockStyle.Fill;
            btnDeleteStep.Margin = new Padding(5, 10, 10, 10);
            btnDeleteStep.Enabled = false;
            btnDeleteStep.Click += BtnDeleteStep_Click;

            btnBack = UIStyles.Buttons.CreateStandard(
                "←",
                "Zurück zur Hauptansicht",
                new Size(100, 35),
                true);

            btnBack.Dock = DockStyle.Fill;
            btnBack.Margin = new Padding(10);
            btnBack.Click += BtnBack_Click;
        }

        private void BuildMainLayout()
        {
            contentTlp.Controls.Add(leftTlp, 0, 0);
            contentTlp.Controls.Add(rightTlp, 1, 0);

            mainTlp.Controls.Add(contentTlp, 0, 0);

            Controls.Add(mainTlp);
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

            StepType selectedType;

            if (!TryGetSelectedStepType(out selectedType))
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

            editorOptionsTable.Visible = true;
            editorOptionsTable.PerformLayout();
            editorOptionsTable.Refresh();

            rightTlp.PerformLayout();
            rightTlp.Refresh();
        }

        private bool TryGetSelectedStepType(out StepType selectedType)
        {
            selectedType = default(StepType);

            if (cmbStepType.SelectedValue is StepType value)
            {
                selectedType = value;
                return true;
            }

            if (cmbStepType.SelectedItem is StepTypeOption option)
            {
                selectedType = option.Type;
                return true;
            }

            return false;
        }

        private Image GetStepIcon(object item)
        {
            if (item is OpenUrlStep)
                return Properties.Resources.IconWeb;

            if (item is OpenFolderStep)
                return Properties.Resources.IconFolder;

            if (item is OpenDocumentStep)
                return Properties.Resources.IconDocument;

            if (item is OpenApplicationStep)
                return Properties.Resources.IconApplication;

            return null;
        }

        // ========== PUBLIC METHODS ==========
        public void LoadRoutine(Routine routine, bool isNewRoutine = false)
        {
            _originalRoutine = RoutineCloneService.DeepCopy(routine ?? new Routine());
            _currentRoutine = RoutineCloneService.DeepCopy(routine ?? new Routine());

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


        private bool HasChanges()
        {
            if (_originalRoutine == null)
                return _currentRoutine != null;

            _currentRoutine.Name =
                txtRoutineName.Text.Trim();

            return ChangeDetector.HasChanges(
                _originalRoutine,
                _currentRoutine,
                "UpdatedAt",
                "LastExecutionAt");
        }

        private void OpenEditorForStep(RoutineStep step = null)
        {
            CloseEditor();
            rightTlp.Visible = true;

            if (step != null)
            {
                LoadStepToEditor(step);
            }
            else
            {
                ClearEditor();
            }

            SetEditorEnabled(true);
        }


        private void CloseEditor()
        {
            Debug.WriteLine("ClearEditor");
            rightTlp.Visible = false;
            _editingStep = null;
        }
        private void CloseStepEditor()
        {
            Debug.WriteLine("CloseStepEditor");
            lstSteps.SelectedIndex = -1;
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = false;
        }
        private void ClearEditor()
        {
            txtStepName.Text = "";
            txtStepDescription.Text = "";
            tglStepEnabled.Checked = true;
            tglAutoStart.Checked = true;

            cmbStepType.SelectedIndex = -1;

            lblStepNameTitle.Text = "Neuen Schritt erstellen";
            BuildBaseEditorTable();
            BuildOptionsEditorTable();

            _editingStep = null;
            SetEditorEnabled(false);
        }

        private void LoadStepToEditor(RoutineStep step)
        {
            _isLoadingStep = true;

            try
            {
                lblStepNameTitle.Text = "Schritt bearbeiten";

                txtStepName.Text = step.Name;
                txtStepDescription.Text = step.Description;
                tglStepEnabled.Checked = step.Show;
                tglAutoStart.Checked = step.AutoStart;

                BuildBaseEditorTable();

                SetSelectedStepType(step.Type);

                BuildOptionsEditorTable();

                editorBaseTable.PerformLayout();
                editorBaseTable.Refresh();

                cmbStepType.PerformLayout();
                cmbStepType.Refresh();

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

                editorOptionsTable.Visible = true;
                editorOptionsTable.BringToFront();

                _editingStep = step;
                SetEditorEnabled(true);
            }
            finally
            {
                _isLoadingStep = false;
            }
        }
        private void SetSelectedStepType(StepType stepType)
        {
            for (int i = 0; i < cmbStepType.Items.Count; i++)
            {
                StepTypeOption option = cmbStepType.Items[i] as StepTypeOption;

                if (option != null && option.Type == stepType)
                {
                    cmbStepType.SelectedIndex = i;
                    return;
                }
            }

            cmbStepType.SelectedIndex = -1;
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
            Debug.WriteLine("LstSteps_SelectedIndexChanged");
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
            CloseStepEditor();
            SaveChanges?.Invoke(this, _currentRoutine);

            var parentForm = this.FindForm();
            ToastForm.ShowToast($"✓ Schritt '{stepToDelete.Name}' gelöscht", parentForm);
        }


        // ========== STEP LIST METHODS ==========
        private void RefreshStepsList(bool silent = false)
        {
            if (_currentRoutine == null) return;

            _isRefreshing = true;
            RoutineStep selectedStep =
                lstSteps.SelectedItem as RoutineStep;
            lstSteps.BeginUpdate();
            lstSteps.Items.Clear();

            // Stelle sicher, dass die Steps aus der aktuellen Routine kommen
            var orderedSteps = _currentRoutine.Steps.OrderBy(s => s.Order).ToList();

            foreach (var step in orderedSteps)
            {
                Debug.WriteLine($"  - {step.Name} (Order: {step.Order})");
                lstSteps.Items.Add(step);
            }

            lstSteps.EndUpdate();
            _isRefreshing = false;

            if (selectedStep != null)
            {
                for (int i = 0; i < lstSteps.Items.Count; i++)
                {
                    RoutineStep step =
                        lstSteps.Items[i] as RoutineStep;

                    if (step != null &&
                        step.Id == selectedStep.Id)
                    {
                        lstSteps.SelectedIndex = i;
                        break;
                    }
                }
            }

            if (!silent)
                Debug.WriteLine($"RefreshStepsList: {orderedSteps.Count} Schritte geladen");
        }
        private void BtnSaveStep_Click(object sender, EventArgs e)
        {
            if (!ValidateCurrentStep()) return;

            SaveCurrentStep();
        }
        private void BtnCancelStep_Click(object sender, EventArgs e)
        {
            CloseStepEditor();
        }
        private void BtnExecuteStep_Click(object sender, EventArgs e)
        {
            if (!PrepareStepForExecution())
                return;

            if (!ValidateStepForExecution())
                return;

            ExecuteEditingStep();
        }
        private bool PrepareStepForExecution()
        {
            if (_editingStep == null)
            {
                return HandleNewStepExecution();
            }

            if (HasUnsavedChanges())
            {
                return HandleUnsavedStepChanges();
            }

            return true;
        }
        private bool HandleNewStepExecution()
        {
            var result = AskToSaveChanges();

            if (result != DialogResult.Yes)
                return false;

            if (!ValidateCurrentStep(true))
                return false;

            SaveCurrentStep(true);

            return _editingStep != null;
        }
        private bool HandleUnsavedStepChanges()
        {
            var result = AskToSaveChanges();

            switch (result)
            {
                case DialogResult.Yes:

                    if (!ValidateCurrentStep(true))
                        return false;

                    SaveCurrentStep(false);
                    return true;

                case DialogResult.No:
                    return true;

                case DialogResult.Cancel:
                    return false;

                default:
                    return false;
            }
        }
        private bool ValidateStepForExecution()
        {
            if (!ValidateCurrentStep(true))
                return false;

            if (_routineService.ValidateStep(
                _editingStep,
                out string validationError))
            {
                return true;
            }

            CustomMessageBox.Show(
                validationError,
                "Ausführung nicht möglich",
                CustomMessageBoxButtons.OK,
                CustomMessageBoxIcon.Warning,
                FindForm());

            return false;
        }
        private void ExecuteEditingStep()
        {
            bool shouldOpenExecutionForm =
                _editingStep is OpenUrlStep urlStep &&
                !urlStep.OpenInExternalBrowser;

            if (shouldOpenExecutionForm)
            {
                OpenExecutionForm();
                return;
            }

            _routineService.ExecuteStep(_editingStep);
        }
        private void OpenExecutionForm()
        {
            var executionForm = new ExecutionForm(
                _currentRoutine,
                _editingStep,
                step =>
                {
                    return _routineService.ExecuteStep(step);
                });

            executionForm.ShowDialog(this);
        }
        
        private bool HasUnsavedChanges()
        {
            if (_editingStep == null)
                return HasNewStepInput();

            var editorStep = CreateStepFromEditor();

            if (editorStep == null)
                return true;

            editorStep.Id = _editingStep.Id;
            editorStep.Order = _editingStep.Order;

            return ChangeDetector.HasChanges(
                _editingStep,
                editorStep);
        }
        private bool HasNewStepInput()
        {
            return !string.IsNullOrWhiteSpace(txtStepName.Text)
                || !string.IsNullOrWhiteSpace(txtStepDescription.Text)
                || cmbStepType.SelectedIndex != -1;
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

            if (_isLoadingStep)
                return;
            BuildOptionsEditorTable();
        }

        private void CreateOpenUrlControls()
        {
            txtUrl = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtUrl.Name = "txtUrl";

            txtUrl.TextChanged += TxtUrl_TextChanged;
            txtUrl.LostFocus += TxtUrl_LostFocus;

            TextDragDropHelper.EnableTextDragDrop(txtUrl, droppedText =>
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
            txtFolderPath = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtFolderPath.Name = "txtFolderPath";

            var btnBrowse = UIStyles.Buttons.CreateBrowseInFolder("Ordner auswählen");

            btnBrowse.Click += (s, e) =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                        txtFolderPath.Text = dialog.SelectedPath;
                }
            };

            editorOptionsTable.AddRow(
                "Ordnerpfad",
                UIColumn.Auto(txtFolderPath),
                UIColumn.Percent(btnBrowse, 30));

            tglOpenInNewWindow = UIStyles.ToggleSwitches.CreateStandard(false, "Ja", "Nein");
            tglOpenInNewWindow.Name = "tglOpenInNewWindow";
            tglOpenInNewWindow.Anchor = AnchorStyles.Left;

            editorOptionsTable.AddRow("Neues Fenster", tglOpenInNewWindow);
        }
        private void CreateOpenApplicationControls()
        {
            txtAppPath = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtAppPath.Name = "txtAppPath";

            var btnBrowse = UIStyles.Buttons.CreateBrowseInFolder("Programm auswählen");

            btnBrowse.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Anwendungen (*.exe)|*.exe|Alle Dateien (*.*)|*.*";

                    if (dialog.ShowDialog() == DialogResult.OK)
                        txtAppPath.Text = dialog.FileName;
                }
            };

            editorOptionsTable.AddRow(
                "Programmpfad",
                UIColumn.Auto(txtAppPath),
                UIColumn.Percent(btnBrowse, 30));

            txtAppArguments = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtAppArguments.Name = "txtAppArguments";

            tglRunAsAdmin = UIStyles.ToggleSwitches.CreateStandard(false, "Ja", "Nein");
            tglRunAsAdmin.Name = "tglRunAsAdmin";
            tglRunAsAdmin.Anchor = AnchorStyles.Left;

            editorOptionsTable.AddRow("Argumente", txtAppArguments);
            editorOptionsTable.AddRow("Als Admin", tglRunAsAdmin);
        }
        private void CreateOpenDocumentControls()
        {
            txtDocumentPath = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtDocumentPath.Name = "txtDocumentPath";

            btnBrowseDocument = UIStyles.Buttons.CreateBrowseInFolder("Dokument auswählen");

            btnBrowseDocument.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Alle Dateien (*.*)|*.*";
                    dialog.Title = "Dokument auswählen";

                    if (dialog.ShowDialog() == DialogResult.OK)
                        txtDocumentPath.Text = dialog.FileName;
                }
            };

            editorOptionsTable.AddRow(
                "Dateipfad",
                UIColumn.Auto(txtDocumentPath),
                UIColumn.Percent(btnBrowseDocument, 30));
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
            if (!ValidateStepName(showMessageBox))
                return false;

            if (!ValidateStepType(showMessageBox))
                return false;

            StepType selectedType;

            if (!TryGetSelectedStepType(out selectedType))
                return false;

            switch (selectedType)
            {
                case StepType.OpenUrl:
                    return ValidateOpenUrlStep(showMessageBox);

                case StepType.OpenFolder:
                    return ValidateOpenFolderStep(showMessageBox);

                case StepType.OpenApplication:
                    return ValidateOpenApplicationStep(showMessageBox);

                case StepType.OpenDocument:
                    return ValidateOpenDocumentStep(showMessageBox);

                default:
                    return false;
            }
        }
        private bool ValidateStepName(bool showMessageBox)
        {
            if (!string.IsNullOrWhiteSpace(txtStepName.Text))
                return true;

            ShowValidationMessage(
                "Bitte geben Sie einen Namen für den Schritt ein.",
                showMessageBox);

            txtStepName.Focus();
            return false;
        }
        private bool ValidateStepType(bool showMessageBox)
        {
            if (cmbStepType.SelectedIndex != -1)
                return true;

            ShowValidationMessage(
                "Bitte wählen Sie einen Aktionstyp aus.",
                showMessageBox);

            cmbStepType.Focus();
            return false;
        }
        private bool ValidateOpenUrlStep(bool showMessageBox)
        {
            if (string.IsNullOrWhiteSpace(txtUrl?.Text))
            {
                ShowValidationMessage(
                    "Bitte geben Sie eine URL ein.",
                    showMessageBox);

                txtUrl?.Focus();
                return false;
            }

            var urlResult = _urlValidationService.ValidateAndRepairUrl(txtUrl.Text, false);

            if (urlResult.IsValid)
                return true;

            ShowValidationMessage(
                urlResult.ErrorMessage,
                showMessageBox,
                "Ungültige URL");

            txtUrl.Focus();
            return false;
        }
        private bool ValidateOpenFolderStep(bool showMessageBox)
        {
            if (string.IsNullOrWhiteSpace(txtFolderPath?.Text))
            {
                ShowValidationMessage(
                    "Bitte geben Sie einen Ordnerpfad ein.",
                    showMessageBox);

                txtFolderPath?.Focus();
                return false;
            }

            if (System.IO.Directory.Exists(txtFolderPath.Text))
                return true;

            ShowValidationMessage(
                "Der angegebene Ordner existiert nicht.",
                showMessageBox);

            txtFolderPath.Focus();
            return false;
        }
        private bool ValidateOpenApplicationStep(bool showMessageBox)
        {
            if (string.IsNullOrWhiteSpace(txtAppPath?.Text))
            {
                ShowValidationMessage(
                    "Bitte geben Sie einen Programmpfad ein.",
                    showMessageBox);

                txtAppPath?.Focus();
                return false;
            }

            if (System.IO.File.Exists(txtAppPath.Text))
                return true;

            ShowValidationMessage(
                "Die angegebene Anwendung existiert nicht.",
                showMessageBox);

            txtAppPath.Focus();
            return false;
        }
        private bool ValidateOpenDocumentStep(bool showMessageBox)
        {
            if (string.IsNullOrWhiteSpace(txtDocumentPath?.Text))
            {
                ShowValidationMessage(
                    "Bitte wählen Sie eine Datei aus.",
                    showMessageBox);

                txtDocumentPath?.Focus();
                return false;
            }

            if (System.IO.File.Exists(txtDocumentPath.Text))
                return true;

            ShowValidationMessage(
                "Die ausgewählte Datei existiert nicht.",
                showMessageBox);

            txtDocumentPath.Focus();
            return false;
        }

        private void ShowValidationMessage(
            string message,
            bool showMessageBox,
            string title = "Fehler bei Validierung")
        {
            if (!showMessageBox || AutoConfirmDialogs)
                return;

            CustomMessageBox.Show(
                message,
                title,
                CustomMessageBoxButtons.OK,
                CustomMessageBoxIcon.Warning,
                FindForm());
        }

        private void SaveCurrentStep(bool refreshList = true)
        {
            if (!ValidateCurrentStep(false))
                return;

            var step = CreateStepFromEditor();

            if (step == null)
                return;

            SaveStep(step);

            if (refreshList)
            {
                RoutineStep selectedStep = _editingStep;

                _isRefreshing = true;

                RefreshStepsList(false);

                if (selectedStep != null)
                {
                    for (int i = 0; i < lstSteps.Items.Count; i++)
                    {
                        RoutineStep item =
                            lstSteps.Items[i] as RoutineStep;

                        if (item != null &&
                            item.Id == selectedStep.Id)
                        {
                            lstSteps.SelectedIndex = i;
                            break;
                        }
                    }
                }

                _isRefreshing = false;

                ToastForm.ShowToast(
                    $"✓ Schritt '{step.Name}' gespeichert",
                    FindForm());
            }
        }

        private RoutineStep CreateStepFromEditor()
        {
            StepType selectedType;

            if (!TryGetSelectedStepType(out selectedType))
                return null;

            RoutineStep step;

            switch (selectedType)
            {
                case StepType.OpenUrl:

                    var urlResult =
                        _urlValidationService.ValidateAndRepairUrl(
                            txtUrl?.Text,
                            false);

                    if (!urlResult.IsValid)
                        return null;

                    step = new OpenUrlStep
                    {
                        Url = urlResult.RepairedUrl,
                        OpenInExternalBrowser =
                            tglOpenInExternBrowser?.Checked ?? true
                    };

                    break;

                case StepType.OpenFolder:

                    step = new OpenFolderStep
                    {
                        FolderPath = txtFolderPath?.Text ?? "",
                        OpenInNewWindow =
                            tglOpenInNewWindow?.Checked ?? true
                    };

                    break;

                case StepType.OpenApplication:

                    step = new OpenApplicationStep
                    {
                        ApplicationPath = txtAppPath?.Text ?? "",
                        Arguments = txtAppArguments?.Text ?? "",
                        RunAsAdmin =
                            tglRunAsAdmin?.Checked ?? false
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
                    return null;
            }

            ApplyCommonStepProperties(step);

            return step;
        }

        private void ApplyCommonStepProperties(RoutineStep step)
        {
            step.Name = txtStepName.Text.Trim();

            step.Description =
                txtStepDescription.Text.Trim();

            step.Show = tglStepEnabled.Checked;

            step.AutoStart = tglAutoStart.Checked;
        }

        private void SaveStep(RoutineStep step)
        {
            if (_editingStep != null)
            {
                step.Id = _editingStep.Id;
                step.Order = _editingStep.Order;

                _routineService.UpdateStep(
                    _currentRoutine.Id,
                    step);

                _editingStep = step;

                _currentRoutine =
                    _routineService.GetRoutine(
                        _currentRoutine.Id);
            }
            else
            {
                _routineService.AddStep(
                    _currentRoutine.Id,
                    step);

                _currentRoutine =
                    _routineService.GetRoutine(
                        _currentRoutine.Id);

                if (_originalRoutine != null)
                    _currentRoutine.Order =
                        _originalRoutine.Order;

                _editingStep =
                    _currentRoutine.Steps.LastOrDefault();
            }
        }
        private void SaveCurrentRoutine()
        {
            _currentRoutine.Name = txtRoutineName.Text.Trim();
            _currentRoutine.UpdatedAt = DateTime.Now;

            _routineService.SaveRoutine(_currentRoutine);

            _originalRoutine = RoutineCloneService.DeepCopy(_currentRoutine);
        }
    }
}