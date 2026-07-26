namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Deduplicates distributed event handling by event id (+ tenant + optional handler key).
/// Handlers should call <see cref="TryBeginAsync"/> before side effects and skip when false.
/// Multiple handlers of the same ETO must pass distinct <paramref name="handlerKey"/> values
/// so one handler's claim does not starve another.
/// </summary>
public interface IDistributedEventIdempotencyStore
{
    /// <summary>
    /// Attempts to mark <paramref name="eventId"/> as in-flight/handled for this handler.
    /// Returns <c>false</c> if the event was already processed (or is being processed) for the same key.
    /// </summary>
    /// <param name="handlerKey">
    /// Stable handler discriminator (e.g. type name). Required when more than one handler
    /// consumes the same event id; omit only for single-consumer ETOs.
    /// </param>
    Task<bool> TryBeginAsync(
        Guid eventId,
        Guid? tenantId,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default,
        string? handlerKey = null);
}
