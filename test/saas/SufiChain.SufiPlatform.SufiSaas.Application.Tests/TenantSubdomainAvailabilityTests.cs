using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.SufiSaas.Licensing;
using SufiChain.SufiPlatform.SufiSaas.Plans;
using SufiChain.SufiPlatform.SufiSaas.Provisioning;
using SufiChain.SufiPlatform.SufiSaas.Subscriptions;
using SufiChain.SufiPlatform.SufiSaas.TenantRequests;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Authorization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Users;
using Xunit;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.SufiSaas;

public class TenantSubdomainAvailabilityTests
{
    [Theory]
    [InlineData("arian", TenantSubdomainAvailabilityStatus.Available)]
    [InlineData("arian_group", TenantSubdomainAvailabilityStatus.Invalid)]
    [InlineData("ab", TenantSubdomainAvailabilityStatus.Invalid)]
    [InlineData("console", TenantSubdomainAvailabilityStatus.Reserved)]
    [InlineData("stage", TenantSubdomainAvailabilityStatus.Reserved)]
    public async Task CheckAvailability_Should_Classify_Without_Hitting_The_Database(
        string subdomain,
        TenantSubdomainAvailabilityStatus expected)
    {
        var result = await NewService().Service.CheckSubdomainAvailabilityAsync(subdomain);

        result.Status.ShouldBe(expected);
        result.IsAvailable.ShouldBe(expected == TenantSubdomainAvailabilityStatus.Available);
    }

    [Fact]
    public async Task CheckAvailability_Should_Report_Taken_When_An_Active_Request_Reserves_It()
    {
        var harness = NewService();
        harness.Requests.IsSubdomainInActiveRequestAsync(
            "arian",
            Arg.Any<Guid?>(),
            Arg.Any<System.Threading.CancellationToken>())
            .Returns(true);

        var result = await harness.Service.CheckSubdomainAvailabilityAsync("arian");

        result.Status.ShouldBe(TenantSubdomainAvailabilityStatus.Taken);
        result.IsAvailable.ShouldBeFalse();
    }

    [Fact]
    public async Task CheckAvailability_Should_Report_Taken_When_An_Existing_Tenant_Owns_It()
    {
        var tenantId = Guid.NewGuid();
        var harness = NewService();
        harness.Tenants.FindByPrimarySubdomainAsync("arian", false, Arg.Any<System.Threading.CancellationToken>())
            .Returns(NewTenant(tenantId, "arian"));

        var result = await harness.Service.CheckSubdomainAvailabilityAsync("arian");

        result.Status.ShouldBe(TenantSubdomainAvailabilityStatus.Taken);
        result.IsAvailable.ShouldBeFalse();
    }

    [Fact]
    public async Task Map_Should_Preview_The_Database_Name_From_The_Subdomain_Not_The_Display_Name()
    {
        var harness = NewService();
        harness.Requests.IsSubdomainInActiveRequestAsync(
            Arg.Any<string>(),
            Arg.Any<Guid?>(),
            Arg.Any<System.Threading.CancellationToken>())
            .Returns(false);
        harness.Tenants.FindByPrimarySubdomainAsync("arian", false, Arg.Any<System.Threading.CancellationToken>())
            .Returns((Tenant?)null);

        var request = new TenantRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "گروه آریان",
            Guid.NewGuid(),
            "arian");
        harness.Plans.FindAsync(request.PlanId, Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns((Plan?)null);

        var dto = await harness.Service.ExposeMapAsync(request);

        dto.DatabaseName.ShouldContain("arian");
    }

