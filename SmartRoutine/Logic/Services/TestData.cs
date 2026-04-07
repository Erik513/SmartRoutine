using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    new RoutineStep { Id = "test_step_1", Order = 0, Type = StepType.OpenUrl, Value = "https://www.wetter.de", Name = "Wetter checken" },
                    new RoutineStep { Id = "test_step_2", Order = 1, Type = StepType.OpenUrl, Value = "https://www.spiegel.de", Name = "Nachrichten lesen" },
                    new RoutineStep { Id = "test_step_3", Order = 2, Type = StepType.OpenUrl, Value = "https://mail.google.com", Name = "E-Mails prüfen" }
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
                    new RoutineStep { Id = "test_step_4", Order = 0, Type = StepType.OpenUrl, Value = "https://trello.com", Name = "Trello öffnen" },
                    new RoutineStep { Id = "test_step_5", Order = 1, Type = StepType.OpenUrl, Value = "https://github.com", Name = "GitHub öffnen" },
                    new RoutineStep { Id = "test_step_6", Order = 2, Type = StepType.OpenUrl, Value = "https://slack.com", Name = "Slack öffnen" }
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
                    new RoutineStep { Id = "test_step_7", Order = 0, Type = StepType.OpenUrl, Value = "https://www.netflix.com", Name = "Netflix öffnen" },
                    new RoutineStep { Id = "test_step_8", Order = 1, Type = StepType.OpenUrl, Value = "https://www.spotify.com", Name = "Spotify öffnen" },
                    new RoutineStep { Id = "test_step_9", Order = 2, Type = StepType.OpenUrl, Value = "https://www.youtube.com", Name = "YouTube öffnen" }
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
                    new RoutineStep { Id = "test_step_10", Order = 0, Type = StepType.OpenUrl, Value = "https://www.eventim.de", Name = "Events checken" },
                    new RoutineStep { Id = "test_step_11", Order = 1, Type = StepType.OpenUrl, Value = "https://www.booking.com", Name = "Reise planen" }
                }
            };
            routines.Add(weekendRoutine);

            // Routine 5: Entwickeln
            var devRoutine = new Routine
            {
                Id = "test_dev_5",
                Name = "Entwicklungsstart",
                Order = 4,
                CreatedAt = DateTime.Now,
                Steps = new List<RoutineStep>
                {
                    new RoutineStep { Id = "test_step_12", Order = 0, Type = StepType.OpenUrl, Value = "https://stackoverflow.com", Name = "Stack Overflow" },
                    new RoutineStep { Id = "test_step_13", Order = 1, Type = StepType.OpenUrl, Value = "https://docs.microsoft.com", Name = "Microsoft Docs" },
                    new RoutineStep { Id = "test_step_14", Order = 2, Type = StepType.OpenUrl, Value = "https://github.com", Name = "GitHub" }
                }
            };
            routines.Add(devRoutine);

            return routines;
        }
    }
}
