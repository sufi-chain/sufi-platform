using SufiChain.SufiPlatform.Calendar.Availability;

using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.Calendar.Caching;

[CacheName("CalendarSnapshots")]
public class CalendarSnapshotCacheItem
{
    public CalendarSnapshot Snapshot { get; set; } = default!;
}
