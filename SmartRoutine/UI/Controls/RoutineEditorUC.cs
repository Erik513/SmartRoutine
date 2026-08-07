using CustomWFUI;
using CustomWFUI.Controls;
using CustomWFUI.Forms;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Controls.StepEditors;
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
    public partial class RoutineEditorUC : UserControl
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
        private RoutineStep _editorSnapshotStep;
        private StepType? _currentEditorStepType;
        private ToolTip _descriptionTooltip = UIStyles.ToolTips.CreateToolTip();
        private ToolTip _errorToolTip = UIStyles.ToolTips.CreateToolTip();
        private bool _isRefreshing = false;
        private bool _isLoadingStep = false;
        private bool _suppressStepChangeConfirmation;

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
        private ToggleSwitch tglAutoContinue;

        private Panel rightFillPanel;
        
        private TableLayoutPanel rightBtnsTlp;
        private Button btnSaveStep, btnCancelStep;

        // Ein IStepTypeEditor pro StepType kapselt dessen Controls, Laden/Speichern und Validierung.
        private readonly Dictionary<StepType, IStepTypeEditor> _stepEditors;
        private IStepTypeEditor _activeStepEditor;

        // Footer
        private Button btnDuplicateStep, btnAddStep, btnDeleteStep;
        private Label lblLastExecution;
        private Button btnBack;

        public RoutineEditorUC(IRoutineService routineService, IUrlValidationService urlValidationService)
        {
            _routineService = routineService;
            _urlValidationService = urlValidationService;
            _originalRoutine = null;
            _currentRoutine = null;
            _editingStep = null;
            _stepEditors = CreateStepEditors();

            this.Dock = DockStyle.Fill;
            this.BackColor = UIStyles.Colors.BackgroundDark;
            this.DoubleBuffered = true;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            InitializeControl();
        }

        private Dictionary<StepType, IStepTypeEditor> CreateStepEditors()
        {
            var editors = new IStepTypeEditor[]
            {
                new OpenUrlStepEditor(
                    _urlValidationService,
                    (message, showMessageBox) => ShowValidationMessage(message, showMessageBox),
                    _errorToolTip,
                    () => UpdateAutoContinueAvailability()),

                new OpenFolderStepEditor(
                    _routineService,
                    (message, showMessageBox) => ShowValidationMessage(message, showMessageBox)),

                new OpenApplicationStepEditor(
                    _routineService,
                    (message, showMessageBox) => ShowValidationMessage(message, showMessageBox)),

                new OpenDocumentStepEditor(
                    _routineService,
                    (message, showMessageBox) => ShowValidationMessage(message, showMessageBox))
            };

            return editors.ToDictionary(editor => editor.Type);
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
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            contentTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            contentTlp.Dock = DockStyle.Fill;

            contentTlp.ColumnStyles.Clear();
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
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
            txtRoutineName.MaxLength = 40;
            txtRoutineName.Leave += TxtRoutineName_Leave;
            
            routineInfoTable.AddRow("Routinenname", UIColumn.Percent(txtRoutineName, 100));

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
            rightTitleTlp = UIStyles.TableLayoutPanels.CreateDark(2, 1);
            rightTitleTlp.Dock = DockStyle.Fill;
            rightTitleTlp.BackColor = UIStyles.Colors.BackgroundMedium;

            rightTitleTlp.ColumnStyles.Clear();
            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));

            lblStepNameTitle = UIStyles.Labels.CreateTitle();
            lblStepNameTitle.Dock = DockStyle.Fill;
            lblStepNameTitle.BackColor = Color.Transparent;
            lblStepNameTitle.Text = "Schritt bearbeiten";

            tglStepEnabled = UIStyles.ToggleSwitches.CreateStandard(
                true,
                "Schritt ist aktiviert",
                "Schritt ist deaktiviert");

            tglStepEnabled.Anchor = AnchorStyles.None;
            tglStepEnabled.CheckedChanged += TglStepEnabled_CheckedChanged;

            rightTitleTlp.Controls.Add(lblStepNameTitle, 0, 0);
            rightTitleTlp.Controls.Add(tglStepEnabled, 1, 0);
        }

        private void TglStepEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoadingStep || _isRefreshing)
                return;

            if (_editingStep == null)
                return;

            _editingStep.Show = tglStepEnabled.Checked;

            _routineService.UpdateStep(_currentRoutine.Id, _editingStep);

            _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

            _editorSnapshotStep = RoutineStepFactory.CreateCopy(_editingStep);

            RefreshStepsList(silent: true);
        }

        private void InitializeEditorControls()
        {
            txtStepName = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtStepName.Dock = DockStyle.Fill;
            txtStepName.MaxLength = 70;

            txtStepDescription = UIStyles.TextBoxes.CreateBorderstyleNone();
            txtStepDescription.Dock = DockStyle.Fill;

            cmbStepType = UIStyles.ComboBoxes.CreateStandard(ComboBoxStyle.DropDownList);
            cmbStepType.Dock = DockStyle.Fill;

            // Bewusst keine DataSource-Bindung: Ein per DataSource gebundenes ComboBox
            // erzeugt einen CurrencyManager, dessen Position bei bestimmten WinForms-
            // internen Ereignissen wieder auf 0 zurückspringen kann, selbst nachdem
            // SelectedIndex explizit auf -1 gesetzt wurde. Mit einfacher Items-Befüllung
            // gibt es diesen CurrencyManager gar nicht erst, SelectedIndex = -1 bleibt zuverlässig.
            cmbStepType.DisplayMember = "DisplayName";

            foreach (StepTypeOption option in StepTypeHelper.GetStepTypeOptions())
                cmbStepType.Items.Add(option);

            cmbStepType.SelectedIndex = -1;
            cmbStepType.SelectedIndexChanged += CmbStepType_SelectedIndexChanged;

            tglAutoStart = UIStyles.ToggleSwitches.CreateStandard(
                true,
                "Automatisch starten",
                "Nicht automatisch starten");

            tglAutoStart.Anchor = AnchorStyles.Left;

            tglAutoContinue = UIStyles.ToggleSwitches.CreateStandard(
                false,
                "Automatisch zum nächsten Schritt wechseln",
                "Nach Ausführung anhalten");

            tglAutoContinue.Anchor = AnchorStyles.Left;
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

            btnSaveStep = UIStyles.Buttons.CreatePrimary("💾", "Speichern", new Size(100, 35), true);
            btnSaveStep.Dock = DockStyle.Fill;
            btnSaveStep.Margin = new Padding(5);
            btnSaveStep.Click += BtnSaveStep_Click;

            btnCancelStep = UIStyles.Buttons.CreatePrimary("✖", "Abbrechen", new Size(100, 35), true);
            btnCancelStep.Dock = DockStyle.Fill;
            btnCancelStep.Margin = new Padding(5);
            btnCancelStep.Click += BtnCancelStep_Click;

            btnExecuteStep = UIStyles.Buttons.CreateGreen("▶", "Schritt ausführen", new Size(100, 35), true);
            btnExecuteStep.Dock = DockStyle.Fill;
            btnExecuteStep.Margin = new Padding(5);
            btnExecuteStep.Click += BtnExecuteStep_Click;

            rightBtnsTlp.Controls.Add(btnSaveStep, 1, 0);
            rightBtnsTlp.Controls.Add(btnCancelStep, 2, 0);
            rightBtnsTlp.Controls.Add(btnExecuteStep, 3, 0);
        }
        private void InitializeFooter()
        {
            var footerPanel = UIStyles.Panels.CreateDark();
            footerPanel.Dock = DockStyle.Fill;

            var footerTlp = UIStyles.TableLayoutPanels.CreateDark(5, 1);
            footerTlp.BackColor = UIStyles.Colors.BackgroundDarkElevated;
            footerTlp.Dock = DockStyle.Fill;
            footerTlp.Padding = new Padding(0);

            footerTlp.ColumnStyles.Clear();
            footerTlp.RowStyles.Clear();

            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            footerTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            InitializeFooterButtons();

            lblLastExecution = UIStyles.Labels.CreateMuted();
            lblLastExecution.Dock = DockStyle.Fill;
            lblLastExecution.Margin = new Padding(15, 0, 10, 0);
            lblLastExecution.TextAlign = ContentAlignment.MiddleLeft;

            footerTlp.Controls.Add(btnDuplicateStep, 0, 0);
            footerTlp.Controls.Add(btnAddStep, 1, 0);
            footerTlp.Controls.Add(btnDeleteStep, 2, 0);
            footerTlp.Controls.Add(lblLastExecution, 3, 0);
            footerTlp.Controls.Add(btnBack, 4, 0);

            footerPanel.Controls.Add(footerTlp);

            mainTlp.Controls.Add(footerPanel, 0, 1);
        }
        private void InitializeFooterButtons()
        {
            btnDuplicateStep = UIStyles.Buttons.CreatePrimary("⧉", "Schritt duplizieren", new Size(155, 35), true);
            btnDuplicateStep.Dock = DockStyle.Fill;
            btnDuplicateStep.Margin = new Padding(10, 10, 5, 10);
            btnDuplicateStep.Enabled = false;
            btnDuplicateStep.Click += BtnDuplicateStep_Click;

            btnAddStep = UIStyles.Buttons.CreateGreen("+", "Schritt hinzufügen", new Size(155, 35), true);
            btnAddStep.Dock = DockStyle.Fill;
            btnAddStep.Margin = new Padding(10, 10, 5, 10);
            btnAddStep.Click += BtnAddStep_Click;

            btnDeleteStep = UIStyles.Buttons.CreateDanger("🗑", "Schritt löschen", new Size(155, 35), true);
            btnDeleteStep.Dock = DockStyle.Fill;
            btnDeleteStep.Margin = new Padding(5, 10, 10, 10);
            btnDeleteStep.Enabled = false;
            btnDeleteStep.Click += BtnDeleteStep_Click;

            btnBack = UIStyles.Buttons.CreatePrimary("←", "Zurück zur Hauptansicht", new Size(155, 35), true);
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

            editorBaseTable.AddSection("Schrittinformationen");
            editorBaseTable.AddRow("Name", txtStepName);
            editorBaseTable.AddRow("Beschreibung", txtStepDescription);
            editorBaseTable.AddRow("Aktion", cmbStepType);
            
            FlowLayoutPanel autostartPanel = UIStyles.FlowPanels.CreateStandard();
            autostartPanel.Dock = DockStyle.Left;
            Label lblAutoStart = UIStyles.Labels.CreateNormal("Start");
            lblAutoStart.Margin = new Padding(0, 4, 5, 0);
            lblAutoStart.AutoSize = true;

            autostartPanel.Controls.Add(lblAutoStart);
            autostartPanel.Controls.Add(tglAutoStart);

            FlowLayoutPanel autocontinuePanel = UIStyles.FlowPanels.CreateStandard();
            autocontinuePanel.Dock = DockStyle.Left;
            Label lblAutoContinue = UIStyles.Labels.CreateNormal("Weiter");
            lblAutoContinue.Margin = new Padding(0, 4, 5, 0);
            lblAutoContinue.AutoSize = true;

            autocontinuePanel.Controls.Add(lblAutoContinue);
            autocontinuePanel.Controls.Add(tglAutoContinue);

            editorBaseTable.AddRow(
                "Automatisierung",
                UIColumn.Percent(autostartPanel, 50),
                UIColumn.Percent(autocontinuePanel, 50));

            txtStepDescription.MouseEnter += (s, e) =>
            {
                _descriptionTooltip.SetToolTip(
                    txtStepDescription,
                    IsTextTruncated(txtStepDescription)
                        ? txtStepDescription.Text
                        : string.Empty);
            };
        }
        private bool IsTextTruncated(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
                return false;

            Size textSize = TextRenderer.MeasureText(
                textBox.Text,
                textBox.Font);

            return textSize.Width > textBox.ClientSize.Width;
        }

        private void BuildOptionsEditorTable()
        {
            if (editorOptionsTable == null)
                return;

            editorOptionsTable.ClearRows();

            StepType selectedType;

            if (!TryGetSelectedStepType(out selectedType))
            {
                _activeStepEditor = null;
                return;
            }

            editorOptionsTable.AddSection("Optionen");

            _activeStepEditor = _stepEditors[selectedType];
            _activeStepEditor.BuildControls(editorOptionsTable);

            editorOptionsTable.Visible = true;
            editorOptionsTable.PerformLayout();
            editorOptionsTable.Refresh();

            rightTlp.PerformLayout();
            rightTlp.Refresh();
        }

        private bool TryGetSelectedStepType(out StepType selectedType)
        {
            selectedType = default(StepType);

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
                return UIStyles.Icons.Web;

            if (item is OpenFolderStep)
                return UIStyles.Icons.Folder;

            if (item is OpenDocumentStep)
                return UIStyles.Icons.Document;

            if (item is OpenApplicationStep)
                return UIStyles.Icons.Application;

            return null;
        }

        private void TxtRoutineName_Leave(object sender, EventArgs e)
        {
            if (_currentRoutine == null)
                return;

            string newName = txtRoutineName.Text.Trim();

            if (newName == _currentRoutine.Name)
                return;

            if (!ValidateRoutineName())
                return;

            SaveCurrentRoutine();
            SaveChanges?.Invoke(this, _currentRoutine);
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

        private bool ConfirmSaveStepChangesIfNeeded()
        {
            if (!HasEditorChanges())
                return true;

            DialogResult result = CustomMessageBox.Show(
                "Möchten Sie die Änderungen am aktuellen Schritt speichern?",
                "Änderungen speichern",
                CustomMessageBoxButtons.YesNoCancel,
                CustomMessageBoxIcon.Question,
                FindForm(),
                CustomMessageBoxSize.Medium);

            if (result == DialogResult.Cancel)
                return false;

            if (result == DialogResult.No)
                return true;

            if (!ValidateCurrentStep(true))
                return false;

            SaveCurrentStepWithoutReselect();

            return true;
        }

        private void SaveCurrentStepWithoutReselect()
        {
            if (!ValidateCurrentStep(false))
                return;

            RoutineStep step = CreateStepFromEditor();

            if (step == null)
                return;

            SaveStep(step);

            _editorSnapshotStep = RoutineStepFactory.CreateCopy(step);
            NormalizeStepForComparison(_editorSnapshotStep);

            if (_editingStep != null && _editorSnapshotStep != null)
            {
                _editorSnapshotStep.Id = _editingStep.Id;
                _editorSnapshotStep.Order = _editingStep.Order;
            }

            RefreshStepsList(silent: true);
        }

        private void RestoreCurrentEditingStepSelection()
        {
            _isRefreshing = true;

            if (_editingStep != null)
            {
                for (int i = 0; i < lstSteps.Items.Count; i++)
                {
                    RoutineStep step = lstSteps.Items[i] as RoutineStep;

                    if (step != null && step.Id == _editingStep.Id)
                    {
                        lstSteps.SelectedIndex = i;
                        break;
                    }
                }
            }

            _isRefreshing = false;
        }


        private void OpenEditorForStep(RoutineStep step = null)
        {
            rightTlp.Visible = true;
            lblStepNameTitle.Text = step != null
                ? "Schritt bearbeiten"
                : "Neuen Schritt erstellen";

            if (step != null)
            {
                LoadStepToEditor(step);
            }
            else
            {
                ClearEditor();
                SetEditorEnabled(true);
            }
        }
        private void CloseStepEditor()
        {
            _isRefreshing = true;
            lstSteps.SelectedIndex = -1;
            _isRefreshing = false;

            rightTlp.Visible = false;
            ClearEditor();
            btnDuplicateStep.Enabled = false;
            btnDeleteStep.Enabled = false;
        }
        private void ClearEditor()
        {
            _editingStep = null;
            _editorSnapshotStep = null;
            _currentEditorStepType = null;
            _activeStepEditor = null;

            txtStepName.Text = "";
            txtStepDescription.Text = "";
            tglStepEnabled.Checked = true;
            tglAutoStart.Checked = true;
            tglAutoContinue.Checked = false;

            cmbStepType.SelectedIndex = -1;

            editorOptionsTable.ClearRows();

            btnDeleteStep.Enabled = false;
            btnDuplicateStep.Enabled = false;

            SetEditorEnabled(false);

            // Ohne diesen erzwungenen Repaint bleibt z.B. die zuvor im ComboBox
            // angezeigte Auswahl und die alten Options-Controls optisch stehen,
            // obwohl der Zustand darunter bereits korrekt zurückgesetzt ist.
            cmbStepType.Refresh();
            editorOptionsTable.PerformLayout();
            editorOptionsTable.Refresh();
            rightTlp.PerformLayout();
            rightTlp.Refresh();
        }

        private void EnsureOptionsEditorForStepType(StepType stepType)
        {
            if (_currentEditorStepType.HasValue &&
                _currentEditorStepType.Value == stepType)
            {
                return;
            }

            _currentEditorStepType = stepType;

            BuildOptionsEditorTable();
        }

        private void LoadStepToEditor(RoutineStep step)
        {
            _isLoadingStep = true;

            try
            {
                txtStepName.Text = step.Name;
                txtStepDescription.Text = step.Description;
                tglStepEnabled.Checked = step.Show;
                tglAutoStart.Checked = step.AutoStart;
                tglAutoContinue.Checked = step.AutoContinue;

                SetSelectedStepType(step.Type);
                EnsureOptionsEditorForStepType(step.Type);

                editorBaseTable.PerformLayout();
                editorBaseTable.Refresh();

                cmbStepType.PerformLayout();
                cmbStepType.Refresh();

                _activeStepEditor?.LoadFrom(step);

                editorOptionsTable.Visible = true;
                editorOptionsTable.BringToFront();

                UpdateAutoContinueAvailability();

                _editingStep = step;
                _editorSnapshotStep = RoutineStepFactory.CreateCopy(step);
                NormalizeStepForComparison(_editorSnapshotStep);

                SetEditorEnabled(true);
            }
            finally
            {
                _isLoadingStep = false;
            }
        }

        private void NormalizeStepForComparison(RoutineStep step)
        {
            if (step == null)
                return;

            step.Name = step.Name ?? "";
            step.Description = step.Description ?? "";

            if (_stepEditors.TryGetValue(step.Type, out IStepTypeEditor editor))
                editor.NormalizeForComparison(step);
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
            if (_isRefreshing)
                return;

            if (_suppressStepChangeConfirmation)
                return;

            RoutineStep selectedStep = lstSteps.SelectedItem as RoutineStep;

            if (selectedStep == null)
            {
                if (_editingStep != null && HasEditorChanges())
                {
                    if (!ConfirmSaveStepChangesIfNeeded())
                    {
                        RestoreCurrentEditingStepSelection();
                        return;
                    }
                }

                _isRefreshing = true;

                rightTlp.Visible = false;
                ClearEditor();
                btnDuplicateStep.Enabled = false;
                btnDeleteStep.Enabled = false;

                _isRefreshing = false;

                return;
            }

            if (_editingStep != null && selectedStep.Id == _editingStep.Id)
                return;

            if (_editingStep != null && HasEditorChanges())
            {
                if (!ConfirmSaveStepChangesIfNeeded())
                {
                    RestoreCurrentEditingStepSelection();
                    return;
                }
            }
            OpenEditorForStep(selectedStep);
            btnDuplicateStep.Enabled = true;
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

        private void BtnDuplicateStep_Click(object sender, EventArgs e)
        {
            RoutineStep selectedStep = lstSteps.SelectedItem as RoutineStep;

            if (selectedStep == null || _currentRoutine == null)
                return;

            string selectedStepId = selectedStep.Id;

            if (!ConfirmSaveStepChangesIfNeeded())
                return;

            _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

            selectedStep = _currentRoutine.Steps
                .FirstOrDefault(s => s.Id == selectedStepId);

            if (selectedStep == null)
                return;

            RoutineStep duplicatedStep = RoutineStepFactory.CreateCopy(selectedStep);

            if (duplicatedStep == null)
                return;

            duplicatedStep.Id = Guid.NewGuid().ToString();
            duplicatedStep.Name = selectedStep.Name + " Kopie";

            List<RoutineStep> orderedSteps = _currentRoutine.Steps
                .OrderBy(s => s.Order)
                .ToList();

            int originalIndex = orderedSteps.FindIndex(s => s.Id == selectedStep.Id);

            if (originalIndex < 0)
                return;

            orderedSteps.Insert(originalIndex + 1, duplicatedStep);

            for (int i = 0; i < orderedSteps.Count; i++)
                orderedSteps[i].Order = i;

            _currentRoutine.Steps = orderedSteps;

            _routineService.SaveRoutine(_currentRoutine);
            _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

            RefreshStepsList(silent: true);
            SelectStepById(duplicatedStep.Id);

            ToastForm.ShowToast($"✓ Schritt '{duplicatedStep.Name}' dupliziert", FindForm());
        }

        private void SelectStepById(string stepId)
        {
            for (int i = 0; i < lstSteps.Items.Count; i++)
            {
                RoutineStep step = lstSteps.Items[i] as RoutineStep;

                if (step != null && step.Id == stepId)
                {
                    lstSteps.SelectedIndex = i;
                    return;
                }
            }
        }
        private void BtnAddStep_Click(object sender, EventArgs e)
        {
            if (!ConfirmSaveStepChangesIfNeeded())
                return;

            _suppressStepChangeConfirmation = true;

            try
            {
                lstSteps.ClearSelected();
            }
            finally
            {
                _suppressStepChangeConfirmation = false;
            }
            btnDuplicateStep.Enabled = false;
            btnDeleteStep.Enabled = false;
            OpenEditorForStep(null);
            txtStepName.Focus();
        }

        private void BtnDeleteStep_Click(object sender, EventArgs e)
        {
            if (!(lstSteps.SelectedItem is RoutineStep stepToDelete))
                return;

            bool shouldDelete = AutoConfirmDialogs;

            if (!shouldDelete)
            {
                shouldDelete = CustomMessageBox.Show(
                    $"Schritt '{stepToDelete.Name}' wirklich löschen?",
                    "Bestätigen",
                    CustomMessageBoxButtons.YesNo,
                    CustomMessageBoxIcon.Question,
                    FindForm(),
                    CustomMessageBoxSize.Small) == DialogResult.Yes;
            }

            if (!shouldDelete)
                return;

            _routineService.RemoveStep(_currentRoutine.Id, stepToDelete.Id);
            _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);
            _originalRoutine = RoutineCloneService.DeepCopy(_currentRoutine);

            _isRefreshing = true;

            rightTlp.Visible = false;
            ClearEditor();

            RefreshStepsList(silent: true, keepSelection: false);

            btnDeleteStep.Enabled = false;
            btnDuplicateStep.Enabled = false;

            _isRefreshing = false;

            SaveChanges?.Invoke(this, _currentRoutine);

            ToastForm.ShowToast(
                $"✓ Schritt '{stepToDelete.Name}' gelöscht",
                FindForm());
        }
        // ========== STEP LIST METHODS ==========
        private void RefreshStepsList(bool silent = false, bool keepSelection = true)
        {
            if (_currentRoutine == null)
                return;

            _isRefreshing = true;

            RoutineStep selectedStep = keepSelection
                ? lstSteps.SelectedItem as RoutineStep
                : null;

            lstSteps.BeginUpdate();
            lstSteps.Items.Clear();

            var orderedSteps = _currentRoutine.Steps.OrderBy(s => s.Order).ToList();

            foreach (var step in orderedSteps)
                lstSteps.Items.Add(step);

            lstSteps.EndUpdate();

            if (keepSelection && selectedStep != null)
            {
                for (int i = 0; i < lstSteps.Items.Count; i++)
                {
                    RoutineStep step = lstSteps.Items[i] as RoutineStep;

                    if (step != null && step.Id == selectedStep.Id)
                    {
                        lstSteps.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                lstSteps.SelectedIndex = -1;
            }

            _isRefreshing = false;
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

            if (HasEditorChanges())
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
                FindForm(),
                CustomMessageBoxSize.Small);

            return false;
        }
        private void ExecuteEditingStep()
        {
            RoutineStep executionStep =
                RoutineStepFactory.CreateCopy(_editingStep);

            if (executionStep == null)
                return;

            executionStep.Show = true;

            bool requiresExecutionForm =
                !executionStep.AutoStart ||
                executionStep is OpenUrlStep &&
                !((OpenUrlStep)executionStep).OpenInExternalBrowser;

            if (requiresExecutionForm)
            {
                OpenExecutionForm(executionStep);
                return;
            }

            OpenAutoRunForm(executionStep);
        }

        private void OpenExecutionForm(RoutineStep executionStep)
        {
            ExecutionForm form = new ExecutionForm(
                _currentRoutine,
                executionStep,
                step =>
                {
                    return _routineService.ExecuteStep(step);
                });

            try
            {
                form.ShowDialog(this);
            }
            finally
            {
                form.Dispose();
            }
        }

        private void OpenAutoRunForm(RoutineStep executionStep)
        {
            executionStep.Show = true;

            Routine routine = new Routine
            {
                Name = executionStep.Name,
                Steps = new List<RoutineStep>
                {
                    executionStep
                }
            };

            AutoRunForm form = new AutoRunForm(
                routine,
                step =>
                {
                    return _routineService.ExecuteStep(step);
                },
                null);

            try
            {
                form.ShowDialog(this);
            }
            finally
            {
                form.Dispose();
            }
        }

        private bool HasEditorChanges()
        {
            if (_editorSnapshotStep == null)
                return HasNewStepInput();

            RoutineStep currentStep = CreateStepFromEditor();

            if (currentStep == null)
                return false;

            currentStep.Id = _editorSnapshotStep.Id;
            currentStep.Order = _editorSnapshotStep.Order;
            currentStep.Show = _editorSnapshotStep.Show;

            NormalizeStepForComparison(_editorSnapshotStep);
            NormalizeStepForComparison(currentStep);

            return ChangeDetector.HasChanges(
                _editorSnapshotStep,
                currentStep,
                "Show");
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
                FindForm(),
                CustomMessageBoxSize.Medium);
        }

        private void CmbStepType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoadingStep)
                return;

            StepType selectedType;

            if (!TryGetSelectedStepType(out selectedType))
            {
                _currentEditorStepType = null;
                _activeStepEditor = null;
                editorOptionsTable.ClearRows();
                return;
            }
            EnsureOptionsEditorForStepType(selectedType);

            UpdateAutoContinueAvailability();
        }

        // ========== BACK BUTTON ==========
        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (rightTlp.Visible && !ConfirmSaveStepChangesIfNeeded())
                return;

            rightTlp.Visible = false;

            if (!ValidateRoutineName())
                return;

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
                        FindForm(),
                        CustomMessageBoxSize.Small);
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
                        FindForm(),
                        CustomMessageBoxSize.Medium);
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

            return _activeStepEditor != null && _activeStepEditor.Validate(showMessageBox);
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
                FindForm(),
                CustomMessageBoxSize.Medium);
        }

        private void SaveCurrentStep(bool showSavedToast = true)
        {
            if (!ValidateCurrentStep(false))
                return;

            var step = CreateStepFromEditor();

            if (step == null)
                return;

            SaveStep(step);

            _editorSnapshotStep = RoutineStepFactory.CreateCopy(step);
            NormalizeStepForComparison(_editorSnapshotStep);

            if (_editingStep != null && _editorSnapshotStep != null)
            {
                _editorSnapshotStep.Id = _editingStep.Id;
                _editorSnapshotStep.Order = _editingStep.Order;
            }

            // Liste (inkl. Icon/Name) muss immer aktualisiert werden, auch wenn direkt
            // im Anschluss ausgeführt wird (showSavedToast = false) und daher keine
            // "gespeichert"-Meldung angezeigt werden soll.
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

            if (showSavedToast)
            {
                ToastForm.ShowToast(
                    $"✓ Schritt '{step.Name}' gespeichert",
                    FindForm());
            }
        }

        private RoutineStep CreateStepFromEditor()
        {
            if (_activeStepEditor == null)
                return null;

            RoutineStep step = _activeStepEditor.CreateStep();

            if (step == null)
                return null;

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

            step.AutoContinue = tglAutoContinue.Checked;
            if (step is OpenUrlStep urlStep && !urlStep.OpenInExternalBrowser)
            {
                step.AutoContinue = false;
            }
        }

        private void UpdateAutoContinueAvailability()
        {
            if (_activeStepEditor == null)
            {
                tglAutoContinue.Enabled = true;
                return;
            }

            _activeStepEditor.UpdateAutoContinueAvailability(tglAutoContinue);
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