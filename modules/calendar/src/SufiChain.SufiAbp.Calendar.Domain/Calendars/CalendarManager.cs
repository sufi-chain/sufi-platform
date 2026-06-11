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

    public virtual async Task<Calendar> CreateAsync(Guid id, Guid? tenantId, string name, CalendarKind kind, string timeZoneId, CalendarOwnerType ownerType = CalendarOwnerType.None, Guid? ownerId = null, bool isDefault = false, int? maxConcurrent = null, CancellationToken cancellationToken = default)
    {
        if (isDefault)
        {
            await EnsureDefaultIsUniqueAsync(tenantId, kind, cancellationToken);
        }

        return new Calendar(id, tenantId, name, kind, timeZoneId, ownerType, ownerId, isDefault, maxConcurrent);
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
