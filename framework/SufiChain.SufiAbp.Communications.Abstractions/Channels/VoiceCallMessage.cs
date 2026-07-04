namespace SufiChain.SufiAbp.Communications;

/// <summary>
/// Generic voice call message object (TTS).
/// </summary>
public class VoiceCallMessage
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
    public int RepeatCount { get; set; } = 1;
}