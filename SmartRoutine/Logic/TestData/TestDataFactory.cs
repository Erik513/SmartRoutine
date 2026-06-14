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
                Name = "Logiktest: Alle Step-Typen",
                Order = 0,
                CreatedAt = now.AddDays(-10),
                UpdatedAt = now.AddDays(-2),
                LastExecutionAt = now.AddHours(-3),
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep
                    {
                        Id = "logic_step_1",
                        Order = 0,
                        Name = "URL intern - manuell sichtbar",
                        Description = "Soll sichtbar sein, nicht automatisch starten, WebView erst nach Klick anzeigen.",
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
                        Name = "URL intern - Autostart sichtbar",
                        Description = "Soll sichtbar sein und beim ersten Betreten automatisch in der WebView starten.",
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
                        Name = "URL extern - Autostart + AutoContinue",
                        Description = "Soll sichtbar sein, automatisch extern starten und danach automatisch weitergehen.",
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
                        Name = "URL ausgeblendet",
                        Description = "Darf in ExecutionForm nicht auftauchen.",
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
                        Name = "Ordner - manuell sichtbar",
                        Description = "Soll sichtbar sein, aber erst per Button ausgeführt werden.",
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
                        Name = "Ordner - Autostart + AutoContinue",
                        Description = "Soll sichtbar sein, automatisch öffnen und danach automatisch weitergehen.",
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
                        Name = "Ordner ausgeblendet",
                        Description = "Darf in ExecutionForm nicht auftauchen.",
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
                        Name = "App - manuell sichtbar",
                        Description = "Soll sichtbar sein und Notepad erst per Button starten.",
                        Show = true,
                        AutoStart = false,
                        AutoContinue = false,
                        ApplicationPath = "notepad.exe",
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    },
                    new OpenApplicationStep
                    {
                        Id = "logic_step_9",
                        Order = 8,
                        Name = "App - Autostart + AutoContinue",
                        Description = "Soll sichtbar sein, Calculator automatisch starten und danach weitergehen.",
                        Show = true,
                        AutoStart = true,
                        AutoContinue = true,
                        ApplicationPath = "calc.exe",
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = ""
                    },
                    new OpenApplicationStep
                    {
                        Id = "logic_step_10",
                        Order = 9,
                        Name = "App ausgeblendet",
                        Description = "Darf in ExecutionForm nicht auftauchen.",
                        Show = false,
                        AutoStart = true,
                        AutoContinue = true,
                        ApplicationPath = "notepad.exe",
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = "",
                    },
                    new OpenDocumentStep
                    {
                        Id = "logic_step_11",
                        Order = 10,
                        Name = "Dokument - manuell sichtbar",
                        Description = "Soll sichtbar sein und erst per Button geöffnet werden.",
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
                        Name = "Dokument - Autostart + AutoContinue",
                        Description = "Soll sichtbar sein, automatisch geöffnet werden und danach weitergehen.",
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
                        Name = "Dokument ausgeblendet",
                        Description = "Darf in ExecutionForm nicht auftauchen.",
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
                Name = "Routine mit ausgeschaltetem Schritt",
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
                        Name = "URL intern - ausgeblendet manuell",
                        Description = "Soll nicht sichtbar sein und nicht automatisch starten.",
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