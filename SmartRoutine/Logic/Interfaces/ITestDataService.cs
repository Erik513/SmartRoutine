using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine.Logic.Interfaces
{
    public interface ITestDataService
    {
        List<Routine> GetTestRoutines();
    }
}
