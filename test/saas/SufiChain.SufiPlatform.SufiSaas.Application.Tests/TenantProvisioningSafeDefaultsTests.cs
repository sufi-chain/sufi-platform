using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiSaas.Billing;
using SufiChain.SufiPlatform.SufiSaas.Plans;
using SufiChain.SufiPlatform.SufiSaas.Provisioning;
using SufiChain.SufiPlatform.SufiSaas.Subscriptions;
using SufiChain.SufiPlatform.SufiSaas.TenantRequests;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp.DistributedLocking;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.SufiSaas;

public class TenantProvisioningSafeDefaultsTests
{
    private static readonly string[] SecuritySettings =
    [
        IdentitySettingNames.SignIn.RequireConfirmedEmail,
        IdentitySettingNames.Registration.RequireConfirmedAccount,
        IdentitySettingNames.SignIn.RequireConfirmedPhoneNumber,
        IdentitySettingNames.TwoFactor.EnforceForAdministrators,
        IdentitySettingNames.TwoFactor.EnforceForNewUsers
    ];

    [Fact]
    public async Task ApplySafeSignInDefaults_Should_Write_False_When_No_Tenant_Override_Exists()
    {
        var tenantId = Guid.NewGuid();
        var settings = Substitute.For<ISettingManager>();
        settings.GetOrNullForTenantAsync(Arg.Any<string>(), tenantId, false)
            .Returns((string?)null);

        await NewJob(settings).ExposeApplySafeSignInDefaultsAsync(tenantId);

        foreach (var name in SecuritySettings)
        {
            await settings.Received(1).SetForTenantAsync(tenantId, name, "false");
        }
    }

    [Fact]
    public async Task ApplySafeSignInDefaults_Should_Not_Override_An_Existing_Tenant_Value()
    {
        var tenantId = Guid.NewGuid();
        var settings = Substitute.For<ISettingManager>();
        settings.GetOrNullForTenantAsync(IdentitySettingNames.SignIn.RequireConfirmedEmail, tenantId, false)
            .Returns("true");
        settings.GetOrNullForTenantAsync(
                Arg.Is<string>(name => name != IdentitySettingNames.SignIn.RequireConfirmedEmail),
                tenantId,
                false)
            .Returns((string?)null);

        await NewJob(settings).ExposeApplySafeSignInDefaultsAsync(tenantId);

        await settings.DidNotReceive().SetForTenantAsync(
            tenantId,
            IdentitySettingNames.SignIn.RequireConfirmedEmail,
            Arg.Any<string?>());
        await settings.Received(1).SetForTenantAsync(
            tenantId,
            IdentitySettingNames.Registration.RequireConfirmedAccount,
            "false");
    }

    [Fact]
    public void PrepareTenantAdminForFirstSignIn_Should_Confirm_Email_And_Force_A_Password_Change()
    {
        var tenantAdmin = new IdentityUser(
            Guid.NewGuid(),
            SufiSaasConsts.TenantAdminUserName,
            "admin@arian.tenant.local");

        NewJob(Substitute.For<ISettingManager>())
            .ExposePrepareTenantAdminForFirstSignIn(tenantAdmin);

        tenantAdmin.EmailConfirmed.ShouldBeTrue();
        tenantAdmin.ShouldChangePasswordOnNextLogin.ShouldBeTrue();
    }

    private static ExposedTenantProvisioningJob NewJob(ISettingManager settings)
    {
        return new ExposedTenantProvisioningJob(
            Substitute.For<ITenantRequestRepository>(),
            Substitute.For<IPlanRepository>(),
            Substitute.For<ISubscriptionRepository>(),
            Substitute.For<ITenantRepository>(),
            Substitute.For<ITenantManager>(),
            Substitute.For<ITenantDatabaseProvisioner>(),
            new TenantDatabaseNameGenerator(Options.Create(new SufiSaasTenantDatabaseOptions())),
            Substitute.For<IIdentityDataSeeder>(),
            Substitute.For<IdentityLinkUserManager>(
                Substitute.For<IIdentityLinkUserRepository>(),
                null!,
                Substitute.For<ICurrentTenant>()),
            null!,
            Array.Empty<Microsoft.AspNetCore.Identity.IPasswordValidator<IdentityUser>>(),
            Options.Create(new Microsoft.AspNetCore.Identity.IdentityOptions()),
            Substitute.For<ISubscriptionBillingGateway>(),
            Substitute.For<ITenantProvisioningNotificationService>(),
            Substitute.For<IStringEncryptionService>(),
            Substitute.For<IAbpDistributedLock>(),
            Substitute.For<ICurrentTenant>(),
            Substitute.For<IClock>(),
            new ConfigurationBuilder().Build(),
            NullLogger<TenantProvisioningJob>.Instance,
            settings,
            Substitute.For<IUnitOfWorkManager>());
    }

