using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Payments;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Payments;

public class PaymentRequestTransitionTests
{
    [Fact]
    public void Complete_Should_Finish_A_Verified_Payment()
    {
        var payment = NewPayment(25m);
        payment.MarkRedirected("gw-1");
        payment.BeginVerification();
        payment.Complete(25m, "gw-1", Guid.NewGuid(), DateTime.UtcNow);

        payment.Status.ShouldBe(PaymentStatus.Completed);
    }

    [Fact]
    public void Create_Should_Reject_A_Non_Positive_Amount()
    {
        var error = Should.Throw<Volo.Abp.BusinessException>(() => NewPayment(0m));
        error.Code.ShouldBe(SufiFinancePaymentsErrorCodes.InvalidAmount);
    }

    [Fact]
    public void Complete_Should_Reject_A_Mismatched_Verified_Amount()
    {
        var payment = NewPayment(25m);
        payment.MarkRedirected("gw-1");
        payment.BeginVerification();
        var error = Should.Throw<Volo.Abp.BusinessException>(() =>
            payment.Complete(24m, "gw-1", Guid.NewGuid(), DateTime.UtcNow));
        error.Code.ShouldBe(SufiFinancePaymentsErrorCodes.VerifiedAmountMismatch);
        payment.Status.ShouldBe(PaymentStatus.Verifying);
    }

    private static PaymentRequest NewPayment(decimal amount) =>
        new(Guid.NewGuid(), null, null, 1001, amount, "USD", Guid.NewGuid(), "virtual", "callback-token", DateTime.UtcNow.AddHours(1));
}
