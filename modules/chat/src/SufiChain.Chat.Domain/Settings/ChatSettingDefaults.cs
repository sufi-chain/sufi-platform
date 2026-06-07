namespace SufiChain.Chat.Settings;

public static class ChatSettingDefaults
{
    public const int MaxConcurrentOpenSessions = 1000;
    public const int MaxMessagesPerTenantPerDay = 100000;
    public const int PublicAnonymousMaxSessionsPerUserPerDay = 10;
    public const int PublicAnonymousMaxMessagesPerSession = 50;
    public const int PublicAuthenticatedMaxSessionsPerUserPerDay = 50;
    public const int PublicAuthenticatedMaxMessagesPerSession = 500;
    public const int InternalMaxSessionsPerUserPerDay = 200;
    public const int InternalMaxMessagesPerSession = 2000;
    public const int MaxAttachmentsPerSession = 10;
    public const long MaxAttachmentBytesPerSession = 104857600;
    public const int MaxSessionsPerIpPerDay = 25;
    public const int MaxMessagesPerIpPerDay = 250;
    public const int MaxAiSessionsPerIpPerHour = 5;
    public const int MaxMessagesBeforeSignupRequired = 20;
    public const int MaxAiQuestionsBeforeSignupRequired = 3;
    public const int AiMaxRepliesPerSession = 25;
    public const int AiMaxTokensPerSession = 50000;
    public const int AiMaxTokensPerTenantPerDay = 1000000;
    public const int AiMaxAnonymousAiSessionsPerHour = 5;
    public const int AiMaxSuggestionsPerOperatorPerDay = 100;
    public const int AiMaxSummariesPerOperatorPerDay = 100;
    public const int AiMaxCopilotMessagesPerArticlePerDay = 50;
    public const int AiMaxRagChunksPerRequest = 8;
    public const int MaxFilesPerMessage = 10;
    public const int MaxVoiceRecordingSeconds = 120;
    public const int AllowedAttachmentFileTypes = (int)ChatAttachmentAllowedFileTypes.All;
}
