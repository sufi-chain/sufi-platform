using Microsoft.Extensions.Caching.Distributed;
using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.DependencyInjection;
using SufiChain.SufiAbp.MultiTenancy;

namespace SufiChain.SufiAbp.Calendar.Caching;

public class CalendarSnapshotCache : ICalendarSnapshotCache, ITransientDependency
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    };

    private readonly IDistributedCache<CalendarSnapshotCacheItem> _cache;
    private readonly ICalendarRepository _calendarRepository;
    private readonly ICurrentTenant _currentTenant;

    public CalendarSnapshotCache(
        IDistributedCache<CalendarSnapshotCacheItem> cache,
        ICalendarRepository calendarRepository,
        ICurrentTenant currentTenant)
    {
        _cache = cache;
        _calendarRepository = calendarRepository;
        _currentTenant = currentTenant;
    }

    public virtual async Task<CalendarSnapshot> GetAsync(Guid calendarId, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(calendarId, _currentTenant.Id);
        var item = await _cache.GetOrAddAsync(
            cacheKey,
            async () => new CalendarSnapshotCacheItem { Snapshot = await LoadAsync(calendarId, cancellationToken) },
            () => CacheOptions);

        return item!.Snapshot;
    }

    public virtual async Task RemoveAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(BuildCacheKey(calendarId, tenantId ?? _currentTenant.Id), token: cancellationToken);
    }

    protected virtual async Task<CalendarSnapshot> LoadAsync(Guid calendarId, CancellationToken cancellationToken)
    {
        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true, cancellationToken: cancellationToken);
        return CalendarSnapshotMapper.ToSnapshot(calendar);
    }

    protected virtual string BuildCacheKey(Guid calendarId, Guid? tenantId)
    {
        return $"{tenantId?.ToString() ?? "host"}:{calendarId}";
    }
}
