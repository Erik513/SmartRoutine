using SmartRoutine.Data.Models;
using SmartRoutine.Localization;
using System;
using System.Collections.Generic;

namespace SmartRoutine.UI.Helpers
{
    public static class StepTypeHelper
    {
        public static List<StepTypeOption> GetStepTypeOptions()
        {
            List<StepTypeOption> list =
                new List<StepTypeOption>();

            foreach (StepType type in Enum.GetValues(typeof(StepType)))
                list.Add(new StepTypeOption(type));

            return list;
        }

        public static string GetDisplayName(StepType type)
        {
            return Loc.S("StepType." + type);
        }
    }

    // An entry of the list of actions. The text is asked each time it is read, so it follows the language.
    public class StepTypeOption
    {
        public StepType Type { get; private set; }

        public string DisplayName
        {
            get { return StepTypeHelper.GetDisplayName(Type); }
        }

        public StepTypeOption(StepType type)
        {
            Type = type;
        }
    }
}
