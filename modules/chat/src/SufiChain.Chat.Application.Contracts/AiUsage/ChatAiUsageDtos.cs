using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.AiUsage;

public class ChatAiOperationContextDto
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

public class ChatAiUsageRecordDto : EntityDto<Guid>
{
    public Guid ReservationId { get; set; }

    public Guid? TenantId { get; set; }

    public Guid SessionId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? OperatorUserId { get; set; }

    public ChatAiOperationKind OperationKind { get; set; }

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }

    public string? ProviderName { get; set; }

    public string? WorkspaceName { get; set; }

    public string? DenyReason { get; set; }

    public DateTime? RecordedAt { get; set; }
}

public class ChatAiUsageReservationDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid SessionId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? OperatorUserId { get; set; }

    public ConversationKind ConversationKind { get; set; }

    public AccessMode AccessMode { get; set; }

    public ChatAiOperationKind OperationKind { get; set; }

    public string? SourceEntityType { get; set; }

    public string? SourceEntityId { get; set; }

    public string? LinkedEntityType { get; set; }

    public string? LinkedEntityId { get; set; }

    public int? EstimatedTokens { get; set; }

    public int? TotalTokens { get; set; }

    public string? ProviderName { get; set; }

    public string? WorkspaceName { get; set; }

    public string? WalletProviderName { get; set; }

    public Guid? WalletId { get; set; }

    public string? BillingSubjectType { get; set; }

    public string? BillingSubjectId { get; set; }

    public bool IsWalletChargeRequired { get; set; }

    public string? Currency { get; set; }

    public string? DenyReason { get; set; }

    public DateTime ReservedAt { get; set; }

    public DateTime? RecordedAt { get; set; }
}

public class ChatAiUsageDashboardDto
{
    public bool AiEnabled { get; set; }

    public bool UsageGuardEnabled { get; set; }

    public long ReservedCount { get; set; }

    public long RecordedCount { get; set; }

    public long DeniedCount { get; set; }

    public long TotalPromptTokens { get; set; }

    public long TotalCompletionTokens { get; set; }

    public long TotalTokens { get; set; }

    public string? AiManagementUsageAnalyticsUrl { get; set; }
}

public class ChatAiUsagePolicyDto
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
