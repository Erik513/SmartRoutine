using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using SmartRoutine.Localization;
using SmartRoutine.Logic.Services;
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
            catch (Exception ex)
            {
                ErrorLog.Write("The internal browser could not be opened: " + url, ex);
                ShowToastWhenReady(Loc.S("Execution.WebViewFailed"));
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
            // The page never gets a window of its own.
            e.Handled = true;

            // Only web and mail links go to Windows: a page must not start a program with a link.
            if (!ExternalLinkPolicy.IsAllowed(e.Uri))
                return;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Event handler of the browser: an exception here must not reach the message loop.
                ErrorLog.Write("A link of a web page could not be opened: " + e.Uri, ex);
            }
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