    [Fact]
    public async Task Catalog_Should_Mark_An_Unpriced_Paid_Plan_As_PriceNotSet()
    {
        var harness = NewService();
        // The seed represents an unpriced paid plan as allowUnpaidUse + price 0 + inactive.
        var plan = NewPlan(
            "growth",
            Guid.NewGuid(),
            billingPeriodDays: 30,
            allowUnpaidUse: true,
            sortOrder: 20,
            isSelfServiceAvailable: true,
            price: 0m);
        plan.SetIsActive(false);
        harness.Plans.GetListAsync(Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(new List<Plan> { plan });

        var catalog = await harness.Service.GetOnboardingCatalogAsync();

        var dto = catalog.Plans.Single(item => item.Code == "GROWTH");
        dto.IsSelectable.ShouldBeFalse();
        dto.UnavailableReason.ShouldBe(SufiSaasConsts.PlanUnavailableReasons.PriceNotSet);
    }

    [Fact]
    public async Task Catalog_Should_Mark_A_Priced_Paid_Plan_As_HostUpgradeOnly()
    {
        var harness = NewService();
        var plan = NewPlan(
            "growth",
            Guid.NewGuid(),
            billingPeriodDays: 30,
            allowUnpaidUse: false,
            sortOrder: 20,
            isSelfServiceAvailable: true,
            price: 99m);
        harness.Plans.GetListAsync(Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(new List<Plan> { plan });

        var catalog = await harness.Service.GetOnboardingCatalogAsync();

        var dto = catalog.Plans.Single(item => item.Code == "GROWTH");
        dto.IsSelectable.ShouldBeFalse();
        dto.UnavailableReason.ShouldBe(SufiSaasConsts.PlanUnavailableReasons.HostUpgradeOnly);
    }

    [Fact]
    public async Task Catalog_Should_Keep_The_Unpriced_Trial_Selectable()
    {
        var harness = NewService();
        var plan = NewPlan(
            "demo",
            Guid.NewGuid(),
            billingPeriodDays: 7,
            allowUnpaidUse: true,
            sortOrder: 0,
            isSelfServiceAvailable: true,
            price: 0m);
        harness.Plans.GetListAsync(Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(new List<Plan> { plan });

        var catalog = await harness.Service.GetOnboardingCatalogAsync();

        var dto = catalog.Plans.Single(item => item.Code == "DEMO");
        dto.IsSelectable.ShouldBeTrue();
        dto.UnavailableReason.ShouldBeNull();
    }

    private static Plan NewPlan(
        string code,
        Guid editionId,
        int billingPeriodDays,
        bool allowUnpaidUse,
        int sortOrder,
        bool isSelfServiceAvailable,
        decimal price) =>
        new(
            Guid.NewGuid(),
            code,
            $"Plan:{code}",
            editionId,
            billingPeriodDays,
            allowUnpaidUse,
            sortOrder,
            isSelfServiceAvailable,
            price,
            "USD");

    private static Tenant NewTenant(Guid id, string subdomain)
    {
        var flags = System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic;
        var tenant = (Tenant)Activator.CreateInstance(
            typeof(Tenant),
            flags,
            binder: null,
            args: new object?[] { id, "Arian group", "ARIAN" },
            culture: null)!;
        typeof(Tenant).GetMethod("ConfigureRouting", flags)!
            .Invoke(tenant, new object?[] { subdomain, Array.Empty<TenantDomain>() });
        return tenant;
    }

    private static Harness NewService()
    {
        var requests = Substitute.For<ITenantRequestRepository>();
        requests.IsSubdomainInActiveRequestAsync(
            Arg.Any<string>(),
            Arg.Any<Guid?>(),
            Arg.Any<System.Threading.CancellationToken>())
            .Returns(false);
        var plans = Substitute.For<IPlanRepository>();
        var tenants = Substitute.For<ITenantRepository>();
        tenants.FindByPrimarySubdomainAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns((Tenant?)null);
        var normalizer = Substitute.For<ITenantNormalizer>();
        normalizer.NormalizeName(Arg.Any<string>()).Returns("arian");

        var featureManager = Substitute.For<IFeatureManager>();
        featureManager.GetAllAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(new List<FeatureNameValue>());
        var featureDefinitionManager = Substitute.For<IFeatureDefinitionManager>();
        featureDefinitionManager.GetAllAsync().Returns(new List<FeatureDefinition>());

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(Guid.NewGuid());
        var authorization = Substitute.For<IAbpAuthorizationService>();
        authorization.AuthorizeAsync(
                Arg.Any<System.Security.Claims.ClaimsPrincipal?>(),
                Arg.Any<object?>(),
                Arg.Any<string>())
            .Returns(AuthorizationResult.Success());
        authorization.AuthorizeAsync(
                Arg.Any<System.Security.Claims.ClaimsPrincipal?>(),
                Arg.Any<object?>(),
                Arg.Any<IEnumerable<IAuthorizationRequirement>>())
            .Returns(AuthorizationResult.Success());

        var services = new ServiceCollection();
        services.AddSingleton(currentUser);
        services.AddSingleton(Substitute.For<ICurrentTenant>());
        services.AddSingleton<IAuthorizationService>(authorization);
        services.AddSingleton(authorization);
        services.AddSingleton<Microsoft.Extensions.Options.IOptions<SufiSaasLicensingOptions>>(
            Microsoft.Extensions.Options.Options.Create(new SufiSaasLicensingOptions()));

        var service = new ExposedTenantRequestAppService(
            requests,
            plans,
            tenants,
            Substitute.For<IdentityLinkUserManager>(
                Substitute.For<IIdentityLinkUserRepository>(),
                null!,
                Substitute.For<ICurrentTenant>()),
            null!,
            Substitute.For<Volo.Abp.BackgroundJobs.IBackgroundJobManager>(),
            Substitute.For<IStringEncryptionService>(),
            Substitute.For<ITenantProvisioningNotificationService>(),
            Substitute.For<ITenantManager>(),
            new ConfigurationBuilder().Build(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            Options.Create(new IdentityOptions()),
            new TenantDatabaseNameGenerator(Options.Create(new SufiSaasTenantDatabaseOptions())),
            normalizer,
            featureManager,
            featureDefinitionManager,
            Substitute.For<ISubscriptionRepository>(),
            licenseIssuer: null!,
            Substitute.For<IStringLocalizerFactory>())
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider())
        };

        return new Harness(service, requests, tenants, plans);
    }

    private sealed class Harness
    {
        public Harness(
            ExposedTenantRequestAppService service,
            ITenantRequestRepository requests,
            ITenantRepository tenants,
            IPlanRepository plans)
        {
            Service = service;
            Requests = requests;
            Tenants = tenants;
            Plans = plans;
        }

        public ExposedTenantRequestAppService Service { get; }
        public ITenantRequestRepository Requests { get; }
        public ITenantRepository Tenants { get; }
        public IPlanRepository Plans { get; }
    }

    private sealed class ExposedTenantRequestAppService : TenantRequestAppService
    {
        public ExposedTenantRequestAppService(
            ITenantRequestRepository tenantRequestRepository,
            IPlanRepository planRepository,
            ITenantRepository tenantRepository,
            IdentityLinkUserManager identityLinkUserManager,
            IdentityUserManager userManager,
            Volo.Abp.BackgroundJobs.IBackgroundJobManager backgroundJobManager,
            IStringEncryptionService stringEncryptionService,
            ITenantProvisioningNotificationService notificationService,
            ITenantManager tenantManager,
            IConfiguration configuration,
            IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators,
            IOptions<IdentityOptions> identityOptions,
            TenantDatabaseNameGenerator databaseNameGenerator,
            ITenantNormalizer tenantNormalizer,
            IFeatureManager featureManager,
            IFeatureDefinitionManager featureDefinitionManager,
            ISubscriptionRepository subscriptionRepository,
            SelfHostedLicenseIssuer licenseIssuer,
            IStringLocalizerFactory stringLocalizerFactory)
            : base(
                tenantRequestRepository,
                planRepository,
                tenantRepository,
                identityLinkUserManager,
                userManager,
                backgroundJobManager,
                stringEncryptionService,
                notificationService,
                tenantManager,
                configuration,
                passwordValidators,
                identityOptions,
                databaseNameGenerator,
                tenantNormalizer,
                featureManager,
                featureDefinitionManager,
                subscriptionRepository,
                licenseIssuer,
                stringLocalizerFactory)
        {
        }

        public Task<TenantRequestDto> ExposeMapAsync(TenantRequest request) => MapAsync(request);
    }
}
