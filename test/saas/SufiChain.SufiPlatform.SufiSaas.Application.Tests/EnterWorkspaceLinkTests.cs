using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.SufiSaas.Billing;
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
using Volo.Abp.Users;
using Xunit;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.SufiSaas;

public class EnterWorkspaceLinkTests
{
    [Fact]
    public async Task GetEnterWorkspaceLink_Should_Refuse_The_Requester_When_No_Link_Exists()
    {
        var requesterId = Guid.NewGuid();
        var harness = NewHarness(callerId: requesterId, callerIsRequester: true, linked: false);

        var error = await Should.ThrowAsync<BusinessException>(() =>
            harness.Service.GetEnterWorkspaceLinkAsync(harness.Request.Id));

        error.Code.ShouldBe(SufiSaasErrorCodes.EnterWorkspaceNotLinked);
        error.Data["Reason"].ShouldBe(LinkLoginRejectionReasons.NotLinked);
        harness.Links.TokenRequested.ShouldBeFalse();
    }

    [Fact]
    public async Task GetEnterWorkspaceLink_Should_Refuse_An_Approver_Who_Is_Not_Linked()
    {
        var harness = NewHarness(callerId: Guid.NewGuid(), callerIsRequester: false, linked: false);

        var error = await Should.ThrowAsync<BusinessException>(() =>
            harness.Service.GetEnterWorkspaceLinkAsync(harness.Request.Id));

        error.Code.ShouldBe(SufiSaasErrorCodes.EnterWorkspaceNotLinked);
        error.Data["Reason"].ShouldBe(LinkLoginRejectionReasons.NotLinked);
        harness.Links.TokenRequested.ShouldBeFalse();
        (await harness.Service.ExposeMapAsync(harness.Request)).CanEnterWorkspace.ShouldBeFalse();
    }

    [Fact]
    public async Task GetEnterWorkspaceLink_Should_Report_TenantMismatch_When_The_Caller_Is_Linked_Elsewhere()
    {
        var harness = NewHarness(callerId: Guid.NewGuid(), callerIsRequester: false, linked: false);
        await harness.Links.LinkAsync(
            new IdentityLinkUserInfo(harness.CallerId),
            new IdentityLinkUserInfo(Guid.NewGuid(), Guid.NewGuid()));

        var error = await Should.ThrowAsync<BusinessException>(() =>
            harness.Service.GetEnterWorkspaceLinkAsync(harness.Request.Id));

        error.Code.ShouldBe(SufiSaasErrorCodes.EnterWorkspaceNotLinked);
        error.Data["Reason"].ShouldBe(LinkLoginRejectionReasons.TenantMismatch);
        harness.Links.TokenRequested.ShouldBeFalse();
    }

    [Fact]
    public async Task GetEnterWorkspaceLink_Should_Issue_A_Token_For_The_Linked_Requester()
    {
        var requesterId = Guid.NewGuid();
        var harness = NewHarness(callerId: requesterId, callerIsRequester: true, linked: true);

        var link = await harness.Service.GetEnterWorkspaceLinkAsync(harness.Request.Id);

        link.SourceLinkToken.ShouldBe(RecordingLinkUserManager.IssuedToken);
        link.SourceUserId.ShouldBe(requesterId);
        link.TargetUserId.ShouldBe(harness.Request.ProvisionedAdminUserId!.Value);
        link.TargetTenantId.ShouldBe(harness.Request.ProvisionedTenantId);
        link.TenantBaseUrl.ShouldContain("arian.sufichain.com");
        harness.Links.TokenRequested.ShouldBeTrue();
        (await harness.Service.ExposeMapAsync(harness.Request)).CanEnterWorkspace.ShouldBeTrue();
    }

    [Fact]
    public async Task GetEnterWorkspaceLink_Should_Allow_An_Approver_Who_Already_Has_The_Link()
    {
        var harness = NewHarness(callerId: Guid.NewGuid(), callerIsRequester: false, linked: true);

        var link = await harness.Service.GetEnterWorkspaceLinkAsync(harness.Request.Id);

        link.SourceUserId.ShouldBe(harness.CallerId);
        link.SourceLinkToken.ShouldBe(RecordingLinkUserManager.IssuedToken);
        (await harness.Service.ExposeMapAsync(harness.Request)).CanEnterWorkspace.ShouldBeTrue();
    }

