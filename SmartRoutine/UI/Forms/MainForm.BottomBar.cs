using ErikwnkWFUI.Forms;
using ErikwnkWFUI;
using ErikwnkWFUI.Controls;
using System;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using FlowLayoutPanel = System.Windows.Forms.FlowLayoutPanel;

namespace SmartRoutine.UI.Forms
{
    // The bar at the bottom of the main window: theme button and language choice on the left, the
    // version and - while there is one - the update button on the right.
    public partial class MainForm
    {
        private const string UpdateRepositoryOwner = "Erik513";
        private const string UpdateRepositoryName = "SmartRoutine";
        private static readonly TimeSpan UpdateCheckTimeout = TimeSpan.FromSeconds(5);
        private const int BottomBarHeight = 44;

        private VersionPanel _versionPanel;
        private HttpClient _checkHttpClient;
        private HttpClient _downloadHttpClient;
        private AppUpdater _updater;

        private VersionPanel CreateBottomBar()
        {
            _versionPanel = UIStyles.VersionPanels.CreatePrimary();
            _versionPanel.Dock = DockStyle.Bottom;
            _versionPanel.Height = BottomBarHeight;

            FlowLayoutPanel left = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(10, 7, 0, 7)
            };

            Button themeButton = UIStyles.Buttons.CreateThemeTogglePrimary(new Size(30, 30));
            themeButton.Margin = new Padding(0, 0, 8, 0);

            System.Windows.Forms.ComboBox languageBox = UIStyles.ComboBoxes.CreateLanguagePrimary();
            languageBox.Width = 120;
            languageBox.Margin = new Padding(0, 1, 0, 0);

            left.Controls.Add(themeButton);
            left.Controls.Add(languageBox);

            _versionPanel.Controls.Add(left);

            return _versionPanel;
        }

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

                await _versionPanel.CheckForUpdateAsync(_updater, UpdateCheckTimeout);
            }
            catch (Exception ex)
            {
                // async void: an exception must never end the program because of an update check.
                Console.WriteLine($"Update-Prüfung fehlgeschlagen: {ex.Message}");
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
