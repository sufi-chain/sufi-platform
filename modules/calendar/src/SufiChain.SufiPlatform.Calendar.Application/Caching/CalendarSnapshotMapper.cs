using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Calendars;

namespace SufiChain.SufiPlatform.Calendar.Caching;

public static class CalendarSnapshotMapper
{
    public static CalendarSnapshot ToSnapshot(Calendars.Calendar calendar, IReadOnlyList<Calendars.Calendar>? inheritedCalendars = null)
    {
        var ownRules = calendar.WorkingHourRules.Select(x => new WorkingHourRuleSnapshot(x.DayOfWeek, x.StartTime, x.EndTime)).ToList();
        var ownExceptions = calendar.Exceptions.Select(x => new CalendarExceptionSnapshot(x.Date, x.Kind, x.Ranges.ToList(), x.Description)).ToList();

        // Parent events are resolved separately by CalendarEventAppService. Availability
        // always includes parent exceptions (for example shared holidays), while working
        // hours are inherited only when the relationship explicitly enables them.
        var rules = ownRules;
        var exceptions = ownExceptions;

        if (inheritedCalendars is { Count: > 0 })
        {
            var workingHourParentIds = calendar.Inheritances
                .Where(x => x.IsInheritedByDefault)
                .Select(x => x.ParentCalendarId)
                .ToHashSet();

            rules = ownRules
                .Concat(inheritedCalendars
                    .Where(parent => workingHourParentIds.Contains(parent.Id))
                    .SelectMany(parent => parent.WorkingHourRules.Select(x =>
                        new WorkingHourRuleSnapshot(x.DayOfWeek, x.StartTime, x.EndTime))))
                .ToList();
            exceptions = ownExceptions
                .Concat(inheritedCalendars.SelectMany(parent => parent.Exceptions.Select(x =>
                    new CalendarExceptionSnapshot(x.Date, x.Kind, x.Ranges.ToList(), x.Description))))
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
