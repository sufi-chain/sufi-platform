namespace SufiChain.Chat.Usage;

public class ChatAiUsagePolicy
{
    public bool Enabled { get; set; }

    public bool UsageGuardEnabled { get; set; }

    public bool RequireOperatorForAnonymousHandoff { get; set; }

    public int MaxRepliesPerSession { get; set; }

    public int MaxTokensPerSession { get; set; }

    public int MaxTokensPerTenantPerDay { get; set; }

    public int MaxAnonymousAiSessionsPerHour { get; set; }

    public int MaxSuggestionsPerOperatorPerDay { get; set; }

    public int MaxSummariesPerOperatorPerDay { get; set; }

    public int MaxCopilotMessagesPerArticlePerDay { get; set; }

    public int MaxRagChunksPerRequest { get; set; }
}
