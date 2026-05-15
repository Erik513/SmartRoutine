using SmartRoutine;
using SmartRoutine.Data;
using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine.Logic.Interfaces
{
    public interface IRoutineService
    {
        event Action<string> OpenUrlInWebView;

        List<Routine> GetAllRoutines();
        Routine GetRoutine(string id);
        Routine CreateRoutine(string name);
        void UpdateRoutine(Routine routine);
        void DeleteRoutine(string id);

        void ReorderRoutines(List<Routine> reorderedRoutines);
        void SaveRoutine(Routine routine);

        void AddStep(string routineId, RoutineStep step);
        void UpdateStep(string routineId, RoutineStep step);
        void RemoveStep(string routineId, string stepId);
        void ReorderSteps(string routineId, int oldIndex, int newIndex);

        void ExecuteStep(RoutineStep step);
        bool ValidateStep(RoutineStep step);
    }
}
