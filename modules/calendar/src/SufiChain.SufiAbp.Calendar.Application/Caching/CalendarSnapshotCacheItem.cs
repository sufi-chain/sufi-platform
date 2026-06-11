using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Caching;

namespace SufiChain.SufiAbp.Calendar.Caching;

[CacheName("CalendarSnapshots")]
public class CalendarSnapshotCacheItem
{
    public CalendarSnapshot Snapshot { get; set; } = default!;
}
