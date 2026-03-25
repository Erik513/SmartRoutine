using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine
{
    class SleepPreventer
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(ExecutionFlag flags);

        [Flags]
        enum ExecutionFlag : uint
        {
            System = 0x00000001,
            Display = 0x00000002,
            Continuous = 0x80000000,
        }

        public static void PreventSleep()
        {
            try
            {
                // Verhindert Standby und Bildschirm-Abschaltung
                SetThreadExecutionState(ExecutionFlag.System |
                                       ExecutionFlag.Display |
                                       ExecutionFlag.Continuous);
            }
            catch (Exception ex)
            {
                // Optional: Logging
                Debug.WriteLine($"Failed to prevent sleep: {ex.Message}");
            }
        }

        public static void AllowSleep()
        {
            try
            {
                // Setzt den normalen Zustand wieder her
                SetThreadExecutionState(ExecutionFlag.Continuous);
            }
            catch (Exception ex)
            {
                // Optional: Logging
                Debug.WriteLine($"Failed to allow sleep: {ex.Message}");
            }
        }
    }
}
