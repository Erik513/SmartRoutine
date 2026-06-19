using System;

namespace SmartRoutine.UI.Helpers
{
    public static class DateTimeHelper
    {
        public static string GetRelativeTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "Noch nie gestartet";

            TimeSpan difference = DateTime.Now - dateTime.Value;

            if (difference.TotalSeconds < 60)
                return "Gerade eben";

            if (difference.TotalMinutes < 60)
                return FormatTimeText(
                    (int)difference.TotalMinutes,
                    "Minute",
                    "Minuten");

            if (difference.TotalHours < 24)
                return FormatTimeText(
                    (int)difference.TotalHours,
                    "Stunde",
                    "Stunden");

            if (difference.TotalDays < 7)
                return FormatTimeText(
                    (int)difference.TotalDays,
                    "Tag",
                    "Tagen");

            return dateTime.Value.ToString("dd.MM.yyyy HH:mm");
        }

        private static string FormatTimeText(
            int value,
            string singular,
            string plural)
        {
            string unit = value == 1
                ? singular
                : plural;

            return $"Vor {value} {unit}";
        }
    }
}