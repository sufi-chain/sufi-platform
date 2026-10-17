using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Currencies;
using SufiChain.SufiPlatform.SufiFinance.Wallets;
using SufiChain.SufiPlatform.SufiSaas.Finance.AiCredits;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public class AiCreditWalletServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly DateTime Now = new(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetOrCreate_is_idempotent_and_host_scoped()
    {
        var (service, wallets) = CreateService();

        await service.GetOrCreateAsync(TenantId);
        await service.GetOrCreateAsync(TenantId);

        wallets.Count.ShouldBe(2);
        wallets.ShouldAllBe(w => w.TenantId == null);
        wallets.ShouldAllBe(w => w.CurrencyCode == SufiFinanceCurrencyCodes.Aic);
        wallets.Select(w => w.OwnerType).OrderBy(x => x).ShouldBe([
            SufiSaasConsts.AiCredit.IncludedOwnerType,
            SufiSaasConsts.AiCredit.PurchasedOwnerType
        ]);
    }

    [Fact]
    public async Task Charge_uses_included_then_purchased_and_may_overdraft_purchased()
    {
        var (service, _) = CreateService();
        await service.GrantIncludedAsync(TenantId, 2m, "grant-1");
        await service.CreditPurchasedAsync(TenantId, 5m, "buy-1");

        await service.ChargeUsageAsync(TenantId, 3m, "usage-1");
        var after = await service.GetBalanceAsync(TenantId);
        after.Included.ShouldBe(0m);
        after.Purchased.ShouldBe(4m);

        await service.ChargeUsageAsync(TenantId, 5m, "usage-2");
        after = await service.GetBalanceAsync(TenantId);
        after.Included.ShouldBe(0m);
        after.Purchased.ShouldBe(-1m);

        await service.ChargeUsageAsync(TenantId, 5m, "usage-2");
        after = await service.GetBalanceAsync(TenantId);
        after.Purchased.ShouldBe(-1m);
    }

    [Theory]
    [InlineData(0.004, 0.00)]
    [InlineData(0.005, 0.01)]
    [InlineData(1.235, 1.24)]
    public async Task ChargeUsage_rounds_to_two_decimals_half_away_from_zero(
        decimal raw,
        decimal expectedCharge)
    {
        var (service, wallets) = CreateService();
        await service.CreditPurchasedAsync(TenantId, 10m, "buy-seed");

        await service.ChargeUsageAsync(TenantId, raw, "usage-round");

        var balance = await service.GetBalanceAsync(TenantId);
        balance.Purchased.ShouldBe(10m - expectedCharge);

        if (expectedCharge == 0m)
        {
            wallets.SelectMany(w => w.Entries)
                .Count(e => e.SourceType == SufiSaasConsts.AiCredit.UsageSource)
                .ShouldBe(0);
        }
        else
        {
            wallets.SelectMany(w => w.Entries)
                .Where(e => e.SourceType == SufiSaasConsts.AiCredit.UsageSource)
                .Sum(e => e.Amount)
                .ShouldBe(expectedCharge);
        }
    }

    [Fact]
    public async Task Expiry_zeroes_included_only()
    {
        var (service, _) = CreateService();
        await service.GrantIncludedAsync(TenantId, 20m, "grant-1");
        await service.CreditPurchasedAsync(TenantId, 50m, "buy-1");

        await service.ExpireIncludedAsync(TenantId, "expire-1");
        await service.ExpireIncludedAsync(TenantId, "expire-1");

        var balance = await service.GetBalanceAsync(TenantId);
        balance.Included.ShouldBe(0m);
        balance.Purchased.ShouldBe(50m);
    }

    [Fact]
    public async Task GetBalance_without_wallets_returns_zeros_and_does_not_insert()
    {
        var (service, wallets) = CreateService();

        var balance = await service.GetBalanceAsync(TenantId);

        balance.ShouldBe((0m, 0m, 0m));
        wallets.ShouldBeEmpty();
    }

    private static (AiCreditWalletService Service, List<Wallet> Wallets) CreateService()
    {
        var wallets = new List<Wallet>();
        var repository = Substitute.For<IWalletRepository>();
        repository.FindByOwnerAsync(
                Arg.Any<Guid?>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var tenantId = call.ArgAt<Guid?>(0);
                var ownerType = call.ArgAt<string>(1);
                var ownerId = call.ArgAt<string>(2);
                var currency = call.ArgAt<string>(3);
                return wallets.FirstOrDefault(w =>
                    w.TenantId == tenantId
                    && w.OwnerType == ownerType
                    && w.OwnerId == ownerId
                    && w.CurrencyCode == currency);
            });
        repository.InsertAsync(Arg.Any<Wallet>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var wallet = call.Arg<Wallet>();
                wallets.Add(wallet);
                return wallet;
            });
        repository.UpdateAsync(Arg.Any<Wallet>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Wallet>());

        var guids = Substitute.For<IGuidGenerator>();
        guids.Create().Returns(_ => Guid.NewGuid());
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);
        var tenant = Substitute.For<ICurrentTenant>();
        Guid? current = null;
        tenant.Id.Returns(_ => current);
        tenant.Change(Arg.Any<Guid?>()).Returns(call =>
        {
            var previous = current;
            current = call.Arg<Guid?>();
            return new DisposeAction(() => current = previous);
        });

        return (new AiCreditWalletService(repository, guids, clock, tenant), wallets);
    }

    private sealed class DisposeAction(Action action) : IDisposable
    {
        public void Dispose() => action();
    }
}
