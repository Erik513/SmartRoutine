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
                        AutoStart = false,
                        Url = "https://www.wetter.de",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_2",
                        Order = 1,
                        Name = "Nachrichten lesen",
                        AutoStart = true,
                        Url = "https://www.spiegel.de",
                        OpenInExternBrowser = true
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_3",
                        Order = 2,
                        Name = "E-Mails prüfen",
                        AutoStart = true,
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
                        AutoStart = true,
                        Url = "https://trello.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_5",
                        Order = 1,
                        Name = "GitHub öffnen",
                        AutoStart = true,
                        Url = "https://github.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_6",
                        Order = 2,
                        Name = "Slack öffnen",
                        AutoStart = true,
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
                        AutoStart = true,
                        Url = "https://www.netflix.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_8",
                        Order = 1,
                        Name = "Spotify öffnen",
                        AutoStart = true,
                        Url = "https://www.spotify.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_9",
                        Order = 2,
                        Name = "YouTube öffnen",
                        AutoStart = true,
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
                        AutoStart = true,
                        Url = "https://www.eventim.de",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_11",
                        Order = 1,
                        Name = "Reise planen",
                        AutoStart = true,
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
                        AutoStart = true,
                        Url = "https://stackoverflow.com",
                        OpenInExternBrowser = false
                    },
                    new OpenUrlStep
                    {
                        Id = "test_step_13",
                        Order = 1,
                        Name = "Microsoft Docs",
                        AutoStart = true,
                        Url = "https://docs.microsoft.com",
                        OpenInExternBrowser = false
                    },
                    new OpenFolderStep
                    {
                        Id = "test_step_14",
                        Order = 2,
                        Name = "Projektordner öffnen",
                        Description = "Öffnet den Projektordner",
                        AutoStart = true,
                        FolderPath = @"C:\Projects",
                        OpenInNewWindow = true
                    },
                    new OpenApplicationStep
                    {
                        Id = "test_step_15",
                        Order = 3,
                        Name = "Visual Studio starten",
                        Description = "Startet Visual Studio",
                        AutoStart = true,
                        ApplicationPath = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
                        Arguments = "",
                        RunAsAdmin = false,
                        WorkingDirectory = ""
                    }
                }
            };
            routines.Add(devRoutine);

            var testRoutine = new Routine
            {
                Id = "test_123",
                Name = "Test123",
                Order = 5,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    
                }
            };
            routines.Add(testRoutine);

            var testRoutine1 = new Routine
            {
                Id = "test_234",
                Name = "Test234",
                Order = 6,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {

                }
            };
            routines.Add(testRoutine1);
            var testRoutine2 = new Routine
            {
                Id = "test_345",
                Name = "Test345",
                Order = 7,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {

                }
            };
            routines.Add(testRoutine2);
            var testRoutine3 = new Routine
            {
                Id = "test_456",
                Name = "Test456",
                Order = 8,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {

                }
            };
            routines.Add(testRoutine3);
            var testRoutine4 = new Routine
            {
                Id = "test_567",
                Name = "Test567",
                Order = 9,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {

                }
            };
            routines.Add(testRoutine4);
            var testRoutine5 = new Routine
            {
                Id = "test_678",
                Name = "Test678",
                Order = 10,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {

                }
            };
            routines.Add(testRoutine5);

            return routines;
        }
    }
}