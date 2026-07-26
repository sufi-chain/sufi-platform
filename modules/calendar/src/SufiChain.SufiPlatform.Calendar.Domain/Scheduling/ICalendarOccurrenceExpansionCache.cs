using SufiChain.SufiPlatform.Calendar.Events;

namespace SufiChain.SufiPlatform.Calendar.Scheduling;

/// <summary>
/// Distributed cache for expanded event occurrences (per calendar + UTC window).
/// </summary>
public interface ICalendarOccurrenceExpansionCache
{
    Task<IReadOnlyList<EventOccurrence>> GetOrAddAsync(
        Guid calendarId,
        DateTime fromUtc,
        DateTime toUtc,
        Func<Task<IReadOnlyList<EventOccurrence>>> factory,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default);

    Task RemoveWithInheritorsAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default);
}
