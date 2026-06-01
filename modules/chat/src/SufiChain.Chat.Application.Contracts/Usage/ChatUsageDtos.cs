namespace SufiChain.Chat.Usage;

public class ChatUsageCheckResultDto
{
    public bool IsAllowed { get; set; }

    public string? ReasonCode { get; set; }

    public string? LocalizationKey { get; set; }

    public LimitExceededAction? Action { get; set; }

    public bool RequiresAuthentication { get; set; }
}

public class ChatUsagePolicyDto
{
    public AccessMode AccessMode { get; set; }

    public int MaxSessionsPerUserPerDay { get; set; }

    public int MaxMessagesPerSession { get; set; }

    public int MaxAttachmentsPerSession { get; set; }

    public long MaxAttachmentBytesPerSession { get; set; }

    public bool EnableAnonymousIpGuard { get; set; }

    public int MaxSessionsPerIpPerDay { get; set; }

    public int MaxMessagesPerIpPerDay { get; set; }

    public int MaxAiSessionsPerIpPerHour { get; set; }

    public int MaxMessagesBeforeSignupRequired { get; set; }

    public int MaxAiQuestionsBeforeSignupRequired { get; set; }

    public LimitExceededAction LimitExceededAction { get; set; }
}

public class ChatStartSessionContextDto
{
    public Guid? TenantId { get; set; }

    public Guid? UserId { get; set; }

    public string? AnonymousVisitorId { get; set; }

    public string? AnonymousClientIpHash { get; set; }

    public AccessMode AccessMode { get; set; }

    public ConversationKind ConversationKind { get; set; }

    public ChannelOrigin ChannelOrigin { get; set; }

    public string? SourceEntityType { get; set; }

    public string? SourceEntityId { get; set; }
}

public class ChatSendMessageContextDto
{
    public Guid? TenantId { get; set; }

    public Guid SessionId { get; set; }

    public Guid? UserId { get; set; }

    public string? AnonymousVisitorId { get; set; }

    public string? AnonymousClientIpHash { get; set; }

    public AccessMode AccessMode { get; set; }

    public ChatMessageSenderKind SenderKind { get; set; }
}
