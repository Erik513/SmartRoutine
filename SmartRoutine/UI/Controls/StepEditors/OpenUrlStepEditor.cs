using CustomWFUI;
using CustomWFUI.Controls;
using CustomWFUI.Helpers;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using System;
using System.Windows.Forms;

namespace SmartRoutine.UI.Controls.StepEditors
{
    internal class OpenUrlStepEditor : IStepTypeEditor
    {
        private readonly IUrlValidationService _urlValidationService;
        private readonly Action<string, bool> _showValidationMessage;
        private readonly ToolTip _errorToolTip;
        private readonly Action _onAutoContinueAvailabilityMightChange;

        private TextBox _txtUrl;
        private ToggleSwitch _tglOpenInExternBrowser;

        public StepType Type => StepType.OpenUrl;

        public OpenUrlStepEditor(
            IUrlValidationService urlValidationService,
            Action<string, bool> showValidationMessage,
            ToolTip errorToolTip,
            Action onAutoContinueAvailabilityMightChange)
        {
            _urlValidationService = urlValidationService;
            _showValidationMessage = showValidationMessage;
            _errorToolTip = errorToolTip;
            _onAutoContinueAvailabilityMightChange = onAutoContinueAvailabilityMightChange;
        }

        public void BuildControls(StyledPropertyTable optionsTable)
        {
            _txtUrl = UIStyles.TextBoxes.CreateBorderstyleNone();
            _txtUrl.Name = "txtUrl";

            _txtUrl.TextChanged += TxtUrl_TextChanged;
            _txtUrl.LostFocus += TxtUrl_LostFocus;

            TextDragDropHelper.EnableTextDragDrop(_txtUrl, droppedText =>
            {
                string cleanedText = droppedText.Trim();

                _txtUrl.Text = cleanedText;
                TxtUrl_TextChanged(_txtUrl, EventArgs.Empty);
                _txtUrl.SelectionStart = _txtUrl.Text.Length;
            });

            _tglOpenInExternBrowser = UIStyles.ToggleSwitches.CreateStandard(
                true,
                "Externer Browser",
                "In App öffnen");

            _tglOpenInExternBrowser.CheckedChanged += (s, e) =>
                _onAutoContinueAvailabilityMightChange?.Invoke();

            _tglOpenInExternBrowser.Name = "tglOpenInternally";
            _tglOpenInExternBrowser.Anchor = AnchorStyles.Left;

            optionsTable.AddRow("URL", _txtUrl);
            optionsTable.AddRow("Öffnen in", _tglOpenInExternBrowser);
        }

        public void LoadFrom(RoutineStep step)
        {
            var urlStep = step as OpenUrlStep;

            if (urlStep == null)
                return;

            _txtUrl.Text = urlStep.Url;
            _tglOpenInExternBrowser.Checked = urlStep.OpenInExternalBrowser;
        }

        public void NormalizeForComparison(RoutineStep step)
        {
            var urlStep = step as OpenUrlStep;

            if (urlStep == null)
                return;

            urlStep.Url = NormalizeUrl(urlStep.Url);

            if (!urlStep.OpenInExternalBrowser)
                urlStep.AutoContinue = false;
        }

        private string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "";

            var result = _urlValidationService.ValidateAndRepairUrl(url, false);

            if (!result.IsValid)
                return url.Trim();

            return result.RepairedUrl ?? "";
        }

        public RoutineStep CreateStep()
        {
            var urlResult = _urlValidationService.ValidateAndRepairUrl(_txtUrl?.Text, false);

            if (!urlResult.IsValid)
                return null;

            return new OpenUrlStep
            {
                Url = urlResult.RepairedUrl,
                OpenInExternalBrowser = _tglOpenInExternBrowser?.Checked ?? true
            };
        }

        public bool Validate(bool showMessageBox)
        {
            if (string.IsNullOrWhiteSpace(_txtUrl?.Text))
            {
                _showValidationMessage("Bitte geben Sie eine URL ein.", showMessageBox);

                _txtUrl?.Focus();
                return false;
            }

            var urlResult = _urlValidationService.ValidateAndRepairUrl(_txtUrl.Text, false);

            if (urlResult.IsValid)
                return true;

            _showValidationMessage(urlResult.ErrorMessage, showMessageBox);

            _txtUrl.Focus();
            return false;
        }

        public void UpdateAutoContinueAvailability(ToggleSwitch autoContinueToggle)
        {
            bool isInternalUrl = _tglOpenInExternBrowser != null && !_tglOpenInExternBrowser.Checked;

            autoContinueToggle.Enabled = !isInternalUrl;

            if (isInternalUrl)
                autoContinueToggle.Checked = false;
        }

        private void TxtUrl_TextChanged(object sender, EventArgs e)
        {
            string url = _txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                _txtUrl.ForeColor = UIStyles.Colors.TextPrimary;
                _errorToolTip.SetToolTip(_txtUrl, "");
                return;
            }

            var result = _urlValidationService.ValidateAndRepairUrl(url, false);

            if (result.IsValid)
            {
                _txtUrl.ForeColor = UIStyles.Colors.TextPrimary;
                _errorToolTip.SetToolTip(_txtUrl, "");

                if (result.RepairedUrl != url && result.RepairedUrl != _txtUrl.Tag as string)
                {
                    _txtUrl.Tag = result.RepairedUrl;
                }
            }
            else
            {
                _txtUrl.ForeColor = UIStyles.Colors.Red;
                _errorToolTip.SetToolTip(_txtUrl, result.ErrorMessage);
            }
        }

        private void TxtUrl_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtUrl.Text))
                return;

            var result = _urlValidationService.ValidateAndRepairUrl(_txtUrl.Text, false);

            if (result.IsValid)
            {
                if (result.RepairedUrl != _txtUrl.Text)
                {
                    _txtUrl.Text = result.RepairedUrl;
                }

                _txtUrl.ForeColor = UIStyles.Colors.TextPrimary;
                _errorToolTip.SetToolTip(_txtUrl, "");
            }
            else
            {
                _txtUrl.ForeColor = UIStyles.Colors.Red;
                _errorToolTip.SetToolTip(_txtUrl, result.ErrorMessage);
            }
        }
    }
}
