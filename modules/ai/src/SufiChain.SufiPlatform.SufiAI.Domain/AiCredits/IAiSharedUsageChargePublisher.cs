namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Notifies the credit ledger that one shared usage log is ready to debit.
/// The SaaS module charges it. Hosts without that module leave the log for no one.
/// </summary>
public interface IAiSharedUsageChargePublisher
{
    Task PublishAsync(
        Guid usageLogId,
        Guid tenantId,
        decimal estimatedCost,
        CancellationToken cancellationToken = default);
}
