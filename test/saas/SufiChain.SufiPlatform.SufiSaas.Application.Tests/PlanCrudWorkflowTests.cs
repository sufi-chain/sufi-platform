using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Editions;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiSaas.Data;
using SufiChain.SufiPlatform.SufiSaas.Plans;
using SufiChain.SufiPlatform.SufiSaas.Subscriptions;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public class PlanCrudWorkflowTests
{
    [Fact]
    public async Task Create_Should_Store_An_Active_Plan_For_An_Active_Edition()
    {
        var edition = new Edition(Guid.NewGuid(), "Pro", "Pro", "PRO", true);
        var plans = Substitute.For<IPlanRepository>();
        plans.FindByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Plan?)null);
        plans.InsertAsync(Arg.Any<Plan>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Plan>());
        var service = NewService(plans, edition, hasSubscription: false);

        var created = await service.CreateAsync(Input(edition.Id, "starter", isActive: true));

        created.Code.ShouldBe("STARTER");
        created.IsActive.ShouldBeTrue();
        created.EditionId.ShouldBe(edition.Id);
        created.Price.ShouldBe(10m);
    }

    [Fact]
    public async Task Create_Should_Reject_A_Zero_Price_Without_Unpaid_Use()
    {
        var edition = new Edition(Guid.NewGuid(), "Pro", "Pro", "PRO", true);
        var service = NewService(Substitute.For<IPlanRepository>(), edition, hasSubscription: false);
        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.CreateAsync(Input(edition.Id, "starter", isActive: true, price: 0m)));
        error.Code.ShouldBe(SufiSaasErrorCodes.InvalidPlanPricing);
    }

    [Fact]
    public async Task Create_Should_Reject_A_Missing_Edition()
    {
        var service = NewService(Substitute.For<IPlanRepository>(), edition: null, hasSubscription: false);
        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.CreateAsync(Input(Guid.NewGuid(), "starter", isActive: true)));
        error.Code.ShouldBe(SufiSaasErrorCodes.PlanEditionNotFound);
    }

    [Fact]
    public async Task Create_Should_Reject_An_Inactive_Edition_For_An_Active_Plan()
    {
        var edition = new Edition(Guid.NewGuid(), "Old", "Old", "OLD", false);
        var service = NewService(Substitute.For<IPlanRepository>(), edition, hasSubscription: false);
        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.CreateAsync(Input(edition.Id, "starter", isActive: true)));
        error.Code.ShouldBe(SufiSaasErrorCodes.PlanEditionInactive);
    }

    [Fact]
    public async Task Delete_Should_Reject_A_Plan_With_An_Active_Subscription()
    {
        var service = NewService(Substitute.For<IPlanRepository>(), edition: null, hasSubscription: true);
        var error = await Should.ThrowAsync<BusinessException>(() => service.DeleteAsync(Guid.NewGuid()));
        error.Code.ShouldBe(SufiSaasErrorCodes.PlanReferencedBySubscription);
    }

    [Fact]
    public async Task Seed_Should_Not_Throw_And_Leaves_Paid_Plans_Inactive_When_Pricing_Config_Is_Missing()
    {
        var inserted = new List<Plan>();
        var logger = new ListLogger<SufiSaasPlanDataSeedContributor>();
        var contributor = NewSeedContributor(
            new Dictionary<string, string?>(),
            inserted,
            logger,
            existingPlan: null);

        await Should.NotThrowAsync(() => contributor.SeedAsync(new DataSeedContext()));

        var paidPlans = inserted
            .Where(plan => !string.Equals(plan.Code, SufiSaasConsts.PlanCodes.Demo, StringComparison.Ordinal))
            .ToList();
        paidPlans.Count.ShouldBe(3);
        paidPlans.ShouldAllBe(plan => !plan.IsActive);
        logger.Messages.Count(message => message.Contains("missing or invalid", StringComparison.Ordinal))
            .ShouldBe(3);
    }

    [Fact]
    public async Task Seed_Should_Keep_An_Existing_Unpriced_Plan_Inactive_Without_Overwriting_Its_Price()
    {
        var existing = new Plan(
            Guid.NewGuid(),
            SufiSaasConsts.PlanCodes.Starter,
            "Plan:STARTER",
            Guid.NewGuid(),
            billingPeriodDays: 30,
            allowUnpaidUse: false,
            sortOrder: 10,
            isSelfServiceAvailable: false,
            price: 15m,
            currencyCode: "USD");
        existing.SetIsActive(false);

        var updated = new List<Plan>();
        var contributor = NewSeedContributor(
            new Dictionary<string, string?>(),
            inserted: new List<Plan>(),
            logger: new ListLogger<SufiSaasPlanDataSeedContributor>(),
            existingPlan: existing,
            updated);

        await contributor.SeedAsync(new DataSeedContext());

        var starter = updated.Single(plan => plan.Code == SufiSaasConsts.PlanCodes.Starter);
        starter.IsActive.ShouldBeFalse();
        starter.Price.ShouldBe(15m);
        starter.CurrencyCode.ShouldBe("USD");
        starter.MonthlyPrice.ShouldBeNull();
        starter.YearlyPrice.ShouldBeNull();
    }

    [Fact]
    public async Task Seed_Should_Create_A_Priced_Paid_Plan_Active_When_Config_Is_Present()
    {
        var inserted = new List<Plan>();
        var contributor = NewSeedContributor(
            new Dictionary<string, string?>
            {
                ["SufiSaas:Plans:STARTER:Price"] = "29",
                ["SufiSaas:Plans:STARTER:CurrencyCode"] = "USD"
            },
            inserted,
            new ListLogger<SufiSaasPlanDataSeedContributor>(),
            existingPlan: null);

        await contributor.SeedAsync(new DataSeedContext());

        var starter = inserted.Single(plan => plan.Code == SufiSaasConsts.PlanCodes.Starter);
        starter.IsActive.ShouldBeTrue();
        starter.Price.ShouldBe(29m);
        starter.CurrencyCode.ShouldBe("USD");
    }

    private static SufiSaasPlanDataSeedContributor NewSeedContributor(
        IReadOnlyDictionary<string, string?> config,
        List<Plan> inserted,
        ILogger<SufiSaasPlanDataSeedContributor> logger,
        Plan? existingPlan,
        List<Plan>? updated = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        var editions = Substitute.For<IEditionRepository>();
        editions.FindByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => new Edition(
                Guid.NewGuid(),
                call.Arg<string>().ToLowerInvariant(),
                "Edition",
                call.Arg<string>(),
                isActive: true));
        editions.InsertAsync(Arg.Any<Edition>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Edition>());
        var editionManager = Substitute.For<EditionManager>(editions);

        var plans = Substitute.For<IPlanRepository>();
        plans.FindByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => existingPlan != null &&
                             string.Equals(call.Arg<string>(), existingPlan.Code, StringComparison.Ordinal)
                ? existingPlan
                : null);
        plans.InsertAsync(Arg.Any<Plan>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                inserted.Add(call.Arg<Plan>());
                return call.Arg<Plan>();
            });
        if (updated != null)
        {
            plans.UpdateAsync(Arg.Any<Plan>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    updated.Add(call.Arg<Plan>());
                    return call.Arg<Plan>();
                });
        }

        var featureDefinitions = Substitute.For<IFeatureDefinitionManager>();
        featureDefinitions.GetAllAsync()
            .Returns(Task.FromResult<IReadOnlyList<FeatureDefinition>>(Array.Empty<FeatureDefinition>()));

        return new SufiSaasPlanDataSeedContributor(
            editions,
            editionManager,
            plans,
            Substitute.For<IFeatureManager>(),
            featureDefinitions,
            SimpleGuidGenerator.Instance,
            configuration,
            logger,
            Substitute.For<ILocalizationTextSeeder>());
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        IDisposable? ILogger.BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    private static PlanAppService NewService(IPlanRepository plans, Edition? edition, bool hasSubscription)
    {
        plans.FindByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Plan?)null);
        var editions = Substitute.For<IEditionRepository>();
        editions.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(edition!);
        var subscriptions = Substitute.For<ISubscriptionRepository>();
        subscriptions.HasActiveByPlanIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(hasSubscription);
        var service = new PlanAppService(plans, editions, subscriptions);
        var services = new ServiceCollection();
        services.AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance);
        services.AddSingleton(Substitute.For<IStringLocalizerFactory>());
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private static CreateUpdatePlanDto Input(Guid editionId, string code, bool isActive, decimal price = 10m) => new()
    {
        Code = code,
        DisplayName = "Starter",
        EditionId = editionId,
        IsActive = isActive,
        Price = price,
        CurrencyCode = "USD"
    };
}
