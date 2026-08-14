using SufiChain.SufiPlatform.Calendar.Calendars;

namespace SufiChain.SufiPlatform.Calendar.Availability;

public sealed record CalendarSnapshot(
    Guid CalendarId,
    Guid? TenantId,
    CalendarKind Kind,
    string TimeZoneId,
    bool IsAlwaysOpen,
    IReadOnlyList<WorkingHourRuleSnapshot> Rules,
    IReadOnlyList<CalendarExceptionSnapshot> Exceptions)
{
    // NOTE: Exceptions include all one-level inherited parents. Rules include only parents
    // whose inheritance explicitly enables inherited working hours.
    public bool IsOpen => IsAlwaysOpen && Rules.Count == 0 && Exceptions.All(x => x.Kind != CalendarExceptionKind.Closed);
}

public sealed record WorkingHourRuleSnapshot(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

public sealed record CalendarExceptionSnapshot(DateOnly Date, CalendarExceptionKind Kind, IReadOnlyList<WorkingHourRange> Ranges, string? Description = null);
