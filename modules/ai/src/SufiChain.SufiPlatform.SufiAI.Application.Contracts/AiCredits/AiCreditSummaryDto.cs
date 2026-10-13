using System;

namespace SufiChain.SufiPlatform.SufiAI.AiCredits;

/// <summary>
/// Summary of AI credit usage for display in UI.
/// </summary>
public class AiCreditSummaryDto
{
    /// <summary>
    /// Tenant ID.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Period start (subscription month start, UTC).
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Period end (subscription month end, UTC).
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Included credits granted this period (Toman).
    /// </summary>
    public decimal IncludedCredits { get; set; }

    /// <summary>
    /// Included credits used so far this period (Toman).
    /// </summary>
    public decimal IncludedUsed { get; set; }

    /// <summary>
    /// Purchased credits remaining (Toman).
    /// </summary>
    public decimal PurchasedRemaining { get; set; }

    /// <summary>
    /// Purchased credits used so far this period (Toman).
    /// </summary>
    public decimal PurchasedUsed { get; set; }

    /// <summary>
    /// Total credits consumed this period (Toman).
    /// </summary>
    public decimal TotalUsed => IncludedUsed + PurchasedUsed;

    /// <summary>
    /// Total credits available (Toman).
    /// </summary>
    public decimal TotalAvailable => (IncludedCredits - IncludedUsed) + PurchasedRemaining;

    /// <summary>
    /// Whether usage is approaching the threshold (80% or more).
    /// </summary>
    public bool AtThreshold => TotalUsed >= (IncludedCredits + PurchasedRemaining) * 0.8m;

    /// <summary>
    /// Whether usage has fully consumed available credits (100%).
    /// </summary>
    public bool AtLimit => TotalUsed >= (IncludedCredits + PurchasedRemaining);

    /// <summary>
    /// Percentage of credits used (0-100).
    /// </summary>
    public int UsagePercent => (IncludedCredits + PurchasedRemaining) > 0
        ? Math.Min(100, (int)((TotalUsed / (IncludedCredits + PurchasedRemaining)) * 100))
        : 0;

    /// <summary>
    /// Threshold level: "none", "warning" (80%+), or "critical" (100%).
    /// </summary>
    public string ThresholdLevel => AtLimit ? "critical" : AtThreshold ? "warning" : "none";
}