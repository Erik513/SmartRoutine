using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;

namespace SmartRoutine.UI.Helpers
{
    public static class StepTypeHelper
    {
        private static readonly Dictionary<StepType, string> StepTypeDisplayNames =
            new Dictionary<StepType, string>
            {
                { StepType.OpenUrl, "Webseite öffnen" },
                { StepType.OpenFolder, "Ordner öffnen" },
                { StepType.OpenApplication, "Programm starten" },
                { StepType.OpenDocument, "Dokument öffnen" }
            };

        public static Dictionary<StepType, string> GetStepTypeDisplayNames()
        {
            return new Dictionary<StepType, string>(StepTypeDisplayNames);
        }

        public static List<StepTypeOption> GetStepTypeOptions()
        {
            List<StepTypeOption> list =
                new List<StepTypeOption>();

            foreach (StepType type in Enum.GetValues(typeof(StepType)))
            {
                list.Add(
                    new StepTypeOption(
                        type,
                        GetDisplayName(type)));
            }

            return list;
        }

        public static string GetDisplayName(StepType type)
        {
            string displayName;

            if (StepTypeDisplayNames.TryGetValue(type, out displayName))
                return displayName;

            return type.ToString();
        }
    }

    public class StepTypeOption
    {
        public StepType Type { get; private set; }

        public string DisplayName { get; private set; }

        public StepTypeOption(
            StepType type,
            string displayName)
        {
            Type = type;
            DisplayName = displayName ?? "";
        }
    }
}