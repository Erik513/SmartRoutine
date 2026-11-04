using ErikwnkWFUI.Controls;
using ErikwnkWFUI.Forms;
using MessageBox = ErikwnkWFUI.Forms.MessageBox;
using MessageBoxButtons = ErikwnkWFUI.Forms.MessageBoxButtons;
using MessageBoxIcon = ErikwnkWFUI.Forms.MessageBoxIcon;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FlowLayoutPanel = System.Windows.Forms.FlowLayoutPanel;
using Panel = System.Windows.Forms.Panel;
using TableLayoutPanel = System.Windows.Forms.TableLayoutPanel;

namespace SmartRoutine.UI.Controls
{
    // Executes the step that is being edited (single step run and auto run).
    public partial class RoutineEditorUC
    {
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

            MessageBox.Show(
                validationError,
                "Ausführung nicht möglich",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                FindForm(),
                FormSize.Small);

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
    }
}
