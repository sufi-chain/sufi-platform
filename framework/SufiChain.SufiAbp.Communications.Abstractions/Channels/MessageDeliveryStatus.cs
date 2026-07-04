namespace SufiChain.SufiAbp.Communications;

/// <summary>
/// Message delivery status information.
/// </summary>
public class MessageDeliveryStatus
{
    public Guid MessageId { get; set; }
    public DeliveryState State { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? FailureReason { get; set; }
    public string? ExternalId { get; set; }
    public string? ProviderCode { get; set; }
}