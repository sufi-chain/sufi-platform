namespace SufiChain.Chat.Settings;

public class ChatUsageTierSettingsDto
{
    public int MaxSessionsPerUserPerDay { get; set; }

    public int MaxMessagesPerSession { get; set; }

    public int MaxAttachmentsPerSession { get; set; }

    public long MaxAttachmentBytesPerSession { get; set; }

    public LimitExceededAction LimitExceededAction { get; set; }

    public bool EnableIpGuard { get; set; }

    public int MaxSessionsPerIpPerDay { get; set; }

    public int MaxMessagesPerIpPerDay { get; set; }

    public int MaxAiSessionsPerIpPerHour { get; set; }

    public int MaxMessagesBeforeSignupRequired { get; set; }

    public int MaxAiQuestionsBeforeSignupRequired { get; set; }
}

public class ChatSettingsDto
{
    public int MaxConcurrentOpenSessions { get; set; }

    public int MaxMessagesPerTenantPerDay { get; set; }

    public ChatUsageTierSettingsDto PublicAnonymous { get; set; } = new();

    public ChatUsageTierSettingsDto PublicAuthenticated { get; set; } = new();

    public ChatUsageTierSettingsDto Internal { get; set; } = new();

    public bool AiEnabled { get; set; }

    public bool AiUsageGuardEnabled { get; set; }

    public int MaxRepliesPerSession { get; set; }

    public int MaxTokensPerSession { get; set; }

    public int MaxTokensPerTenantPerDay { get; set; }

    public int MaxAnonymousAiSessionsPerHour { get; set; }

    public int MessageRetentionDays { get; set; }

    public int ClosedSessionRetentionDays { get; set; }

    public int UsageRecordRetentionDays { get; set; }

    public bool RealtimeEnabled { get; set; }
}

public class UpdateChatSettingsInput
{
    public int MaxConcurrentOpenSessions { get; set; }

    public int MaxMessagesPerTenantPerDay { get; set; }

    public ChatUsageTierSettingsDto PublicAnonymous { get; set; } = new();

    public ChatUsageTierSettingsDto PublicAuthenticated { get; set; } = new();

    public ChatUsageTierSettingsDto Internal { get; set; } = new();

    public bool AiEnabled { get; set; }

    public bool AiUsageGuardEnabled { get; set; }

    public int MaxRepliesPerSession { get; set; }

    public int MaxTokensPerSession { get; set; }

    public int MaxTokensPerTenantPerDay { get; set; }

    public int MaxAnonymousAiSessionsPerHour { get; set; }

    public int MessageRetentionDays { get; set; }

    public int ClosedSessionRetentionDays { get; set; }

    public int UsageRecordRetentionDays { get; set; }

    public bool RealtimeEnabled { get; set; }
}
