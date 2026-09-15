using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.ExchangeRates;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Tests;

public class ExchangeRateTests
{
    [Fact]
    public void Should_Normalize_And_Reject_Invalid_Rates()
    {
        ExchangeRate.Normalize(" usd ").ShouldBe("USD");
        Should.Throw<BusinessException>(() => new ExchangeRate(
            Guid.NewGuid(), null, "USD", "IRR", 0, "test", false, 1,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(5), Guid.NewGuid()));
    }

    [Fact]
    public void Should_Round_Quote_Away_From_Zero()
    {
        var quote = new ConversionQuote(Guid.NewGuid(), null, "USD", "EUR", 1.005m, 1m, 2,
            "test", DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), "USD>EUR");
        quote.ResultAmount.ShouldBe(1.01m);
    }

    [Fact]
    public void Should_Reject_Expired_Quote()
    {
        var now = DateTime.UtcNow;
        var quote = new ConversionQuote(Guid.NewGuid(), null, "USD", "EUR", 1m, 1m, 2,
            "test", now, now, now.AddSeconds(1), "USD>EUR");
        Should.Throw<BusinessException>(() => quote.EnsureUsable(now.AddSeconds(1)));
    }
}
