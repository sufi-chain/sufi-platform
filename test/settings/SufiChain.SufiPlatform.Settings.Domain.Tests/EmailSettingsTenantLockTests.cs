using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Smtp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Settings;

public class EmailSettingsTenantLockTests
{
    [Fact]
    public async Task Tenant_load_returns_no_secrets_and_does_not_read_settings()
    {
        var provider = Substitute.For<ISettingProvider>();
        var manager = Substitute.For<ISettingManager>();
        var service = Create(provider, manager, tenant: true, allowEmailChanges: false);

        var result = await service.GetAsync();

        result.LockedByHost.ShouldBeTrue();
        result.SmtpPassword.ShouldBeNull();
        result.SmtpHost.ShouldBeNull();
        result.SmtpUserName.ShouldBeNull();
        await provider.DidNotReceive().GetAllAsync(Arg.Any<string[]>());
        await manager.DidNotReceive().SetForTenantOrGlobalAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task Tenant_cannot_save_email_settings()
    {
        var manager = Substitute.For<ISettingManager>();
        var service = Create(Substitute.For<ISettingProvider>(), manager, tenant: true, allowEmailChanges: false);

        await Should.ThrowAsync<AbpAuthorizationException>(() => service.UpdateAsync(new UpdateEmailSettingsDto
        {
            SmtpHost = "smtp.secret",
            SmtpPassword = "secret",
            SmtpPort = 587
        }));

        await manager.DidNotReceive().SetForTenantOrGlobalAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task Host_reads_one_batch_without_the_password_and_can_save()
    {
        var provider = Substitute.For<ISettingProvider>();
        string[]? requested = null;
        provider.GetAllAsync(Arg.Do<string[]>(names => requested = names))
            .Returns(call =>
            {
                var names = call.Arg<string[]>();
                return names.Select(name => new SettingValue(
                    name,
                    name == SufiComSenderSettingNames.Email.SmtpHost ? "smtp.example" :
                    name == SufiComSenderSettingNames.Email.SmtpPort ? "587" : "")).ToList();
            });
        var manager = Substitute.For<ISettingManager>();
        var service = Create(provider, manager, tenant: false, allowEmailChanges: false);

        var result = await service.GetAsync();

        result.LockedByHost.ShouldBeFalse();
        result.SmtpHost.ShouldBe("smtp.example");
        result.SmtpPort.ShouldBe(587);
        result.SmtpPassword.ShouldBeNull();
        requested.ShouldNotBeNull();
        requested.ShouldNotContain(SufiComSenderSettingNames.Email.SmtpPassword);

        await service.UpdateAsync(new UpdateEmailSettingsDto
        {
            SmtpHost = "smtp.example",
            SmtpPort = 587,
            SmtpPassword = "kept-on-the-server"
        });

        await manager.Received().SetForTenantOrGlobalAsync(null, SufiComSenderSettingNames.Email.SmtpHost, "smtp.example");
        await manager.Received().SetForTenantOrGlobalAsync(null, SufiComSenderSettingNames.Email.SmtpPassword, "kept-on-the-server");
    }

    private static EmailSettingsAppService Create(
        ISettingProvider provider,
        ISettingManager manager,
        bool tenant,
        bool allowEmailChanges)
    {
        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(SettingsFeatures.Enable).Returns(true);
        features.IsEnabledAsync(SettingsFeatures.AllowChangingEmailSettings).Returns(allowEmailChanges);
        var current = Substitute.For<ICurrentTenant>();
        current.IsAvailable.Returns(tenant);
        current.Id.Returns(tenant ? Guid.NewGuid() : null);

        var services = new ServiceCollection();
        services.AddSingleton<ISettingProvider>(provider);
        services.AddSingleton<IFeatureChecker>(features);
        services.AddSingleton<ICurrentTenant>(current);
        var service = new EmailSettingsAppService(manager, Substitute.For<ISmtpEmailSender>());
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }
}
