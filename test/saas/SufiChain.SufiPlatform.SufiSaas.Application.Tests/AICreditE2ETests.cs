using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.AiCredits;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.SufiFinance.Wallets;
using SufiChain.SufiPlatform.SufiSaas.Finance.AiCredits;
using SufiChain.SufiPlatform.SufiSaas.Plans;
using SufiChain.SufiPlatform.SufiSaas.Settings;
using SufiChain.SufiPlatform.SufiSaas.Subscriptions;
using Volo.Abp;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public class AICreditE2ETests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid PlanId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid EditionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public async Task Shared_usage_charge_debits_that_tenant_with_markup()
    {
        var wallets = CreateWalletService();
        await wallets.CreditPurchasedAsync(TenantId, 1000m, "seed");

        var log = new AIUsageLog(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AICapabilityType.ChatCompletion,
            "gpt-4",
            AIProviderType.OpenAI,
            TenantId,
            isSharedWorkspaceUsage: true);
        log.RecordSuccess(10, 10, 100, estimatedCost: 10m);

        var idem = AiCreditUsageChargeWorker.BuildIdempotencyKey(TenantId, [log]);
        await wallets.ChargeUsageAsync(TenantId, 10m * 1.20m, idem);

        var balance = await wallets.GetBalanceAsync(TenantId);
        balance.Purchased.ShouldBe(988m);
    }

    [Fact]
    public async Task Own_key_usage_is_not_chargeable()
    {
        var log = new AIUsageLog(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AICapabilityType.ChatCompletion,
            "gpt-4",
            AIProviderType.OpenAI,
            TenantId,
            isSharedWorkspaceUsage: false);

        var usageLogs = Substitute.For<IAIUsageLogRepository>();
        usageLogs.GetAsync(log.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(log);

        var resolver = new AiCreditChatUsageWalletResolver(usageLogs, CreateWalletService());
        (await resolver.IsChargeableAsync(log.Id)).ShouldBeFalse();
        (await resolver.ResolveWalletTypeAsync(log.Id)).ShouldBe("None");
    }

    [Fact]
    public async Task Gate_denies_shared_workspace_below_grace()
    {
        var wallets = CreateWalletService();
        await wallets.ChargeUsageAsync(TenantId, 5m, "overdraft");

        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(SufiSaasSettings.AiCredits.GraceCredits).Returns("2");

        var gate = new AiCreditGate(
            wallets,
            new CurrentTenantStub(TenantId),
            settings,
            new MemoryCache(new MemoryCacheOptions()));

        var shared = new Workspace(Guid.NewGuid(), "host-shared", AIProviderType.OpenAI, "gpt-4", tenantId: null);

        var ex = await Should.ThrowAsync<BusinessException>(() => gate.EnsureAccessAsync(shared));
        ex.Code.ShouldBe(AIErrorCodes.AICreditsExhausted);
    }

    [Fact]
    public async Task Gate_allows_own_tenant_workspace()
    {
        var wallets = CreateWalletService();
        await wallets.ChargeUsageAsync(TenantId, 100m, "deep-overdraft");

        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(SufiSaasSettings.AiCredits.GraceCredits).Returns("2");

        var gate = new AiCreditGate(
            wallets,
            new CurrentTenantStub(TenantId),
            settings,
            new MemoryCache(new MemoryCacheOptions()));

        var own = new Workspace(Guid.NewGuid(), "tenant-own", AIProviderType.OpenAI, "gpt-4", tenantId: TenantId);
        await gate.EnsureAccessAsync(own);
    }

    [Fact]
    public async Task Summary_reports_warning_at_eighty_percent_of_grant()
    {
        var wallets = CreateWalletService();
        await wallets.GrantIncludedAsync(TenantId, 1000m, "grant");
        await wallets.ChargeUsageAsync(TenantId, 850m, "use");

        var summary = await CreateSummaryRepo(wallets, 1000m).GetSummaryAsync(TenantId);
        var dto = Map(summary!);
        dto.UsagePercent.ShouldBeGreaterThanOrEqualTo(80);
        dto.ThresholdLevel.ShouldBe("warning");
    }

    [Fact]
    public async Task Summary_reports_critical_at_full_included_usage()
    {
        var wallets = CreateWalletService();
        await wallets.GrantIncludedAsync(TenantId, 1000m, "grant");
        await wallets.ChargeUsageAsync(TenantId, 1000m, "use");

        var summary = await CreateSummaryRepo(wallets, 1000m).GetSummaryAsync(TenantId);
        var dto = Map(summary!);
        dto.AtLimit.ShouldBeTrue();
        dto.ThresholdLevel.ShouldBe("critical");
    }

    [Fact]
    public void Idempotency_key_is_stable_for_same_log_set()
    {
        var a = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var b = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var logs = new List<AIUsageLog>
        {
            new(a, Guid.NewGuid(), AICapabilityType.ChatCompletion, "m", AIProviderType.OpenAI, TenantId, true),
            new(b, Guid.NewGuid(), AICapabilityType.ChatCompletion, "m", AIProviderType.OpenAI, TenantId, true)
        };
        var first = AiCreditUsageChargeWorker.BuildIdempotencyKey(TenantId, logs);
        var second = AiCreditUsageChargeWorker.BuildIdempotencyKey(TenantId, logs.AsEnumerable().Reverse().ToList());
        first.ShouldBe(second);
    }

    private static WalletAiCreditSummaryRepository CreateSummaryRepo(AiCreditWalletService wallets, decimal grant)
    {
        var subscription = new Subscription(Guid.NewGuid(), TenantId, PlanId, SubscriptionStatus.Active);
        subscription.Activate(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddMonths(1));

        var subscriptions = Substitute.For<ISubscriptionRepository>();
        subscriptions.FindByTenantIdAsync(TenantId, Arg.Any<CancellationToken>()).Returns(subscription);

        var plan = new Plan(PlanId, "TEST", "Test", EditionId, price: 29m, currencyCode: "USD");
        plan.SetIncludedMonthlyAiCredits(grant);
        var plans = Substitute.For<IPlanRepository>();
        plans.FindAsync(PlanId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(plan);

        return new WalletAiCreditSummaryRepository(
            wallets,
            Substitute.For<IWalletRepository>(),
            subscriptions,
            plans,
            new CurrentTenantStub(null),
            new FixedClock(DateTime.UtcNow));
    }

    private static AiCreditSummaryDto Map(AICreditSummary summary) => new()
    {
        TenantId = summary.TenantId,
        PeriodStart = summary.PeriodStart,
        PeriodEnd = summary.PeriodEnd,
        IncludedCredits = summary.IncludedCredits,
        IncludedUsed = summary.IncludedUsed,
        PurchasedRemaining = summary.PurchasedRemaining,
        PurchasedUsed = summary.PurchasedUsed
    };

    private static AiCreditWalletService CreateWalletService()
    {
        var store = new Dictionary<string, Wallet>();
        var repo = Substitute.For<IWalletRepository>();
        repo.FindByOwnerAsync(
                Arg.Any<Guid?>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var key = $"{ci.ArgAt<string>(1)}:{ci.ArgAt<string>(2)}";
                return store.TryGetValue(key, out var wallet) ? wallet : null;
            });
        repo.InsertAsync(Arg.Any<Wallet>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var wallet = ci.ArgAt<Wallet>(0);
                store[$"{wallet.OwnerType}:{wallet.OwnerId}"] = wallet;
                return wallet;
            });
        repo.UpdateAsync(Arg.Any<Wallet>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var wallet = ci.ArgAt<Wallet>(0);
                store[$"{wallet.OwnerType}:{wallet.OwnerId}"] = wallet;
                return wallet;
            });

        return new AiCreditWalletService(
            repo,
            new SequentialGuidGenerator(),
            new FixedClock(DateTime.UtcNow),
            new CurrentTenantStub(null));
    }

    private sealed class CurrentTenantStub : ICurrentTenant
    {
        public CurrentTenantStub(Guid? id) => Id = id;
        public Guid? Id { get; }
        public string? Name => null;
        public bool IsAvailable => Id.HasValue;
        public IDisposable Change(Guid? id, string? name = null) => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() { } }
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime now) => Now = now;
        public DateTime Now { get; set; }
        public DateTimeKind Kind => DateTimeKind.Utc;
        public bool SupportsMultipleTimezone => false;
        public DateTime Normalize(DateTime dateTime) => dateTime;
        public DateTime ConvertToUserTime(DateTime utcDateTime) => utcDateTime;
        public DateTimeOffset ConvertToUserTime(DateTimeOffset dateTimeOffset) => dateTimeOffset;
        public DateTime ConvertToUtc(DateTime dateTime) => dateTime;
    }

    private sealed class SequentialGuidGenerator : IGuidGenerator
    {
        public Guid Create() => Guid.NewGuid();
    }
}
