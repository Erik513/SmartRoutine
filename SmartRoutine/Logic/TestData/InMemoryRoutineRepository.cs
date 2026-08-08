using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmartRoutine.Logic.TestData
{
    public class InMemoryRoutineRepository : IRoutineRepository
    {
        private readonly List<Routine> _routines;

        public InMemoryRoutineRepository(IEnumerable<Routine> seedRoutines = null)
        {
            _routines = seedRoutines?.ToList() ?? new List<Routine>();
        }

        public List<Routine> LoadRoutines()
        {
            return _routines.OrderBy(r => r.Order).ToList();
        }

        public Routine GetRoutine(string routineId)
        {
            return _routines.FirstOrDefault(r => r.Id == routineId);
        }

        public void AddRoutine(Routine routine)
        {
            _routines.Add(routine);
        }

        public void UpdateRoutine(Routine routine)
        {
            int index = _routines.FindIndex(r => r.Id == routine.Id);

            if (index < 0)
                return;

            _routines[index] = routine;
        }

        public void DeleteRoutine(string routineId)
        {
            _routines.RemoveAll(r => r.Id == routineId);
        }
    }
}
