using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Calendar.Caching;

[CacheName("CalendarOccurrenceExpansions")]
public class CalendarOccurrenceExpansionCacheItem
{
    public List<EventOccurrence> Occurrences { get; set; } = new();
}
