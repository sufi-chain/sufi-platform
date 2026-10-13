using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.AiCredits;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI.Application.AiCredits;

public class AiCreditSummaryAppService : ApplicationService, IAiCreditSummaryAppService
{
    private readonly IAiCreditSummaryRepository _summaryRepository;

    public AiCreditSummaryAppService(IAiCreditSummaryRepository summaryRepository)
    {
        _summaryRepository = summaryRepository;
    }

    public async Task<AiCreditSummaryDto?> GetAsync(
        Guid? tenantId = null,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        var id = tenantId ?? CurrentTenant.Id;
        if (!id.HasValue || id == Guid.Empty)
        {
            return null;
        }

        var summary = await _summaryRepository.GetSummaryAsync(
            id.Value,
            periodStart,
            periodEnd,
            cancellationToken);

        return summary == null ? null : Map(summary);
    }

    public async Task<List<AiCreditSummaryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var summaries = await _summaryRepository.GetAllSummaryAsync(cancellationToken);
        return summaries.Select(Map).ToList();
    }

    public Task<decimal> ConvertTomanToRialAsync(
        decimal tomanAmount,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return Task.FromResult(tomanAmount * 10m);
    }

    public Task<string> GetThresholdLevelAsync(
        AiCreditSummaryDto summary,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return Task.FromResult(summary.ThresholdLevel);
    }

    private static AiCreditSummaryDto Map(AICreditSummary summary) => new()
    {
        TenantId = summary.TenantId,
        PeriodStart = summary.PeriodStart,
        PeriodEnd = summary.PeriodEnd,
        IncludedCredits = summary.IncludedCredits,
        IncludedUsed = summary.IncludedUsed,
        PurchasedRemaining = summary.PurchasedRemaining,
        PurchasedUsed = summary.PurchasedUsed
    };
}
