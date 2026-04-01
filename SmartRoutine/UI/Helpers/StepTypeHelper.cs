using SmartRoutine.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine.UI.Helpers
{
    public static class StepTypeHelper
    {
        public static Dictionary<StepType, string> GetStepTypeDisplayNames()
        {
            return new Dictionary<StepType, string>
        {
            { StepType.OpenUrl, "Webseite öffnen" }
            // Weitere später:
            // { StepType.OpenFolder, "Ordner öffnen" },
            // { StepType.OpenApplication, "Programm starten" },
            // { StepType.Wait, "Warten" },
            // { StepType.Message, "Nachricht anzeigen" }
        };
        }

        public static List<KeyValuePair<StepType, string>> GetStepTypeListWithEmpty()
        {
            var list = new List<KeyValuePair<StepType, string>>();
            // Alle Enum-Werte mit Anzeigenamen
            var displayNames = GetStepTypeDisplayNames();
            foreach (StepType type in Enum.GetValues(typeof(StepType)))
            {
                if (displayNames.ContainsKey(type))
                {
                    list.Add(new KeyValuePair<StepType, string>(type, displayNames[type]));
                }
                else
                {
                    list.Add(new KeyValuePair<StepType, string>(type, type.ToString()));
                }
            }
            return list;
        }
    }
}
