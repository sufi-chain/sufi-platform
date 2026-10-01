using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Editions;
using SufiChain.SufiPlatform.SufiSaas.Plans;
using SufiChain.SufiPlatform.SufiSaas.Subscriptions;
using Volo.Abp;
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
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private static CreateUpdatePlanDto Input(Guid editionId, string code, bool isActive) => new()
    {
        Code = code,
        DisplayName = "Starter",
        EditionId = editionId,
        IsActive = isActive,
        CurrencyCode = "USD"
    };
}
