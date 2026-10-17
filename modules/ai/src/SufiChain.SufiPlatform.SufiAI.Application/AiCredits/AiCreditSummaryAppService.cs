using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.SufiAI.AiCredits;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;

namespace SufiChain.SufiPlatform.SufiAI.Application.AiCredits;

/// <summary>
/// Tenant-scoped AI credit summary. Exposed as a conventional controller when the
/// host registers <c>SufiAIApplicationModule</c> (route root typically
/// <c>api/app/ai-credit-summary</c>: GET get, get-all). The composition Blazor
/// host calls this in-process; the AI demo host also creates conventional controllers.
/// </summary>
[Authorize]
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
        await EnsureAuthenticatedAsync();

        var id = await ResolveTenantIdAsync(tenantId);
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

    [Authorize(AIPermissions.AiCredits.ViewAll)]
    public async Task<List<AiCreditSummaryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await AuthorizationService.CheckAsync(AIPermissions.AiCredits.ViewAll);

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

    protected virtual async Task EnsureAuthenticatedAsync()
    {
        if (CurrentUser.IsAuthenticated)
        {
            return;
        }

        throw new AbpAuthorizationException(
            code: "Volo.Authorization:010001",
            message: "Authorization failed! Given policy has not granted.");
    }

    /// <summary>
    /// Tenant callers always see CurrentTenant.Id. Host callers may read a
    /// specific tenant only with <see cref="AIPermissions.AiCredits.ViewAll"/>.
    /// </summary>
    protected virtual async Task<Guid?> ResolveTenantIdAsync(Guid? tenantId)
    {
        if (CurrentTenant.Id.HasValue)
        {
            if (tenantId.HasValue
                && tenantId.Value != Guid.Empty
                && tenantId.Value != CurrentTenant.Id.Value)
            {
                throw new AbpAuthorizationException(
                    code: "Volo.Authorization:010001",
                    message: "Authorization failed! Given policy has not granted.");
            }

            return CurrentTenant.Id;
        }

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
        {
            return null;
        }

        await AuthorizationService.CheckAsync(AIPermissions.AiCredits.ViewAll);
        return tenantId;
    }

    private static AiCreditSummaryDto Map(AICreditSummary summary) => new()
    {
        TenantId = summary.TenantId,
        PeriodStart = summary.PeriodStart,
        PeriodEnd = summary.PeriodEnd,
        IncludedCredits = RoundAiCredit(summary.IncludedCredits),
        IncludedUsed = RoundAiCredit(summary.IncludedUsed),
        PurchasedRemaining = RoundAiCredit(summary.PurchasedRemaining),
        PurchasedUsed = RoundAiCredit(summary.PurchasedUsed)
    };

    /// <summary>AIC is stored and shown with 2 decimals (half-away-from-zero).</summary>
    private static decimal RoundAiCredit(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
