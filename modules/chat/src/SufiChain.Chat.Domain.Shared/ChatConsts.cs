using SufiChain.Chat.Settings;

namespace SufiChain.Chat;

public static class ChatConsts
{
    public const int MaxTitleLength = 256;
    public const int MaxMetadataJsonLength = 8192;
    public const int MaxMessageBodyLength = 16000;
    public const int MaxAnonymousVisitorIdLength = 128;
    public const int MaxDisplayNameLength = 128;
    public const int MaxLinkedEntityTypeLength = 256;
    public const int MaxLinkedEntityIdLength = 128;
    public const int MaxLinkRoleLength = 64;
    public const int MaxProviderNameLength = 128;
    public const int MaxWorkspaceNameLength = 128;
    public const int MaxCurrencyLength = 16;
    public const int MaxUsageCounterKeyLength = 256;
    public const int MaxUsageReasonLength = 256;
    public const int MaxConnectorNameLength = 64;
    public const int MaxExternalIdLength = 512;

    public static class Settings
    {
        public const string AiEnabled = ChatSettingNames.Ai.Enabled;
        public const string AiUsageGuard = ChatSettingNames.Ai.UsageGuard;
        public const string AiDefaultWorkspaceName = ChatSettingNames.Ai.DefaultWorkspaceName;
        public const string AiMaxRepliesPerSession = ChatSettingNames.Ai.MaxRepliesPerSession;
        public const string AiMaxTokensPerSession = ChatSettingNames.Ai.MaxTokensPerSession;
        public const string AiMaxTokensPerTenantPerDay = ChatSettingNames.Ai.MaxTokensPerTenantPerDay;
        public const string AiMaxSuggestionsPerOperatorPerDay = ChatSettingNames.Ai.MaxSuggestionsPerOperatorPerDay;
        public const string AiMaxSummariesPerOperatorPerDay = ChatSettingNames.Ai.MaxSummariesPerOperatorPerDay;
        public const string AiMaxRagChunksPerRequest = ChatSettingNames.Ai.MaxRagChunksPerRequest;
    }
}
