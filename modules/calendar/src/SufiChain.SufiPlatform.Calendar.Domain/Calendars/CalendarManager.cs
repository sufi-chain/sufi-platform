using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CalendarManager : DomainService
{
    private readonly ICalendarRepository _calendarRepository;

    public CalendarManager(ICalendarRepository calendarRepository)
    {
        _calendarRepository = calendarRepository;
    }

    public virtual async Task<Calendar> CreateAsync(
        Guid id,
        Guid? tenantId,
        string name,
        CalendarKind kind,
        string timeZoneId,
        Guid? ownerUserId = null,
        string? ownerName = null,
        bool isDefault = false,
        bool isAlwaysOpen = false,
        string? color = null,
        CancellationToken cancellationToken = default)
    {
        if (isDefault)
        {
            await EnsureDefaultIsUniqueAsync(tenantId, kind, cancellationToken);
        }

        var calendar = new Calendar(
            id,
            tenantId,
            name,
            kind,
            timeZoneId,
            ownerUserId,
            ownerName,
            isDefault,
            isAlwaysOpen,
            color);

        await EnsureDefaultCalendarInheritanceAsync(calendar, cancellationToken);

        return calendar;
    }

    public virtual async Task<CalendarInheritance> AddInheritanceAsync(Calendar calendar, Calendar parentCalendar, bool isInheritedByDefault = false, CancellationToken cancellationToken = default)
    {
        if (calendar.Id == parentCalendar.Id)
        {
            throw new BusinessException(CalendarErrorCodes.CalendarCannotInheritItself);
        }

        var parentInheritedCalendars = await _calendarRepository.GetInheritedCalendarsAsync(parentCalendar.Id, cancellationToken);
        if (parentInheritedCalendars.Any(x => x.Kind != CalendarKind.Default))
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

    public virtual Task UpdateInheritanceAsync(Calendar calendar, Guid parentCalendarId, bool isInheritedByDefault, CancellationToken cancellationToken = default)
    {
        var inheritance = calendar.Inheritances.FirstOrDefault(x => x.ParentCalendarId == parentCalendarId);
        if (inheritance == null)
        {
            throw new BusinessException(CalendarErrorCodes.CalendarInheritanceNotFound);
        }

        inheritance.SetInheritedByDefault(isInheritedByDefault);
        calendar.NotifyChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ensures a non-default calendar inherits the tenant's default calendar.
    /// The relationship includes shared events and exceptions only; working hours
    /// remain calendar-specific unless a user explicitly enables them later.
    /// </summary>
    public virtual async Task<bool> EnsureDefaultCalendarInheritanceAsync(
        Calendar calendar,
        CancellationToken cancellationToken = default)
    {
        if (calendar.Kind == CalendarKind.Default)
        {
            return false;
        }

        var defaultCalendar = await _calendarRepository.FindDefaultAsync(
            calendar.TenantId,
            CalendarKind.Default,
            cancellationToken);

        if (defaultCalendar == null ||
            calendar.Inheritances.Any(x => x.ParentCalendarId == defaultCalendar.Id))
        {
            return false;
        }

        await AddInheritanceAsync(
            calendar,
            defaultCalendar,
            isInheritedByDefault: false,
            cancellationToken);
        return true;
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
