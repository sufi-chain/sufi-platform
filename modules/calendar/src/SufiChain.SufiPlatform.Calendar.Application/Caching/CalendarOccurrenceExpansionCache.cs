using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Calendars;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.Calendar.Scheduling;

namespace SufiChain.SufiPlatform.Calendar.Caching;

/// <summary>
/// Versioned distributed cache for expanded occurrences per calendar + window.
/// Inheritance-safe: each calendar id is cached independently; parent event changes
/// invalidate the parent key (child GetOccurrences re-reads parent expansion).
/// </summary>
public class CalendarOccurrenceExpansionCache : ICalendarOccurrenceExpansionCache
{
    private static readonly DistributedCacheEntryOptions ExpansionCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
    };

    private static readonly DistributedCacheEntryOptions VersionCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
    };

    private readonly IDistributedCache<CalendarOccurrenceExpansionCacheItem> _expansionCache;
    private readonly IDistributedCache<CalendarOccurrenceVersionCacheItem> _versionCache;
    private readonly ICalendarRepository _calendarRepository;
    private readonly ICurrentTenant _currentTenant;

    public CalendarOccurrenceExpansionCache(
        IDistributedCache<CalendarOccurrenceExpansionCacheItem> expansionCache,
        IDistributedCache<CalendarOccurrenceVersionCacheItem> versionCache,
        ICalendarRepository calendarRepository,
        ICurrentTenant currentTenant)
    {
        _expansionCache = expansionCache;
        _versionCache = versionCache;
        _calendarRepository = calendarRepository;
        _currentTenant = currentTenant;
    }

    public virtual async Task<IReadOnlyList<EventOccurrence>> GetOrAddAsync(
        Guid calendarId,
        DateTime fromUtc,
        DateTime toUtc,
        Func<Task<IReadOnlyList<EventOccurrence>>> factory,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenant.Id;
        var version = await GetOrCreateVersionAsync(calendarId, tenantId, cancellationToken);
        var cacheKey = BuildExpansionKey(tenantId, calendarId, version, fromUtc, toUtc);

        var item = await _expansionCache.GetOrAddAsync(
            cacheKey,
            async () => new CalendarOccurrenceExpansionCacheItem
            {
                Occurrences = (await factory()).ToList()
            },
            () => ExpansionCacheOptions,
            hideErrors: false,
            considerUow: false,
            token: cancellationToken);

        return item!.Occurrences;
    }

    public virtual async Task RemoveAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var effectiveTenantId = tenantId ?? _currentTenant.Id;
        using (_currentTenant.Change(effectiveTenantId))
        {
            // Removing the version stamp orphans all window entries for this calendar.
            await _versionCache.RemoveAsync(BuildVersionKey(effectiveTenantId, calendarId), token: cancellationToken);
        }
    }

    public virtual async Task RemoveWithInheritorsAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        await RemoveAsync(calendarId, tenantId, cancellationToken);

        var inheritorIds = await _calendarRepository.GetInheritingCalendarIdsAsync(calendarId, cancellationToken);
        foreach (var inheritorId in inheritorIds)
        {
            await RemoveAsync(inheritorId, tenantId, cancellationToken);
        }
    }

    protected virtual async Task<Guid> GetOrCreateVersionAsync(Guid calendarId, Guid? tenantId, CancellationToken cancellationToken)
    {
        var key = BuildVersionKey(tenantId, calendarId);
        var item = await _versionCache.GetOrAddAsync(
            key,
            () => Task.FromResult(new CalendarOccurrenceVersionCacheItem { Version = Guid.NewGuid() }),
            () => VersionCacheOptions,
            hideErrors: false,
            considerUow: false,
            token: cancellationToken);

        return item!.Version;
    }

    protected virtual string BuildVersionKey(Guid? tenantId, Guid calendarId)
        => $"{tenantId?.ToString() ?? "host"}:{calendarId}";

    protected virtual string BuildExpansionKey(Guid? tenantId, Guid calendarId, Guid version, DateTime fromUtc, DateTime toUtc)
        => $"{tenantId?.ToString() ?? "host"}:{calendarId}:{version:N}:{fromUtc.Ticks}:{toUtc.Ticks}";
}
