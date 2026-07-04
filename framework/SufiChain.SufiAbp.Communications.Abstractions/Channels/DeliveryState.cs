namespace SufiChain.SufiAbp.Communications;

/// <summary>
/// Message delivery state.
/// </summary>
public enum DeliveryState
{
    Queued = 0,
    Sending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Expired = 5
}