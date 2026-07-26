namespace SufiChain.SufiPlatform.SufiCom.Payments;

/// <summary>
/// Normalized result of a foreign payment (Stripe / PayPal) posted by the ForeignHost back to
/// the Iran host's <c>PaymentIntegrationService</c> (Phase 7). Independent of Telegram; the
/// Finance module consumes this into its existing payment pipeline (no new Finance schema here).
/// </summary>
public class PaymentResultEto
{
    /// <summary>Provider name: <c>stripe</c> or <c>paypal</c>.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Provider-native payment / intent id.</summary>
    public string? ExternalPaymentId { get; set; }

    /// <summary>ForeignHost-side correlation id (used for idempotency / dedupe).</summary>
    public Guid ForeignHostReferenceId { get; set; }

    /// <summary>Normalized outcome: <c>succeeded</c>, <c>pending</c>, <c>failed</c>, <c>refunded</c>.</summary>
    public string Status { get; set; } = "pending";

    /// <summary>Amount in the provider currency's smallest unit (Stripe) or major unit (PayPal).</summary>
    public decimal? Amount { get; set; }

    public string? Currency { get; set; }

    /// <summary>Opaque tenant/order correlation token Iran handed to the redirect start call.</summary>
    public string? Reference { get; set; }

    /// <summary>Provider raw event id (for webhook dedupe).</summary>
    public string? ExternalEventId { get; set; }

    public string? FailureReason { get; set; }
}
