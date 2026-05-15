using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;

namespace SmartRoutine.Logic.Interfaces
{
    public interface IRoutineService
    {
        event Action<string> OpenUrlInWebView;

        // Routines
        List<Routine> GetAllRoutines();
        Routine GetRoutine(string id);
        Routine CreateRoutine(string name);
        void UpdateRoutine(Routine routine);
        void DeleteRoutine(string id);
        void ReorderRoutines(List<Routine> reorderedRoutines);
        void SaveRoutine(Routine routine);

        // Steps
        void AddStep(string routineId, RoutineStep step);
        void UpdateStep(string routineId, RoutineStep step);
        void RemoveStep(string routineId, string stepId);
        void ReorderSteps(string routineId, int oldIndex, int newIndex);

        // Execution
        void ExecuteStep(RoutineStep step);
        bool ValidateStep(RoutineStep step);
    }
}
