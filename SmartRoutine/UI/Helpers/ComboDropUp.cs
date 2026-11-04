using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    /// <summary>
    /// Makes the drop-down list of a combo box open above the box instead of below it (for a box at the
    /// bottom of a window). The list is placed before it is shown, and the slide animation of the system
    /// (which always slides downwards) is switched off while the list is open.
    /// </summary>
    internal static class ComboDropUp
    {
        private const int WmWindowPosChanging = 0x0046;
        private const uint SwpNoMove = 0x0002;
        private const uint SpiGetComboBoxAnimation = 0x1004;
        private const uint SpiSetComboBoxAnimation = 0x1005;

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

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowPos
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x, y, cx, cy;
            public uint flags;
        }

        [DllImport("user32.dll")]
        private static extern bool GetComboBoxInfo(IntPtr hwnd, ref ComboBoxInfo info);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint action, uint param, ref bool value, uint flags);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint action, uint param, IntPtr value, uint flags);

        public static void Enable(ComboBox comboBox)
        {
            DropList list = new DropList(comboBox);

            comboBox.DropDown += (sender, e) => list.Attach();
            comboBox.DropDownClosed += (sender, e) => list.Detach();
            comboBox.Disposed += (sender, e) => list.Detach();
        }

        // Watches the window of the list while it is open and puts it above the box when the system moves
        // or shows it.
        private sealed class DropList : NativeWindow
        {
            private readonly ComboBox _comboBox;
            private bool _animationSwitchedOff;

            public DropList(ComboBox comboBox)
            {
                _comboBox = comboBox;
            }

            public void Attach()
            {
                if (Handle != IntPtr.Zero || !_comboBox.IsHandleCreated)
                    return;

                ComboBoxInfo info = new ComboBoxInfo { cbSize = Marshal.SizeOf(typeof(ComboBoxInfo)) };

                if (!GetComboBoxInfo(_comboBox.Handle, ref info) || info.hwndList == IntPtr.Zero)
                    return;

                SwitchAnimationOff();
                AssignHandle(info.hwndList);
            }

            public void Detach()
            {
                if (Handle != IntPtr.Zero)
                    ReleaseHandle();

                RestoreAnimation();
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmWindowPosChanging)
                    MoveAboveBox(m.LParam);

                base.WndProc(ref m);
            }

            private void MoveAboveBox(IntPtr windowPosPointer)
            {
                WindowPos position = (WindowPos)Marshal.PtrToStructure(windowPosPointer, typeof(WindowPos));

                Rect current;
                int height = position.cy;

                if (height <= 0 && GetWindowRect(Handle, out current))
                    height = current.Bottom - current.Top;

                int top = _comboBox.PointToScreen(Point.Empty).Y - height;

                // No room above the box: the list stays where the system puts it.
                if (height <= 0 || top < Screen.FromControl(_comboBox).WorkingArea.Top)
                    return;

                position.y = top;
                position.flags &= ~SwpNoMove;

                Marshal.StructureToPtr(position, windowPosPointer, false);
            }

            // The slide animation of a combo box list is a setting of the system and always slides down:
            // it is off only while the list is open, and put back as it was (in memory only).
            private void SwitchAnimationOff()
            {
                bool on = false;

                if (!SystemParametersInfo(SpiGetComboBoxAnimation, 0, ref on, 0) || !on)
                    return;

                _animationSwitchedOff = SystemParametersInfo(SpiSetComboBoxAnimation, 0, IntPtr.Zero, 0);
            }

            private void RestoreAnimation()
            {
                if (!_animationSwitchedOff)
                    return;

                SystemParametersInfo(SpiSetComboBoxAnimation, 0, new IntPtr(1), 0);

                _animationSwitchedOff = false;
            }
        }
    }
}
