using LiteDB;
using SmartRoutine.Data.Interfaces;
using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartRoutine.Data
{
    public class RoutineRepository : IRoutineRepository
    {
        private readonly string _dbPath;

        // Standardkonstruktor für die echte App
        public RoutineRepository() : this("SmartRoutine.db")
        {
        }

        // Konstruktor für Tests (erlaubt eigenen Pfad)
        public RoutineRepository(string dbPath)
        {
            _dbPath = dbPath;

            // Konfiguriere den Mapper für bessere Typ-Namen
            BsonMapper.Global.RegisterType<RoutineStep>
            (
                serialize: (step) => step.GetType().Name,  // Speichert "OpenUrlStep"
                deserialize: (bson) => null // Wird automatisch von LiteDB gemacht
            );
        }

        // Hilfsmethode, um eine Datenbankverbindung zu öffnen
        private LiteDatabase OpenDatabase()
        {
            // 'connection=true' ist wichtig, damit BsonMapper automatisch Ihre Eigenschaften mapped
            return new LiteDatabase($"Filename={_dbPath}; connection=shared");
        }

        public List<Routine> LoadRoutines()
        {
            using (var db = OpenDatabase())
            {
                // Holt alle Routinen aus der Collection "routines"
                // OrderBy sorgt für die richtige Reihenfolge
                return db.GetCollection<Routine>("routines")
                         .Query()
                         .OrderBy(r => r.Order)
                         .ToList();
            }
        }

        public void AddRoutine(Routine routine)
        {
            using (var db = OpenDatabase())
            {
                var col = db.GetCollection<Routine>("routines");
                col.Insert(routine); // Die ID wird automatisch vergeben
            }
        }

        public void UpdateRoutine(Routine routine)
        {
            using (var db = OpenDatabase())
            {
                var col = db.GetCollection<Routine>("routines");
                col.Update(routine); // Aktualisiert die Routine anhand der Id
            }
        }

        public void DeleteRoutine(string routineId)
        {
            using (var db = OpenDatabase())
            {
                var col = db.GetCollection<Routine>("routines");
                col.Delete(routineId);
            }
        }

        // Diese Methode brauchen Sie für Ihre Service-Logik
        public Routine GetRoutine(string routineId)
        {
            using (var db = OpenDatabase())
            {
                return db.GetCollection<Routine>("routines")
                         .FindById(routineId);
            }
        }

        // Die SaveRoutines-Methode wird mit LiteDB nicht mehr benötigt.
        // Sie können sie aus dem Interface entfernen oder leer lassen.
        public void SaveRoutines(List<Routine> routines)
        {
            // Wird nicht mehr benötigt, da jede Änderung direkt gespeichert wird.
            // Sie können die Methode aus Ihrem Interface IRoutineRepository entfernen.
            throw new NotImplementedException("Use AddRoutine, UpdateRoutine, and DeleteRoutine instead.");
        }
    }
}