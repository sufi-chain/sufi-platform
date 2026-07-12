using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Caching;
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
        var cacheKey = BuildCacheKey(calendarId, tenantId ?? _currentTenant.Id);
        await _cache.RemoveAsync(cacheKey, token: cancellationToken);
    }

    public virtual async Task RemoveWithInheritorsAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        await RemoveAsync(calendarId, tenantId, cancellationToken);

        // A change on a parent calendar invalidates every calendar that inherits from it,
        // since their cached snapshots fold in the parent's rules/exceptions.
        var inheritorIds = await _calendarRepository.GetInheritingCalendarIdsAsync(calendarId, cancellationToken);
        foreach (var inheritorId in inheritorIds)
        {
            await RemoveAsync(inheritorId, tenantId, cancellationToken);
        }
    }

    protected virtual async Task<CalendarSnapshot> LoadAsync(Guid calendarId, CancellationToken cancellationToken)
    {
        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true, cancellationToken: cancellationToken);
        var inheritedCalendars = await _calendarRepository.GetInheritedCalendarsAsync(calendarId, cancellationToken);
        return CalendarSnapshotMapper.ToSnapshot(calendar, inheritedCalendars);
    }

    protected virtual string BuildCacheKey(Guid calendarId, Guid? tenantId)
    {
        return $"{tenantId?.ToString() ?? "host"}:{calendarId}";
    }
}
