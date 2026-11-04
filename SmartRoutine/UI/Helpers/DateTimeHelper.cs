using SmartRoutine.Localization;
using System;
using System.Globalization;

namespace SmartRoutine.UI.Helpers
{
    public static class DateTimeHelper
    {
        public static string GetRelativeTime(DateTime? dateTime)
        {
            return GetRelativeTime(dateTime, DateTime.Now);
        }

        // "now" is a parameter so that the texts can be tested.
        public static string GetRelativeTime(DateTime? dateTime, DateTime now)
        {
            if (!dateTime.HasValue)
                return Loc.S("Time.Never");

            TimeSpan difference = now - dateTime.Value;

            if (difference.TotalSeconds < 60)
                return Loc.S("Time.JustNow");

            if (difference.TotalMinutes < 60)
                return FormatAgo((int)difference.TotalMinutes, "Time.MinuteAgo", "Time.MinutesAgo");

            if (difference.TotalHours < 24)
                return FormatAgo((int)difference.TotalHours, "Time.HourAgo", "Time.HoursAgo");

            if (difference.TotalDays < 7)
                return FormatAgo((int)difference.TotalDays, "Time.DayAgo", "Time.DaysAgo");

            // The format of the date follows the language (the culture of the computer does not matter).
            return dateTime.Value.ToString(Loc.S("Time.DateFormat"), CultureInfo.InvariantCulture);
        }

        private static string FormatAgo(int value, string singularKey, string pluralKey)
        {
            return value == 1 ? Loc.S(singularKey) : Loc.S(pluralKey, value);
        }
    }
}
