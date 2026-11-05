using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;

namespace SmartRoutine.Logic.TestData
{
    public class TestDataFactory
    {
        public List<Routine> GetTestRoutines()
        {
            return CreateTestData();
        }

        private List<Routine> CreateTestData()
        {
            var routines = new List<Routine>();
            var now = DateTime.Now;
            var logicTestRoutine = new Routine
            {
                Id = "test_logic_all_step_types",
                Name = "Logic test: all step types",
                Order = 0,
                CreatedAt = now.AddDays(-10),
                UpdatedAt = now.AddDays(-2),
                LastExecutionAt = now.AddDays(-3),
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep
                    {
                        Id = "logic_step_1",
                        Order = 0,
                        Name = "URL internal - visible, manual",
                        Description = "Must be visible, must not start automatically, shows the web view only after a click.",
                        Show = true,
                        AutoStart = false,
                        AutoContinue = false,
                        Url = "https://www.example.com",
                        OpenInExternalBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "logic_step_2",
                        Order = 1,
                        Name = "URL internal - visible, autostart",
                        Description = "Must be visible and start automatically in the web view when it is entered the first time.",
                        Show = true,
                        AutoStart = true,
                        AutoContinue = false,
                        Url = "https://www.wikipedia.org",
                        OpenInExternalBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "logic_step_3",
                        Order = 2,
                        Name = "URL external - autostart + auto-continue",
                        Description = "Must be visible, start externally by itself and then move on by itself.",
                        Show = true,
                        AutoStart = true,
                        AutoContinue = true,
                        Url = "https://www.github.com",
                        OpenInExternalBrowser = true
                    },
                    new OpenUrlStep
                    {
                        Id = "logic_step_4",
                        Order = 3,
                        Name = "URL hidden",
                        Description = "Must not appear in the execution form.",
                        Show = false,
                        AutoStart = true,
                        AutoContinue = true,
                        Url = "https://www.google.com",
                        OpenInExternalBrowser = true
                    },
                    new OpenFolderStep
                    {
                        Id = "logic_step_5",
                        Order = 4,
                        Name = "Folder - visible, manual",
                        Description = "Must be visible, but only run by a button.",
                        Show = true,
                        AutoStart = false,
                        AutoContinue = true,
                        FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        OpenInNewWindow = true
                    },
                    new OpenFolderStep
                    {
                        Id = "logic_step_6",
                        Order = 5,
                        Name = "Folder - autostart + auto-continue",
                        Description = "Must be visible, open by itself and then move on by itself.",
                        Show = true,
                        AutoStart = true,
                        AutoContinue = true,
                        FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        OpenInNewWindow = true
                    },
                    new OpenFolderStep
                    {
                        Id = "logic_step_7",
                        Order = 6,
                        Name = "Folder hidden",
                        Description = "Must not appear in the execution form.",
                        Show = false,
                        AutoStart = true,
                        AutoContinue = true,
                        FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        OpenInNewWindow = false
                    },
                    new OpenApplicationStep
                    {
                        Id = "logic_step_8",
                        Order = 7,
                        Name = "App - visible, manual",
                        Description = "Must be visible and start Notepad only by a button.",
                        Show = true,
                        AutoStart = false,
                        AutoContinue = false,
                        ApplicationPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe"),
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = ""
                    },
                    new OpenApplicationStep
                    {
                        Id = "logic_step_9",
                        Order = 8,
                        Name = "App - autostart + auto-continue",
                        Description = "Must be visible, start Calculator by itself and then move on.",
                        Show = true,
                        AutoStart = true,
                        AutoContinue = true,
                        ApplicationPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "calc.exe"),
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = ""
                    },
                    new OpenApplicationStep
                    {
                        Id = "logic_step_10",
                        Order = 9,
                        Name = "App hidden",
                        Description = "Must not appear in the execution form.",
                        Show = false,
                        AutoStart = true,
                        AutoContinue = true,
                        ApplicationPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe"),
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = "",
                    },
                    new OpenDocumentStep
                    {
                        Id = "logic_step_11",
                        Order = 10,
                        Name = "Document - visible, manual",
                        Description = "Must be visible and be opened only by a button.",
                        Show = true,
                        AutoStart = false,
                        AutoContinue = false,
                        FilePath = System.IO.Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            "SmartRoutine-Test.txt"
                        ),
                        OpenWithAssociatedApp = true
                    },
                    new OpenDocumentStep
                    {
                        Id = "logic_step_12",
                        Order = 11,
                        Name = "Document - autostart + auto-continue",
                        Description = "Must be visible, be opened by itself and then move on.",
                        Show = true,
                        AutoStart = true,
                        AutoContinue = true,
                        FilePath = System.IO.Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            "SmartRoutine-Test.txt"
                        ),
                        OpenWithAssociatedApp = true
                    },
                    new OpenDocumentStep
                    {
                        Id = "logic_step_13",
                        Order = 12,
                        Name = "Document hidden",
                        Description = "Must not appear in the execution form.",
                        Show = false,
                        AutoStart = true,
                        AutoContinue = true,
                        FilePath = System.IO.Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            "SmartRoutine-Test.txt"
                        ),
                        OpenWithAssociatedApp = true
                    }
                }
            }; 
            routines.Add(logicTestRoutine);

            var TestRoutineWithDisabledStep = new Routine
            {
                Id = "test_routine_with_disabled_step",
                Name = "Routine with a disabled step",
                Order = 1,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-1),
                LastExecutionAt = null,
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep
                    {
                        Id = "step_1",
                        Order = 0,
                        Name = "URL internal - hidden, manual",
                        Description = "Must not be visible and must not start automatically.",
                        Show = false,
                        AutoStart = false,
                        AutoContinue = false,
                        Url = "https://www.example.com",
                        OpenInExternalBrowser = false
                    }
                }
            };
            routines.Add(TestRoutineWithDisabledStep);

            for (int i = 0; i < 10; i++)
            {
                routines.Add(new Routine
                {
                    Id = $"test_empty_{i}",
                    Name = $"Test {i + 1}",
                    Order = 2 + i,
                    CreatedAt = now.AddDays(-(i + 1)),
                    UpdatedAt = i % 2 == 0 ? now.AddHours(-i) : (DateTime?)null,
                    LastExecutionAt = i % 3 == 0 ? now.AddMinutes(-(i + 1) * 10) : (DateTime?)null,
                    Steps = new List<RoutineStep>()
                });
            }

            return routines;
        }
    }
}