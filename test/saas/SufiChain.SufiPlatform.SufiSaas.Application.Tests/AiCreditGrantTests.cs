using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Currencies;
using SufiChain.SufiPlatform.SufiFinance.Invoicing;
using SufiChain.SufiPlatform.SufiFinance.Invoicing.ETOs;
using SufiChain.SufiPlatform.SufiFinance.Wallets;
using SufiChain.SufiPlatform.SufiSaas.Finance;
using SufiChain.SufiPlatform.SufiSaas.Finance.AiCredits;
using SufiChain.SufiPlatform.SufiSaas.Plans;
using SufiChain.SufiPlatform.SufiSaas.Subscriptions;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public class AiCreditGrantTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid PlanId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid EditionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid InvoiceId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid SubscriptionId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private static readonly DateTime PeriodStart = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Paid_monthly_invoice_grants_included_credits_once()
    {
        var harness = Harness.Create(20m, PeriodStart, PeriodStart.AddDays(30));

        await harness.Handler.HandleEventAsync(harness.SubscriptionPaid(PeriodStart));
        await harness.Handler.HandleEventAsync(harness.SubscriptionPaid(PeriodStart));

        var balance = await harness.Credits.GetBalanceAsync(TenantId);
        balance.Included.ShouldBe(20m);
        balance.Purchased.ShouldBe(0m);
    }

    [Fact]
    public async Task Paid_buy_invoice_credits_purchased_once()
    {
        var harness = Harness.Create(20m, PeriodStart, PeriodStart.AddDays(30));
        var lineId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var purchaseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var invoice = PurchaseInvoice(lineId, purchaseId, 50m);
        harness.Invoices.FindAsync(invoice.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(invoice);

        var paid = new InvoicePaidEto
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            ExternalReference = SufiSaasAiCreditPurchaseInvoice.ExternalReference(TenantId, purchaseId),
            PaidAmount = 50m,
            CurrencyCode = "IRR",
            OccurredAt = PeriodStart
        };

        await harness.Handler.HandleEventAsync(paid);
        await harness.Handler.HandleEventAsync(paid);

        var balance = await harness.Credits.GetBalanceAsync(TenantId);
        balance.Purchased.ShouldBe(50m);
        balance.Included.ShouldBe(0m);
    }

    [Fact]
    public async Task Yearly_worker_grants_months_2_through_12_then_expires_without_renewal()
    {
        var periodEnd = PeriodStart.AddDays(365);
        var harness = Harness.Create(20m, PeriodStart, periodEnd);
        await harness.Handler.HandleEventAsync(harness.SubscriptionPaid(PeriodStart));
        await harness.Credits.CreditPurchasedAsync(TenantId, 50m, "buy-kept");

        for (var month = 1; month <= 11; month++)
        {
            harness.Now = PeriodStart.AddMonths(month);
            await harness.Processor.ProcessAsync();
            await harness.Processor.ProcessAsync();

            var balance = await harness.Credits.GetBalanceAsync(TenantId);
            balance.Included.ShouldBe(20m);
            balance.Purchased.ShouldBe(50m);
        }

        harness.Now = PeriodStart.AddMonths(12);
        await harness.Processor.ProcessAsync();

        var expired = await harness.Credits.GetBalanceAsync(TenantId);
        expired.Included.ShouldBe(0m);
        expired.Purchased.ShouldBe(50m);
    }

    [Fact]
    public async Task Suspended_subscription_expires_included_credits_without_a_new_grant()
    {
        var harness = Harness.Create(20m, PeriodStart, PeriodStart.AddDays(365));
        await harness.Handler.HandleEventAsync(harness.SubscriptionPaid(PeriodStart));
        await harness.Credits.CreditPurchasedAsync(TenantId, 50m, "buy-kept");
        harness.Subscription.SetStatus(SubscriptionStatus.Suspended);

        harness.Now = PeriodStart.AddMonths(1);
        await harness.Processor.ProcessAsync();

        var balance = await harness.Credits.GetBalanceAsync(TenantId);
        balance.Included.ShouldBe(0m);
        balance.Purchased.ShouldBe(50m);
    }

    [Fact]
    public async Task Grace_deficit_on_purchased_survives_expiry_and_the_next_grant()
    {
        var harness = Harness.Create(20m, PeriodStart, PeriodStart.AddDays(365));
        await harness.Handler.HandleEventAsync(harness.SubscriptionPaid(PeriodStart));
        await harness.Credits.ChargeUsageAsync(TenantId, 22m, "grace");

        harness.Now = PeriodStart.AddMonths(1);
        await harness.Processor.ProcessAsync();

        var balance = await harness.Credits.GetBalanceAsync(TenantId);
        balance.Included.ShouldBe(20m);
        balance.Purchased.ShouldBe(-2m);
    }

    private static Invoice PurchaseInvoice(Guid lineId, Guid purchaseId, decimal credits)
    {
        var invoice = new Invoice(
            InvoiceId,
            tenantId: null,
            "INV-AIC-1",
            Guid.NewGuid(),
            "IRR",
            "AI Credits",
            dueDate: null,
            notes: null,
            SufiSaasAiCreditPurchaseInvoice.ExternalReference(TenantId, purchaseId));
        invoice.ReplaceLines(
            [
                new InvoiceLineItem(
                    lineId,
                    invoice.Id,
                    "AI Credits",
                    credits,
                    1m,
                    0m,
                    SufiSaasConsts.AiCredit.PurchaseProductCode,
                    "credit",
                    0)
            ],
            vatEnabled: false,
            vatPercent: 0m);
        invoice.Issue(Guid.NewGuid(), PeriodStart);
        invoice.RecordWalletPayment(Guid.NewGuid(), Guid.NewGuid(), "pay-aic-1", PeriodStart, Guid.NewGuid());
        return invoice;
    }

    private sealed class Harness
    {
        public InvoicePaidAiCreditHandler Handler { get; }
        public AiCreditMonthlyProcessor Processor { get; }
        public AiCreditWalletService Credits { get; }
        public IInvoiceRepository Invoices { get; }
        public Subscription Subscription { get; }
        public DateTime Now
        {
            get => _clock.Value;
            set => _clock.Value = value;
        }

        private readonly ClockBox _clock;

        private Harness(
            InvoicePaidAiCreditHandler handler,
            AiCreditMonthlyProcessor processor,
            AiCreditWalletService credits,
            IInvoiceRepository invoices,
            Subscription subscription,
            ClockBox clock)
        {
            Handler = handler;
            Processor = processor;
            Credits = credits;
            Invoices = invoices;
            Subscription = subscription;
            _clock = clock;
        }

        public InvoicePaidEto SubscriptionPaid(DateTime paidAt) => new()
        {
            Id = Guid.NewGuid(),
            InvoiceId = InvoiceId,
            ExternalReference = SufiSaasSubscriptionInvoice.ExternalReference(SubscriptionId, PlanId),
            PaidAmount = 29m,
            CurrencyCode = "IRR",
            OccurredAt = paidAt
        };

        public static Harness Create(decimal includedCredits, DateTime periodStart, DateTime periodEnd)
        {
            var plan = new Plan(PlanId, "GROWTH", "Growth", EditionId, billingPeriodDays: 365, price: 29m, currencyCode: "IRR");
            plan.SetIncludedMonthlyAiCredits(includedCredits);
            var subscription = new Subscription(SubscriptionId, TenantId, PlanId);
            subscription.RequestPaidPlanChange(PlanId, 29m, "IRR", (int)(periodEnd - periodStart).TotalDays);
            subscription.AttachBillingInvoice(InvoiceId);
            subscription.CompletePaidPlanChange(InvoiceId, 29m, "IRR", periodStart);

            var subscriptions = Substitute.For<ISubscriptionRepository>();
            subscriptions.FindByBillingInvoiceIdAsync(InvoiceId, Arg.Any<CancellationToken>()).Returns(subscription);
            subscriptions.FindByTenantIdAsync(TenantId, Arg.Any<CancellationToken>()).Returns(subscription);

            var plans = Substitute.For<IPlanRepository>();
            plans.GetAsync(PlanId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(plan);

            var invoices = Substitute.For<IInvoiceRepository>();
            var (credits, _) = CreateCredits();
            var tenant = HostTenant();

            var clock = new ClockBox(periodStart);
            var handler = new InvoicePaidAiCreditHandler(
                subscriptions,
                plans,
                invoices,
                credits,
                tenant,
                Clock(() => periodStart),
                NullLogger<InvoicePaidAiCreditHandler>.Instance);

            var marker = new Wallet(
                Guid.NewGuid(),
                tenantId: null,
                SufiSaasConsts.AiCredit.IncludedOwnerType,
                TenantId.ToString("N"),
                SufiFinanceCurrencyCodes.Aic,
                Guid.NewGuid(),
                periodStart);
            var discovery = Substitute.For<IWalletRepository>();
            discovery.GetListAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<WalletStatus?>(),
                    Arg.Any<int>(),
                    Arg.Any<int>(),
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(call => call.ArgAt<int>(4) == 0 ? new List<Wallet> { marker } : []);

            var harness = new Harness(handler, null!, credits, invoices, subscription, clock);
            var processor = new AiCreditMonthlyProcessor(
                discovery,
                subscriptions,
                plans,
                credits,
                tenant,
                Clock(() => clock.Value),
                NullLogger<AiCreditMonthlyProcessor>.Instance);
            return new Harness(handler, processor, credits, invoices, subscription, clock);
        }

        private static (AiCreditWalletService Service, List<Wallet> Wallets) CreateCredits()
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
                .Returns(call => wallets.FirstOrDefault(wallet =>
                    wallet.TenantId == call.ArgAt<Guid?>(0)
                    && wallet.OwnerType == call.ArgAt<string>(1)
                    && wallet.OwnerId == call.ArgAt<string>(2)
                    && wallet.CurrencyCode == call.ArgAt<string>(3)));
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
            return (new AiCreditWalletService(repository, guids, Clock(() => PeriodStart), HostTenant()), wallets);
        }

        private static IClock Clock(Func<DateTime> now)
        {
            var clock = Substitute.For<IClock>();
            clock.Now.Returns(_ => now());
            return clock;
        }

        private static ICurrentTenant HostTenant()
        {
            var tenant = Substitute.For<ICurrentTenant>();
            Guid? current = null;
            tenant.Id.Returns(_ => current);
            tenant.Change(Arg.Any<Guid?>()).Returns(call =>
            {
                var previous = current;
                current = call.Arg<Guid?>();
                return new DisposeAction(() => current = previous);
            });
            return tenant;
        }

        private sealed class ClockBox(DateTime value)
        {
            public DateTime Value { get; set; } = value;
        }

        private sealed class DisposeAction(Action action) : IDisposable
        {
            public void Dispose() => action();
        }
    }
}
