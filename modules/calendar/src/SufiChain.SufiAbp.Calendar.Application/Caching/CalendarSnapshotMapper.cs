using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;

namespace SufiChain.SufiAbp.Calendar.Caching;

public static class CalendarSnapshotMapper
{
    public static CalendarSnapshot ToSnapshot(Calendars.Calendar calendar, IReadOnlyList<Calendars.Calendar>? inheritedCalendars = null)
    {
        var ownRules = calendar.WorkingHourRules.Select(x => new WorkingHourRuleSnapshot(x.DayOfWeek, x.StartTime, x.EndTime)).ToList();
        var ownExceptions = calendar.Exceptions.Select(x => new CalendarExceptionSnapshot(x.Date, x.Kind, x.Ranges.ToList(), x.Description)).ToList();

        // Merge one-level inherited calendars' rules and exceptions so the calculator
        // honors parent working hours and parent off-day exceptions transparently.
        var rules = ownRules;
        var exceptions = ownExceptions;

        if (inheritedCalendars is { Count: > 0 })
        {
            rules = ownRules
                .Concat(inheritedCalendars.SelectMany(p => p.WorkingHourRules.Select(x => new WorkingHourRuleSnapshot(x.DayOfWeek, x.StartTime, x.EndTime))))
                .ToList();
            exceptions = ownExceptions
                .Concat(inheritedCalendars.SelectMany(p => p.Exceptions.Select(x => new CalendarExceptionSnapshot(x.Date, x.Kind, x.Ranges.ToList(), x.Description))))
                .ToList();
        }

        return new CalendarSnapshot(
            calendar.Id,
            calendar.TenantId,
            calendar.Kind,
            calendar.TimeZoneId,
            calendar.IsAlwaysOpen,
            rules,
            exceptions);
    }
}
