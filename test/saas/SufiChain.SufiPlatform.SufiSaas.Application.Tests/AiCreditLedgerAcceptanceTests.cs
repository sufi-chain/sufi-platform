using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Wallets;
using SufiChain.SufiPlatform.SufiSaas.Finance.AiCredits;
using NSubstitute;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

/// <summary>
/// Pins ledger acceptance for #407: idempotent sources and Included-then-Purchased spend order.
/// </summary>
public class AiCreditLedgerAcceptanceTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly DateTime Now = new(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Grant_purchase_expiry_and_usage_each_applied_twice_produce_one_entry()
    {
        var (service, wallets) = CreateService();

        await service.GrantIncludedAsync(TenantId, 10m, "aic:grant:once");
        await service.GrantIncludedAsync(TenantId, 10m, "aic:grant:once");

        await service.CreditPurchasedAsync(TenantId, 20m, "aic:buy:once");
        await service.CreditPurchasedAsync(TenantId, 20m, "aic:buy:once");

        await service.ExpireIncludedAsync(TenantId, "aic:expire:once");
        await service.ExpireIncludedAsync(TenantId, "aic:expire:once");

        await service.CreditPurchasedAsync(TenantId, 5m, "aic:buy:for-usage");
        await service.ChargeUsageAsync(TenantId, 3m, "aic:usage:once");
        await service.ChargeUsageAsync(TenantId, 3m, "aic:usage:once");

        var entries = wallets.SelectMany(w => w.Entries).ToList();
        entries.Count(e => e.SourceType == SufiSaasConsts.AiCredit.GrantSource).ShouldBe(1);
        entries.Count(e => e.SourceType == SufiSaasConsts.AiCredit.PurchaseSource).ShouldBe(2);
        entries.Count(e => e.SourceType == SufiSaasConsts.AiCredit.ExpirySource).ShouldBe(1);
        entries.Count(e => e.SourceType == SufiSaasConsts.AiCredit.UsageSource).ShouldBe(1);
    }

    [Fact]
    public async Task Charge_of_three_with_included_two_and_purchased_five_leaves_zero_and_four()
    {
        var (service, _) = CreateService();
        await service.GrantIncludedAsync(TenantId, 2m, "grant");
        await service.CreditPurchasedAsync(TenantId, 5m, "buy");

        await service.ChargeUsageAsync(TenantId, 3m, "usage");

        var balance = await service.GetBalanceAsync(TenantId);
        balance.Included.ShouldBe(0m);
        balance.Purchased.ShouldBe(4m);
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
