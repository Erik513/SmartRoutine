using ErikwnkWFUI.Controls;
using ErikwnkWFUI.Forms;
using MessageBox = ErikwnkWFUI.Forms.MessageBox;
using MessageBoxButtons = ErikwnkWFUI.Forms.MessageBoxButtons;
using MessageBoxIcon = ErikwnkWFUI.Forms.MessageBoxIcon;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Controls.StepEditors;
using SmartRoutine.UI.Forms;
using SmartRoutine.UI.Helpers;
using SmartRoutine.Localization;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using ComboBox = System.Windows.Forms.ComboBox;
using FlowLayoutPanel = System.Windows.Forms.FlowLayoutPanel;
using Panel = System.Windows.Forms.Panel;
using TableLayoutPanel = System.Windows.Forms.TableLayoutPanel;

namespace SmartRoutine.UI.Controls
{
    // Validation of the routine name and the step input.
    public partial class RoutineEditorUC
    {
        private bool ValidateRoutineName()
        {
            string routineName = txtRoutineName.Text.Trim();

            // Prüfen ob Name leer ist
            if (string.IsNullOrWhiteSpace(routineName))
            {
                if (!AutoConfirmDialogs)
                {
                    MessageBox.Show(
                        Loc.S("Editor.Validation.RoutineNameEmpty"),
                        Loc.S("Editor.Validation.Title"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        FindForm(),
                        FormSize.Small);
                }

                txtRoutineName.BeginEdit();
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
                    MessageBox.Show(
                        Loc.S("Editor.Validation.RoutineNameExists", routineName),
                        Loc.S("Editor.Validation.Title"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        FindForm(),
                        FormSize.Medium);
                }

                txtRoutineName.BeginEdit();
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
                Loc.S("Editor.Validation.StepNameEmpty"),
                showMessageBox);

            txtStepName.Focus();
            return false;
        }
        private bool ValidateStepType(bool showMessageBox)
        {
            if (cmbStepType.SelectedIndex != -1)
                return true;

            ShowValidationMessage(
                Loc.S("Editor.Validation.StepTypeEmpty"),
                showMessageBox);

            cmbStepType.Focus();
            return false;
        }
        private void ShowValidationMessage(
            string message,
            bool showMessageBox,
            string title = null)
        {
            if (!showMessageBox || AutoConfirmDialogs)
                return;

            MessageBox.Show(
                message,
                title ?? Loc.S("Editor.Validation.ErrorTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                FindForm(),
                FormSize.Medium);
        }
    }
}
