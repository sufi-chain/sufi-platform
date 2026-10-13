using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.AiCredits;

/// <summary>
/// Stores the AI credit summary for a tenant per period.
/// </summary>
public class AiCreditSummary : CreationAuditedEntity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// Period start (subscription month start, UTC).
    /// </summary>
    public virtual DateTime PeriodStart { get; protected set; }

    /// <summary>
    /// Period end (subscription month end, UTC).
    /// </summary>
    public virtual DateTime PeriodEnd { get; protected set; }

    /// <summary>
    /// Included credits granted this period (in Toman).
    /// </summary>
    public virtual decimal IncludedCredits { get; protected set; }

    /// <summary>
    /// Included credits used so far this period (in Toman).
    /// </summary>
    public virtual decimal IncludedUsed { get; protected set; }

    /// <summary>
    /// Purchased credits remaining (in Toman).
    /// </summary>
    public virtual decimal PurchasedRemaining { get; protected set; }

    /// <summary>
    /// Purchased credits used so far this period (in Toman).
    /// </summary>
    public virtual decimal PurchasedUsed { get; protected set; }

    protected AiCreditSummary() { }

    public AiCreditSummary(
        Guid id,
        Guid? tenantId,
        DateTime periodStart,
        DateTime periodEnd,
        decimal includedCredits,
        decimal includedUsed,
        decimal purchasedRemaining,
        decimal purchasedUsed) : base(id)
    {
        TenantId = tenantId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        IncludedCredits = includedCredits;
        IncludedUsed = includedUsed;
        PurchasedRemaining = purchasedRemaining;
        PurchasedUsed = purchasedUsed;
    }
}