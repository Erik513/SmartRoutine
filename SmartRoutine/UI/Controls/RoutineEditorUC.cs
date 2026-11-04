using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using ErikwnkWFUI.Forms;
using MessageBox = ErikwnkWFUI.Forms.MessageBox;
using MessageBoxButtons = ErikwnkWFUI.Forms.MessageBoxButtons;
using MessageBoxIcon = ErikwnkWFUI.Forms.MessageBoxIcon;
using SmartRoutine.Data.Models;
using SmartRoutine.Localization;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Controls.StepEditors;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ComboBox = System.Windows.Forms.ComboBox;
using Panel = System.Windows.Forms.Panel;
using TableLayoutPanel = System.Windows.Forms.TableLayoutPanel;

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
        private bool _isRefreshing = false;
        private bool _isLoadingStep = false;
        private bool _suppressStepChangeConfirmation;

        // UI Controls
        private TableLayoutPanel mainTlp;

        // Inhalt (links + rechts)
        private TableLayoutPanel contentTlp;

        // Linke Seite
        private TableLayoutPanel leftTlp;
        private PropertyTable routineInfoTable;
        private LockableTextBox txtRoutineName;
        private TitledListBox lstSteps;

        // Rechte Seite
        private TableLayoutPanel rightTlp;
        private TableLayoutPanel rightTitleTlp;
        private Button btnExecuteStep;
        private Label lblStepNameTitle;
        private ToggleSwitch tglStepEnabled;

        private PropertyTable editorBaseTable;
        private TextBox txtStepName;
        private TextBox txtStepDescription;
        private ComboBox cmbStepType;
        private ToggleSwitch tglAutoStart;
        private PropertyTable editorOptionsTable;
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
            UIStyles.Colors.Follow(this, () => BackColor = UIStyles.Colors.BackgroundDark);
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

        // The name is locked against accidental changes: it is saved when the user accepts an edit
        // (check mark or Enter), not when the box loses the focus.
        private void TxtRoutineName_EditAccepted(object sender, EventArgs e)
        {
            if (_currentRoutine == null)
                return;

            string newName = txtRoutineName.Text.Trim();

            if (newName == _currentRoutine.Name)
                return;

            if (!ValidateRoutineName())
            {
                txtRoutineName.BeginEdit();
                return;
            }

            SaveCurrentRoutine();
            SaveChanges?.Invoke(this, _currentRoutine);
        }

        // ========== PUBLIC METHODS ==========
        public void LoadRoutine(Routine routine, bool isNewRoutine = false)
        {
            _originalRoutine = RoutineCloneService.DeepCopy(routine ?? new Routine());
            _currentRoutine = RoutineCloneService.DeepCopy(routine ?? new Routine());

            txtRoutineName.Text = _currentRoutine.Name;

            // A new routine opens with the name box unlocked so the name can be typed at once;
            // an existing one is locked until the user unlocks it.
            txtRoutineName.IsLocked = !isNewRoutine;
            if (isNewRoutine)
                txtRoutineName.BeginEdit();

            UpdateLastExecutionText();

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

            DialogResult result = MessageBox.Show(
                Loc.S("Editor.SaveChanges.Step"),
                Loc.S("Editor.SaveChanges.Title"),
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                FindForm(),
                FormSize.Medium);

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
            if (PersistEditorStep() == null)
                return;

            RefreshStepsList(silent: true);
        }

        // Validates the editor input, saves it as the step and renews the snapshot used to detect changes.
        // Returns the saved step, or null when nothing was saved.
        private RoutineStep PersistEditorStep()
        {
            if (!ValidateCurrentStep(false))
                return null;

            RoutineStep step = CreateStepFromEditor();

            if (step == null)
                return null;

            SaveStep(step);

            _editorSnapshotStep = RoutineStepFactory.CreateCopy(step);
            NormalizeStepForComparison(_editorSnapshotStep);

            if (_editingStep != null && _editorSnapshotStep != null)
            {
                _editorSnapshotStep.Id = _editingStep.Id;
                _editorSnapshotStep.Order = _editingStep.Order;
            }

            return step;
        }

        private void RestoreCurrentEditingStepSelection()
        {
            _isRefreshing = true;

            if (_editingStep != null)
                TrySelectStep(_editingStep.Id);

            _isRefreshing = false;
        }

        // Selects the list item of the step with this id; false when the list has none.
        private bool TrySelectStep(string stepId)
        {
            if (stepId == null)
                return false;

            for (int i = 0; i < lstSteps.Items.Count; i++)
            {
                RoutineStep step = lstSteps.Items[i] as RoutineStep;

                if (step != null && step.Id == stepId)
                {
                    lstSteps.SelectedIndex = i;
                    return true;
                }
            }

            return false;
        }

        // The key of the text above the step editor ("edit step" / "new step"); the label follows the language.
        private string _stepTitleKey = "Editor.StepEdit";

        // "Last run: 5 minutes ago" at the bottom of the page; empty for a routine that was never run.
        private void UpdateLastExecutionText()
        {
            if (lblLastExecution == null)
                return;

            lblLastExecution.Text =
                _currentRoutine != null && _currentRoutine.LastExecutionAt.HasValue
                    ? Loc.S("Editor.LastRun", DateTimeHelper.GetRelativeTime(_currentRoutine.LastExecutionAt))
                    : "";
        }

        // Fills the list of actions with the texts of the current language and keeps what was selected.
        // (Without a data source on purpose, see InitializeEditorControls; no event while it is filled.)
        private void RefreshStepTypeTexts()
        {
            StepType selectedType;
            bool hadSelection = TryGetSelectedStepType(out selectedType);
            bool wasLoading = _isLoadingStep;

            _isLoadingStep = true;

            try
            {
                cmbStepType.Items.Clear();

                foreach (StepTypeOption option in StepTypeHelper.GetStepTypeOptions())
                    cmbStepType.Items.Add(option);

                if (hadSelection)
                    SetSelectedStepType(selectedType);
                else
                    cmbStepType.SelectedIndex = -1;
            }
            finally
            {
                _isLoadingStep = wasLoading;
            }
        }

        private void OpenEditorForStep(RoutineStep step = null)
        {
            rightTlp.Visible = true;

            _stepTitleKey = step != null ? "Editor.StepEdit" : "Editor.StepNew";
            lblStepNameTitle.Text = Loc.S(_stepTitleKey);

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

            HideStepEditor();
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
                if (!ConfirmLeavingEditedStep())
                    return;

                _isRefreshing = true;
                HideStepEditor();
                _isRefreshing = false;

                return;
            }

            if (_editingStep != null && selectedStep.Id == _editingStep.Id)
                return;

            if (!ConfirmLeavingEditedStep())
                return;

            OpenEditorForStep(selectedStep);
            btnDuplicateStep.Enabled = true;
            btnDeleteStep.Enabled = true;
        }

        // Asks about unsaved changes of the step in the editor before the editor is left.
        // False when the user cancels; the selection of the editing step is restored then.
        private bool ConfirmLeavingEditedStep()
        {
            if (_editingStep == null || ConfirmSaveStepChangesIfNeeded())
                return true;

            RestoreCurrentEditingStepSelection();

            return false;
        }

        // Hides the step editor and empties it (this also disables the duplicate and delete buttons).
        private void HideStepEditor()
        {
            rightTlp.Visible = false;
            ClearEditor();
        }

        private void LstSteps_ItemsReordered(object sender, EventArgs e)
        {
            if (_isRefreshing) return;

            var reorderedSteps = lstSteps.Items.Cast<RoutineStep>().ToList();

            RenumberSteps(reorderedSteps);
            _currentRoutine.Steps = reorderedSteps;
            _routineService.SaveRoutine(_currentRoutine);
            lstSteps.Invalidate();
            lstSteps.Update();
        }

        // The order of a step is its position in the list.
        private static void RenumberSteps(List<RoutineStep> steps)
        {
            for (int i = 0; i < steps.Count; i++)
                steps[i].Order = i;
        }

        // Puts a copy of the step right behind it (new id, name + the copy suffix) and renumbers the list.
        // Returns the copy, or null when the step is not in the list.
        private static RoutineStep InsertDuplicateAfter(List<RoutineStep> orderedSteps, RoutineStep original)
        {
            int originalIndex = orderedSteps.FindIndex(s => s.Id == original.Id);

            if (originalIndex < 0)
                return null;

            RoutineStep duplicate = RoutineStepFactory.CreateCopy(original);

            if (duplicate == null)
                return null;

            duplicate.Id = Guid.NewGuid().ToString();
            duplicate.Name = original.Name + Loc.S("Editor.CopySuffix");

            orderedSteps.Insert(originalIndex + 1, duplicate);
            RenumberSteps(orderedSteps);

            return duplicate;
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

            List<RoutineStep> orderedSteps = _currentRoutine.Steps
                .OrderBy(s => s.Order)
                .ToList();

            RoutineStep duplicatedStep = InsertDuplicateAfter(orderedSteps, selectedStep);

            if (duplicatedStep == null)
                return;

            _currentRoutine.Steps = orderedSteps;

            _routineService.SaveRoutine(_currentRoutine);
            _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

            RefreshStepsList(silent: true);
            TrySelectStep(duplicatedStep.Id);

            ToastForm.ShowToast(Loc.S("Editor.Toast.Duplicated", duplicatedStep.Name), FindForm());
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
                shouldDelete = MessageBox.Show(
                    Loc.S("Editor.DeleteStep.Text", stepToDelete.Name),
                    Loc.S("Common.Confirm"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    FindForm(),
                    FormSize.Small) == DialogResult.Yes;
            }

            if (!shouldDelete)
                return;

            _routineService.RemoveStep(_currentRoutine.Id, stepToDelete.Id);
            _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);
            _originalRoutine = RoutineCloneService.DeepCopy(_currentRoutine);

            _isRefreshing = true;

            HideStepEditor();

            RefreshStepsList(silent: true, keepSelection: false);

            _isRefreshing = false;

            SaveChanges?.Invoke(this, _currentRoutine);

            ToastForm.ShowToast(
                Loc.S("Editor.Toast.Deleted", stepToDelete.Name),
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
                TrySelectStep(selectedStep.Id);
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
            return MessageBox.Show(
                Loc.S("Editor.SaveChanges.BeforeRun"),
                Loc.S("Editor.SaveChanges.Title"),
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                FindForm(),
                FormSize.Medium);
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
        private void SaveCurrentStep(bool showSavedToast = true)
        {
            RoutineStep step = PersistEditorStep();

            if (step == null)
                return;

            // Liste (inkl. Icon/Name) muss immer aktualisiert werden, auch wenn direkt
            // im Anschluss ausgeführt wird (showSavedToast = false) und daher keine
            // "gespeichert"-Meldung angezeigt werden soll.
            RoutineStep selectedStep = _editingStep;

            _isRefreshing = true;

            RefreshStepsList(false);

            if (selectedStep != null)
                TrySelectStep(selectedStep.Id);

            _isRefreshing = false;

            if (showSavedToast)
            {
                ToastForm.ShowToast(
                    Loc.S("Editor.Toast.Saved", step.Name),
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