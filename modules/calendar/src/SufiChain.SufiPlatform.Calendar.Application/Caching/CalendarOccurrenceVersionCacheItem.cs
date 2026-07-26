using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Calendar.Caching;

/// <summary>
/// Per-calendar stamp. Bumping/removing this orphans all expansion cache entries for that calendar
/// without needing Redis KEYS scans.
/// </summary>
[CacheName("CalendarOccurrenceVersions")]
public class CalendarOccurrenceVersionCacheItem
{
    public Guid Version { get; set; }
}
