using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.AiCredits;

/// <summary>
/// Read model for AI credit summary (used for UI display). Not a persisted aggregate repository.
/// </summary>
public interface IAiCreditSummaryRepository : ITransientDependency
{
    /// <summary>
    /// Gets the credit summary for a tenant over a date range.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="periodStart">Start of period (UTC).</param>
    /// <param name="periodEnd">End of period (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary data or null if not found.</returns>
    Task<AICreditSummary?> GetSummaryAsync(
        Guid tenantId,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets summaries for all tenants (admin-only).
    /// </summary>
    Task<List<AICreditSummary>> GetAllSummaryAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Internal DTO for AI credit summary data.
/// </summary>
public class AICreditSummary
{
    public Guid TenantId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal IncludedCredits { get; set; }
    public decimal IncludedUsed { get; set; }
    public decimal PurchasedRemaining { get; set; }
    public decimal PurchasedUsed { get; set; }
}