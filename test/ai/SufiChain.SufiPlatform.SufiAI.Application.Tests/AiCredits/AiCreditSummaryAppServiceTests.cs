using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.AiCredits;
using SufiChain.SufiPlatform.SufiAI.Application.AiCredits;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class AiCreditSummaryAppServiceTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Tenant_with_no_wallets_gets_null_summary_for_own_tenant()
    {
        var fixture = Fixture.ForTenant(TenantA, grantViewAll: false);
        fixture.Repository.GetSummaryAsync(TenantA, null, null, Arg.Any<CancellationToken>())
            .Returns((AICreditSummary?)null);

        var result = await fixture.Service.GetAsync();

        result.ShouldBeNull();
        await fixture.Repository.Received(1)
            .GetSummaryAsync(TenantA, null, null, Arg.Any<CancellationToken>());
        await fixture.Repository.DidNotReceive()
            .GetSummaryAsync(TenantB, Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Tenant_B_cannot_read_tenant_A_by_passing_tenant_id()
    {
        var fixture = Fixture.ForTenant(TenantB, grantViewAll: false);

        await Should.ThrowAsync<AbpAuthorizationException>(() =>
            fixture.Service.GetAsync(TenantA));

        await fixture.Repository.DidNotReceive()
            .GetSummaryAsync(Arg.Any<Guid>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Tenant_GetAsync_uses_current_tenant_when_input_matches()
    {
        var fixture = Fixture.ForTenant(TenantA, grantViewAll: false);
        fixture.Repository.GetSummaryAsync(TenantA, null, null, Arg.Any<CancellationToken>())
            .Returns(new AICreditSummary
            {
                TenantId = TenantA,
                IncludedCredits = 0m,
                IncludedUsed = 0m,
                PurchasedRemaining = 0m,
                PurchasedUsed = 0m
            });

        var result = await fixture.Service.GetAsync(TenantA);

        result.ShouldNotBeNull();
        result!.TenantId.ShouldBe(TenantA);
        result.IncludedCredits.ShouldBe(0m);
        result.PurchasedRemaining.ShouldBe(0m);
    }

    [Fact]
    public async Task GetAllAsync_as_tenant_user_throws()
    {
        var fixture = Fixture.ForTenant(TenantA, grantViewAll: false);

        await Should.ThrowAsync<AbpAuthorizationException>(() =>
            fixture.Service.GetAllAsync());

        await fixture.Repository.DidNotReceive()
            .GetAllSummaryAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Anonymous_GetAsync_throws()
    {
        var fixture = Fixture.Anonymous();

        await Should.ThrowAsync<AbpAuthorizationException>(() =>
            fixture.Service.GetAsync());

        await fixture.Repository.DidNotReceive()
            .GetSummaryAsync(Arg.Any<Guid>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Host_with_ViewAll_can_list_all_and_read_a_tenant()
    {
        var fixture = Fixture.ForHost(grantViewAll: true);
        fixture.Repository.GetAllSummaryAsync(Arg.Any<CancellationToken>())
            .Returns([
                new AICreditSummary { TenantId = TenantA },
                new AICreditSummary { TenantId = TenantB }
            ]);
        fixture.Repository.GetSummaryAsync(TenantA, null, null, Arg.Any<CancellationToken>())
            .Returns(new AICreditSummary { TenantId = TenantA, PurchasedRemaining = 3m });

        var all = await fixture.Service.GetAllAsync();
        all.Count.ShouldBe(2);

        var one = await fixture.Service.GetAsync(TenantA);
        one.ShouldNotBeNull();
        one!.TenantId.ShouldBe(TenantA);
        one.PurchasedRemaining.ShouldBe(3m);
    }

    [Fact]
    public async Task Summary_DTO_rounds_amounts_to_two_decimals()
    {
        var fixture = Fixture.ForTenant(TenantA, grantViewAll: false);
        fixture.Repository.GetSummaryAsync(TenantA, null, null, Arg.Any<CancellationToken>())
            .Returns(new AICreditSummary
            {
                TenantId = TenantA,
                IncludedCredits = 10.004m,
                IncludedUsed = 1.005m,
                PurchasedRemaining = 2.235m,
                PurchasedUsed = 0.004m
            });

        var result = await fixture.Service.GetAsync();

        result.ShouldNotBeNull();
        result!.IncludedCredits.ShouldBe(10.00m);
        result.IncludedUsed.ShouldBe(1.01m);
        result.PurchasedRemaining.ShouldBe(2.24m);
        result.PurchasedUsed.ShouldBe(0.00m);
    }

    private sealed class Fixture
    {
        public IAiCreditSummaryRepository Repository { get; }
        public AiCreditSummaryAppService Service { get; }

        private Fixture(
            Guid? currentTenantId,
            bool authenticated,
            bool grantViewAll)
        {
            Repository = Substitute.For<IAiCreditSummaryRepository>();

            var tenant = Substitute.For<ICurrentTenant>();
            tenant.Id.Returns(currentTenantId);

            var user = Substitute.For<ICurrentUser>();
            user.IsAuthenticated.Returns(authenticated);
            user.Id.Returns(authenticated ? Guid.NewGuid() : (Guid?)null);

            var authorization = new TestAuthorizationService(grantViewAll);

            var services = new ServiceCollection();
            services.AddSingleton(tenant);
            services.AddSingleton(user);
            services.AddSingleton<IAuthorizationService>(authorization);
            services.AddSingleton<IAbpAuthorizationService>(authorization);

            Service = new AiCreditSummaryAppService(Repository)
            {
                LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider())
            };
        }

        public static Fixture ForTenant(Guid tenantId, bool grantViewAll) =>
            new(tenantId, authenticated: true, grantViewAll);

        public static Fixture ForHost(bool grantViewAll) =>
            new(currentTenantId: null, authenticated: true, grantViewAll);

        public static Fixture Anonymous() =>
            new(currentTenantId: null, authenticated: false, grantViewAll: false);
    }

    private sealed class TestAuthorizationService(bool grantViewAll) : IAbpAuthorizationService
    {
        private readonly IServiceProvider _serviceProvider = new ServiceCollection().BuildServiceProvider();

        public ClaimsPrincipal CurrentPrincipal { get; } = new(new ClaimsIdentity("test"));

        public IServiceProvider ServiceProvider => _serviceProvider;

        public Task<AuthorizationResult> AuthorizeAsync(
            ClaimsPrincipal user,
            object? resource,
            IEnumerable<IAuthorizationRequirement> requirements) =>
            Task.FromResult(AuthorizationResult.Failed());

        public Task<AuthorizationResult> AuthorizeAsync(
            ClaimsPrincipal user,
            object? resource,
            string policyName)
        {
            var granted = grantViewAll && policyName == AIPermissions.AiCredits.ViewAll;
            return Task.FromResult(granted
                ? AuthorizationResult.Success()
                : AuthorizationResult.Failed());
        }
    }
}
