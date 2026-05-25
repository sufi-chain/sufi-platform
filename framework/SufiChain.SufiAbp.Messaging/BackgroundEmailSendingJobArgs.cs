namespace SufiChain.SufiAbp.Messaging.BackgroundJobs;

public class BackgroundEmailSendingJobArgs
{
    public string To { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public bool IsBodyHtml { get; set; }
    public string? From { get; set; }
    public string? ReplyTo { get; set; }
    public string[]? Cc { get; set; }
    public string[]? Bcc { get; set; }
    public BackgroundEmailAttachment[]? Attachments { get; set; }
    public MessagePriority Priority { get; set; }
}

public class BackgroundEmailAttachment
{
    public byte[] File { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string? ContentType { get; set; }
    public string? ContentId { get; set; }
}
