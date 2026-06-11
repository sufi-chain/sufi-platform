using SufiChain.SufiAbp.Calendar.Calendars;

namespace SufiChain.SufiAbp.Calendar.Availability;

public sealed record CalendarSnapshot(
    Guid CalendarId,
    Guid? TenantId,
    CalendarKind Kind,
    string TimeZoneId,
    int? MaxConcurrent,
    IReadOnlyList<WorkingHourRuleSnapshot> Rules,
    IReadOnlyList<CalendarExceptionSnapshot> Exceptions)
{
    public bool IsAlwaysOpen => Kind == CalendarKind.WorkingHours && Rules.Count == 0 && Exceptions.All(x => x.Kind != CalendarExceptionKind.Closed);
}

public sealed record WorkingHourRuleSnapshot(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, int? MaxConcurrent);

public sealed record CalendarExceptionSnapshot(DateOnly Date, CalendarExceptionKind Kind, IReadOnlyList<WorkingHourRange> Ranges, string? Description = null);
