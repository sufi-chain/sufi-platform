using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Calendar.Scheduling;

/// <summary>
/// Passthrough used when distributed expansion cache is not registered (domain-only tests).
/// Application module replaces this with <c>CalendarOccurrenceExpansionCache</c>.
/// </summary>
public class NullCalendarOccurrenceExpansionCache : ICalendarOccurrenceExpansionCache, ITransientDependency
{
    public virtual Task<IReadOnlyList<EventOccurrence>> GetOrAddAsync(
        Guid calendarId,
        DateTime fromUtc,
        DateTime toUtc,
        Func<Task<IReadOnlyList<EventOccurrence>>> factory,
        CancellationToken cancellationToken = default)
        => factory();

    public virtual Task RemoveAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public virtual Task RemoveWithInheritorsAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
