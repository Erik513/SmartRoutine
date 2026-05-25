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

        public static List<KeyValuePair<StepType, string>> GetStepTypeListWithEmpty()
        {
            List<KeyValuePair<StepType, string>> list =
                new List<KeyValuePair<StepType, string>>();

            foreach (StepType type in Enum.GetValues(typeof(StepType)))
            {
                string displayName;

                if (!StepTypeDisplayNames.TryGetValue(type, out displayName))
                    displayName = type.ToString();

                list.Add(new KeyValuePair<StepType, string>(type, displayName));
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
}