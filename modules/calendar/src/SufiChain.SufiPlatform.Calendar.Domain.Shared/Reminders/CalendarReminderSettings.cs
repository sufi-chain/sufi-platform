using System.Globalization;

namespace SufiChain.SufiPlatform.Calendar.Reminders;

public static class CalendarReminderSettings
{
    public const string DispatchIntervalSeconds = "Calendar.Reminders.DispatchIntervalSeconds";

    public const string CatchUpMinutes = "Calendar.Reminders.CatchUpMinutes";

    public const int DefaultDispatchIntervalSeconds = 30;

    public const int MinimumDispatchIntervalSeconds = 10;

    public const int DefaultCatchUpMinutes = 15;

    public static int ResolveIntervalSeconds(string? raw)
    {
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
        {
            seconds = DefaultDispatchIntervalSeconds;
        }

        return seconds < MinimumDispatchIntervalSeconds ? MinimumDispatchIntervalSeconds : seconds;
    }

    public static int ResolveCatchUpMinutes(string? raw)
    {
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes))
        {
            minutes = DefaultCatchUpMinutes;
        }

        return minutes < 1 ? DefaultCatchUpMinutes : minutes;
    }
}
