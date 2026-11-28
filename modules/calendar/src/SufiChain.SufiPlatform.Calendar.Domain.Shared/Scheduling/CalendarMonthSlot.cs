namespace SufiChain.SufiPlatform.Calendar.Scheduling;

/// <summary>
/// A month-day click opens a one-hour event: the next whole hour when the day is today,
/// otherwise 09:00, matching the event editor's default times.
/// </summary>
public static class CalendarMonthSlot
{
    public static (DateTime StartUtc, DateTime EndUtc) ForDay(DateTime localDay, DateTime nowLocal, TimeZoneInfo zone)
    {
        var day = localDay.Date;
        var localStart = day == nowLocal.Date ? NextWholeHour(day, nowLocal) : day.AddHours(9);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localStart, DateTimeKind.Unspecified), zone);
        return (startUtc, startUtc.AddHours(1));
    }

    private static DateTime NextWholeHour(DateTime day, DateTime nowLocal)
    {
        var onTheHour = nowLocal.Minute == 0 && nowLocal.Second == 0 && nowLocal.Millisecond == 0;
        var hour = onTheHour ? nowLocal.Hour : nowLocal.Hour + 1;
        if (hour > 23)
        {
            hour = 23;
        }

        return new DateTime(day.Year, day.Month, day.Day, hour, 0, 0, DateTimeKind.Unspecified);
    }
}
