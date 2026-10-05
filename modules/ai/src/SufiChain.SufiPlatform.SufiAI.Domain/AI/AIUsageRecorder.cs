using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiAI;

public class AIUsageRecorder : DomainService, IAIUsageRecorder
{
    public const string UsageUnavailable = "UsageUnavailable";
    public const string PricingNotConfigured = "PricingNotConfigured";

    protected IAIUsageLogRepository UsageLogRepository { get; }
    protected ILogger<AIUsageRecorder> RecorderLogger { get; }
    protected IUnitOfWorkManager? UsageUnitOfWorkManager { get; }
    protected ICurrentTenant? UsageCurrentTenant { get; }

    public AIUsageRecorder(
        IAIUsageLogRepository usageLogRepository,
        ILogger<AIUsageRecorder> logger,
        IUnitOfWorkManager? unitOfWorkManager = null,
        ICurrentTenant? currentTenant = null)
    {
        UsageLogRepository = usageLogRepository;
        RecorderLogger = logger;
        UsageUnitOfWorkManager = unitOfWorkManager;
        UsageCurrentTenant = currentTenant;
    }

    public virtual async Task RecordAsync(AIUsageRecord record, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var configuration = record.Configuration;
        var workspace = configuration.Workspace;
        try
        {
            // A host workspace used from a tenant must be visible to that tenant's analytics.
            // Caller cancellation must not drop the row: streaming completes the ambient unit
            // of work before this insert, and the request token is often already cancelled.
            var tenantId = workspace.TenantId ?? UsageCurrentTenant?.Id;
            var log = new AIUsageLog(
                GuidGenerator.Create(),
                workspace.Id,
                record.CapabilityType ?? configuration.CapabilityType,
                record.ModelId ?? configuration.ModelId,
                configuration.Provider,
                tenantId);

            if (!configuration.IsFallback)
            {
                log.SetModelConfigurationId(configuration.ModelConfigurationId);
            }

            if (record.IsSuccess)
            {
                var cost = CalculateCost(
                    configuration,
                    record.InputTokens,
                    record.OutputTokens,
                    record.TotalTokens,
                    record.AudioSeconds,
                    record.CharacterCount,
                    record.ImageCount);
                log.RecordSuccess(
                    record.InputTokens,
                    record.OutputTokens,
                    record.LatencyMs,
                    cost.EstimatedCost,
                    record.TotalTokens,
                    cost.IsCostCalculated,
                    record.UsageUnavailableReason,
                    cost.CostCalculationNote,
                    record.AudioSeconds,
                    record.CharacterCount,
                    record.ImageCount);
            }
            else
            {
                log.RecordFailure(record.ErrorMessage ?? "Unknown error", record.LatencyMs);
            }

            await InsertCommittedAsync(log);
        }
        catch (Exception ex)
        {
            RecorderLogger.LogError(
                ex,
                "Failed to log AI usage for workspace {WorkspaceName}",
                workspace.Name);
        }
    }

    protected virtual async Task InsertCommittedAsync(AIUsageLog log)
    {
        if (UsageUnitOfWorkManager == null)
        {
            await UsageLogRepository.InsertAsync(log, autoSave: true, cancellationToken: CancellationToken.None);
            return;
        }

        using var unitOfWork = UsageUnitOfWorkManager.Begin(
            new AbpUnitOfWorkOptions { IsTransactional = false },
            requiresNew: true);
        await UsageLogRepository.InsertAsync(log, autoSave: true, cancellationToken: CancellationToken.None);
        await unitOfWork.CompleteAsync(CancellationToken.None);
    }

    protected virtual CostCalculationResult CalculateCost(
        WorkspaceRuntimeConfiguration configuration,
        int? inputTokens,
        int? outputTokens,
        int? totalTokens,
        decimal? audioSeconds = null,
        int? characterCount = null,
        int? imageCount = null)
    {
        var inputPrice = configuration.InputPrice;
        var outputPrice = configuration.OutputPrice;
        if (!(inputPrice.HasValue || outputPrice.HasValue))
        {
            return new CostCalculationResult(0, false, PricingNotConfigured);
        }

        if (!TryCharge(inputPrice, configuration.InputPriceUnit, inputTokens, outputTokens, totalTokens, audioSeconds, characterCount, imageCount, isOutput: false, out var inputCharge) ||
            !TryCharge(outputPrice, configuration.OutputPriceUnit, inputTokens, outputTokens, totalTokens, audioSeconds, characterCount, imageCount, isOutput: true, out var outputCharge))
        {
            return new CostCalculationResult(0, false, UsageUnavailable);
        }

        return new CostCalculationResult(inputCharge + outputCharge, true, null);
    }

    private static bool TryCharge(
        decimal? price,
        AIPriceUnit unit,
        int? inputTokens,
        int? outputTokens,
        int? totalTokens,
        decimal? audioSeconds,
        int? characterCount,
        int? imageCount,
        bool isOutput,
        out decimal charge)
    {
        charge = 0;
        if (price is not decimal rate)
        {
            return true;
        }

        var hasTokens = inputTokens.HasValue || outputTokens.HasValue || totalTokens.HasValue;
        switch (unit)
        {
            case AIPriceUnit.PerMillionTokens:
                if (!hasTokens)
                {
                    return false;
                }

                var tokens = isOutput ? outputTokens ?? 0 : inputTokens ?? 0;
                charge = tokens * rate / 1_000_000m;
                return true;
            case AIPriceUnit.PerMinute:
                if (audioSeconds is not decimal minutesSource)
                {
                    return false;
                }

                charge = minutesSource / 60m * rate;
                return true;
            case AIPriceUnit.PerHour:
                if (audioSeconds is not decimal hoursSource)
                {
                    return false;
                }

                charge = hoursSource / 3600m * rate;
                return true;
            case AIPriceUnit.PerMillionCharacters:
                if (characterCount is not int characters)
                {
                    return false;
                }

                charge = characters * rate / 1_000_000m;
                return true;
            case AIPriceUnit.PerImage:
                if (imageCount is not int images)
                {
                    return false;
                }

                charge = images * rate;
                return true;
            case AIPriceUnit.PerRequest:
                charge = rate;
                return true;
            default:
                return false;
        }
    }

    protected sealed record CostCalculationResult(
        decimal EstimatedCost,
        bool IsCostCalculated,
        string? CostCalculationNote);
}
