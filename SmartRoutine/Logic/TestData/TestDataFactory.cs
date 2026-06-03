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
                CreatedAt = now,
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
                        Url = "https://www.wikipedia.org",
                        OpenInExternalBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "logic_step_3",
                        Order = 2,
                        Name = "URL extern - Autostart sichtbar",
                        Description = "Soll sichtbar sein und beim ersten Betreten im externen Browser starten.",
                        Show = true,
                        AutoStart = true,
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
                        Url = "https://www.google.com",
                        OpenInExternalBrowser = false
                    },
                    new OpenFolderStep
                    {
                        Id = "logic_step_5",
                        Order = 4,
                        Name = "Ordner - manuell sichtbar",
                        Description = "Soll sichtbar sein, aber erst per Button ausgeführt werden.",
                        Show = true,
                        AutoStart = false,
                        FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        OpenInNewWindow = true
                    },
                    new OpenFolderStep
                    {
                        Id = "logic_step_6",
                        Order = 5,
                        Name = "Ordner - Autostart sichtbar",
                        Description = "Soll sichtbar sein und beim ersten Betreten automatisch öffnen.",
                        Show = true,
                        AutoStart = true,
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
                        FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        OpenInNewWindow = true
                    },
                    new OpenApplicationStep
                    {
                        Id = "logic_step_8",
                        Order = 7,
                        Name = "App - manuell sichtbar",
                        Description = "Soll sichtbar sein und Notepad erst per Button starten.",
                        Show = true,
                        AutoStart = false,
                        ApplicationPath = "notepad.exe",
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = ""
                    },
                    new OpenApplicationStep
                    {
                        Id = "logic_step_9",
                        Order = 8,
                        Name = "App - Autostart sichtbar",
                        Description = "Soll sichtbar sein und Calculator beim ersten Betreten automatisch starten.",
                        Show = true,
                        AutoStart = true,
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
                        ApplicationPath = "notepad.exe",
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = ""
                    },
                    new OpenDocumentStep
                    {
                        Id = "logic_step_11",
                        Order = 10,
                        Name = "Dokument - manuell sichtbar",
                        Description = "Soll sichtbar sein und erst per Button geöffnet werden.",
                        Show = true,
                        AutoStart = false,
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
                        Name = "Dokument - Autostart sichtbar",
                        Description = "Soll sichtbar sein und beim ersten Betreten automatisch geöffnet werden.",
                        Show = true,
                        AutoStart = true,
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
                CreatedAt = now,
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep
                    {
                        Id = "step_1",
                        Order = 0,
                        Name = "URL intern - manuell sichtbar",
                        Description = "Soll sichtbar sein, nicht automatisch starten, WebView erst nach Klick anzeigen.",
                        Show = false,
                        AutoStart = false,
                        Url = "https://www.example.com",
                        OpenInExternalBrowser = false
                    },
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
                    CreatedAt = now,
                    Steps = new List<RoutineStep>()
                });
            }

            return routines;
        }
    }
}