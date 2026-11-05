using System;
using System.IO;
using System.Text;

namespace SmartRoutine.Logic.Services
{
    /// <summary>
    /// Writes unexpected errors into a text file (%AppData%\SmartRoutine\errors.log), so that an error is
    /// still there after the dialog has been closed. It never throws: a log that fails must not cause
    /// another error.
    /// </summary>
    public static class ErrorLog
    {
        private const string FileName = "errors.log";
        private const long MaxBytes = 512 * 1024;
        private static readonly object FileLock = new object();

        /// <summary>The folder of the log; the program data folder of SmartRoutine unless a test sets it.</summary>
        public static string Folder { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmartRoutine");

        public static string FilePath
        {
            get { return Path.Combine(Folder, FileName); }
        }

        /// <summary>Appends one entry: the time, what was going on and the exception with its stack trace.</summary>
        public static void Write(string context, Exception exception)
        {
            try
            {
                lock (FileLock)
                {
                    Directory.CreateDirectory(Folder);

                    // A file that has grown too large is replaced by a fresh one (the old one is kept once).
                    FileInfo info = new FileInfo(FilePath);

                    if (info.Exists && info.Length > MaxBytes)
                    {
                        string old = FilePath + ".old";

                        if (File.Exists(old))
                            File.Delete(old);

                        File.Move(FilePath, old);
                    }

                    StringBuilder entry = new StringBuilder();
                    entry.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + (context ?? ""));
                    entry.AppendLine(exception == null ? "(no exception)" : exception.ToString());
                    entry.AppendLine();

                    File.AppendAllText(FilePath, entry.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
                // Nothing more can be done when even the log fails.
            }
        }
    }
}
