using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Invoicing;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Domain.Tests;

public class InvoiceFxRepriceTests
{
    [Fact]
    public void RepriceFxLines_ceils_usd_original_into_irr_while_pending()
    {
        var invoice = new Invoice(
            Guid.NewGuid(),
            null,
            "INV-FX-1",
            Guid.NewGuid(),
            "IRR",
            "AI Credits",
            null,
            null,
            "ref");
        var line = new InvoiceLineItem(
            Guid.NewGuid(),
            invoice.Id,
            "AI Credits",
            5m,
            1m,
            0m,
            "AI-CREDIT-PURCHASE",
            "cr",
            0,
            "USD",
            1m);
        invoice.ReplaceLines([line], vatEnabled: false, vatPercent: 0m);
        invoice.Issue(Guid.NewGuid(), DateTime.UtcNow, vatEnabled: false, vatPercent: 0m);

        invoice.RepriceFxLines((_, _) => 42_500.1m);

        invoice.Status.ShouldBe(InvoiceStatus.Pending);
        line.FxRate.ShouldBe(42_500.1m);
        line.UnitPrice.ShouldBe(42_501m);
        invoice.TotalAmount.ShouldBe(5m * 42_501m);
    }

    [Fact]
    public void Included_zero_unit_price_line_does_not_need_fx()
    {
        var line = new InvoiceLineItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Included",
            20m,
            0m,
            0m,
            "AI-CREDIT-INCLUDED",
            "cr",
            0,
            "USD",
            1m);

        line.NeedsFx("IRR").ShouldBeFalse();
    }

    [Fact]
    public void RepriceFxLines_refuses_when_not_pending()
    {
        var invoice = new Invoice(
            Guid.NewGuid(),
            null,
            "INV-FX-2",
            Guid.NewGuid(),
            "IRR",
            "Plan",
            null,
            null,
            "ref");
        invoice.ReplaceLines(
            [
                new InvoiceLineItem(Guid.NewGuid(), invoice.Id, "X", 1, 1m, 0m, null, null, 0, "USD", 1m)
            ],
            false,
            0m);

        Should.Throw<BusinessException>(() => invoice.RepriceFxLines((_, _) => 10m))
            .Code.ShouldBe(SufiFinanceInvoicingErrorCodes.InvoiceNotEditable);
    }
}
