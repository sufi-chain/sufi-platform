namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Durable consumer receipt identity and processing metadata.
/// </summary>
[Serializable]
public sealed class EventInboxReceipt
{
    public Guid EventId { get; set; }
    public Guid? TenantId { get; set; }
    public string Consumer { get; set; } = string.Empty;
    public EventProcessingStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? LastFailedAt { get; set; }
    public string? LastError { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
}