    private static Harness NewHarness(Guid callerId, bool callerIsRequester, bool linked)
    {
        var requesterId = callerIsRequester ? callerId : Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var request = new TenantRequest(Guid.NewGuid(), requesterId, "Arian group", Guid.NewGuid(), "arian");
        request.MarkProvisioned(tenantId, adminUserId, DateTime.UtcNow);

        var store = new List<IdentityLinkUser>();
        var repository = Substitute.For<IIdentityLinkUserRepository>();
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns((Guid?)null);
        currentTenant.Change(Arg.Any<Guid?>()).Returns(Substitute.For<IDisposable>());
        repository.FindAsync(
                Arg.Any<IdentityLinkUserInfo>(),
                Arg.Any<IdentityLinkUserInfo>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var source = call.ArgAt<IdentityLinkUserInfo>(0);
                var target = call.ArgAt<IdentityLinkUserInfo>(1);
                return store.Find(link =>
                    link.SourceUserId == source.UserId && link.SourceTenantId == source.TenantId &&
                    link.TargetUserId == target.UserId && link.TargetTenantId == target.TenantId ||
                    link.TargetUserId == source.UserId && link.TargetTenantId == source.TenantId &&
                    link.SourceUserId == target.UserId && link.SourceTenantId == target.TenantId);
            });
        repository.GetListAsync(
                Arg.Any<IdentityLinkUserInfo>(),
                Arg.Any<List<IdentityLinkUserInfo>?>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var info = call.ArgAt<IdentityLinkUserInfo>(0);
                return store.FindAll(link =>
                    link.SourceUserId == info.UserId && link.SourceTenantId == info.TenantId ||
                    link.TargetUserId == info.UserId && link.TargetTenantId == info.TenantId);
            });
        repository.InsertAsync(Arg.Any<IdentityLinkUser>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var link = call.Arg<IdentityLinkUser>();
                store.Add(link);
                return link;
            });

        var links = new RecordingLinkUserManager(repository, currentTenant);
        if (linked)
        {
            links.LinkAsync(
                new IdentityLinkUserInfo(callerId),
                new IdentityLinkUserInfo(adminUserId, tenantId)).GetAwaiter().GetResult();
        }

        var requests = Substitute.For<ITenantRequestRepository>();
        requests.GetAsync(request.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(request);
        var plans = Substitute.For<IPlanRepository>();
        plans.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns((Plan?)null);
        var tenants = Substitute.For<ITenantRepository>();
        tenants.GetAsync(tenantId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(NewTenant(tenantId, "arian"));
        var normalizer = Substitute.For<ITenantNormalizer>();
        normalizer.NormalizeName(Arg.Any<string>()).Returns("arian");

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(callerId);
        currentUser.IsAuthenticated.Returns(true);
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
        authorization.CurrentPrincipal.Returns(new ClaimsPrincipal(new ClaimsIdentity("test")));

        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(currentUser);
        services.AddSingleton(currentTenant);
        services.AddSingleton<IAuthorizationService>(authorization);
        services.AddSingleton(authorization);
        var service = new ExposedTenantRequestAppService(
            requests,
            plans,
            tenants,
            links,
            userManager: null!,
            Substitute.For<IBackgroundJobManager>(),
            Substitute.For<IStringEncryptionService>(),
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
            Substitute.For<ISubscriptionBillingGateway>(),
            licenseIssuer: null!,
            Substitute.For<IStringLocalizerFactory>())
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider())
        };

        return new Harness(service, request, links, callerId);
    }

    private static Tenant NewTenant(Guid id, string subdomain)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
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

    private sealed class Harness
    {
        public Harness(
            ExposedTenantRequestAppService service,
            TenantRequest request,
            RecordingLinkUserManager links,
            Guid callerId)
        {
            Service = service;
            Request = request;
            Links = links;
            CallerId = callerId;
        }

        public ExposedTenantRequestAppService Service { get; }
        public TenantRequest Request { get; }
        public RecordingLinkUserManager Links { get; }
        public Guid CallerId { get; }
    }

    private sealed class ExposedTenantRequestAppService : TenantRequestAppService
    {
        public ExposedTenantRequestAppService(
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
            ISubscriptionBillingGateway billingGateway,
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
                billingGateway,
                licenseIssuer,
                stringLocalizerFactory)
        {
        }

        public Task<TenantRequestDto> ExposeMapAsync(TenantRequest request) => MapAsync(request);
    }

    private sealed class RecordingLinkUserManager : IdentityLinkUserManager
    {
        public const string IssuedToken = "issued-link-token";

        public RecordingLinkUserManager(
            IIdentityLinkUserRepository repository,
            ICurrentTenant currentTenant)
            : base(repository, userManager: null!, currentTenant)
        {
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetService(Arg.Any<Volo.Abp.Guids.IGuidGenerator>())
                .Returns(call => call.ArgAt<Volo.Abp.Guids.IGuidGenerator>(0));
            LazyServiceProvider = lazy;
        }

        public bool TokenRequested { get; private set; }

        public override Task<string> GenerateLinkTokenAsync(
            IdentityLinkUserInfo targetLinkUser,
            string tokenPurpose,
            CancellationToken cancellationToken = default)
        {
            TokenRequested = true;
            return Task.FromResult(IssuedToken);
        }
    }
}
