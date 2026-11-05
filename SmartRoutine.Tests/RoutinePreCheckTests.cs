using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartRoutine.Tests
{
    [TestClass]
    public class RoutinePreCheckTests
    {
        private static RoutineService CreateService()
        {
            return new RoutineService(new InMemoryRepository());
        }

        [TestMethod]
        public void RoutineWithRunnableSteps_HasNoProblems()
        {
            string folder = Path.GetTempPath();
            var routine = new Routine
            {
                Steps = new List<RoutineStep>
                {
                    new OpenFolderStep { Name = "Folder", Order = 0, FolderPath = folder },
                    new OpenUrlStep { Name = "Site", Order = 1, Url = "https://example.com" }
                }
            };

            Assert.AreEqual(0, RoutinePreCheck.FindProblems(routine, CreateService()).Count);
        }

        [TestMethod]
        public void MissingProgramFolderAndFile_AreListedInTheOrderOfTheRun()
        {
            var routine = new Routine
            {
                Steps = new List<RoutineStep>
                {
                    new OpenDocumentStep { Name = "Letter", Order = 2, FilePath = "C:/does-not-exist/letter.docx" },
                    new OpenApplicationStep { Name = "Editor", Order = 0, ApplicationPath = "C:/does-not-exist/editor.exe" },
                    new OpenFolderStep { Name = "Work", Order = 1, FolderPath = "C:/does-not-exist/work" }
                }
            };

            var problems = RoutinePreCheck.FindProblems(routine, CreateService());

            Assert.AreEqual(3, problems.Count);
            StringAssert.StartsWith(problems[0], "Editor: ");
            StringAssert.StartsWith(problems[1], "Work: ");
            StringAssert.StartsWith(problems[2], "Letter: ");
            StringAssert.Contains(problems[0], "editor.exe");
        }

        [TestMethod]
        public void Problem_IsOneLine()
        {
            var routine = new Routine
            {
                Steps = new List<RoutineStep> { new OpenFolderStep { Name = "Work", FolderPath = "C:/does-not-exist/work" } }
            };

            var problems = RoutinePreCheck.FindProblems(routine, CreateService());

            Assert.IsFalse(problems[0].Contains("\n"));
        }

        [TestMethod]
        public void DisabledSteps_AreNotChecked()
        {
            var routine = new Routine
            {
                Steps = new List<RoutineStep>
                {
                    new OpenApplicationStep { Name = "Gone", Show = false, ApplicationPath = "C:/does-not-exist/gone.exe" },
                    new OpenFolderStep { Name = "Folder", Show = true, FolderPath = Path.GetTempPath() }
                }
            };

            Assert.AreEqual(0, RoutinePreCheck.FindProblems(routine, CreateService()).Count);
        }

        [TestMethod]
        public void StepWithoutName_IsNamedByItsType()
        {
            var routine = new Routine { Steps = new List<RoutineStep> { new OpenUrlStep { Name = "", Url = "" } } };

            var problems = RoutinePreCheck.FindProblems(routine, CreateService());

            StringAssert.StartsWith(problems[0], "OpenUrl: ");
        }

        [TestMethod]
        public void NothingToCheck_GivesNoProblems()
        {
            var service = CreateService();

            Assert.AreEqual(0, RoutinePreCheck.FindProblems(null, service).Count);
            Assert.AreEqual(0, RoutinePreCheck.FindProblems(new Routine { Steps = null }, service).Count);
            Assert.AreEqual(0, RoutinePreCheck.FindProblems(new Routine(), null).Count);
        }
    }

    [TestClass]
    public class BrowsePathTests
    {
        private string _folder;
        private string _file;

        [TestInitialize]
        public void Setup()
        {
            _folder = Path.Combine(Path.GetTempPath(), "SmartRoutine Browse " + Guid.NewGuid());
            Directory.CreateDirectory(_folder);
            _file = Path.Combine(_folder, "program.exe");
            File.WriteAllText(_file, "x");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(_folder, true);
        }

        [TestMethod]
        public void SavedFolder_IsTheStartFolder()
        {
            Assert.AreEqual(_folder, BrowsePath.StartDirectory(_folder));
        }

        [TestMethod]
        public void SavedFile_StartsInItsFolder()
        {
            Assert.AreEqual(_folder, BrowsePath.StartDirectory(_file));
        }

        [TestMethod]
        public void FileThatIsGone_StartsInItsFolderWhenThatExists()
        {
            Assert.AreEqual(_folder, BrowsePath.StartDirectory(Path.Combine(_folder, "gone.exe")));
        }

        [TestMethod]
        public void NothingSavedOrFolderGone_GivesNoStartFolder()
        {
            Assert.IsNull(BrowsePath.StartDirectory(null));
            Assert.IsNull(BrowsePath.StartDirectory(""));
            Assert.IsNull(BrowsePath.StartDirectory("   "));
            Assert.IsNull(BrowsePath.StartDirectory(Path.Combine(_folder, "no", "such", "file.txt")));
        }

        [TestMethod]
        public void InvalidPath_GivesNoStartFolder()
        {
            Assert.IsNull(BrowsePath.StartDirectory("C:\\a|b<c>\\file.txt"));
        }

        [TestMethod]
        public void PathInQuotes_IsUnderstood()
        {
            Assert.AreEqual(_folder, BrowsePath.StartDirectory("\"" + _file + "\""));
        }

        [TestMethod]
        public void FileDialog_StartsInTheFolderWithTheSavedFile()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                BrowsePath.Prepare(dialog, _file);

                Assert.AreEqual(_folder, dialog.InitialDirectory);
                Assert.AreEqual("program.exe", dialog.FileName);
            }
        }

        [TestMethod]
        public void FileDialog_WithoutSavedPath_IsLeftAlone()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                BrowsePath.Prepare(dialog, "");

                Assert.AreEqual("", dialog.InitialDirectory);
                Assert.AreEqual("", dialog.FileName);
            }
        }

        [TestMethod]
        public void FolderDialog_StartsOnTheSavedFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                BrowsePath.Prepare(dialog, _folder);

                Assert.AreEqual(_folder, dialog.SelectedPath);
            }
        }
    }
}
