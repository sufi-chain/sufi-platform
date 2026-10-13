using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI;

public class AIUsageLog : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid WorkspaceId { get; protected set; }
    public Guid? ModelConfigurationId { get; protected set; }
    public AICapabilityType CapabilityType { get; protected set; }
    public string ModelId { get; protected set; } = string.Empty;
    public AIProviderType Provider { get; protected set; }
    public int? InputTokens { get; protected set; }
    public int? OutputTokens { get; protected set; }
    public int? TotalTokens { get; protected set; }
    public decimal? AudioSeconds { get; protected set; }
    public int? CharacterCount { get; protected set; }
    public int? ImageCount { get; protected set; }

    // A7: shared-usage charge tracking
    public bool IsSharedWorkspaceUsage { get; private set; }
    public DateTime? CreditChargedAt { get; private set; }
    public Guid? CreditChargeBatchId { get; private set; }

    public void MarkCreditCharged(Guid batchId, DateTime at)
    {
        CreditChargedAt = at;
        CreditChargeBatchId = batchId;
    }

    public bool HasTokenUsage { get; protected set; }
    public string? UsageUnavailableReason { get; protected set; }
    public decimal EstimatedCost { get; protected set; }
    public bool IsCostCalculated { get; protected set; }
    public string? CostCalculationNote { get; protected set; }
    public long LatencyMs { get; protected set; }
    public bool IsSuccess { get; protected set; }
    public string? ErrorMessage { get; protected set; }
    public Guid? FileId { get; protected set; }
    public string? FileUrl { get; protected set; }

    protected AIUsageLog() { }

    public AIUsageLog(Guid id, Guid workspaceId, AICapabilityType capabilityType, string modelId, AIProviderType provider, Guid? tenantId = null) : base(id)
    {
        WorkspaceId = workspaceId;
        CapabilityType = capabilityType;
        ModelId = modelId;
        Provider = provider;
        TenantId = tenantId;
        IsSharedWorkspaceUsage = tenantId == null; // null tenant = shared/host workspace
        IsSuccess = true;
    }

    public void SetModelConfigurationId(Guid? modelConfigurationId) => ModelConfigurationId = modelConfigurationId;

    public void RecordSuccess(int? inputTokens, int? outputTokens, long latencyMs, decimal estimatedCost, int? totalTokens = null, bool isCostCalculated = false, string? usageUnavailableReason = null, string? costCalculationNote = null, decimal? audioSeconds = null, int? characterCount = null, int? imageCount = null)
    {
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        TotalTokens = totalTokens ?? (inputTokens.HasValue || outputTokens.HasValue ? (inputTokens ?? 0) + (outputTokens ?? 0) : null);
        AudioSeconds = audioSeconds;
        CharacterCount = characterCount;
        ImageCount = imageCount;
        HasTokenUsage = InputTokens.HasValue || OutputTokens.HasValue || TotalTokens.HasValue;
        UsageUnavailableReason = HasTokenUsage || isCostCalculated ? null : usageUnavailableReason ?? "ProviderDidNotReturnUsage";
        LatencyMs = latencyMs;
        EstimatedCost = estimatedCost;
        IsCostCalculated = isCostCalculated;
        CostCalculationNote = costCalculationNote;
        IsSuccess = true;
    }

    public void RecordFailure(string errorMessage, long latencyMs)
    {
        ErrorMessage = errorMessage;
        LatencyMs = latencyMs;
        IsSuccess = false;
    }

    public void SetFileInfo(Guid fileId, string fileUrl)
    {
        FileId = fileId;
        FileUrl = fileUrl;
    }
}
