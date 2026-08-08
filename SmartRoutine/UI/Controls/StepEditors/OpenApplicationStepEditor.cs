using CustomWFUI;
using CustomWFUI.Controls;
using SmartRoutine.Data.Models;
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

        public void BuildControls(StyledPropertyTable optionsTable)
        {
            _txtAppPath = UIStyles.TextBoxes.CreateBorderstyleNone();
            _txtAppPath.Name = "txtAppPath";

            var btnBrowse = UIStyles.Buttons.CreateBrowseInFolder("Programm auswählen", new Size(50, 30));

            btnBrowse.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Anwendungen (*.exe)|*.exe|Alle Dateien (*.*)|*.*";

                    if (dialog.ShowDialog() == DialogResult.OK)
                        _txtAppPath.Text = dialog.FileName;
                }
            };

            optionsTable.AddRow(
                "Programmpfad",
                UIColumn.Percent(_txtAppPath, 100),
                UIColumn.Absolute(btnBrowse, 50));

            _txtAppArguments = UIStyles.TextBoxes.CreateBorderstyleNone();
            _txtAppArguments.Name = "txtAppArguments";

            _tglRunAsAdmin = UIStyles.ToggleSwitches.CreateStandard(false, "Ja", "Nein");
            _tglRunAsAdmin.Name = "tglRunAsAdmin";
            _tglRunAsAdmin.Anchor = AnchorStyles.Left;

            optionsTable.AddRow("Argumente", _txtAppArguments);
            optionsTable.AddRow("Als Admin", _tglRunAsAdmin);
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
                _showValidationMessage("Bitte geben Sie einen Programmpfad ein.", showMessageBox);

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
