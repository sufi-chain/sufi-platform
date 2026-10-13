using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.AiCredits;
using Volo.Abp;
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
        var summary = await _summaryRepository.GetSummaryAsync(
            tenantId ?? CurrentTenant.Id.GetValueOrDefault(),
            periodStart,
            periodEnd,
            cancellationToken);

        if (summary == null)
        {
            return null;
        }

        return new AiCreditSummaryDto
        {
            TenantId = summary.TenantId,
            PeriodStart = summary.PeriodStart,
            PeriodEnd = summary.PeriodEnd,
            IncludedCredits = summary.IncludedCredits,
            IncludedUsed = summary.IncludedUsed,
            PurchasedRemaining = summary.PurchasedRemaining,
            PurchasedUsed = summary.PurchasedUsed,
        };
    }

    public async Task<List<AiCreditSummaryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var summaries = await _summaryRepository.GetAllSummaryAsync(
            cancellationToken);

        return summaries.Select(s => new AiCreditSummaryDto
        {
            TenantId = s.TenantId,
            PeriodStart = s.PeriodStart,
            PeriodEnd = s.PeriodEnd,
            IncludedCredits = s.IncludedCredits,
            IncludedUsed = s.IncludedUsed,
            PurchasedRemaining = s.PurchasedRemaining,
            PurchasedUsed = s.PurchasedUsed,
        }).ToList();
    }

    public async Task<decimal> ConvertTomanToRialAsync(
        decimal tomanAmount,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        // 1 Toman = 10 RIAL in the SufiChain system (local IRR pricing)
        // Convert using the project's default currency: IRR → RIAL factor
        return tomanAmount * 10m;
    }

    public async Task<string> GetThresholdLevelAsync(
        AiCreditSummaryDto summary,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        if (summary.AtLimit)
        {
            return "critical";
        }

        if (summary.AtThreshold)
        {
            return "warning";
        }

        return "none";
    }
}