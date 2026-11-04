using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using ErikwnkWFUI.Forms;
using MessageBox = ErikwnkWFUI.Forms.MessageBox;
using MessageBoxButtons = ErikwnkWFUI.Forms.MessageBoxButtons;
using MessageBoxIcon = ErikwnkWFUI.Forms.MessageBoxIcon;
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
                        "Bitte geben Sie einen Namen für die Routine ein.",
                        "Validierung",
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
                        $"Eine Routine mit dem Namen '{routineName}' existiert bereits.\nBitte wählen Sie einen anderen Namen.",
                        "Validierung",
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

            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                FindForm(),
                FormSize.Medium);
        }
    }
}
