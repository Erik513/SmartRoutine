using LiteDB;
using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartRoutine.Data
{
    public class RoutineRepository : IRoutineRepository
    {
        private const string DatabaseFileName = "SmartRoutine.db";
        private const string RoutinesCollectionName = "routines";

        private readonly string _dbPath;

        public RoutineRepository()
            : this(GetDefaultDatabasePath())
        {
        }

        public RoutineRepository(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
                throw new ArgumentException(
                    "Database path cannot be null or empty.",
                    nameof(dbPath));

            _dbPath = dbPath;
        }

        public List<Routine> LoadRoutines()
        {
            return WithRoutines(routines => routines
                .Query()
                .OrderBy(r => r.Order)
                .ToList());
        }

        public void AddRoutine(Routine routine)
        {
            WithRoutines(routines => routines.Insert(routine));
        }

        public void UpdateRoutine(Routine routine)
        {
            WithRoutines(routines => routines.Update(routine));
        }

        public void DeleteRoutine(string routineId)
        {
            WithRoutines(routines => routines.Delete(routineId));
        }

        public Routine GetRoutine(string routineId)
        {
            return WithRoutines(routines => routines.FindById(routineId));
        }

        // Opens the database for one operation and closes it again (other programs and the tests
        // can use the file in between: the connection is shared).
        private T WithRoutines<T>(Func<ILiteCollection<Routine>, T> operation)
        {
            using (LiteDatabase db = new LiteDatabase($"Filename={_dbPath}; connection=shared"))
            {
                return operation(db.GetCollection<Routine>(RoutinesCollectionName));
            }
        }

        private static string GetDefaultDatabasePath()
        {
            string appDataFolder =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData);

            string smartRoutineFolder =
                Path.Combine(appDataFolder, "SmartRoutine");

            if (!Directory.Exists(smartRoutineFolder))
            {
                Directory.CreateDirectory(smartRoutineFolder);
            }

            return Path.Combine(
                smartRoutineFolder,
                DatabaseFileName);
        }
    }
}