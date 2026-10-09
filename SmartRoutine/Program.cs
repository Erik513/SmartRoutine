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
                        size: ErikwnkWFUI.Forms.MessageBoxSize.Small);

                    return;
                }

                RegisterGlobalExceptionCleanup();

                UIStyles.Language = UILanguage.German;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                IRoutineRepository routineRepository = AppSettings.UseTestData
                    ? (IRoutineRepository)new InMemoryRoutineRepository(new TestDataFactory().GetTestRoutines())
                    : new RoutineRepository();

                IRoutineService routineService = new RoutineService(routineRepository);

                Application.Run(new MainForm(routineService));
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

        private static void RegisterGlobalExceptionCleanup()
        {
            AppDomain.CurrentDomain.UnhandledException += delegate
            {
                SleepPreventer.AllowSleep();
            };

            Application.ThreadException += delegate
            {
                SleepPreventer.AllowSleep();
            };
        }
    }
}