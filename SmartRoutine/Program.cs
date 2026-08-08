using CustomWFUI;
using CustomWFUI.Helpers;
using CustomWFUI.Styles;
using SmartRoutine.Data;
using SmartRoutine.Data.Interfaces;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.Logic.TestData;
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

            UIStyles.Language = UILanguage.German;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            IRoutineRepository routineRepository = AppSettings.UseTestData
                ? (IRoutineRepository)new InMemoryRoutineRepository(new TestDataFactory().GetTestRoutines())
                : new RoutineRepository();

            IRoutineService routineService = new RoutineService(routineRepository);

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