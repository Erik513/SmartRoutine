using ErikwnkWFUI;
using ErikwnkCore;
using ErikwnkWFUI.Styles;
using SmartRoutine.Data;
using SmartRoutine.Data.Interfaces;
using SmartRoutine.Localization;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.Logic.TestData;
using SmartRoutine.UI.Forms;
using SmartRoutine.UI.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SmartRoutine
{
    internal static class Program
    {
        private const string SingleInstanceMutexName = "SmartRoutine_SingleInstance";

        [STAThread]
        private static void Main()
        {
            // Before the first window of the program, also the "already running" notice.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // The language of Windows on the first start (German or English); after that the choice of the user (theme, accent color, language)
            // from the bar at the bottom of the main window is loaded and kept.
            UIStyles.Language = StartupLanguage.FromWindows();
            UIStyles.RememberAppearance();

            using (SingleInstanceGuard guard = new SingleInstanceGuard(SingleInstanceMutexName))
            {
                if (!guard.IsFirstInstance)
                {
                    BringExistingInstanceToFront();

                    ErikwnkWFUI.Forms.MessageBox.Show(
                        Loc.S("Program.AlreadyRunning"),
                        "SmartRoutine",
                        ErikwnkWFUI.Forms.MessageBoxButtons.OK,
                        ErikwnkWFUI.Forms.MessageBoxIcon.Info,
                        size: ErikwnkWFUI.Forms.FormSize.Small);

                    return;
                }

                RegisterGlobalExceptionHandling();

                try
                {
                    Application.Run(new MainForm(CreateRoutineService()));
                }
                catch (Exception ex)
                {
                    // The start failed (the database is locked, damaged or not writable ...): say so
                    // instead of ending with a crash window of the system.
                    ShowError(Loc.S("Program.StartFailed"), ex);
                }
            }
        }

        private static IRoutineService CreateRoutineService()
        {
            IRoutineRepository routineRepository = AppSettings.UseTestData
                ? (IRoutineRepository)new InMemoryRoutineRepository(new TestDataFactory().GetTestRoutines())
                : new RoutineRepository();

            return new RoutineService(routineRepository);
        }

        // True while an error dialog is open: errors that come meanwhile (the same error again and again,
        // for example from a paint handler) are only written to the log, not shown one on top of the other.
        private static bool _showingError;

        private static void ShowError(string message, Exception ex)
        {
            ErrorLog.Write(message, ex);

            if (_showingError)
                return;

            _showingError = true;

            try
            {
                ErikwnkWFUI.Forms.MessageBox.Show(
                    message + "\n\n" + ex.Message + "\n\n" + Loc.S("Program.LogHint", ErrorLog.FilePath),
                    "SmartRoutine",
                    ErikwnkWFUI.Forms.MessageBoxButtons.OK,
                    ErikwnkWFUI.Forms.MessageBoxIcon.Error,
                    size: ErikwnkWFUI.Forms.FormSize.Medium);
            }
            catch
            {
                // Nothing more can be done when even the notice fails.
            }
            finally
            {
                _showingError = false;
            }
        }

        private static void BringExistingInstanceToFront()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process existingProcess = Process.GetProcessesByName(currentProcess.ProcessName)
                .FirstOrDefault(p => p.Id != currentProcess.Id);

            IntPtr windowHandle = existingProcess?.MainWindowHandle ?? IntPtr.Zero;
            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            ShowWindow(windowHandle, ShowWindowRestore);
            SetForegroundWindow(windowHandle);
        }

        private const int ShowWindowRestore = 9;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void RegisterGlobalExceptionHandling()
        {
            // The program ends after an unhandled exception: the computer may sleep again then, and the error
            // is written to the log (there is nothing else left to show it).
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                SleepPreventer.AllowSleep();
                ErrorLog.Write("Unhandled exception (the program ends)", e.ExceptionObject as Exception);
            };

            // An exception in an event handler no longer disappears silently: the user is told, the
            // program goes on.
            Application.ThreadException += (sender, e) =>
            {
                SleepPreventer.AllowSleep();

                ShowError(Loc.S("Program.UnexpectedError"), e.Exception);
            };
        }
    }
}
