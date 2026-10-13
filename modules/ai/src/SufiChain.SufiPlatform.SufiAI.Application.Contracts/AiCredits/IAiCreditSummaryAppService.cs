using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.AiCredits;

/// <summary>
/// Application service for AI credit summary and threshold warnings.
/// </summary>
public interface IAiCreditSummaryAppService
{
    /// <summary>
    /// Gets the credit summary for a tenant over a date range.
    /// </summary>
    /// <param name="tenantId">The tenant ID (optional, current tenant if not provided).</param>
    /// <param name="periodStart">Start of period (UTC).</param>
    /// <param name="periodEnd">End of period (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary DTO or null if not found.</returns>
    Task<AiCreditSummaryDto?> GetAsync(
        Guid? tenantId = null,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets summaries for all tenants (admin-only).
    /// </summary>
    Task<List<AiCreditSummaryDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts a Toman amount to RIAL.
    /// </summary>
    /// <param name="tomanAmount">Amount in Toman.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Equivalent RIAL amount.</returns>
    Task<decimal> ConvertTomanToRialAsync(
        decimal tomanAmount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates threshold levels for a given summary.
    /// </summary>
    /// <param name="summary">The summary DTO.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Threshold level: "none", "warning", or "critical".</returns>
    Task<string> GetThresholdLevelAsync(
        AiCreditSummaryDto summary,
        CancellationToken cancellationToken = default);
}