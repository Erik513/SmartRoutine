using ErikwnkWFUI.Controls;
using SmartRoutine.Data.Models;

namespace SmartRoutine.UI.Controls.StepEditors
{
    // Wraps building the controls, loading, creating and validating for exactly one StepType.
    // Each implementation owns its controls and knows nothing about RoutineEditorUC.
    internal interface IStepTypeEditor
    {
        StepType Type { get; }

        // Builds the controls that are specific to the type and adds them to the given options table.
        void BuildControls(PropertyTable optionsTable);

        // Loads the values of an existing step into the controls.
        void LoadFrom(RoutineStep step);

        // Normalizes a step for the comparison of changes (for example trim, default values).
        void NormalizeForComparison(RoutineStep step);

        // Builds a new step of this type from the current values of the controls.
        // The common fields (Name, Description, ...) are added by the caller.
        // Returns null when no valid step can be built from the current input.
        RoutineStep CreateStep();

        // Validates the fields of this type; shows a message if needed and sets the focus.
        bool Validate(bool showMessageBox);

        // Updates whether "continue automatically" is available (only relevant for OpenUrl).
        void UpdateAutoContinueAvailability(ToggleSwitch autoContinueToggle);
    }
}
