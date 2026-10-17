using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Currencies;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Domain.Tests;

public class AiCreditCurrencySeedTests
{
    [Fact]
    public async Task Seed_creates_AIC_with_precision_two_and_away_from_zero()
    {
        var store = new List<Currency>();
        var contributor = CreateContributor(store);

        await contributor.SeedAsync(new DataSeedContext(null));

        var aic = store.Single(c => c.Code == SufiFinanceCurrencyCodes.Aic);
        aic.Precision.ShouldBe(2);
        aic.RoundingMode.ShouldBe(CurrencyRoundingMode.AwayFromZero);
        aic.BaseUnitCode.ShouldBe(SufiFinanceCurrencyCodes.Usd);
        aic.BaseUnitRatio.ShouldBe(1m);
    }

    [Fact]
    public async Task Re_seeding_precision_six_AIC_updates_to_precision_two()
    {
        var stale = new Currency(
            Guid.NewGuid(),
            null,
            SufiFinanceCurrencyCodes.Aic,
            "AI Credit",
            "cr",
            6,
            CurrencyRoundingMode.ToEven,
            baseUnitCode: SufiFinanceCurrencyCodes.Usd,
            baseUnitRatio: 1m);
        var store = new List<Currency> { stale };
        var contributor = CreateContributor(store);

        await contributor.SeedAsync(new DataSeedContext(null));

        store.Count(c => c.Code == SufiFinanceCurrencyCodes.Aic).ShouldBe(1);
        stale.Precision.ShouldBe(2);
        stale.RoundingMode.ShouldBe(CurrencyRoundingMode.AwayFromZero);
        stale.BaseUnitCode.ShouldBe(SufiFinanceCurrencyCodes.Usd);
        stale.BaseUnitRatio.ShouldBe(1m);
    }

    private static SufiFinanceCurrencyDataSeedContributor CreateContributor(List<Currency> store)
    {
        var repository = Substitute.For<ISufiFinanceCurrencyRepository>();
        repository.FindByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var code = call.ArgAt<string>(0).Trim().ToUpperInvariant();
                return store.FirstOrDefault(c => c.Code == code);
            });
        repository.InsertAsync(Arg.Any<Currency>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var currency = call.Arg<Currency>();
                store.Add(currency);
                return currency;
            });
        repository.UpdateAsync(Arg.Any<Currency>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Currency>());

        var guids = Substitute.For<IGuidGenerator>();
        guids.Create().Returns(_ => Guid.NewGuid());

        Guid? current = null;
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns(_ => current);
        tenant.Change(Arg.Any<Guid?>()).Returns(call =>
        {
            var previous = current;
            current = call.Arg<Guid?>();
            return new DisposeAction(() => current = previous);
        });

        var manager = new SufiFinanceCurrencyManager(repository, guids, tenant);
        return new SufiFinanceCurrencyDataSeedContributor(repository, manager, tenant);
    }

    private sealed class DisposeAction(Action action) : IDisposable
    {
        public void Dispose() => action();
    }
}