    private sealed class ExposedTenantProvisioningJob : TenantProvisioningJob
    {
        public ExposedTenantProvisioningJob(
            ITenantRequestRepository tenantRequestRepository,
            IPlanRepository planRepository,
            ISubscriptionRepository subscriptionRepository,
            ITenantRepository tenantRepository,
            ITenantManager tenantManager,
            ITenantDatabaseProvisioner databaseProvisioner,
            TenantDatabaseNameGenerator databaseNameGenerator,
            IIdentityDataSeeder identityDataSeeder,
            IdentityLinkUserManager identityLinkUserManager,
            IdentityUserManager userManager,
            IEnumerable<Microsoft.AspNetCore.Identity.IPasswordValidator<IdentityUser>> passwordValidators,
            IOptions<Microsoft.AspNetCore.Identity.IdentityOptions> identityOptions,
            ISubscriptionBillingGateway billingGateway,
            ITenantProvisioningNotificationService notificationService,
            IStringEncryptionService stringEncryptionService,
            IAbpDistributedLock distributedLock,
            ICurrentTenant currentTenant,
            IClock clock,
            IConfiguration configuration,
            Microsoft.Extensions.Logging.ILogger<TenantProvisioningJob> logger,
            ISettingManager settingManager,
            IUnitOfWorkManager unitOfWorkManager)
            : base(
                tenantRequestRepository,
                planRepository,
                subscriptionRepository,
                tenantRepository,
                tenantManager,
                databaseProvisioner,
                databaseNameGenerator,
                identityDataSeeder,
                identityLinkUserManager,
                userManager,
                passwordValidators,
                identityOptions,
                billingGateway,
                notificationService,
                stringEncryptionService,
                distributedLock,
                currentTenant,
                clock,
                configuration,
                logger,
                settingManager,
                unitOfWorkManager)
        {
        }

        public Task ExposeApplySafeSignInDefaultsAsync(Guid tenantId) =>
            ApplySafeSignInDefaultsAsync(tenantId);

        public void ExposePrepareTenantAdminForFirstSignIn(IdentityUser tenantAdmin) =>
            PrepareTenantAdminForFirstSignIn(tenantAdmin);

        public void ExposeSetUnifiedTenantConnectionStrings(Tenant tenant) =>
            SetUnifiedTenantConnectionStrings(tenant);

        public Task<Tenant> ExposeEnsureTenantAsync(TenantRequest request, Guid editionId) =>
            EnsureTenantAsync(request, editionId);

        public Task ExposePersistFailureAsync(Guid requestId, string error) =>
            PersistFailureAsync(requestId, error);
    }

