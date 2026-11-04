using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.IO;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    // The internal browser of the execution form (a WebView2 that is created when a step wants it).
    public partial class ExecutionForm
    {
        private WebView2 _webView;

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
            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SmartRoutine",
                "WebView2");

            if (!Directory.Exists(userDataFolder))
                Directory.CreateDirectory(userDataFolder);

            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

            await _webView.EnsureCoreWebView2Async(environment);

            _webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }
        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrWhiteSpace(e.Uri))
                return;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri,
                UseShellExecute = true
            });
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
    }
}
