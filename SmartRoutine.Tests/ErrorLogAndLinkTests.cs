using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Logic.Services;
using System;
using System.IO;

namespace SmartRoutine.Tests
{
    [TestClass]
    public class ErrorLogTests
    {
        private string _originalFolder;
        private string _folder;

        [TestInitialize]
        public void Setup()
        {
            _originalFolder = ErrorLog.Folder;
            _folder = Path.Combine(Path.GetTempPath(), "SmartRoutine Log " + Guid.NewGuid());
            ErrorLog.Folder = _folder;
        }

        [TestCleanup]
        public void Cleanup()
        {
            ErrorLog.Folder = _originalFolder;

            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [TestMethod]
        public void Write_CreatesTheFolderAndTheEntryWithContextAndException()
        {
            ErrorLog.Write("while testing", new InvalidOperationException("broken thing"));

            string text = File.ReadAllText(ErrorLog.FilePath);

            StringAssert.Contains(text, "while testing");
            StringAssert.Contains(text, "InvalidOperationException");
            StringAssert.Contains(text, "broken thing");
        }

        [TestMethod]
        public void Write_AppendsToTheFile()
        {
            ErrorLog.Write("first", new Exception("one"));
            ErrorLog.Write("second", new Exception("two"));

            string text = File.ReadAllText(ErrorLog.FilePath);

            Assert.IsTrue(text.IndexOf("first", StringComparison.Ordinal) < text.IndexOf("second", StringComparison.Ordinal));
            StringAssert.Contains(text, "one");
            StringAssert.Contains(text, "two");
        }

        [TestMethod]
        public void Write_WithoutException_StillWritesTheContext()
        {
            ErrorLog.Write("context only", null);

            StringAssert.Contains(File.ReadAllText(ErrorLog.FilePath), "context only");
        }

        [TestMethod]
        public void Write_TooLargeFile_IsMovedAside()
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(ErrorLog.FilePath, new string('x', 600 * 1024));

            ErrorLog.Write("fresh", new Exception("new"));

            Assert.IsTrue(File.Exists(ErrorLog.FilePath + ".old"));
            Assert.IsTrue(new FileInfo(ErrorLog.FilePath).Length < 10 * 1024);
            StringAssert.Contains(File.ReadAllText(ErrorLog.FilePath), "fresh");
        }

        [TestMethod]
        public void Write_ToAnImpossibleFolder_DoesNotThrow()
        {
            ErrorLog.Folder = "C:\\a|b<c>\\log";

            ErrorLog.Write("nowhere", new Exception("x"));
        }
    }

    [TestClass]
    public class ExternalLinkPolicyTests
    {
        [TestMethod]
        [DataRow("https://example.com/page")]
        [DataRow("http://example.com")]
        [DataRow("HTTPS://EXAMPLE.COM")]
        [DataRow("mailto:someone@example.com")]
        [DataRow("  https://example.com  ")]
        public void WebAndMailLinks_AreAllowed(string uri)
        {
            Assert.IsTrue(ExternalLinkPolicy.IsAllowed(uri));
        }

        [TestMethod]
        [DataRow("file:///C:/Windows/System32/calc.exe")]
        [DataRow("ms-msdt:/id PCWDiagnostic")]
        [DataRow("javascript:alert(1)")]
        [DataRow("data:text/html,<b>x</b>")]
        [DataRow("ftp://example.com/file")]
        [DataRow("C:\\Windows\\notepad.exe")]
        [DataRow("\\\\server\\share\\file.exe")]
        [DataRow("/relative/path")]
        [DataRow("example.com")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow(null)]
        public void EverythingElse_IsNotOpened(string uri)
        {
            Assert.IsFalse(ExternalLinkPolicy.IsAllowed(uri));
        }
    }
}
