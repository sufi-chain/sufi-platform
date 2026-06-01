using System;

namespace SufiChain.Chat.Usage;

public class ChatAiOperationContext
{
    public Guid? TenantId { get; set; }

    public Guid SessionId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? OperatorUserId { get; set; }

    public string? AnonymousVisitorId { get; set; }

    public string? AnonymousClientIpHash { get; set; }

    public AccessMode AccessMode { get; set; }

    public ConversationKind ConversationKind { get; set; }

    public ChatAiOperationKind OperationKind { get; set; }

    public string? SourceEntityType { get; set; }

    public string? SourceEntityId { get; set; }

    public string? LinkedEntityType { get; set; }

    public string? LinkedEntityId { get; set; }

    public string? ProviderName { get; set; }

    public string? WorkspaceName { get; set; }

    public int? EstimatedTokens { get; set; }
}
