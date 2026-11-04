using ErikwnkWFUI;
using ErikwnkCore;
using ErikwnkWFUI.Styles;
using SmartRoutine.Data;
using SmartRoutine.Data.Interfaces;
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

            UIStyles.Language = UILanguage.German;

            using (SingleInstanceGuard guard = new SingleInstanceGuard(SingleInstanceMutexName))
            {
                if (!guard.IsFirstInstance)
                {
                    BringExistingInstanceToFront();

                    ErikwnkWFUI.Forms.MessageBox.Show(
                        "SmartRoutine läuft bereits.",
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
                    ShowError("SmartRoutine konnte nicht gestartet werden.", ex);
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

        private static void ShowError(string message, Exception ex)
        {
            try
            {
                ErikwnkWFUI.Forms.MessageBox.Show(
                    message + "\n\n" + ex.Message,
                    "SmartRoutine",
                    ErikwnkWFUI.Forms.MessageBoxButtons.OK,
                    ErikwnkWFUI.Forms.MessageBoxIcon.Error,
                    size: ErikwnkWFUI.Forms.FormSize.Medium);
            }
            catch
            {
                // Nothing more can be done when even the notice fails.
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
            // The program ends after an unhandled exception: the computer may sleep again then.
            AppDomain.CurrentDomain.UnhandledException += delegate
            {
                SleepPreventer.AllowSleep();
            };

            // An exception in an event handler no longer disappears silently: the user is told, the
            // program goes on.
            Application.ThreadException += (sender, e) =>
            {
                SleepPreventer.AllowSleep();

                ShowError("Es ist ein unerwarteter Fehler aufgetreten.", e.Exception);
            };
        }
    }
}
