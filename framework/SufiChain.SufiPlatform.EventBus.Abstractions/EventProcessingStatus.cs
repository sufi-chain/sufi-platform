namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Durable lifecycle states for an Inbox receipt.
/// </summary>
public enum EventProcessingStatus
{
    Received = 0,
    Processing = 1,
    Processed = 2,
    RetryScheduled = 3,
    Failed = 4,
    DeadLettered = 5
}
