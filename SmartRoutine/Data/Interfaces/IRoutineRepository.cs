using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
