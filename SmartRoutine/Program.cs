using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartRoutine.Data;
using SmartRoutine.Data.Interfaces;
using SmartRoutine.Logic;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI;

namespace SmartRoutine
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => SleepPreventer.AllowSleep();
            Application.ThreadException += (sender, e) => SleepPreventer.AllowSleep();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // DataRepository
            IRoutineRepository data = new RoutineRepository();

            // BusinessLogic
            IRoutineService logic = new RoutineService(data);

            // UI
            Application.Run(new MainForm(logic));
        }
    }
}
