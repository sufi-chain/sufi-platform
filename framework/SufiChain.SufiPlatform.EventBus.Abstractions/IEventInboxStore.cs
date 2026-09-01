namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Durable Inbox port. Implementations must use a unique key of
/// consumer + tenant + event id and make acquisition atomic.
/// </summary>
public interface IEventInboxStore
{
    /// <summary>
    /// Atomically creates or claims a receipt for processing.
    /// Returns <c>false</c> when the receipt is already processed or currently owned
    /// by another attempt.
    /// </summary>
    Task<bool> TryBeginAsync(
        Guid eventId,
        Guid? tenantId,
        string consumer,
        string? correlationId = null,
        string? causationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a receipt as successfully processed.
    /// </summary>
    Task MarkProcessedAsync(
        Guid eventId,
        Guid? tenantId,
        string consumer,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a retryable failure and the next scheduled attempt.
    /// </summary>
    Task MarkRetryAsync(
        Guid eventId,
        Guid? tenantId,
        string consumer,
        DateTime nextAttemptAt,
        string error,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently quarantines a poison message.
    /// </summary>
    Task MarkDeadLetteredAsync(
        Guid eventId,
        Guid? tenantId,
        string consumer,
        string error,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current durable receipt for diagnostics and replay tooling.
    /// </summary>
    Task<EventInboxReceipt?> FindAsync(
        Guid eventId,
        Guid? tenantId,
        string consumer,
        CancellationToken cancellationToken = default);
}
