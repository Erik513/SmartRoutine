using Microsoft.Web.WebView2.WinForms;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Controls;
using SmartRoutine.UI.Helpers;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    public partial class ExecutionForm : SmartRoutineForm
    {
        private readonly Routine _routine;
        private readonly RoutineStep _specificStep;
        private readonly Func<RoutineStep, StepExecutionResult> _onStepExecute;
        private RoutineExecutionSession _session;

        // WebView für interne URL-Anzeige
        private WebView2 _webView;
        private Panel _contentPanel;

        // Footer Controls
        private Label _stepCounterLabel;
        private Label _stepNameLabel;
        private Label _infoLabel;
        private Button _prevBtn, _nextBtn, _executeBtn;
        private ToolTip _toolTip;
        private InfoPopupForm _infoPopup;
        private string _pendingToastMessage;

        // Konstruktor für komplette Routine
        public ExecutionForm(
            Routine routine,
            Func<RoutineStep, StepExecutionResult> onExecute)
            : base(
                title: $"Routine: {routine.Name}")
        {
            InitializeForm();
            _routine = routine;
            _specificStep = null;
            _onStepExecute = onExecute;
            InitializeExecution();
        }

        // Konstruktor für einzelnen Step
        public ExecutionForm(
            Routine routine,
            RoutineStep step,
            Func<RoutineStep, StepExecutionResult> onExecute)
            : base(
                title: $"Routine: {routine.Name}")
        {
            InitializeForm();
            _routine = routine;
            _specificStep = step;
            _onStepExecute = onExecute;
            InitializeExecution();
        }

        private void ConfigureForm()
        {
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(600, 450);
            Size = new Size(900, 700);
            BackColor = UIStyles.Colors.BackgroundDark;
        }

        private void InitializeForm()
        {
            ConfigureForm();

            _toolTip = new ToolTip();
            _infoPopup = new InfoPopupForm("Beschreibung:");

            Shown += (s, e) =>
            {
                if (!string.IsNullOrEmpty(_pendingToastMessage))
                {
                    ToastForm.ShowToast(_pendingToastMessage, this);
                    _pendingToastMessage = null;
                }
            };
        }

        private void InitializeExecution()
        {
            _session = new RoutineExecutionSession(_routine, _specificStep);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundMedium
            };

            mainLayout.Controls.Add(_contentPanel, 0, 0);
            mainLayout.Controls.Add(CreateFooterPanel(), 0, 1);

            ContentPanel.Controls.Clear();
            ContentPanel.Controls.Add(mainLayout);

            LoadCurrentStep();
        }
        private Panel CreateFooterPanel()
        {
            var footer = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 60,
                BackColor = UIStyles.Colors.BackgroundDark,
                Padding = new Padding(10, 8, 10, 8)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35));   // Info-Icon
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65));   // Step-Counter
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // Step-Name
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // Navigation

            _infoLabel = new Label
            {
                Text = "ⓘ",
                ForeColor = UIStyles.Colors.TextSecondary,
                Font = new Font("Segoe UI", 12),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Cursor = Cursors.Help,
                Margin = new Padding(0)
            };

            _infoLabel.MouseEnter += (s, e) => ShowStepInfo();
            _infoLabel.MouseLeave += (s, e) => _infoPopup.Hide();
            _toolTip.SetToolTip(_infoLabel, "Schritt-Details anzeigen");

            _stepCounterLabel = new Label
            {
                Text = "1/1",
                ForeColor = UIStyles.Colors.TextSecondary,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            _stepNameLabel = new Label
            {
                Text = "",
                ForeColor = UIStyles.Colors.TextPrimary,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                Margin = new Padding(0)
            };

            var navPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            navPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            navPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            navPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            navPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

            _prevBtn = UIStyles.Buttons.CreatePrimary("⏮", "Vorheriger Schritt", new Size(50,50), isIcon: true);
            _prevBtn.Click += (s, e) => NavigateToPreviousStep();

            _executeBtn = UIStyles.Buttons.CreateGreen("▶", "Schritt ausführen", new Size(50, 50), isIcon: true);
            _executeBtn.Click += (s, e) => ExecuteCurrentStep();

            _nextBtn = UIStyles.Buttons.CreatePrimary("⏭", "Nächster Schritt",  new Size(50, 50), isIcon: true);
            _nextBtn.Click += (s, e) => NavigateToNextStep();
            
            navPanel.Controls.Add(_prevBtn, 0, 0);
            navPanel.Controls.Add(_executeBtn, 1, 0);
            navPanel.Controls.Add(_nextBtn, 2, 0);

            layout.Controls.Add(_infoLabel, 0, 0);
            layout.Controls.Add(_stepCounterLabel, 1, 0);
            layout.Controls.Add(_stepNameLabel, 2, 0);
            layout.Controls.Add(navPanel, 3, 0);

            footer.Controls.Add(layout);

            return footer;
        }


        private Button CreateFooterButton(string text, string tooltip)
        {
            var button = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = UIStyles.Colors.BackgroundLight,
                ForeColor = UIStyles.Colors.TextPrimary,
                Text = text,
                Font = new Font("Segoe UI", 12),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Margin = new Padding(3, 2, 3, 2),
                Padding = new Padding(0)
            };

            button.FlatAppearance.BorderSize = 0;
            _toolTip.SetToolTip(button, tooltip);

            return button;
        }

        private void LoadCurrentStep()
        {
            var currentStep = _session.CurrentStep;

            _stepCounterLabel.Text = _session.StepCounterText;
            _stepNameLabel.Text = currentStep.Name;

            _prevBtn.Enabled = _session.CanGoPrevious;
            _nextBtn.Enabled = _session.CanGoNext;

            bool canExecuteManually = _session.CanExecuteCurrentStepManually;

            _executeBtn.Enabled = canExecuteManually;
            _executeBtn.BackColor = canExecuteManually
                ? UIStyles.Colors.Green
                : UIStyles.Colors.BackgroundLight;

            _executeBtn.ForeColor = canExecuteManually
                ? UIStyles.Colors.White
                : UIStyles.Colors.TextSecondary;

            StopWebViewAudio();
            LoadStepContent(currentStep);

            if (_session.ShouldAutoExecuteCurrentStep)
            {
                ExecuteCurrentStep();
            }
        }

        private void StopWebViewAudio()
        {
            if (_webView?.CoreWebView2 == null)
                return;

            try
            {
                _webView.CoreWebView2.Stop();
                _webView.CoreWebView2.Navigate("about:blank");
            }
            catch
            {
                // WebView ist evtl. gerade noch nicht bereit oder schon disposed
            }
        }
        private void LoadStepContent(RoutineStep step)
        {
            _contentPanel.Controls.Clear();
            ShowEmptyPanel();
        }

        private void ShowEmptyPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundMedium
            };

            _contentPanel.Controls.Add(panel);
        }

        private async void ShowWebView(string url)
        {
            if (_webView == null)
            {
                _webView = new WebView2
                {
                    Dock = DockStyle.Fill
                };

                await _webView.EnsureCoreWebView2Async();
            }

            if (!_contentPanel.Controls.Contains(_webView))
            {
                _contentPanel.Controls.Clear();
                _contentPanel.Controls.Add(_webView);
            }

            _webView.CoreWebView2.Navigate(url);
        }

        private void ShowStepInfo()
        {
            var step = _session.CurrentStep;

            _infoPopup.ShowInfo(step.Description, _infoLabel);
        }

        private void NavigateToPreviousStep()
        {
            _session.GoPrevious();
            LoadCurrentStep();
        }

        private void NavigateToNextStep()
        {
            _session.GoNext();
            LoadCurrentStep();
        }

        private void ExecuteCurrentStep()
        {
            var step = _session.CurrentStep;

            var result = _onStepExecute?.Invoke(step);
            _session.MarkCurrentStepExecuted();

            if (result?.ShouldOpenInInternalBrowser == true)
            {
                ShowWebView(result.InternalBrowserUrl);
            }

            _executeBtn.Enabled = true;
            _executeBtn.BackColor = UIStyles.Colors.Green;
            _executeBtn.ForeColor = UIStyles.Colors.White;

            string message = GetSuccessMessage(step);
            ShowToastWhenReady(message);
        }

        private void ShowToastWhenReady(string message)
        {
            if (!IsHandleCreated || !Visible)
            {
                _pendingToastMessage = message;
                return;
            }

            BeginInvoke(new Action(() =>
            {
                ToastForm.ShowToast(message, this);
            }));
        }

        private string GetSuccessMessage(RoutineStep step)
        {
            if (step is OpenUrlStep urlStep)
                return $"✓ URL geöffnet: {urlStep.Url}";
            if (step is OpenFolderStep folderStep)
                return $"✓ Ordner geöffnet: {folderStep.FolderPath}";
            if (step is OpenApplicationStep appStep)
                return $"✓ Gestartet: {System.IO.Path.GetFileName(appStep.ApplicationPath)}";
            if (step is OpenDocumentStep docStep)
                return $"✓ Geöffnet: {System.IO.Path.GetFileName(docStep.FilePath)}";
            return $"✓ {step.Name} ausgeführt";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAndDisposeWebView();
            base.OnFormClosing(e);
        }

        private void StopAndDisposeWebView()
        {
            if (_webView == null)
                return;

            try
            {
                if (_webView.CoreWebView2 != null)
                {
                    _webView.CoreWebView2.Stop();
                    _webView.CoreWebView2.Navigate("about:blank");
                }

                _webView.Dispose();
                _webView = null;
            }
            catch
            {
                // Falls WebView beim Schließen schon disposed ist
            }
        }
    }
}