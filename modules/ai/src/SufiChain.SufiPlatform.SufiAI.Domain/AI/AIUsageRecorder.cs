using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.SufiAI;

public class AIUsageRecorder : DomainService, IAIUsageRecorder
{
    public const string UsageUnavailable = "UsageUnavailable";
    public const string PricingNotConfigured = "PricingNotConfigured";

    protected IAIUsageLogRepository UsageLogRepository { get; }
    protected ILogger<AIUsageRecorder> RecorderLogger { get; }

    public AIUsageRecorder(
        IAIUsageLogRepository usageLogRepository,
        ILogger<AIUsageRecorder> logger)
    {
        UsageLogRepository = usageLogRepository;
        RecorderLogger = logger;
    }

    public virtual async Task RecordAsync(AIUsageRecord record, CancellationToken cancellationToken = default)
    {
        var configuration = record.Configuration;
        var workspace = configuration.Workspace;
        try
        {
            var log = new AIUsageLog(
                GuidGenerator.Create(),
                workspace.Id,
                record.CapabilityType ?? configuration.CapabilityType,
                record.ModelId ?? configuration.ModelId,
                configuration.Provider,
                workspace.TenantId);

            if (!configuration.IsFallback)
            {
                log.SetModelConfigurationId(configuration.ModelConfigurationId);
            }

            if (record.IsSuccess)
            {
                var cost = CalculateCost(configuration, record.InputTokens, record.OutputTokens, record.TotalTokens);
                log.RecordSuccess(
                    record.InputTokens,
                    record.OutputTokens,
                    record.LatencyMs,
                    cost.EstimatedCost,
                    record.TotalTokens,
                    cost.IsCostCalculated,
                    record.UsageUnavailableReason,
                    cost.CostCalculationNote);
            }
            else
            {
                log.RecordFailure(record.ErrorMessage ?? "Unknown error", record.LatencyMs);
            }

            await UsageLogRepository.InsertAsync(log, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            RecorderLogger.LogError(
                ex,
                "Failed to log AI usage for workspace {WorkspaceName}",
                workspace.Name);
        }
    }

    protected virtual CostCalculationResult CalculateCost(
        WorkspaceRuntimeConfiguration configuration,
        int? inputTokens,
        int? outputTokens,
        int? totalTokens)
    {
        var hasTokenUsage = inputTokens.HasValue || outputTokens.HasValue || totalTokens.HasValue;
        if (!hasTokenUsage)
        {
            return new CostCalculationResult(0, false, UsageUnavailable);
        }

        var inputCostPer1MTokens = configuration.InputCostPer1MTokens
            ?? configuration.ModelConfiguration?.InputCostPer1MTokens
            ?? configuration.Workspace.InputCostPer1MTokens;
        var outputCostPer1MTokens = configuration.OutputCostPer1MTokens
            ?? configuration.ModelConfiguration?.OutputCostPer1MTokens
            ?? configuration.Workspace.OutputCostPer1MTokens;
        if (!(inputCostPer1MTokens.HasValue || outputCostPer1MTokens.HasValue))
        {
            return new CostCalculationResult(0, false, PricingNotConfigured);
        }

        var estimatedCost = ((inputTokens ?? 0) * (inputCostPer1MTokens ?? 0) +
                             (outputTokens ?? 0) * (outputCostPer1MTokens ?? 0)) / 1_000_000m;

        return new CostCalculationResult(estimatedCost, true, null);
    }

    protected sealed record CostCalculationResult(
        decimal EstimatedCost,
        bool IsCostCalculated,
        string? CostCalculationNote);
}
