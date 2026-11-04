using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using MessageBox = ErikwnkWFUI.Forms.MessageBox;
using MessageBoxButtons = ErikwnkWFUI.Forms.MessageBoxButtons;
using MessageBoxIcon = ErikwnkWFUI.Forms.MessageBoxIcon;
using SmartRoutine.Data.Models;
using SmartRoutine.Localization;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Controls.StepEditors;
using SmartRoutine.UI.Forms;
using SmartRoutine.UI.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using FlowLayoutPanel = System.Windows.Forms.FlowLayoutPanel;
using TableLayoutPanel = System.Windows.Forms.TableLayoutPanel;

namespace SmartRoutine.UI.Controls
{
    // Builds the controls and the layout of the editor.
    public partial class RoutineEditorUC
    {
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

            routineInfoTable = new PropertyTable();

            txtRoutineName = UIStyles.TextBoxes.CreateLockable();
            txtRoutineName.Dock = DockStyle.Fill;
            txtRoutineName.MaxLength = 40;
            txtRoutineName.EditAccepted += TxtRoutineName_EditAccepted;
            
            routineInfoTable.AddRow(Loc.F("Editor.RoutineName"), PropertyColumn.Percent(txtRoutineName, 100));

            lstSteps = UIStyles.TitledListBoxes.CreateStandard(
                headerTitle: Loc.S("Editor.StepsTitle"),
                displayMember: "Name",
                allowReorder: true,
                showEnumeration: true,
                headerTextAlign: ContentAlignment.MiddleCenter);
            lstSteps.Dock = DockStyle.Fill;
            UIStyles.Localize(lstSteps, list => list.Title = Loc.S("Editor.StepsTitle"));

            // The selected step is the one in the editor next to the list: a click into the editor
            // (name box, combo box, switches) must not take the selection away and close the editor.
            lstSteps.DeselectOnOutsideClick = false;

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
            UIStyles.Colors.Follow(rightTitleTlp, () => rightTitleTlp.BackColor = UIStyles.Colors.BackgroundMedium);

