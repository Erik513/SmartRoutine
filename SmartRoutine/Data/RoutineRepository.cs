using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace SmartRoutine.Data
{
    public class RoutineRepository : IRoutineRepository
    {
        private readonly string _dataPath;
        private List<Routine> _routines;
        public RoutineRepository()
        {
            _dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SmartRoutine",
                "routines.json");

            _routines = LoadFromFile();
        }

        private List<Routine> LoadFromFile()
        {
            try
            {
                if (File.Exists(_dataPath))
                {
                    var json = File.ReadAllText(_dataPath);
                    return JsonSerializer.Deserialize<List<Routine>>(json) ?? new List<Routine>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden: {ex.Message}");
            }

            return new List<Routine>();
        }

        private void SaveToFile()
        {
            try
            {
                var directory = Path.GetDirectoryName(_dataPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var json = JsonSerializer.Serialize(_routines, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_dataPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern: {ex.Message}");
            }
        }

        public List<Routine> LoadRoutines() => _routines;

        public void SaveRoutines(List<Routine> routines)
        {
            _routines = routines;
            SaveToFile();
        }

        public void AddRoutine(Routine routine)
        {
            _routines.Add(routine);
            SaveToFile();
        }

        public void UpdateRoutine(Routine routine)
        {
            var index = _routines.FindIndex(r => r.Id == routine.Id);
            if (index >= 0)
            {
                // Steps vor dem Speichern sortieren
                if (routine.Steps != null && routine.Steps.Any())
                {
                    routine.Steps = routine.Steps.OrderBy(s => s.Order).ToList();
                }

                routine.UpdatedAt = DateTime.Now;
                _routines[index] = routine;
                SaveToFile();
            }
        }

        public void DeleteRoutine(string routineId)
        {
            _routines.RemoveAll(r => r.Id == routineId);
            SaveToFile();
        }

        public Routine GetRoutine(string routineId)
        {
            return _routines.FirstOrDefault(r => r.Id == routineId);
        }
    }
}
