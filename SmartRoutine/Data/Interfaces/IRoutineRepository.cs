using System.Collections.Generic;
using SmartRoutine.Data.Models;

namespace SmartRoutine.Data.Interfaces
{
    public interface IRoutineRepository
    {
        Routine GetRoutine(string routineId);
        List<Routine> LoadRoutines();
        void AddRoutine(Routine routine);
        void UpdateRoutine(Routine routine);
        void DeleteRoutine(string routineId);
    }
}
