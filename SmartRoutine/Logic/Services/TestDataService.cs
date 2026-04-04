using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine.Logic.Services
{
    public class TestDataService : ITestDataService
    {
        public List<Routine> GetTestRoutines()
        {
            // WICHTIG: Tiefe Kopie zurückgeben, damit Änderungen die Originale nicht beeinflussen
            var testRoutines = CreateOriginalTestData();
            return DeepCopy(testRoutines);
        }

        private List<Routine> CreateOriginalTestData()
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
                    new RoutineStep { Id = "test_step_1", Order = 0, Type = StepType.OpenUrl, Value = "https://www.wetter.de", Description = "Wetter checken" },
                    new RoutineStep { Id = "test_step_2", Order = 1, Type = StepType.OpenUrl, Value = "https://www.spiegel.de", Description = "Nachrichten lesen" },
                    new RoutineStep { Id = "test_step_3", Order = 2, Type = StepType.OpenUrl, Value = "https://mail.google.com", Description = "E-Mails prüfen" }
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
                    new RoutineStep { Id = "test_step_4", Order = 0, Type = StepType.OpenUrl, Value = "https://trello.com", Description = "Trello öffnen" },
                    new RoutineStep { Id = "test_step_5", Order = 1, Type = StepType.OpenUrl, Value = "https://github.com", Description = "GitHub öffnen" },
                    new RoutineStep { Id = "test_step_6", Order = 2, Type = StepType.OpenUrl, Value = "https://slack.com", Description = "Slack öffnen" }
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
                    new RoutineStep { Id = "test_step_7", Order = 0, Type = StepType.OpenUrl, Value = "https://www.netflix.com", Description = "Netflix öffnen" },
                    new RoutineStep { Id = "test_step_8", Order = 1, Type = StepType.OpenUrl, Value = "https://www.spotify.com", Description = "Spotify öffnen" },
                    new RoutineStep { Id = "test_step_9", Order = 2, Type = StepType.OpenUrl, Value = "https://www.youtube.com", Description = "YouTube öffnen" }
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
                    new RoutineStep { Id = "test_step_10", Order = 0, Type = StepType.OpenUrl, Value = "https://www.eventim.de", Description = "Events checken" },
                    new RoutineStep { Id = "test_step_11", Order = 1, Type = StepType.OpenUrl, Value = "https://www.booking.com", Description = "Reise planen" }
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
                    new RoutineStep { Id = "test_step_12", Order = 0, Type = StepType.OpenUrl, Value = "https://stackoverflow.com", Description = "Stack Overflow" },
                    new RoutineStep { Id = "test_step_13", Order = 1, Type = StepType.OpenUrl, Value = "https://docs.microsoft.com", Description = "Microsoft Docs" },
                    new RoutineStep { Id = "test_step_14", Order = 2, Type = StepType.OpenUrl, Value = "https://github.com", Description = "GitHub" }
                }
            };
            routines.Add(devRoutine);

            return routines;
        }

        private List<Routine> DeepCopy(List<Routine> originals)
        {
            return originals.Select(r => new Routine
            {
                Id = Guid.NewGuid().ToString(),  // Neue ID, damit nicht mit Original kollidiert
                Name = r.Name,
                Order = r.Order,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Steps = r.Steps.Select(s => new RoutineStep
                {
                    Id = Guid.NewGuid().ToString(),
                    Order = s.Order,
                    Type = s.Type,
                    Value = s.Value,
                    Description = s.Description,
                    UserDescription = s.UserDescription
                }).ToList()
            }).ToList();
        }
    }
}
