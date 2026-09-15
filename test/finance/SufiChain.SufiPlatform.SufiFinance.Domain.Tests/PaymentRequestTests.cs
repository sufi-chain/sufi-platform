using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Payments;
using SufiChain.SufiPlatform.SufiFinance.Payments.Virtual;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Tests;

public class PaymentRequestTests
{
    [Fact]
    public void Should_Complete_Only_After_Verification()
    {
        var payment = CreatePayment();
        payment.MarkRedirected("gateway-reference");
        payment.BeginVerification();
        payment.Complete(1000m, "provider-reference", Guid.NewGuid(), DateTime.UtcNow);

        payment.Status.ShouldBe(PaymentStatus.Completed);
        payment.GatewayReference.ShouldBe("gateway-reference");
        payment.GetDistributedEvents().Count().ShouldBe(1);
    }

    [Fact]
    public void Should_Recognize_Processed_Webhook_Event()
    {
        var payment = CreatePayment();
        payment.AddAttempt(
            Guid.NewGuid(),
            PaymentAttemptType.Webhook,
            true,
            "{}",
            "{}",
            DateTime.UtcNow,
            "event-123",
            "provider-reference");

        payment.HasProcessedWebhookEvent("event-123").ShouldBeTrue();
        payment.HasProcessedWebhookEvent("event-456").ShouldBeFalse();
    }

    [Fact]
    public void Should_Reject_Verified_Amount_Mismatch()
    {
        var payment = CreatePayment();
        payment.MarkRedirected("gateway-reference");
        payment.BeginVerification();

        Should.Throw<BusinessException>(() =>
            payment.Complete(999m, "provider-reference", Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public async Task Virtual_Should_Block_Production_And_Bind_Reference()
    {
        var provider = new VirtualPaymentProvider();
        var context = new PaymentProviderContext(
            Guid.NewGuid(),
            1,
            1000m,
            "IRR",
            "https://localhost/callback",
            GatewayEnvironment.Production,
            new Dictionary<string, string>());
        await Should.ThrowAsync<InvalidOperationException>(() => provider.RequestAsync(context));

        var verify = await provider.VerifyAsync(new PaymentCallbackContext(
            Guid.NewGuid(),
            1,
            1000m,
            "IRR",
            "expected",
            GatewayEnvironment.Sandbox,
            new Dictionary<string, string>(),
            new Dictionary<string, string>
            {
                [VirtualPaymentProvider.DecisionParameter] = VirtualPaymentProvider.ApprovalDecision,
                ["virtualReference"] = "tampered"
            }));
        verify.Succeeded.ShouldBeFalse();
        verify.RawCode.ShouldBe("REFERENCE_MISMATCH");
    }

    private static PaymentRequest CreatePayment()
    {
        return new PaymentRequest(
            Guid.NewGuid(),
            null,
            null,
            1,
            1000m,
            "IRR",
            Guid.NewGuid(),
            "Virtual",
            "callback-token",
            DateTime.UtcNow.AddMinutes(15));
    }
}
