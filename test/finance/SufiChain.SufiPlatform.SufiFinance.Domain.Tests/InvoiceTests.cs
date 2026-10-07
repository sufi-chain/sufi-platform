using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Invoicing;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Domain.Tests;

public class InvoiceTests
{
    [Fact]
    public void Issue_Should_Calculate_Authoritative_Totals()
    {
        var invoice = CreateInvoice();
        invoice.ReplaceLines(
        [
            new(Guid.NewGuid(), invoice.Id, "Service", 2, 100, 10, null, null, 0),
            new(Guid.NewGuid(), invoice.Id, "Entered at zero", 1, 50, 0, null, null, 1)
        ], vatEnabled: true, vatPercent: 10);

        invoice.Issue(Guid.NewGuid(), Now, vatEnabled: true, vatPercent: 10);

        invoice.Subtotal.ShouldBe(250);
        invoice.TaxAmount.ShouldBe(25);
        invoice.TotalAmount.ShouldBe(275);
        invoice.VatEnabledAtIssue.ShouldBeTrue();
        invoice.VatPercentAtIssue.ShouldBe(10);
        invoice.LineItems.ShouldAllBe(line => line.TaxRate == 10 && line.TaxAmount == line.Subtotal * 10m / 100m);
        invoice.Status.ShouldBe(InvoiceStatus.Pending);
    }

    [Fact]
    public void Issue_With_Vat_Disabled_Should_Clear_Line_Tax()
    {
        var invoice = CreateInvoice();
        invoice.ReplaceLines(
            [new(Guid.NewGuid(), invoice.Id, "Service", 1, 100, 10, null, null, 0)],
            vatEnabled: false,
            vatPercent: 10);

        invoice.Issue(Guid.NewGuid(), Now, vatEnabled: false, vatPercent: 10);

        invoice.TaxAmount.ShouldBe(0);
        invoice.TotalAmount.ShouldBe(100);
        invoice.VatEnabledAtIssue.ShouldBeFalse();
        invoice.VatPercentAtIssue.ShouldBe(10);
        invoice.LineItems.Single().TaxRate.ShouldBe(0);
        invoice.LineItems.Single().TaxAmount.ShouldBe(0);
    }

    [Fact]
    public void Failed_Gateway_Attempt_Should_Return_Invoice_To_Pending_And_Allow_Retry()
    {
        var invoice = IssuedInvoice();
        var firstPayment = Guid.NewGuid();
        invoice.BeginGatewayPayment(Guid.NewGuid(), firstPayment, "Virtual", "attempt-1", Now);

        invoice.FailGatewayPayment(firstPayment, "Rejected");
        invoice.BeginGatewayPayment(Guid.NewGuid(), Guid.NewGuid(), "Virtual", "attempt-2", Now);

        invoice.Status.ShouldBe(InvoiceStatus.WaitingForPayment);
        invoice.PaymentAttempts.Count.ShouldBe(2);
        invoice.PaymentAttempts.Single(item => item.IdempotencyKey == "attempt-1").FailureReason.ShouldBe("Rejected");
    }

    [Fact]
    public void Completion_Should_Be_Idempotent_And_Reject_Mismatch()
    {
        var invoice = IssuedInvoice();
        var paymentId = Guid.NewGuid();
        invoice.BeginGatewayPayment(Guid.NewGuid(), paymentId, "Virtual", "attempt", Now);

        invoice.CompleteGatewayPayment(paymentId, invoice.TotalAmount, invoice.CurrencyCode, Now, Guid.NewGuid());
        invoice.CompleteGatewayPayment(paymentId, invoice.TotalAmount, invoice.CurrencyCode, Now, Guid.NewGuid());

        invoice.Status.ShouldBe(InvoiceStatus.Paid);
        invoice.PaymentAttempts.Single().Succeeded.ShouldBeTrue();

        var other = IssuedInvoice();
        var otherPayment = Guid.NewGuid();
        other.BeginGatewayPayment(Guid.NewGuid(), otherPayment, "Virtual", "attempt", Now);
        Should.Throw<BusinessException>(() =>
            other.CompleteGatewayPayment(otherPayment, other.TotalAmount + 1, other.CurrencyCode, Now, Guid.NewGuid()));
    }

    [Fact]
    public void Wallet_Payment_Should_Prevent_Double_Payment()
    {
        var invoice = IssuedInvoice();
        invoice.RecordWalletPayment(Guid.NewGuid(), Guid.NewGuid(), "wallet-1", Now, Guid.NewGuid());

        invoice.Status.ShouldBe(InvoiceStatus.Paid);
        Should.Throw<BusinessException>(() =>
            invoice.RecordWalletPayment(Guid.NewGuid(), Guid.NewGuid(), "wallet-2", Now, Guid.NewGuid()));
    }

    [Fact]
    public void Expiry_Should_Be_Idempotent()
    {
        var invoice = IssuedInvoice(Now.AddMinutes(-1));

        invoice.Expire(Now, Guid.NewGuid());
        invoice.Expire(Now, Guid.NewGuid());

        invoice.Status.ShouldBe(InvoiceStatus.Expired);
    }

    private static readonly DateTime Now = new(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc);

    private static Invoice CreateInvoice(DateTime? dueDate = null) =>
        new(Guid.NewGuid(), null, "INV-TEST", Guid.NewGuid(), "USD", "Test invoice", dueDate, null, null);

    private static Invoice IssuedInvoice(DateTime? dueDate = null)
    {
        var invoice = CreateInvoice(dueDate);
        invoice.ReplaceLines([new InvoiceLineItem(Guid.NewGuid(), invoice.Id, "Service", 1, 100, 0, null, null, 0)]);
        invoice.Issue(Guid.NewGuid(), Now.AddMinutes(-2));
        return invoice;
    }
}
