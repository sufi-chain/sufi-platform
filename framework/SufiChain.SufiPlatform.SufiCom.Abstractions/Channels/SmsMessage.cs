namespace SufiChain.SufiPlatform.SufiCom;

/// <summary>
/// Generic SMS message object.
/// </summary>
public class SmsMessage
{
    public string Phone { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? TemplateKey { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
    public string? Culture { get; set; }
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    public Dictionary<string, object>? Metadata { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime? ScheduledFor { get; set; }
}