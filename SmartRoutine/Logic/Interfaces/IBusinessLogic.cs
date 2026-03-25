using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartRoutine.Data;
using SmartRoutine;

namespace SmartRoutine.Logic.Interfaces
{
    public interface IBusinessLogic
    {
        List<string> GetProcessedData();
        bool ProcessData(string input);
        void Initialize();
        void Cleanup();
    }
}
