using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;

namespace SmartRoutine.Logic.Services
{
    public class TestData
    {
        public List<Routine> GetTestRoutines()
        {
            return CreateTestData();
        }

        private List<Routine> CreateTestData()
        {
            var routines = new List<Routine>();

            // Routine 1: Morgens
            var morningRoutine = new Routine
            {
                Id = "test_morning_1",
                Name = "Morgenroutine",
                Order = 0,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep
                    {
                        Id = "test_step_1",
                        Order = 0,
                        Name = "Wetter checken",
                        Url = "https://www.wetter.de",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_2",
                        Order = 1,
                        Name = "Nachrichten lesen",
                        Url = "https://www.spiegel.de",
                        OpenInExternBrowser = true
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_3",
                        Order = 2,
                        Name = "E-Mails prüfen",
                        Url = "https://mail.google.com",
                        OpenInExternBrowser = false
                    }
                }
            };
            routines.Add(morningRoutine);

            // Routine 2: Arbeit
            var workRoutine = new Routine
            {
                Id = "test_work_2",
                Name = "Arbeitsstart",
                Order = 1,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep
                    {
                        Id = "test_step_4",
                        Order = 0,
                        Name = "Trello öffnen",
                        Url = "https://trello.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_5",
                        Order = 1,
                        Name = "GitHub öffnen",
                        Url = "https://github.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_6",
                        Order = 2,
                        Name = "Slack öffnen",
                        Url = "https://slack.com",
                        OpenInExternBrowser = false
                    }
                }
            };
            routines.Add(workRoutine);

            // Routine 3: Feierabend
            var eveningRoutine = new Routine
            {
                Id = "test_evening_3",
                Name = "Feierabend",
                Order = 2,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep
                    {
                        Id = "test_step_7",
                        Order = 0,
                        Name = "Netflix öffnen",
                        Url = "https://www.netflix.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_8",
                        Order = 1,
                        Name = "Spotify öffnen",
                        Url = "https://www.spotify.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_9",
                        Order = 2,
                        Name = "YouTube öffnen",
                        Url = "https://www.youtube.com",
                        OpenInExternBrowser = false
                    }
                }
            };
            routines.Add(eveningRoutine);

            // Routine 4: Wochenende
            var weekendRoutine = new Routine
            {
                Id = "test_weekend_4",
                Name = "Wochenende",
                Order = 3,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep
                    {
                        Id = "test_step_10",
                        Order = 0,
                        Name = "Events checken",
                        Url = "https://www.eventim.de",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_11",
                        Order = 1,
                        Name = "Reise planen",
                        Url = "https://www.booking.com",
                        OpenInExternBrowser = false
                    }
                }
            };
            routines.Add(weekendRoutine);

            // Routine 5: Entwickeln (mit gemischten Step-Typen als Beispiel)
            var devRoutine = new Routine
            {
                Id = "test_dev_5",
                Name = "Entwicklungsstart",
                Order = 4,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new OpenUrlStep
                    {
                        Id = "test_step_12",
                        Order = 0,
                        Name = "Stack Overflow",
                        Url = "https://stackoverflow.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_13",
                        Order = 1,
                        Name = "Microsoft Docs",
                        Url = "https://docs.microsoft.com",
                        OpenInExternBrowser = false
                    },
                    new OpenFolderStep
                    {
                        Id = "test_step_14",
                        Order = 2,
                        Name = "Projektordner öffnen",
                        Description = "Öffnet den Projektordner",
                        FolderPath = @"C:\Projects",
                        OpenInNewWindow = true
                    },
                    new OpenApplicationStep
                    {
                        Id = "test_step_15",
                        Order = 3,
                        Name = "Visual Studio starten",
                        Description = "Startet Visual Studio",
                        ApplicationPath = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = ""
                    }
                }
            };
            routines.Add(devRoutine);

            return routines;
        }
    }
}