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
    internal class OpenApplicationStepEditor : IStepTypeEditor
    {
        private readonly IRoutineService _routineService;
        private readonly Action<string, bool> _showValidationMessage;

        private TextBox _txtAppPath;
        private TextBox _txtAppArguments;
        private ToggleSwitch _tglRunAsAdmin;

        public StepType Type => StepType.OpenApplication;

        public OpenApplicationStepEditor(
            IRoutineService routineService,
            Action<string, bool> showValidationMessage)
        {
            _routineService = routineService;
            _showValidationMessage = showValidationMessage;
        }

        public void BuildControls(PropertyTable optionsTable)
        {
            _txtAppPath = UIStyles.TextBoxes.CreateBorderless();
            _txtAppPath.Name = "txtAppPath";

            var btnBrowse = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Browse, "StepEditor.App.Browse", new Size(50, 30));

            btnBrowse.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = Loc.S("StepEditor.App.Filter");

                    if (dialog.ShowDialog() == DialogResult.OK)
                        _txtAppPath.Text = dialog.FileName;
                }
            };

            optionsTable.AddRow(
                Loc.F("StepEditor.App.Row.Path"),
                PropertyColumn.Percent(_txtAppPath, 100),
                PropertyColumn.Absolute(btnBrowse, 50));

            _txtAppArguments = UIStyles.TextBoxes.CreateBorderless();
            _txtAppArguments.Name = "txtAppArguments";

            _tglRunAsAdmin = LocalizedControls.CreateYesNoToggle(false);
            _tglRunAsAdmin.Name = "tglRunAsAdmin";
            _tglRunAsAdmin.Anchor = AnchorStyles.Left;

            optionsTable.AddRow(Loc.F("StepEditor.App.Row.Arguments"), _txtAppArguments);
            optionsTable.AddRow(Loc.F("StepEditor.App.Row.RunAsAdmin"), _tglRunAsAdmin);
        }

        public void LoadFrom(RoutineStep step)
        {
            var appStep = step as OpenApplicationStep;

            if (appStep == null)
                return;

            _txtAppPath.Text = appStep.ApplicationPath;
            _txtAppArguments.Text = appStep.Arguments;
            _tglRunAsAdmin.Checked = appStep.RunAsAdmin;
        }

        public void NormalizeForComparison(RoutineStep step)
        {
            var appStep = step as OpenApplicationStep;

            if (appStep == null)
                return;

            appStep.ApplicationPath = appStep.ApplicationPath ?? "";
            appStep.Arguments = appStep.Arguments ?? "";
            appStep.WorkingDirectory = appStep.WorkingDirectory ?? "";
        }

        public RoutineStep CreateStep()
        {
            return new OpenApplicationStep
            {
                ApplicationPath = _txtAppPath?.Text ?? "",
                Arguments = _txtAppArguments?.Text ?? "",
                RunAsAdmin = _tglRunAsAdmin?.Checked ?? false,
                WorkingDirectory = ""
            };
        }

        public bool Validate(bool showMessageBox)
        {
            if (string.IsNullOrWhiteSpace(_txtAppPath?.Text))
            {
                _showValidationMessage(Loc.S("StepEditor.App.Validation.Empty"), showMessageBox);

                _txtAppPath?.Focus();
                return false;
            }

            var step = new OpenApplicationStep { ApplicationPath = _txtAppPath.Text };

            if (_routineService.ValidateStep(step, out string errorMessage))
                return true;

            _showValidationMessage(errorMessage, showMessageBox);

            _txtAppPath.Focus();
            return false;
        }

        public void UpdateAutoContinueAvailability(ToggleSwitch autoContinueToggle)
        {
            autoContinueToggle.Enabled = true;
        }
    }
}
