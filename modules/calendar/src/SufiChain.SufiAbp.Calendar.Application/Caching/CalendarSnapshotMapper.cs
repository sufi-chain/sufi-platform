using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;

namespace SufiChain.SufiAbp.Calendar.Caching;

public static class CalendarSnapshotMapper
{
    public static CalendarSnapshot ToSnapshot(Calendars.Calendar calendar)
    {
        return new CalendarSnapshot(
            calendar.Id,
            calendar.TenantId,
            calendar.Kind,
            calendar.TimeZoneId,
            calendar.MaxConcurrent,
            calendar.WorkingHourRules.Select(x => new WorkingHourRuleSnapshot(x.DayOfWeek, x.StartTime, x.EndTime, x.MaxConcurrent)).ToList(),
            calendar.Exceptions.Select(x => new CalendarExceptionSnapshot(x.Date, x.Kind, x.Ranges.ToList(), x.Description)).ToList());
    }
}
