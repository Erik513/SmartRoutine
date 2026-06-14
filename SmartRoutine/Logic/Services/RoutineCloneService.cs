using SmartRoutine.Data.Models;
using System.Collections.Generic;

namespace SmartRoutine.Logic.Services
{
    public static class RoutineCloneService
    {
        public static Routine DeepCopy(Routine original)
        {
            if (original == null)
                return null;

            return new Routine
            {
                Id = original.Id,
                Name = original.Name,
                Order = original.Order,
                CreatedAt = original.CreatedAt,
                UpdatedAt = original.UpdatedAt,
                LastExecutionAt = original.LastExecutionAt,
                Steps = CloneSteps(original.Steps)
            };
        }

        private static List<RoutineStep> CloneSteps(List<RoutineStep> steps)
        {
            var clonedSteps = new List<RoutineStep>();

            if (steps == null)
                return clonedSteps;

            foreach (var step in steps)
            {
                clonedSteps.Add(RoutineStepFactory.CreateCopy(step));
            }

            return clonedSteps;
        }
    }
}