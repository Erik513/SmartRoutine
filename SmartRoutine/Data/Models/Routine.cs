using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine.Data.Models
{
    public class Routine
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public List<RoutineStep> Steps { get; set; } = new List<RoutineStep>();

        public override string ToString()
        {
            return Name;
        }
    }

    public class RoutineStep
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Order { get; set; }
        public StepType Type { get; set; }
        public string Value { get; set; } = string.Empty; // URL oder Pfad
        public string Description { get; set; } = string.Empty;
    }

    public enum StepType
    {
        OpenUrl,
        OpenFolder,
        OpenApplication,
        Wait,
        Message
    }
}
