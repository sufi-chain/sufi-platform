using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Currencies;
using SufiChain.SufiPlatform.SufiFinance.Wallets;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Domain.Tests;

public class AiCreditCurrencyAndOverdraftTests
{
    private static readonly DateTime Now = new(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void AIC_currency_has_precision_six_and_usd_base_unit()
    {
        var currency = new Currency(
            Guid.NewGuid(),
            null,
            SufiFinanceCurrencyCodes.Aic,
            "AI Credit",
            "cr",
            6,
            baseUnitCode: SufiFinanceCurrencyCodes.Usd,
            baseUnitRatio: 1m);

        currency.Code.ShouldBe("AIC");
        currency.Precision.ShouldBe(6);
        currency.Symbol.ShouldBe("cr");
        currency.BaseUnitCode.ShouldBe("USD");
        currency.BaseUnitRatio.ShouldBe(1m);
        SufiFinanceCurrencyCodes.Aic.ShouldBe("AIC");
    }

    [Fact]
    public void DebitAllowingOverdraft_can_go_negative_and_is_idempotent()
    {
        var wallet = new Wallet(Guid.NewGuid(), null, "SufiSaas.AiCredit.Purchased", Guid.NewGuid().ToString("N"), "AIC", Guid.NewGuid(), Now);
        wallet.Credit(Guid.NewGuid(), 1m, "Grant", null, "credit-1", Now, null, Guid.NewGuid());

        var first = wallet.DebitAllowingOverdraft(
            Guid.NewGuid(), 2.5m, "AiUsage", "u1", "usage-1", Now, null, Guid.NewGuid());
        var second = wallet.DebitAllowingOverdraft(
            Guid.NewGuid(), 2.5m, "AiUsage", "u1", "usage-1", Now, null, Guid.NewGuid());

        first.Id.ShouldBe(second.Id);
        wallet.Balance.ShouldBe(-1.5m);
        wallet.Entries.Count(e => e.EntryType == WalletEntryType.Debit).ShouldBe(1);
    }

    [Fact]
    public void Debit_still_throws_when_insufficient()
    {
        var wallet = new Wallet(Guid.NewGuid(), null, "SufiSaas.AiCredit.Included", Guid.NewGuid().ToString("N"), "AIC", Guid.NewGuid(), Now);
        wallet.Credit(Guid.NewGuid(), 1m, "Grant", null, "credit-1", Now, null, Guid.NewGuid());

        var exception = Should.Throw<BusinessException>(() =>
            wallet.Debit(Guid.NewGuid(), 2m, "AiUsage", "u1", "usage-1", Now, null, Guid.NewGuid()));

        exception.Code.ShouldBe(SufiFinanceWalletsErrorCodes.InsufficientAvailableBalance);
        wallet.Balance.ShouldBe(1m);
    }
}
