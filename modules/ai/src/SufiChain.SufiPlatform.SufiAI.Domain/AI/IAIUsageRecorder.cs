using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

public interface IAIUsageRecorder : ITransientDependency
{
    Task RecordAsync(AIUsageRecord record, CancellationToken cancellationToken = default);
}

public sealed class AIUsageRecord
{
    public required WorkspaceRuntimeConfiguration Configuration { get; init; }

    public AICapabilityType? CapabilityType { get; init; }

    public string? ModelId { get; init; }

    public int? InputTokens { get; init; }

    public int? OutputTokens { get; init; }

    public int? TotalTokens { get; init; }

    public string? UsageUnavailableReason { get; init; }

    public long LatencyMs { get; init; }

    public bool IsSuccess { get; init; }

    public string? ErrorMessage { get; init; }
}
