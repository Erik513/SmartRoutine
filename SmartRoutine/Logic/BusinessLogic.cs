using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SmartRoutine.Data.Interfaces;
using SmartRoutine.Logic.Interfaces;

namespace SmartRoutine.Logic
{
    public class BusinessLogic : IBusinessLogic
    {
        // Fields
        private readonly IDataRepository _data;
        // Constructors
        public BusinessLogic(IDataRepository data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }
        public List<string> GetProcessedData()
        {
            try
            {
                var rawData = _data.LoadData();
                // Geschäftslogik anwenden
                return rawData.Select(d => $"Verarbeitet: {d}").ToList();
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung
                Console.WriteLine($"Fehler beim Laden der Daten: {ex.Message}");
                return new List<string>();
            }
        }

        public bool ProcessData(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    return false;

                // Geschäftslogik
                var processedData = input.ToUpper().Trim();
                _data.SaveData(processedData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Verarbeiten: {ex.Message}");
                return false;
            }
        }

        public void Initialize()
        {
            _data.Connect();
        }

        public void Cleanup()
        {
            _data.Disconnect();
        }
    }
}
