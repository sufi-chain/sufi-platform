using SufiChain.SufiAbp.Data;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public static class CalendarDtoMapper
{
    public static CalendarDto ToDto(Calendar calendar)
    {
        return new CalendarDto
        {
            Id = calendar.Id,
            TenantId = calendar.TenantId,
            Name = calendar.Name,
            Kind = calendar.Kind,
            TimeZoneId = calendar.TimeZoneId,
            OwnerUserId = calendar.OwnerUserId,
            OwnerName = calendar.OwnerName,
            IsDefault = calendar.IsDefault,
            IsAlwaysOpen = calendar.IsAlwaysOpen,
            CreationTime = calendar.CreationTime,
            CreatorId = calendar.CreatorId,
            LastModificationTime = calendar.LastModificationTime,
            LastModifierId = calendar.LastModifierId,
            IsDeleted = calendar.IsDeleted,
            DeleterId = calendar.DeleterId,
            DeletionTime = calendar.DeletionTime,
            ExtraProperties = new ExtraPropertyDictionary(calendar.ExtraProperties),
            WorkingHourRules = calendar.WorkingHourRules.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).Select(ToDto).ToList(),
            Exceptions = calendar.Exceptions.Select(ToDto).ToList(),
            Inheritances = calendar.Inheritances.Select(x => new CalendarInheritanceDto
            {
                Id = x.Id,
                CalendarId = x.CalendarId,
                ParentCalendarId = x.ParentCalendarId,
                IsInheritedByDefault = x.IsInheritedByDefault
            }).ToList()
        };
    }

    public static CalendarLookupDto ToLookupDto(Calendar calendar)
    {
        return new CalendarLookupDto
        {
            Id = calendar.Id,
            Name = calendar.Name,
            Kind = calendar.Kind,
            TimeZoneId = calendar.TimeZoneId,
            OwnerUserId = calendar.OwnerUserId,
            OwnerName = calendar.OwnerName,
            IsDefault = calendar.IsDefault
        };
    }

    public static WorkingHourRuleDto ToDto(WorkingHourRule rule)
    {
        return new WorkingHourRuleDto
        {
            Id = rule.Id,
            CalendarId = rule.CalendarId,
            DayOfWeek = rule.DayOfWeek,
            StartTime = rule.StartTime.ToTimeSpan(),
            EndTime = rule.EndTime.ToTimeSpan(),
        };
    }

    public static CalendarExceptionDto ToDto(CalendarException exception)
    {
        return new CalendarExceptionDto
        {
            Id = exception.Id,
            CalendarId = exception.CalendarId,
            Date = exception.Date.ToDateTime(TimeOnly.MinValue),
            Kind = exception.Kind,
            Description = exception.Description,
            Ranges = exception.Ranges.Select(x => new WorkingHourRangeDto
            {
                StartTime = x.StartTime.ToTimeSpan(),
                EndTime = x.EndTime.ToTimeSpan()
            }).ToList()
        };
    }
}
