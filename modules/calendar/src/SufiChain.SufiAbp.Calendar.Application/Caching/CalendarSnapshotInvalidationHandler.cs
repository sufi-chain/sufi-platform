using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace SufiChain.SufiAbp.Calendar.Caching;

public class CalendarSnapshotInvalidationHandler : IDistributedEventHandler<CalendarChangedEto>, ITransientDependency
{
    private readonly ICalendarSnapshotCache _snapshotCache;

    public CalendarSnapshotInvalidationHandler(ICalendarSnapshotCache snapshotCache)
    {
        _snapshotCache = snapshotCache;
    }

   public virtual async Task HandleEventAsync(CalendarChangedEto eventData)
   {
        await _snapshotCache.RemoveWithInheritorsAsync(eventData.CalendarId, eventData.TenantId);
   }
}
