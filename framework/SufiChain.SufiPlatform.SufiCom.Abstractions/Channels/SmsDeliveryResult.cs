namespace SufiChain.SufiPlatform.SufiCom.Channels;

public class SmsDeliveryResult
{
    public bool Success { get; set; }
    public string? ExternalId { get; set; }
    public string? ErrorMessage { get; set; }
    public int? StatusCode { get; set; }
    public string? AdditionalData { get; set; }
}