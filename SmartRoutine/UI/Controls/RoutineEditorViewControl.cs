using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Controls
{
    public partial class RoutineEditorViewControl : UserControl
    {
        public event EventHandler BackToRoutinesClicked;

        private Button btnBack;

        public RoutineEditorViewControl(IRoutineService routineService)
        {
            // Wichtig: Diese Einstellungen müssen hier sein
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);
            this.BackColor = Color.Black;

            InitializeControl();
        }

        private void InitializeControl()
        {
            // Inneres Panel für den Inhalt
            var innerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundMediumElevated
            };

            this.Controls.Add(innerPanel);

            // Zurück-Button unten rechts
            btnBack = UIStyles.Buttons.CreateStandard("← Zurück", "", new Size(100, 35));
            btnBack.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnBack.Location = new Point(innerPanel.Width - 120, innerPanel.Height - 50);
            btnBack.Click += (s, e) => BackToRoutinesClicked?.Invoke(s, e);

            innerPanel.Controls.Add(btnBack);

            // Button-Position beim Resize aktualisieren
            innerPanel.Resize += (s, e) =>
            {
                btnBack.Location = new Point(innerPanel.Width - 120, innerPanel.Height - 50);
            };
        }
    }
}