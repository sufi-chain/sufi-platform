using System.Security.Claims;
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
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.SufiSaas;

public class TenantRequestApproveSubdomainTests
{
    [Fact]
    public async Task Approve_Succeeds_When_The_Subdomain_Is_Held_Only_By_The_Request_Itself()
    {
        var request = NewSubmittedRequest("arian");
        var harness = NewService();
        harness.Requests.GetAsync(request.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(request);
        harness.Requests.IsSubdomainInActiveRequestAsync(
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Guid?>() != request.Id);
        harness.Requests.UpdateAsync(Arg.Any<TenantRequest>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<TenantRequest>());

        var result = await harness.Service.ApproveAsync(request.Id, new ApproveTenantRequestDto
        {
            Subdomain = "arian",
            AdminPassword = "Approve-Test-Password-1"
        });

        result.Status.ShouldBe(TenantRequestStatus.Queued);
        result.ApprovedSubdomain.ShouldBe("arian");
    }

    [Fact]
    public async Task Approve_Throws_Taken_When_Another_Active_Request_Holds_It()
    {
        var request = NewSubmittedRequest("arian");
        var harness = NewService();
        harness.Requests.GetAsync(request.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(request);
        harness.Requests.IsSubdomainInActiveRequestAsync(
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var error = await Should.ThrowAsync<BusinessException>(() =>
            harness.Service.ApproveAsync(request.Id, new ApproveTenantRequestDto { Subdomain = "arian" }));

        error.Code.ShouldBe(SufiSaasErrorCodes.TenantSubdomainTaken);
        request.Status.ShouldBe(TenantRequestStatus.Submitted);
    }

    [Fact]
    public async Task Approve_Throws_Taken_When_An_Existing_Tenant_Owns_It()
    {
        var request = NewSubmittedRequest("arian");
        var harness = NewService();
        harness.Requests.GetAsync(request.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(request);
        harness.Requests.IsSubdomainInActiveRequestAsync(
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
        harness.Tenants.FindByPrimarySubdomainAsync("arian", false, Arg.Any<CancellationToken>())
            .Returns(NewTenant(Guid.NewGuid(), "arian"));

        var error = await Should.ThrowAsync<BusinessException>(() =>
            harness.Service.ApproveAsync(request.Id, new ApproveTenantRequestDto { Subdomain = "arian" }));

        error.Code.ShouldBe(SufiSaasErrorCodes.TenantSubdomainTaken);
        request.Status.ShouldBe(TenantRequestStatus.Submitted);
    }

    [Fact]
    public async Task Create_Still_Throws_Taken_For_An_Active_Request()
    {
        var harness = NewService();
        var plan = new Plan(
            Guid.NewGuid(),
            SufiSaasConsts.PlanCodes.Demo,
            "Demo",
            Guid.NewGuid(),
            billingPeriodDays: 7,
            allowUnpaidUse: true,
            isSelfServiceAvailable: true,
            price: 0m);
        harness.Plans.GetAsync(plan.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(plan);
        harness.Requests.IsSubdomainInActiveRequestAsync(
                "arian",
                null,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var error = await Should.ThrowAsync<BusinessException>(() =>
            harness.Service.CreateAsync(new CreateTenantRequestDto
            {
                TenantName = "Arian group",
                PlanId = plan.Id,
                PreferredSubdomain = "arian"
            }));

        error.Code.ShouldBe(SufiSaasErrorCodes.TenantSubdomainTaken);
        await harness.Requests.DidNotReceive().InsertAsync(
            Arg.Any<TenantRequest>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    private static TenantRequest NewSubmittedRequest(string subdomain) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Arian group", Guid.NewGuid(), subdomain);

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
                Arg.Any<CancellationToken>())
            .Returns(false);
        var plans = Substitute.For<IPlanRepository>();
        plans.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns((Plan?)null);
        var tenants = Substitute.For<ITenantRepository>();
        tenants.FindByPrimarySubdomainAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Tenant?)null);
        var normalizer = Substitute.For<ITenantNormalizer>();
        normalizer.NormalizeName(Arg.Any<string>()).Returns("arian");

        var encryption = Substitute.For<IStringEncryptionService>();
        encryption.Encrypt(Arg.Any<string>()).Returns("encrypted-secret");
        var jobs = Substitute.For<IBackgroundJobManager>();
        jobs.EnqueueAsync(
                Arg.Any<TenantProvisioningJobArgs>(),
                Arg.Any<BackgroundJobPriority>(),
                Arg.Any<TimeSpan?>())
            .Returns("job");

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(Guid.NewGuid());
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns((Guid?)null);
        var authorization = Substitute.For<IAbpAuthorizationService>();
        authorization.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal?>(),
                Arg.Any<object?>(),
                Arg.Any<string>())
            .Returns(AuthorizationResult.Success());
        authorization.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal?>(),
                Arg.Any<object?>(),
                Arg.Any<IEnumerable<IAuthorizationRequirement>>())
            .Returns(AuthorizationResult.Success());

        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 10, 14, 13, 40, 0, DateTimeKind.Utc));
        clock.Kind.Returns(DateTimeKind.Utc);

        var services = new ServiceCollection();
        services.AddSingleton(currentUser);
        services.AddSingleton(currentTenant);
        services.AddSingleton<IAuthorizationService>(authorization);
        services.AddSingleton(authorization);
        services.AddSingleton(clock);
        services.AddSingleton(Options.Create(new SufiSaasLicensingOptions()));

        var service = new ApproveExposedTenantRequestAppService(
            requests,
            plans,
            tenants,
            Substitute.For<IdentityLinkUserManager>(
                Substitute.For<IIdentityLinkUserRepository>(),
                null!,
                Substitute.For<ICurrentTenant>()),
            null!,
            jobs,
            encryption,
            Substitute.For<ITenantProvisioningNotificationService>(),
            Substitute.For<ITenantManager>(),
            new ConfigurationBuilder().Build(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            Options.Create(new IdentityOptions()),
            new TenantDatabaseNameGenerator(Options.Create(new SufiSaasTenantDatabaseOptions())),
            normalizer,
            Substitute.For<IFeatureManager>(),
            Substitute.For<IFeatureDefinitionManager>(),
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
            ApproveExposedTenantRequestAppService service,
            ITenantRequestRepository requests,
            ITenantRepository tenants,
            IPlanRepository plans)
        {
            Service = service;
            Requests = requests;
            Tenants = tenants;
            Plans = plans;
        }

        public ApproveExposedTenantRequestAppService Service { get; }
        public ITenantRequestRepository Requests { get; }
        public ITenantRepository Tenants { get; }
        public IPlanRepository Plans { get; }
    }

    private sealed class ApproveExposedTenantRequestAppService : TenantRequestAppService
    {
        public ApproveExposedTenantRequestAppService(
            ITenantRequestRepository tenantRequestRepository,
            IPlanRepository planRepository,
            ITenantRepository tenantRepository,
            IdentityLinkUserManager identityLinkUserManager,
            IdentityUserManager userManager,
            IBackgroundJobManager backgroundJobManager,
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

        protected override Task ValidateAdminPasswordAsync(TenantRequest request, string password) =>
            Task.CompletedTask;
    }
}
