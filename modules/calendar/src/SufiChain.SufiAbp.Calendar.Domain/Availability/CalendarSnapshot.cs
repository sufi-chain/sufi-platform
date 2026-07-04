using SufiChain.SufiAbp.Calendar.Calendars;

namespace SufiChain.SufiAbp.Calendar.Availability;

public sealed record CalendarSnapshot(
    Guid CalendarId,
    Guid? TenantId,
    CalendarKind Kind,
    string TimeZoneId,
    bool IsAlwaysOpen,
    IReadOnlyList<WorkingHourRuleSnapshot> Rules,
    IReadOnlyList<CalendarExceptionSnapshot> Exceptions)
{
    // NOTE: Rules and Exceptions include the calendar's own entries plus any entries
    // inherited from parent calendars (one level). They are merged by the snapshot
    // mapper so the calculator treats inherited rules/exceptions exactly like local ones.
    public bool IsOpen => IsAlwaysOpen && Rules.Count == 0 && Exceptions.All(x => x.Kind != CalendarExceptionKind.Closed);
}

public sealed record WorkingHourRuleSnapshot(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

public sealed record CalendarExceptionSnapshot(DateOnly Date, CalendarExceptionKind Kind, IReadOnlyList<WorkingHourRange> Ranges, string? Description = null);
