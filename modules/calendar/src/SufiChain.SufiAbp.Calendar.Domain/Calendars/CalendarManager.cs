using SufiChain.SufiAbp;
using SufiChain.SufiAbp.Domain.Services;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public class CalendarManager : DomainService
{
    private readonly ICalendarRepository _calendarRepository;

    public CalendarManager(ICalendarRepository calendarRepository)
    {
        _calendarRepository = calendarRepository;
    }

    public virtual async Task<Calendar> CreateAsync(Guid id, Guid? tenantId, string name, CalendarKind kind, string timeZoneId, Guid? ownerUserId = null, string? ownerName = null, bool isDefault = false, bool isAlwaysOpen = false, CancellationToken cancellationToken = default)
    {
        if (isDefault)
        {
            await EnsureDefaultIsUniqueAsync(tenantId, kind, cancellationToken);
        }

        return new Calendar(id, tenantId, name, kind, timeZoneId, ownerUserId, ownerName, isDefault, isAlwaysOpen);
    }

    public virtual async Task<CalendarInheritance> AddInheritanceAsync(Calendar calendar, Calendar parentCalendar, bool isInheritedByDefault = false, CancellationToken cancellationToken = default)
    {
        if (calendar.Id == parentCalendar.Id)
        {
            throw new BusinessException(CalendarErrorCodes.CalendarCannotInheritItself);
        }

        if (parentCalendar.Inheritances.Count > 0)
        {
            throw new BusinessException(CalendarErrorCodes.InheritanceExceedsOneLevel);
        }

        if (calendar.Inheritances.Any(x => x.ParentCalendarId == parentCalendar.Id))
        {
            throw new BusinessException(CalendarErrorCodes.InheritanceCycleDetected);
        }

        var inheritance = new CalendarInheritance(GuidGenerator.Create(), calendar.Id, parentCalendar.Id, isInheritedByDefault);
        calendar.AddInheritance(inheritance);
        return await Task.FromResult(inheritance);
    }

    public virtual Task RemoveInheritanceAsync(Calendar calendar, Guid parentCalendarId, CancellationToken cancellationToken = default)
    {
        calendar.RemoveInheritance(parentCalendarId);
        return Task.CompletedTask;
    }

    public virtual async Task SetDefaultAsync(Calendar calendar, bool isDefault, CancellationToken cancellationToken = default)
    {
        if (isDefault && !calendar.IsDefault)
        {
            await EnsureDefaultIsUniqueAsync(calendar.TenantId, calendar.Kind, cancellationToken);
        }

        calendar.SetDefault(isDefault);
    }

    protected virtual async Task EnsureDefaultIsUniqueAsync(Guid? tenantId, CalendarKind kind, CancellationToken cancellationToken)
    {
        var existing = await _calendarRepository.FindDefaultAsync(tenantId, kind, cancellationToken);
        if (existing != null)
        {
            throw new BusinessException(CalendarErrorCodes.DefaultCalendarAlreadyExists);
        }
    }
}
