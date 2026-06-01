using System;

namespace SufiChain.Chat.Usage;

public class ChatAiUsageRecord
{
    public Guid? UserId { get; set; }

    public Guid? OperatorUserId { get; set; }

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }

    public string? ProviderName { get; set; }

    public string? WorkspaceName { get; set; }

    public string? DenyReason { get; set; }

    public DateTime? RecordedAt { get; set; }
}