    [Fact]
    public void SetUnifiedTenantConnectionStrings_Should_Not_Write_Per_Module_Rows()
    {
        var tenant = CreateTenant(Guid.NewGuid(), "touristpanel", "TOURISTPANEL");
        tenant.SetDefaultConnectionString("Host=tenant-default");

        NewJob(Substitute.For<ISettingManager>())
            .ExposeSetUnifiedTenantConnectionStrings(tenant);

        tenant.FindDefaultConnectionString().ShouldBe("Host=tenant-default");
        tenant.FindConnectionString("SufiHelpDesk").ShouldBeNull();
        tenant.FindConnectionString("SufiHelpDeskKnowledgeBase").ShouldBeNull();
        tenant.FindConnectionString("SufiHelpDeskTicketing").ShouldBeNull();
        tenant.FindConnectionString("SufiHelpDeskLiveChat").ShouldBeNull();
        tenant.FindConnectionString("SufiAI").ShouldBeNull();
        tenant.ConnectionStrings.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Retry_adopts_an_existing_tenant_instead_of_creating_another()
    {
        var tenants = Substitute.For<ITenantRepository>();
        var manager = Substitute.For<ITenantManager>();
        var databases = Substitute.For<ITenantDatabaseProvisioner>();
        databases.CreateDatabaseAsync(Arg.Any<string>()).Returns("Server=127.0.0.1;Database=adopted");
        var existing = CreateTenant(Guid.NewGuid(), "acme", "ACME");
        existing.SetDatabaseName("SufiClient_acme");
        typeof(Tenant).GetProperty(nameof(Tenant.PrimarySubdomain))!
            .SetValue(existing, "acme");
        tenants.FindByPrimarySubdomainAsync("acme").Returns(existing);

        var request = new TenantRequest(Guid.NewGuid(), Guid.NewGuid(), "acme", Guid.NewGuid(), "acme");
        request.SetApprovedRouting("acme", null);
        var job = new ExposedTenantProvisioningJob(
            Substitute.For<ITenantRequestRepository>(),
            Substitute.For<IPlanRepository>(),
            Substitute.For<ISubscriptionRepository>(),
            tenants,
            manager,
            databases,
            new TenantDatabaseNameGenerator(Options.Create(new SufiSaasTenantDatabaseOptions())),
            Substitute.For<IIdentityDataSeeder>(),
            Substitute.For<IdentityLinkUserManager>(
                Substitute.For<IIdentityLinkUserRepository>(),
                null!,
                Substitute.For<ICurrentTenant>()),
            null!,
            Array.Empty<Microsoft.AspNetCore.Identity.IPasswordValidator<IdentityUser>>(),
            Options.Create(new Microsoft.AspNetCore.Identity.IdentityOptions()),
            Substitute.For<ISubscriptionBillingGateway>(),
            Substitute.For<ITenantProvisioningNotificationService>(),
            Substitute.For<IStringEncryptionService>(),
            Substitute.For<IAbpDistributedLock>(),
            Substitute.For<ICurrentTenant>(),
            Substitute.For<IClock>(),
            new ConfigurationBuilder().Build(),
            NullLogger<TenantProvisioningJob>.Instance,
            Substitute.For<ISettingManager>(),
            Substitute.For<IUnitOfWorkManager>());

        var adopted = await job.ExposeEnsureTenantAsync(request, Guid.NewGuid());

        adopted.Id.ShouldBe(existing.Id);
        request.ProvisionedTenantId.ShouldBe(existing.Id);
        request.DatabaseName.ShouldBe("SufiClient_acme");
        await manager.DidNotReceive().CreateAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Failure_is_stored_in_a_new_unit_of_work()
    {
        var request = new TenantRequest(Guid.NewGuid(), Guid.NewGuid(), "acme", Guid.NewGuid(), "acme");
        var requests = Substitute.For<ITenantRequestRepository>();
        requests.GetAsync(request.Id, Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(request);
        var uow = Substitute.For<IUnitOfWork>();
        var uows = Substitute.For<IUnitOfWorkManager>();
        uows.Begin(Arg.Any<AbpUnitOfWorkOptions>(), true).Returns(uow);
        var clock = Substitute.For<IClock>();
        var failedAt = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);
        clock.Now.Returns(failedAt);

        var job = new ExposedTenantProvisioningJob(
            requests,
            Substitute.For<IPlanRepository>(),
            Substitute.For<ISubscriptionRepository>(),
            Substitute.For<ITenantRepository>(),
            Substitute.For<ITenantManager>(),
            Substitute.For<ITenantDatabaseProvisioner>(),
            new TenantDatabaseNameGenerator(Options.Create(new SufiSaasTenantDatabaseOptions())),
            Substitute.For<IIdentityDataSeeder>(),
            Substitute.For<IdentityLinkUserManager>(
                Substitute.For<IIdentityLinkUserRepository>(),
                null!,
                Substitute.For<ICurrentTenant>()),
            null!,
            Array.Empty<Microsoft.AspNetCore.Identity.IPasswordValidator<IdentityUser>>(),
            Options.Create(new Microsoft.AspNetCore.Identity.IdentityOptions()),
            Substitute.For<ISubscriptionBillingGateway>(),
            Substitute.For<ITenantProvisioningNotificationService>(),
            Substitute.For<IStringEncryptionService>(),
            Substitute.For<IAbpDistributedLock>(),
            Substitute.For<ICurrentTenant>(),
            clock,
            new ConfigurationBuilder().Build(),
            NullLogger<TenantProvisioningJob>.Instance,
            Substitute.For<ISettingManager>(),
            uows);

        await job.ExposePersistFailureAsync(request.Id, "Password=***");

        request.Status.ShouldBe(TenantRequestStatus.Failed);
        request.LastError.ShouldBe("Password=***");
        request.LastFailedAt.ShouldBe(failedAt);
        await uow.Received(1).CompleteAsync(Arg.Any<System.Threading.CancellationToken>());
    }

    private static Tenant CreateTenant(Guid id, string name, string normalizedName)
    {
        var ctor = typeof(Tenant).GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            types: [typeof(Guid), typeof(string), typeof(string)],
            modifiers: null);
        ctor.ShouldNotBeNull();
        return (Tenant)ctor.Invoke([id, name, normalizedName]);
    }
}
