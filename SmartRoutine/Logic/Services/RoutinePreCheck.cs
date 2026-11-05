using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SmartRoutine.Logic.Services
{
    /// <summary>
    /// Checks the steps of a routine before it is run, so that a program or a folder that is gone is found
    /// at the start and not in the middle of the run.
    /// </summary>
    public static class RoutinePreCheck
    {
        /// <summary>
        /// The problems of the steps that would be run (the enabled ones), in the order of the run: one line
        /// "step name: problem" each. Empty when every step can be run.
        /// </summary>
        public static List<string> FindProblems(Routine routine, IRoutineService service)
        {
            var problems = new List<string>();

            if (routine == null || routine.Steps == null || service == null)
                return problems;

            foreach (RoutineStep step in routine.Steps.Where(s => s != null && s.Show).OrderBy(s => s.Order))
            {
                string message;

                if (service.ValidateStep(step, out message))
                    continue;

                // One line: the messages put the path on a second line.
                string oneLine = (message ?? "").Replace("\r", "").Replace("\n", " ").Trim();
                string name = string.IsNullOrWhiteSpace(step.Name) ? step.Type.ToString() : step.Name;

                problems.Add(name + ": " + oneLine);
            }

            return problems;
        }
    }
}
