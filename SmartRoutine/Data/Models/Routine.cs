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

    public abstract class RoutineStep
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Order { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Show { get; set; } = true;

        public abstract StepType Type { get; }

        public override string ToString()
        {
            return $"{Order + 1}. {Name}";
        }
    }

    // OpenUrl Step
    public class OpenUrlStep : RoutineStep
    {
        public override StepType Type => StepType.OpenUrl;

        public string Url { get; set; } = string.Empty;
        public bool OpenInExternBrowser { get; set; } = true; // false = in App, true = externer Browser
    }

    // OpenFolder Step
    public class OpenFolderStep : RoutineStep
    {
        public override StepType Type => StepType.OpenFolder;

        public string FolderPath { get; set; } = string.Empty;
        public bool OpenInNewWindow { get; set; } = true;
    }

    // OpenApplication Step
    public class OpenApplicationStep : RoutineStep
    {
        public override StepType Type => StepType.OpenApplication;

        public string ApplicationPath { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public bool RunAsAdmin { get; set; } = false;
        public string WorkingDirectory { get; set; } = string.Empty;
    }

    //// OpenDocument Step
    //public class OpenDocumentStep : RoutineStep
    //{
    //    public override StepType Type => StepType.OpenDocument;

    //    public string FilePath { get; set; } = string.Empty;
    //    public bool OpenWithAssociatedApp { get; set; } = true;
    //}

    public enum StepType
    {
        OpenUrl,
        OpenFolder,
        OpenApplication
    }

}
