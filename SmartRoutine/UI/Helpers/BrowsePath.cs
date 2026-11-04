using System;
using System.IO;
using System.Windows.Forms;

namespace SmartRoutine.UI.Helpers
{
    /// <summary>
    /// Lets the browse dialogs of the step editors start where the step already points to: in the folder
    /// that is saved, or in the folder of the saved file / program.
    /// </summary>
    public static class BrowsePath
    {
        /// <summary>
        /// The existing folder to start in for a saved path: the path itself when it is a folder, else the
        /// folder of a file path, else null (nothing saved, or the folder is gone as well).
        /// </summary>
        public static string StartDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                string trimmed = path.Trim().Trim('"');

                if (Directory.Exists(trimmed))
                    return trimmed;

                string parent = Path.GetDirectoryName(trimmed);

                if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                    return parent;
            }
            catch (ArgumentException)
            {
                // Characters that are not allowed in a path: nothing to start in.
            }
            catch (NotSupportedException)
            {
            }
            catch (PathTooLongException)
            {
            }

            return null;
        }

        /// <summary>Opens a file dialog in the folder of the saved path, with the saved file selected when it exists.</summary>
        public static void Prepare(FileDialog dialog, string path)
        {
            string directory = StartDirectory(path);

            if (directory == null)
                return;

            dialog.InitialDirectory = directory;

            string trimmed = path.Trim().Trim('"');

            if (File.Exists(trimmed))
                dialog.FileName = Path.GetFileName(trimmed);
        }

        /// <summary>Opens a folder dialog on the saved folder (or the folder of the saved path).</summary>
        public static void Prepare(FolderBrowserDialog dialog, string path)
        {
            string directory = StartDirectory(path);

            if (directory != null)
                dialog.SelectedPath = directory;
        }
    }
}
