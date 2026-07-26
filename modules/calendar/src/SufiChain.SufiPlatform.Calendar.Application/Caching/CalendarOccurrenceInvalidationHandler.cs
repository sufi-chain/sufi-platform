using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Scheduling;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Calendar.Caching;

public class CalendarOccurrenceInvalidationHandler : IDistributedEventHandler<CalendarEventChangedEto>, ITransientDependency
{
    private readonly ICalendarOccurrenceExpansionCache _occurrenceCache;
    private readonly ICurrentTenant _currentTenant;

    public CalendarOccurrenceInvalidationHandler(
        ICalendarOccurrenceExpansionCache occurrenceCache,
        ICurrentTenant currentTenant)
    {
        _occurrenceCache = occurrenceCache;
        _currentTenant = currentTenant;
    }

    public virtual async Task HandleEventAsync(CalendarEventChangedEto eventData)
    {
        using (_currentTenant.Change(eventData.TenantId))
        {
            // Parent calendar event changes must also drop inheritor merged views if we ever
            // cache merged results; ExpandAsync is per-calendar so only this calendar's stamp
            // needs bumping — child GETs re-read the parent's (also invalidated) expansion.
            await _occurrenceCache.RemoveAsync(eventData.CalendarId, eventData.TenantId);
        }
    }
}
