using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine.Data.Models
{
    public class Routine
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public List<RoutineStep> Steps { get; set; } = new List<RoutineStep>();
        public bool IsNew { get; set; } = false;

        public override string ToString()
        {
            return Name;
        }
    }

    public class RoutineStep
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Order { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Show { get; set; } = true;
        public StepType Type { get; set; }
        public string Value { get; set; } = string.Empty; // URL oder Pfad

        public override string ToString()
        {
            string result = $"{Order + 1}. {Name}";
            System.Diagnostics.Debug.WriteLine($"ToString: {result}, Order={Order}");
            return result;
        }
        public void EnsureCorrectOrder(int newOrder)
        {
            Order = newOrder;
        }
    }

    public enum StepType
    {
        OpenUrl,
        //OpenFolder,
        //OpenApplication,
        //Wait,
        //Message
    }

}
