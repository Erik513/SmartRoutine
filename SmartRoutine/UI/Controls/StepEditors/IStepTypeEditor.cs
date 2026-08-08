using CustomWFUI.Controls;
using SmartRoutine.Data.Models;

namespace SmartRoutine.UI.Controls.StepEditors
{
    // Kapselt UI-Aufbau, Laden, Erzeugen und Validieren für genau einen StepType.
    // Jede Implementierung besitzt ihre eigenen Controls und weiß nichts von RoutineEditorUC.
    internal interface IStepTypeEditor
    {
        StepType Type { get; }

        // Baut die typspezifischen Controls und fügt sie der übergebenen Optionen-Tabelle hinzu.
        void BuildControls(StyledPropertyTable optionsTable);

        // Lädt die Werte eines vorhandenen Steps in die Controls.
        void LoadFrom(RoutineStep step);

        // Normalisiert einen Step für den Änderungsvergleich (z.B. Trim, Default-Werte).
        void NormalizeForComparison(RoutineStep step);

        // Baut einen neuen, typspezifischen Step aus den aktuellen Control-Werten.
        // Gemeinsame Felder (Name, Description, ...) werden vom Aufrufer ergänzt.
        // Liefert null, wenn aus den aktuellen Eingaben kein gültiger Step gebaut werden kann.
        RoutineStep CreateStep();

        // Validiert die typspezifischen Felder; zeigt bei Bedarf eine Meldung und setzt den Fokus.
        bool Validate(bool showMessageBox);

        // Aktualisiert die Verfügbarkeit von "Automatisch weiter" (nur für OpenUrl relevant).
        void UpdateAutoContinueAvailability(ToggleSwitch autoContinueToggle);
    }
}
