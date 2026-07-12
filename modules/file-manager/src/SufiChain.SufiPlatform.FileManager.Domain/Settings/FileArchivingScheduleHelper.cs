namespace SufiChain.SufiPlatform.FileManager.Settings;

/// <summary>
/// Computes delay until the next archiving run from a cron expression.
/// Supports standard daily schedules in the form "minute hour * * *".
/// </summary>
public static class FileArchivingScheduleHelper
{
    private const int DefaultPeriodMs = 24 * 60 * 60 * 1000;

    /// <summary>
    /// Returns milliseconds until the next scheduled run.
    /// Falls back to 24 hours when the cron expression cannot be parsed.
    /// </summary>
    public static int GetPeriodMilliseconds(string? cronExpression, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return DefaultPeriodMs;
        }

        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            return DefaultPeriodMs;
        }

        if (!int.TryParse(parts[0], out var minute) || !int.TryParse(parts[1], out var hour))
        {
            return DefaultPeriodMs;
        }

        if (minute is < 0 or > 59 || hour is < 0 or > 23)
        {
            return DefaultPeriodMs;
        }

        var nextRun = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, hour, minute, 0, DateTimeKind.Utc);
        if (nextRun <= utcNow)
        {
            nextRun = nextRun.AddDays(1);
        }

        var delay = nextRun - utcNow;
        var milliseconds = (int)Math.Min(delay.TotalMilliseconds, int.MaxValue);
        return milliseconds > 0 ? milliseconds : DefaultPeriodMs;
    }
}
