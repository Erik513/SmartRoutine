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
            LiteDatabase db = OpenDatabase();

            try
            {
                return db.GetCollection<Routine>("routines")
                         .Query()
                         .OrderBy(r => r.Order)
                         .ToList();
            }
            finally
            {
                db.Dispose();
            }
        }

        public void AddRoutine(Routine routine)
        {
            LiteDatabase db = OpenDatabase();

            try
            {
                ILiteCollection<Routine> collection =
                    db.GetCollection<Routine>("routines");

                collection.Insert(routine);
            }
            finally
            {
                db.Dispose();
            }
        }

        public void UpdateRoutine(Routine routine)
        {
            LiteDatabase db = OpenDatabase();

            try
            {
                ILiteCollection<Routine> collection =
                    db.GetCollection<Routine>("routines");

                collection.Update(routine);
            }
            finally
            {
                db.Dispose();
            }
        }

        public void DeleteRoutine(string routineId)
        {
            LiteDatabase db = OpenDatabase();

            try
            {
                ILiteCollection<Routine> collection =
                    db.GetCollection<Routine>("routines");

                collection.Delete(routineId);
            }
            finally
            {
                db.Dispose();
            }
        }

        public Routine GetRoutine(string routineId)
        {
            LiteDatabase db = OpenDatabase();

            try
            {
                return db.GetCollection<Routine>("routines")
                         .FindById(routineId);
            }
            finally
            {
                db.Dispose();
            }
        }

        private LiteDatabase OpenDatabase()
        {
            return new LiteDatabase(
                $"Filename={_dbPath}; connection=shared");
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