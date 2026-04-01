using System;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    /// <summary>
    /// Utility class to attach Drag&Drop text support to any Control.
    /// </summary>
    public static class DragDropHelper
    {
        /// <summary>
        /// Enables drag & drop for text on the given control.
        /// </summary>
        /// <param name="control">The control that should accept dragged text.</param>
        /// <param name="onTextDropped">Action that is called with the dropped text.</param>
        public static void EnableTextDragDrop(Control control, Action<string> onTextDropped)
        {
            control.AllowDrop = true;

            control.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    e.Effect = DragDropEffects.Copy;
                    control.Cursor = Cursors.Hand; // 👆 Hand cursor while hovering
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    control.Cursor = Cursors.No;   // 🚫 Not allowed
                }
            };
            control.DragOver += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            };

            control.DragLeave += (s, e) =>
            {
                control.Cursor = Cursors.Default; // reset cursor when leaving
            };
            control.DragDrop += (s, e) =>
            {
                string droppedText = e.Data.GetData(DataFormats.Text) as string;
                if (!string.IsNullOrWhiteSpace(droppedText))
                {
                    onTextDropped?.Invoke(droppedText);
                }
                control.Cursor = Cursors.Default; // reset after drop
            };
        }
    }
}

