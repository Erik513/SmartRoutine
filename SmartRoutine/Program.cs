using SmartRoutine.Data;
using SmartRoutine.Data.Interfaces;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Forms;
using SmartRoutine.UI.Helpers;
using System;
using System.Windows.Forms;

namespace SmartRoutine
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            RegisterGlobalExceptionCleanup();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            IRoutineRepository routineRepository = new RoutineRepository();
            IRoutineService routineService = new RoutineService(routineRepository, AppSettings.UseTestData);

            Application.Run(new MainForm(routineService));
        }

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