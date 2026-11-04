using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    /// <summary>
    /// Makes the drop-down list of a combo box open above the box instead of below it (for a box at the
    /// bottom of a window, where the list would cover the edge of the window or be cut off).
    /// </summary>
    internal static class ComboDropUp
    {
        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoZOrder = 0x0004;
        private const uint SwpNoActivate = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ComboBoxInfo
        {
            public int cbSize;
            public Rect rcItem;
            public Rect rcButton;
            public int stateButton;
            public IntPtr hwndCombo;
            public IntPtr hwndItem;
            public IntPtr hwndList;
        }

        [DllImport("user32.dll")]
        private static extern bool GetComboBoxInfo(IntPtr hwnd, ref ComboBoxInfo info);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

        public static void Enable(ComboBox comboBox)
        {
            comboBox.DropDown += (sender, e) =>
            {
                // After the list has been placed by the system.
                comboBox.BeginInvoke(new Action(() => MoveAbove(comboBox)));
            };
        }

        private static void MoveAbove(ComboBox comboBox)
        {
            if (comboBox.IsDisposed || !comboBox.IsHandleCreated || !comboBox.DroppedDown)
                return;

            ComboBoxInfo info = new ComboBoxInfo { cbSize = Marshal.SizeOf(typeof(ComboBoxInfo)) };

            if (!GetComboBoxInfo(comboBox.Handle, ref info) || info.hwndList == IntPtr.Zero)
                return;

            Rect list;

            if (!GetWindowRect(info.hwndList, out list))
                return;

            int top = comboBox.PointToScreen(Point.Empty).Y - (list.Bottom - list.Top);

            // No room above the box: leave the list where it is.
            if (top < Screen.FromControl(comboBox).WorkingArea.Top)
                return;

            SetWindowPos(info.hwndList, IntPtr.Zero, list.Left, top, 0, 0, SwpNoSize | SwpNoZOrder | SwpNoActivate);
        }
    }
}
