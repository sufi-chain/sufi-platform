using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Tracks usage, cost, and performance metrics for AI API calls.
/// Used for billing, monitoring, and optimization.
/// </summary>
public class AIUsageLog : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    
    /// <summary>
    /// The workspace that made this API call
    /// </summary>
    public Guid WorkspaceId { get; protected set; }
    
    /// <summary>
    /// The capability type used (Chat, Audio, Vision, etc.)
    /// </summary>
    public AICapabilityType CapabilityType { get; protected set; }
    
    /// <summary>
    /// The specific model used (e.g., "gpt-4", "whisper-1")
    /// </summary>
    public string ModelId { get; protected set; } = string.Empty;
    
    /// <summary>
    /// The AI provider (OpenAI, Azure, etc.)
    /// </summary>
    public AIProviderType Provider { get; protected set; }
    
    /// <summary>
    /// Number of input tokens consumed
    /// </summary>
    public int? InputTokens { get; protected set; }
    
    /// <summary>
    /// Number of output tokens generated
    /// </summary>
    public int? OutputTokens { get; protected set; }
    
    /// <summary>
    /// Total tokens (input + output)
    /// </summary>
    public int? TotalTokens { get; protected set; }

    public bool HasTokenUsage { get; protected set; }

    public string? UsageUnavailableReason { get; protected set; }
    
    /// <summary>
    /// Estimated cost in USD (calculated based on model pricing)
    /// </summary>
    public decimal EstimatedCost { get; protected set; }

    public bool IsCostCalculated { get; protected set; }

    public string? CostCalculationNote { get; protected set; }
    
    /// <summary>
    /// API call latency in milliseconds
    /// </summary>
    public long LatencyMs { get; protected set; }
    
    /// <summary>
    /// Whether the API call succeeded
    /// </summary>
    public bool IsSuccess { get; protected set; }
    
    /// <summary>
    /// Error message if the call failed
    /// </summary>
    public string? ErrorMessage { get; protected set; }
    
    /// <summary>
    /// Additional request metadata as JSON (prompt length, parameters, etc.)
    /// </summary>
    public string? RequestMetadataJson { get; protected set; }
    
    /// <summary>
    /// Additional response metadata as JSON (finish reason, model version, etc.)
    /// </summary>
    public string? ResponseMetadataJson { get; protected set; }
    
    /// <summary>
    /// File ID from File-Manager (for audio/image uploads)
    /// </summary>
    public Guid? FileId { get; protected set; }
    
    /// <summary>
    /// File URL for accessing the uploaded file
    /// </summary>
    public string? FileUrl { get; protected set; }
    
    protected AIUsageLog() { }
    
    public AIUsageLog(
        Guid id,
        Guid workspaceId,
        AICapabilityType capabilityType,
        string modelId,
        AIProviderType provider,
        Guid? tenantId = null
    ) : base(id)
    {
        WorkspaceId = workspaceId;
        CapabilityType = capabilityType;
        ModelId = modelId;
        Provider = provider;
        TenantId = tenantId;
        IsSuccess = true;
    }
    
    public void RecordSuccess(
        int? inputTokens,
        int? outputTokens,
        long latencyMs,
        decimal estimatedCost,
        int? totalTokens = null,
        bool isCostCalculated = false,
        string? usageUnavailableReason = null,
        string? costCalculationNote = null,
        string? requestMetadataJson = null,
        string? responseMetadataJson = null
    )
    {
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        TotalTokens = totalTokens ?? (inputTokens.HasValue || outputTokens.HasValue
            ? (inputTokens ?? 0) + (outputTokens ?? 0)
            : null);
        HasTokenUsage = InputTokens.HasValue || OutputTokens.HasValue || TotalTokens.HasValue;
        UsageUnavailableReason = HasTokenUsage ? null : usageUnavailableReason ?? "ProviderDidNotReturnUsage";
        LatencyMs = latencyMs;
        EstimatedCost = estimatedCost;
        IsCostCalculated = isCostCalculated;
        CostCalculationNote = costCalculationNote;
        IsSuccess = true;
        RequestMetadataJson = requestMetadataJson;
        ResponseMetadataJson = responseMetadataJson;
    }
    
    public void RecordFailure(
        string errorMessage,
        long latencyMs,
        string? requestMetadataJson = null
    )
    {
        ErrorMessage = errorMessage;
        LatencyMs = latencyMs;
        IsSuccess = false;
        RequestMetadataJson = requestMetadataJson;
    }
    
    public void SetFileInfo(Guid fileId, string fileUrl)
    {
        FileId = fileId;
        FileUrl = fileUrl;
    }
}
