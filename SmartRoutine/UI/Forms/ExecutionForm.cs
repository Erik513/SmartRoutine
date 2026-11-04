using ErikwnkWFUI;
using ErikwnkWFUI.Forms;
using ErikwnkWFUI.Styles;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    public partial class ExecutionForm : AppForm
    {
        public event EventHandler BeforeCloseRequested;

        private const int FooterHeight = 60;
        private const int FooterInfoColumnWidth = 35;
        private const int FooterCounterColumnWidth = 65;
        private const int NavigationButtonSize = 50;
        private const int AutoContinueSeconds = 2;
        private const int BringToFrontMilliseconds = 200;

        private readonly Routine _routine;
        private readonly RoutineStep _specificStep;
        private readonly Func<RoutineStep, StepExecutionResult> _onStepExecute;

        private RoutineExecutionSession _session;

        private Panel _stepContentPanel;

        private Label _stepCounterLabel;
        private Label _stepNameLabel;
        private Label _infoLabel;

        private Button _previousButton;
        private Button _nextButton;
        private Button _executeButton;

        private InfoPopupForm _infoPopup;
        private string _pendingToastMessage;
        private string _pendingErrorMessage;
        private bool _isExecuting;
        private Timer _bringToFrontTimer;

        private Timer _autoContinueTimer;
        private int _autoContinueSecondsLeft;

        public ExecutionForm(
            Routine routine,
            Func<RoutineStep, StepExecutionResult> onExecute)
            : this(routine, null, onExecute)
        {
        }

        public ExecutionForm(
            Routine routine,
            RoutineStep step,
            Func<RoutineStep, StepExecutionResult> onExecute)
            : base(FormOptions.CreateStandard(
                routine != null ? $"Routine: {routine.Name}" : "Routine",
                ContentAlignment.MiddleCenter,
                UIColors.BackgroundDarkElevated,
                Properties.Resources.IconLogo))
        {
            _routine = routine;
            _specificStep = step;
            _onStepExecute = onExecute;

            InitializeForm();
            InitializeExecution();
        }

        private void InitializeForm()
        {
            ConfigureForm();

            _infoPopup = UIStyles.InfoPopups.CreateStandard();
        }

        private void ConfigureForm()
        {
            MinimumSize = new Size(600, 450);
            Size = new Size(1200, 800);
            BackColor = UIStyles.Colors.BackgroundDark;
            ShowIcon = true;
            CenterToScreen();
        }

        private void InitializeExecution()
        {
            _session = new RoutineExecutionSession(_routine, _specificStep);

            TableLayoutPanel mainLayout = CreateMainLayout();

            _stepContentPanel = CreateStepContentPanel();

            mainLayout.Controls.Add(_stepContentPanel, 0, 0);
            mainLayout.Controls.Add(CreateFooterPanel(), 0, 1);

            ContentPanel.Controls.Clear();
            ContentPanel.Controls.Add(mainLayout);

            LoadCurrentStep();
        }

        private TableLayoutPanel CreateMainLayout()
        {
            TableLayoutPanel layout = UIStyles.TableLayoutPanels.CreateStandard(1,2);
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Color.Transparent;

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, FooterHeight));

            return layout;
        }

        private Panel CreateStepContentPanel()
        {
            return UIStyles.Panels.CreateMedium();
        }

        private Panel CreateFooterPanel()
        {
            Panel footer = UIStyles.Panels.CreateDark();
            footer.Height = FooterHeight;
            footer.Padding = new Padding(10, 8, 10, 8);
            footer.Margin = new Padding(0);

            TableLayoutPanel layout = CreateFooterLayout();

            _infoLabel = CreateInfoLabel();
            _stepCounterLabel = CreateStepCounterLabel();
            _stepNameLabel = CreateStepNameLabel();

            layout.Controls.Add(_infoLabel, 0, 0);
            layout.Controls.Add(_stepCounterLabel, 1, 0);
            layout.Controls.Add(_stepNameLabel, 2, 0);
            layout.Controls.Add(CreateNavigationPanel(), 3, 0);

            footer.Controls.Add(layout);

            return footer;
        }

        private TableLayoutPanel CreateFooterLayout()
        {
            TableLayoutPanel layout = UIStyles.TableLayoutPanels.CreateStandard(4, 1);
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Color.Transparent;

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, FooterInfoColumnWidth));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, FooterCounterColumnWidth));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            return layout;
        }

        // The info symbol of the library; it shows the step details in a popup while the mouse is over it
        // (no tooltip: the popup says it, and a click does nothing, so it is a label and not a button).
        private Label CreateInfoLabel()
        {
            Label label = UIStyles.Labels.CreateStandard(UIStyles.Symbols.Info);

            label.Dock = DockStyle.Fill;
            label.ForeColor = UIStyles.Colors.TextSecondary;
            label.Font = new Font(UIStyles.Symbols.GetFontName(UIStyles.Symbols.Info), 14f);
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Cursor = Cursors.Help;
            label.Margin = new Padding(0);

            label.MouseEnter += OnInfoLabelMouseEnter;
            label.MouseLeave += OnInfoLabelMouseLeave;

            return label;
        }

        private Label CreateStepCounterLabel()
        {
            Label label = UIStyles.Labels.CreateStandard("1/1");

            label.Dock = DockStyle.Fill;
            label.ForeColor = UIStyles.Colors.TextSecondary;
            label.Font = UIStyles.Fonts.Title;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.BackColor = Color.Transparent;
            label.Margin = new Padding(0);

            return label;
        }

        private Label CreateStepNameLabel()
        {
            Label label = UIStyles.Labels.CreateTitle("");

            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.BackColor = Color.Transparent;
            label.AutoEllipsis = true;
            label.Margin = new Padding(0);

            return label;
        }

        private TableLayoutPanel CreateNavigationPanel()
        {
            TableLayoutPanel navigationPanel = UIStyles.TableLayoutPanels.CreateStandard(3, 1);
            navigationPanel.Dock = DockStyle.Fill;
            navigationPanel.BackColor = Color.Transparent;
            navigationPanel.AutoSize = true;

            navigationPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            navigationPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, NavigationButtonSize));
            navigationPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, NavigationButtonSize));
            navigationPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, NavigationButtonSize));

            _previousButton = CreateNavigationButton(UIStyles.Buttons.Presets.SkipBack, "Vorheriger Schritt", OnPreviousButtonClick);
            _executeButton = CreateNavigationButton(UIStyles.Buttons.Presets.Play, "Schritt ausführen", OnExecuteButtonClick);
            _nextButton = CreateNavigationButton(UIStyles.Buttons.Presets.SkipForward, "Nächster Schritt", OnNextButtonClick);

            navigationPanel.Controls.Add(_previousButton, 0, 0);
            navigationPanel.Controls.Add(_executeButton, 1, 0);
            navigationPanel.Controls.Add(_nextButton, 2, 0);

            return navigationPanel;
        }

        private Button CreateNavigationButton(UIButtonPreset preset, string tooltip, EventHandler clickHandler)
        {
            Button button = UIStyles.Buttons.CreateSymbol(
                preset,
                tooltip,
                new Size(NavigationButtonSize, NavigationButtonSize));

            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(3, 0, 3, 0);
            button.Click += clickHandler;

            return button;
        }

        private void LoadCurrentStep()
        {
            if (_session == null || _session.CurrentStep == null)
                return;

            RoutineStep currentStep = _session.CurrentStep;

            UpdateFooter(currentStep);
            StopWebViewAudio();
            LoadStepContent(currentStep);

            if (_session.ShouldAutoExecuteCurrentStep)
                ExecuteCurrentStep();
        }

        private void UpdateFooter(RoutineStep currentStep)
        {
            _stepCounterLabel.Text = _session.StepCounterText;
            _stepNameLabel.Text = currentStep.Name ?? "";

            _previousButton.Enabled = _session.CanGoPrevious;

            UpdateNextButtonState();
            UpdateExecuteButtonState();
        }

        private void UpdateNextButtonState()
        {
            // On the last step the button ends the routine.
            bool canGoNext = _session.CanGoNext;

            _nextButton.Text = canGoNext ? UIStyles.Symbols.SkipForward : UIStyles.Symbols.Check;
            UIStyles.Buttons.UpdateTooltip(_nextButton, canGoNext ? "Nächster Schritt" : "Routine beenden");
            _nextButton.Enabled = true;
        }

        private void UpdateExecuteButtonState()
        {
            // The symbol button draws its disabled look itself.
            _executeButton.Enabled = _session.CanExecuteCurrentStepManually;
        }

        private void LoadStepContent(RoutineStep step)
        {
            _stepContentPanel.Controls.Clear();
            ShowEmptyPanel();
        }

        private void ShowEmptyPanel()
        {
            Panel panel = UIStyles.Panels.CreateMedium();
            panel.Dock = DockStyle.Fill;

            _stepContentPanel.Controls.Add(panel);
        }

        private void ExecuteCurrentStep()
        {
            if (_session == null || _session.CurrentStep == null)
                return;

            // A second click while the step is still being started must not start it twice.
            if (_isExecuting)
                return;

            RoutineStep step = _session.CurrentStep;

            StepExecutionResult result = null;

            _isExecuting = true;

            try
            {
                if (_onStepExecute != null)
                    result = _onStepExecute(step);
            }
            catch (Exception ex)
            {
                // The step failed (a file, folder or program that is gone, no permission ...): it is not
                // marked as executed, nothing continues by itself, and the user is told.
                ReportExecutionFailure(step, ex);
                return;
            }
            finally
            {
                _isExecuting = false;
            }

            _session.MarkCurrentStepExecuted();

            if (result != null && result.ShouldOpenInInternalBrowser)
                ShowWebView(result.InternalBrowserUrl);

            UpdateExecuteButtonAsExecuted();

            ShowToastWhenReady(GetSuccessMessage(step));

            if (ShouldAutoContinue(step, result))
            {
                StartAutoContinueCountdown();
            }
            KeepExecutionFormInFront();
        }

        private void KeepExecutionFormInFront()
        {
            TopMost = true;
            BringToFront();
            Activate();

            StopBringToFrontTimer();

            _bringToFrontTimer = new Timer();
            _bringToFrontTimer.Interval = BringToFrontMilliseconds;
            _bringToFrontTimer.Tick += (s, e) =>
            {
                StopBringToFrontTimer();

                if (!IsDisposed)
                    TopMost = false;
            };

            _bringToFrontTimer.Start();
        }

        private void StopBringToFrontTimer()
        {
            if (_bringToFrontTimer == null)
                return;

            _bringToFrontTimer.Stop();
            _bringToFrontTimer.Dispose();
            _bringToFrontTimer = null;
        }

        private void ReportExecutionFailure(RoutineStep step, Exception ex)
        {
            _pendingErrorMessage =
                $"Der Schritt '{step?.Name}' konnte nicht ausgeführt werden:\n\n{ex.Message}";

            // Shown at once when the window is up; when the step ran while the window was still being
            // built (an auto-start step), OnShown shows it.
            if (IsHandleCreated && Visible)
                ShowPendingErrorIfNeeded();
        }

        private void ShowPendingErrorIfNeeded()
        {
            if (string.IsNullOrWhiteSpace(_pendingErrorMessage))
                return;

            string message = _pendingErrorMessage;
            _pendingErrorMessage = null;

            ErikwnkWFUI.Forms.MessageBox.Show(
                message,
                "Ausführung fehlgeschlagen",
                ErikwnkWFUI.Forms.MessageBoxButtons.OK,
                ErikwnkWFUI.Forms.MessageBoxIcon.Warning,
                this,
                FormSize.Medium);
        }

        private bool ShouldAutoContinue(
            RoutineStep step,
            StepExecutionResult result)
        {
            if (step == null)
                return false;

            if (!step.AutoContinue)
                return false;

            if (!_session.CanGoNext)
                return false;

            if (result != null && result.ShouldOpenInInternalBrowser)
                return false;

            return true;
        }

        private void StartAutoContinueCountdown()
        {
            StopAutoContinueCountdown();

            _autoContinueSecondsLeft = AutoContinueSeconds;

            UpdateAutoContinueText();

            _autoContinueTimer = new Timer();
            _autoContinueTimer.Interval = 1000;
            _autoContinueTimer.Tick += OnAutoContinueTimerTick;
            _autoContinueTimer.Start();

            SetNavigationEnabled(false);
        }

        private void OnAutoContinueTimerTick(object sender, EventArgs e)
        {
            _autoContinueSecondsLeft--;

            if (_autoContinueSecondsLeft > 0)
            {
                UpdateAutoContinueText();
                return;
            }

            StopAutoContinueCountdown();

            if (_session != null && _session.CanGoNext)
                NavigateToNextStep();
        }

        private void UpdateAutoContinueText()
        {
            _stepNameLabel.Text =
                $"Weiter in {_autoContinueSecondsLeft} Sekunden...";
        }

        private void StopAutoContinueCountdown()
        {
            if (_autoContinueTimer == null)
                return;

            _autoContinueTimer.Stop();
            _autoContinueTimer.Tick -= OnAutoContinueTimerTick;
            _autoContinueTimer.Dispose();
            _autoContinueTimer = null;

            SetNavigationEnabled(true);
        }

        private void SetNavigationEnabled(bool enabled)
        {
            if (_previousButton != null)
                _previousButton.Enabled = enabled && _session.CanGoPrevious;

            if (_nextButton != null)
                _nextButton.Enabled = enabled;

            if (_executeButton != null)
                _executeButton.Enabled = enabled;
        }

        private void UpdateExecuteButtonAsExecuted()
        {
            _executeButton.Enabled = true;
        }

        private void NavigateToPreviousStep()
        {
            StopAutoContinueCountdown();
            _session.GoPrevious();
            LoadCurrentStep();
        }

        private void NavigateToNextStep()
        {
            StopAutoContinueCountdown();
            _session.GoNext();
            LoadCurrentStep();
        }

        private void ShowStepInfo()
        {
            if (_session == null || _session.CurrentStep == null || _infoPopup == null)
                return;

            string description = _session.CurrentStep.Description;

            _infoPopup.SetContent(
                "Beschreibung",
                string.IsNullOrWhiteSpace(description) ? "Keine Beschreibung" : description);
            _infoPopup.ShowNextTo(_infoLabel);
        }

        private void ShowToastWhenReady(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (!IsHandleCreated || !Visible)
            {
                _pendingToastMessage = message;
                return;
            }

            BeginInvoke(new Action(delegate
            {
                ToastForm.ShowToast(message, this);
            }));
        }

        private void ShowPendingToastIfNeeded()
        {
            if (string.IsNullOrWhiteSpace(_pendingToastMessage))
                return;

            string message = _pendingToastMessage;
            _pendingToastMessage = null;

            ToastForm.ShowToast(message, this);
        }

        private static string GetSuccessMessage(RoutineStep step)
        {
            OpenUrlStep urlStep = step as OpenUrlStep;
            if (urlStep != null)
                return $"✓ URL geöffnet: {urlStep.Url}";

            OpenFolderStep folderStep = step as OpenFolderStep;
            if (folderStep != null)
                return $"✓ Ordner geöffnet: {folderStep.FolderPath}";

            OpenApplicationStep appStep = step as OpenApplicationStep;
            if (appStep != null)
                return $"✓ Gestartet: {Path.GetFileName(appStep.ApplicationPath)}";

            OpenDocumentStep documentStep = step as OpenDocumentStep;
            if (documentStep != null)
                return $"✓ Geöffnet: {Path.GetFileName(documentStep.FilePath)}";

            return $"✓ {step.Name} ausgeführt";
        }

        private void OnInfoLabelMouseEnter(object sender, EventArgs e)
        {
            ShowStepInfo();
        }

        private void OnInfoLabelMouseLeave(object sender, EventArgs e)
        {
            if (_infoPopup != null)
                _infoPopup.Hide();
        }

        private void OnPreviousButtonClick(object sender, EventArgs e)
        {
            NavigateToPreviousStep();
        }

        private void OnNextButtonClick(object sender, EventArgs e)
        {
            if (_session == null)
                return;

            if (_session.CanGoNext)
            {
                NavigateToNextStep();
                return;
            }

            Close();
        }

        private void OnExecuteButtonClick(object sender, EventArgs e)
        {
            ExecuteCurrentStep();
        }
        private void DisposePopup()
        {
            if (_infoPopup == null)
                return;

            _infoPopup.Dispose();
            _infoPopup = null;
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            TopMost = true;
            BringToFront();
            Activate();
            TopMost = false;

            ShowPendingToastIfNeeded();
            ShowPendingErrorIfNeeded();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            BeforeCloseRequested?.Invoke(this, EventArgs.Empty);
            StopAndDisposeWebView();
            DisposePopup();
            StopAutoContinueCountdown();
            StopBringToFrontTimer();

            base.OnFormClosing(e);
        }
    }
}