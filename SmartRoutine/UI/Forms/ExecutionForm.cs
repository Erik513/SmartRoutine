using Microsoft.Web.WebView2.WinForms;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CustomWFUI;
using CustomWFUI.Forms;

namespace SmartRoutine.UI.Forms
{
    public partial class ExecutionForm : StyledForm
    {
        private const int FooterHeight = 60;
        private const int FooterInfoColumnWidth = 35;
        private const int FooterCounterColumnWidth = 65;
        private const int NavigationButtonSize = 50;

        private readonly Routine _routine;
        private readonly RoutineStep _specificStep;
        private readonly Func<RoutineStep, StepExecutionResult> _onStepExecute;

        private RoutineExecutionSession _session;

        private WebView2 _webView;
        private Panel _stepContentPanel;

        private Label _stepCounterLabel;
        private Label _stepNameLabel;
        private Label _infoLabel;

        private Button _previousButton;
        private Button _nextButton;
        private Button _executeButton;

        private ToolTip _toolTip;
        private InfoPopupForm _infoPopup;
        private string _pendingToastMessage;

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
            : base(new StyledFormOptions
            {
                Title = routine != null ? $"Routine: {routine.Name}" : "Routine",
                TitleBarBackColor = UIStyles.Colors.BackgroundBlack
            })
        {
            _routine = routine;
            _specificStep = step;
            _onStepExecute = onExecute;

            InitializeForm();
            InitializeExecution();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ShowPendingToastIfNeeded();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAndDisposeWebView();
            DisposePopup();

            base.OnFormClosing(e);
        }

        private void InitializeForm()
        {
            ConfigureForm();

            _toolTip = new ToolTip();
            _infoPopup = new InfoPopupForm("Beschreibung:");
        }

        private void ConfigureForm()
        {
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(600, 450);
            Size = new Size(900, 700);
            BackColor = UIStyles.Colors.BackgroundDark;
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

        private Label CreateInfoLabel()
        {
            Label label = UIStyles.Labels.CreateNormal("ⓘ");

            label.Dock = DockStyle.Fill;
            label.ForeColor = UIStyles.Colors.TextSecondary;
            label.Font = UIStyles.Fonts.Icon;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.BackColor = Color.Transparent;
            label.Cursor = Cursors.Help;
            label.Margin = new Padding(0);

            label.MouseEnter += OnInfoLabelMouseEnter;
            label.MouseLeave += OnInfoLabelMouseLeave;

            _toolTip.SetToolTip(label, "Schritt-Details anzeigen");

            return label;
        }

        private Label CreateStepCounterLabel()
        {
            Label label = UIStyles.Labels.CreateNormal("1/1");

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

            _previousButton = CreateNavigationButton("⏮", "Vorheriger Schritt", OnPreviousButtonClick);
            _executeButton = CreateExecuteButton();
            _nextButton = CreateNavigationButton("⏭", "Nächster Schritt", OnNextButtonClick);

            navigationPanel.Controls.Add(_previousButton, 0, 0);
            navigationPanel.Controls.Add(_executeButton, 1, 0);
            navigationPanel.Controls.Add(_nextButton, 2, 0);

            return navigationPanel;
        }

        private Button CreateNavigationButton(string text, string tooltip, EventHandler clickHandler)
        {
            Button button = UIStyles.Buttons.CreatePrimary(
                text,
                tooltip,
                new Size(NavigationButtonSize, NavigationButtonSize),
                true);

            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(3, 0, 3, 0);
            button.Click += clickHandler;

            return button;
        }

        private Button CreateExecuteButton()
        {
            Button button = UIStyles.Buttons.CreateGreen(
                "▶",
                "Schritt ausführen",
                new Size(NavigationButtonSize, NavigationButtonSize),
                true);

            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(3, 0, 3, 0);
            button.Click += OnExecuteButtonClick;

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
            _nextButton.Enabled = _session.CanGoNext;

            UpdateExecuteButtonState();
        }

        private void UpdateExecuteButtonState()
        {
            bool canExecuteManually = _session.CanExecuteCurrentStepManually;

            _executeButton.Enabled = canExecuteManually;
            _executeButton.BackColor = canExecuteManually
                ? UIStyles.Colors.Green
                : UIStyles.Colors.BackgroundLight;

            _executeButton.ForeColor = canExecuteManually
                ? UIStyles.Colors.White
                : UIStyles.Colors.TextSecondary;
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

        private async void ShowWebView(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                if (_webView == null)
                    await CreateWebView();

                ShowWebViewControl();
                _webView.CoreWebView2.Navigate(url);
            }
            catch
            {
                ShowToastWhenReady("WebView konnte nicht geöffnet werden.");
            }
        }

        private async System.Threading.Tasks.Task CreateWebView()
        {
            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            await _webView.EnsureCoreWebView2Async();
        }

        private void ShowWebViewControl()
        {
            if (_stepContentPanel.Controls.Contains(_webView))
                return;

            _stepContentPanel.Controls.Clear();
            _stepContentPanel.Controls.Add(_webView);
        }

        private void StopWebViewAudio()
        {
            if (_webView == null || _webView.CoreWebView2 == null)
                return;

            try
            {
                _webView.CoreWebView2.Stop();
                _webView.CoreWebView2.Navigate("about:blank");
            }
            catch
            {
            }
        }

        private void StopAndDisposeWebView()
        {
            if (_webView == null)
                return;

            try
            {
                StopWebViewAudio();
                _webView.Dispose();
                _webView = null;
            }
            catch
            {
            }
        }

        private void ExecuteCurrentStep()
        {
            if (_session == null || _session.CurrentStep == null)
                return;

            RoutineStep step = _session.CurrentStep;

            StepExecutionResult result = null;

            if (_onStepExecute != null)
                result = _onStepExecute(step);

            _session.MarkCurrentStepExecuted();

            if (result != null && result.ShouldOpenInInternalBrowser)
                ShowWebView(result.InternalBrowserUrl);

            UpdateExecuteButtonAsExecuted();

            ShowToastWhenReady(GetSuccessMessage(step));
        }

        private void UpdateExecuteButtonAsExecuted()
        {
            _executeButton.Enabled = true;
            _executeButton.BackColor = UIStyles.Colors.Green;
            _executeButton.ForeColor = UIStyles.Colors.White;
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

        private void ShowStepInfo()
        {
            if (_session == null || _session.CurrentStep == null || _infoPopup == null)
                return;

            _infoPopup.ShowInfo(_session.CurrentStep.Description, _infoLabel);
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

        private string GetSuccessMessage(RoutineStep step)
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

        private void DisposePopup()
        {
            if (_infoPopup == null)
                return;

            _infoPopup.Dispose();
            _infoPopup = null;
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
            NavigateToNextStep();
        }

        private void OnExecuteButtonClick(object sender, EventArgs e)
        {
            ExecuteCurrentStep();
        }
    }
}