            rightTitleTlp.ColumnStyles.Clear();
            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));

            lblStepNameTitle = UIStyles.Labels.CreateTitle();
            lblStepNameTitle.Dock = DockStyle.Fill;
            lblStepNameTitle.BackColor = Color.Transparent;
            UIStyles.Localize(lblStepNameTitle, label => label.Text = Loc.S(_stepTitleKey));

            tglStepEnabled = LocalizedControls.CreateToggle(
                true,
                "Editor.Toggle.StepEnabled",
                "Editor.Toggle.StepDisabled");

            tglStepEnabled.Anchor = AnchorStyles.None;
            tglStepEnabled.CheckedChanged += TglStepEnabled_CheckedChanged;

            rightTitleTlp.Controls.Add(lblStepNameTitle, 0, 0);
            rightTitleTlp.Controls.Add(tglStepEnabled, 1, 0);
        }

        private void InitializeEditorControls()
        {
            txtStepName = UIStyles.TextBoxes.CreateBorderless();
            txtStepName.Dock = DockStyle.Fill;
            txtStepName.MaxLength = 70;

            txtStepDescription = UIStyles.TextBoxes.CreateBorderless();
            txtStepDescription.Dock = DockStyle.Fill;

            cmbStepType = UIStyles.ComboBoxes.CreateStandard(ComboBoxStyle.DropDownList);
            cmbStepType.Dock = DockStyle.Fill;

            // No DataSource binding on purpose: a combo box bound by DataSource
            // creates a CurrencyManager whose position can jump back to 0 on certain WinForms
            // internal events, even after
            // SelectedIndex has explicitly been set to -1. With plain filling of Items
            // there is no CurrencyManager at all, and SelectedIndex = -1 stays reliable.
            cmbStepType.DisplayMember = "DisplayName";

            // Fills the list now and again each time the language changes (the texts of the actions).
            UIStyles.Localize(cmbStepType, combo => RefreshStepTypeTexts());
            cmbStepType.SelectedIndexChanged += CmbStepType_SelectedIndexChanged;

            tglAutoStart = LocalizedControls.CreateToggle(
                true,
                "Editor.Toggle.AutoStartOn",
                "Editor.Toggle.AutoStartOff");

            tglAutoStart.Anchor = AnchorStyles.Left;

            tglAutoContinue = LocalizedControls.CreateToggle(
                false,
                "Editor.Toggle.ContinueOn",
                "Editor.Toggle.ContinueOff");

            tglAutoContinue.Anchor = AnchorStyles.Left;
        }

        private void InitializeEditorTables()
        {
            editorBaseTable = new PropertyTable
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            BuildBaseEditorTable();

            editorOptionsTable = new PropertyTable
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

            btnSaveStep = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Save, "Common.Save", new Size(100, 35));
            btnSaveStep.Dock = DockStyle.Fill;
            btnSaveStep.Margin = new Padding(5);
            btnSaveStep.Click += BtnSaveStep_Click;

            btnCancelStep = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Cancel, "Common.Cancel", new Size(100, 35));
            btnCancelStep.Dock = DockStyle.Fill;
            btnCancelStep.Margin = new Padding(5);
            btnCancelStep.Click += BtnCancelStep_Click;

            btnExecuteStep = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Play, "Editor.Button.Execute", new Size(100, 35));
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
            UIStyles.Colors.Follow(footerTlp, () => footerTlp.BackColor = UIStyles.Colors.BackgroundDarkElevated);
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
            UIStyles.Localize(lblLastExecution, label => UpdateLastExecutionText());
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
            btnDuplicateStep = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Copy, "Editor.Button.Duplicate", new Size(155, 35));
            btnDuplicateStep.Dock = DockStyle.Fill;
            btnDuplicateStep.Margin = new Padding(10, 10, 5, 10);
            btnDuplicateStep.Enabled = false;
            btnDuplicateStep.Click += BtnDuplicateStep_Click;

            btnAddStep = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Add, "Editor.Button.Add", new Size(155, 35));
            btnAddStep.Dock = DockStyle.Fill;
            btnAddStep.Margin = new Padding(10, 10, 5, 10);
            btnAddStep.Click += BtnAddStep_Click;

            btnDeleteStep = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Delete, "Editor.Button.Delete", new Size(155, 35));
            btnDeleteStep.Dock = DockStyle.Fill;
            btnDeleteStep.Margin = new Padding(5, 10, 10, 10);
            btnDeleteStep.Enabled = false;
            btnDeleteStep.Click += BtnDeleteStep_Click;

            btnBack = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Back, "Editor.Button.Back", new Size(155, 35));
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

            editorBaseTable.AddSection(Loc.F("Editor.Section.StepInfo"));
            editorBaseTable.AddRow(Loc.F("Editor.Row.Name"), txtStepName);
            editorBaseTable.AddRow(Loc.F("Editor.Row.Description"), txtStepDescription);
            editorBaseTable.AddRow(Loc.F("Editor.Row.Action"), cmbStepType);
            
            FlowLayoutPanel autostartPanel = UIStyles.FlowLayoutPanels.CreateTransparent();
            autostartPanel.Dock = DockStyle.Left;
            autostartPanel.WrapContents = false;
            // PropertyTable takes the row height from the control's height when the row is added,
            // so give it the real height now (AutoSize would only apply after the first layout).
            autostartPanel.Height = 30;
            Label lblAutoStart = UIStyles.Labels.CreateStandard(Loc.S("Editor.Label.Start"));
            UIStyles.Localize(lblAutoStart, l => l.Text = Loc.S("Editor.Label.Start"));
            lblAutoStart.Margin = new Padding(0, 4, 5, 0);
            lblAutoStart.AutoSize = true;

            autostartPanel.Controls.Add(lblAutoStart);
            autostartPanel.Controls.Add(tglAutoStart);

            FlowLayoutPanel autocontinuePanel = UIStyles.FlowLayoutPanels.CreateTransparent();
            autocontinuePanel.Dock = DockStyle.Left;
            autocontinuePanel.WrapContents = false;
            // PropertyTable takes the row height from the control's height when the row is added,
            // so give it the real height now (AutoSize would only apply after the first layout).
            autocontinuePanel.Height = 30;
            Label lblAutoContinue = UIStyles.Labels.CreateStandard(Loc.S("Editor.Label.Continue"));
            UIStyles.Localize(lblAutoContinue, l => l.Text = Loc.S("Editor.Label.Continue"));
            lblAutoContinue.Margin = new Padding(0, 4, 5, 0);
            lblAutoContinue.AutoSize = true;

            autocontinuePanel.Controls.Add(lblAutoContinue);
            autocontinuePanel.Controls.Add(tglAutoContinue);

            editorBaseTable.AddRow(
                Loc.F("Editor.Row.Automation"),
                PropertyColumn.Percent(autostartPanel, 50),
                PropertyColumn.Percent(autocontinuePanel, 50));
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

            editorOptionsTable.AddSection(Loc.F("Editor.Section.Options"));

            _activeStepEditor = _stepEditors[selectedType];
            _activeStepEditor.BuildControls(editorOptionsTable);

            editorOptionsTable.Visible = true;
            editorOptionsTable.PerformLayout();
            editorOptionsTable.Refresh();

            rightTlp.PerformLayout();
            rightTlp.Refresh();
        }
    }
}
