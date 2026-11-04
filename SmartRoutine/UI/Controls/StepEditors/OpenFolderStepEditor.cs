using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using SmartRoutine.Data.Models;
using SmartRoutine.Localization;
using SmartRoutine.UI.Helpers;
using SmartRoutine.Logic.Interfaces;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Controls.StepEditors
{
    internal class OpenFolderStepEditor : IStepTypeEditor
    {
        private readonly IRoutineService _routineService;
        private readonly Action<string, bool> _showValidationMessage;

        private TextBox _txtFolderPath;
        private ToggleSwitch _tglOpenInNewWindow;

        public StepType Type => StepType.OpenFolder;

        public OpenFolderStepEditor(
            IRoutineService routineService,
            Action<string, bool> showValidationMessage)
        {
            _routineService = routineService;
            _showValidationMessage = showValidationMessage;
        }

        public void BuildControls(PropertyTable optionsTable)
        {
            _txtFolderPath = UIStyles.TextBoxes.CreateBorderless();
            _txtFolderPath.Name = "txtFolderPath";

            var btnBrowse = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Browse, "StepEditor.Folder.Browse", new Size(50, 30));

            btnBrowse.Click += (s, e) =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                        _txtFolderPath.Text = dialog.SelectedPath;
                }
            };

            optionsTable.AddRow(
                Loc.F("StepEditor.Folder.Row.Path"),
                PropertyColumn.Percent(_txtFolderPath, 100),
                PropertyColumn.Absolute(btnBrowse, 50));

            _tglOpenInNewWindow = LocalizedControls.CreateYesNoToggle(false);
            _tglOpenInNewWindow.Name = "tglOpenInNewWindow";
            _tglOpenInNewWindow.Anchor = AnchorStyles.Left;

            optionsTable.AddRow(Loc.F("StepEditor.Folder.Row.NewWindow"), _tglOpenInNewWindow);
        }

        public void LoadFrom(RoutineStep step)
        {
            var folderStep = step as OpenFolderStep;

            if (folderStep == null)
                return;

            _txtFolderPath.Text = folderStep.FolderPath;
            _tglOpenInNewWindow.Checked = folderStep.OpenInNewWindow;
        }

        public void NormalizeForComparison(RoutineStep step)
        {
            var folderStep = step as OpenFolderStep;

            if (folderStep == null)
                return;

            folderStep.FolderPath = folderStep.FolderPath ?? "";
        }

        public RoutineStep CreateStep()
        {
            return new OpenFolderStep
            {
                FolderPath = _txtFolderPath?.Text ?? "",
                OpenInNewWindow = _tglOpenInNewWindow?.Checked ?? true
            };
        }

        public bool Validate(bool showMessageBox)
        {
            if (string.IsNullOrWhiteSpace(_txtFolderPath?.Text))
            {
                _showValidationMessage(Loc.S("StepEditor.Folder.Validation.Empty"), showMessageBox);

                _txtFolderPath?.Focus();
                return false;
            }

            // Existenzprüfung: delegiert an RoutineService.ValidateStep, damit die
            // Regel nur an einer Stelle gepflegt werden muss (auch für Ausführung ohne Editor).
            var step = new OpenFolderStep { FolderPath = _txtFolderPath.Text };

            if (_routineService.ValidateStep(step, out string errorMessage))
                return true;

            _showValidationMessage(errorMessage, showMessageBox);

            _txtFolderPath.Focus();
            return false;
        }

        public void UpdateAutoContinueAvailability(ToggleSwitch autoContinueToggle)
        {
            autoContinueToggle.Enabled = true;
        }
    }
}
