using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine.Data.Interfaces
{
    public interface IDataRepository
    {
        List<string> LoadData();
        void SaveData(string data);
        bool Connect();
        void Disconnect();
    }
}
