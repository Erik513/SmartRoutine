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
    internal class OpenDocumentStepEditor : IStepTypeEditor
    {
        private readonly IRoutineService _routineService;
        private readonly Action<string, bool> _showValidationMessage;

        private TextBox _txtDocumentPath;

        public StepType Type => StepType.OpenDocument;

        public OpenDocumentStepEditor(
            IRoutineService routineService,
            Action<string, bool> showValidationMessage)
        {
            _routineService = routineService;
            _showValidationMessage = showValidationMessage;
        }

        public void BuildControls(PropertyTable optionsTable)
        {
            _txtDocumentPath = UIStyles.TextBoxes.CreateBorderless();
            _txtDocumentPath.Name = "txtDocumentPath";

            var btnBrowseDocument = LocalizedControls.CreateSymbolButton(UIStyles.Buttons.Presets.Browse, "StepEditor.Document.Browse", new Size(50, 30));

            btnBrowseDocument.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = Loc.S("StepEditor.Document.Filter");
                    dialog.Title = Loc.S("StepEditor.Document.Browse");
                    BrowsePath.Prepare(dialog, _txtDocumentPath.Text);

                    if (dialog.ShowDialog() == DialogResult.OK)
                        _txtDocumentPath.Text = dialog.FileName;
                }
            };

            optionsTable.AddRow(
                Loc.F("StepEditor.Document.Row.Path"),
                PropertyColumn.Percent(_txtDocumentPath, 100),
                PropertyColumn.Absolute(btnBrowseDocument, 50));
        }

        public void LoadFrom(RoutineStep step)
        {
            var docStep = step as OpenDocumentStep;

            if (docStep == null)
                return;

            _txtDocumentPath.Text = docStep.FilePath;
        }

        public void NormalizeForComparison(RoutineStep step)
        {
            var docStep = step as OpenDocumentStep;

            if (docStep == null)
                return;

            docStep.FilePath = docStep.FilePath ?? "";
        }

        public RoutineStep CreateStep()
        {
            return new OpenDocumentStep
            {
                FilePath = _txtDocumentPath?.Text ?? "",
                OpenWithAssociatedApp = true
            };
        }

        public bool Validate(bool showMessageBox)
        {
            if (string.IsNullOrWhiteSpace(_txtDocumentPath?.Text))
            {
                _showValidationMessage(Loc.S("StepEditor.Document.Validation.Empty"), showMessageBox);

                _txtDocumentPath?.Focus();
                return false;
            }

            var step = new OpenDocumentStep { FilePath = _txtDocumentPath.Text };

            if (_routineService.ValidateStep(step, out string errorMessage))
                return true;

            _showValidationMessage(errorMessage, showMessageBox);

            _txtDocumentPath.Focus();
            return false;
        }

        public void UpdateAutoContinueAvailability(ToggleSwitch autoContinueToggle)
        {
            autoContinueToggle.Enabled = true;
        }
    }
}
