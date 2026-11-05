using ErikwnkWFUI.Forms;
using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using System;
using System.Net.Http;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    // The update check of the main window: the bar at the bottom of the routine list (see
    // RoutinesViewUC.BottomBar.cs) shows the update button when there is a newer version.
    public partial class MainForm
    {
        private const string UpdateRepositoryOwner = "Erik513";
        private const string UpdateRepositoryName = "SmartRoutine";
        private static readonly TimeSpan UpdateCheckTimeout = TimeSpan.FromSeconds(5);

        private HttpClient _checkHttpClient;
        private HttpClient _downloadHttpClient;
        private AppUpdater _updater;

        // Looks for a newer version once per program start and shows the update button in the bar when
        // there is one (silent: no network or a slow answer just leave the bar as it is). The HTTP
        // clients stay open for the click on the button, which downloads the update.
        private async void CheckForUpdatesAsync()
        {
            try
            {
                _checkHttpClient = new HttpClient();
                _downloadHttpClient = new HttpClient();

                _updater = new AppUpdater(
                    UpdateRepositoryOwner,
                    UpdateRepositoryName,
                    _checkHttpClient,
                    _downloadHttpClient);

                VersionPanel bar = _routinesView.VersionBar;

                if (bar != null)
                    await bar.CheckForUpdateAsync(_updater, UpdateCheckTimeout);
            }
            catch (Exception ex)
            {
                // async void: an exception must never end the program because of an update check.
                Console.WriteLine($"Update check failed: {ex.Message}");
            }
        }

        private void DisposeUpdateResources()
        {
            if (_checkHttpClient != null)
            {
                _checkHttpClient.Dispose();
                _checkHttpClient = null;
            }

            if (_downloadHttpClient != null)
            {
                _downloadHttpClient.Dispose();
                _downloadHttpClient = null;
            }
        }
    }
}
