using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRoutine.UI.Helpers
{
    public static class DateTimeHelper
    {
        public static string GetRelativeTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "Noch nie gestartet";

            TimeSpan diff = DateTime.Now - dateTime.Value;

            if (diff.TotalSeconds < 60)
                return "Gerade eben";

            if (diff.TotalMinutes < 60)
                return $"Vor {(int)diff.TotalMinutes} Minute(n)";

            if (diff.TotalHours < 24)
                return $"Vor {(int)diff.TotalHours} Stunde(n)";

            if (diff.TotalDays < 7)
                return $"Vor {(int)diff.TotalDays} Tag(en)";

            return dateTime.Value.ToString("dd.MM.yyyy HH:mm");
        }
    }
}
