using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Smtp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Validation;
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

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("qa@")]
    public async Task Host_rejects_a_malformed_from_address(string address)
    {
        var manager = Substitute.For<ISettingManager>();
        var service = Create(Substitute.For<ISettingProvider>(), manager, tenant: false, allowEmailChanges: false);

        var exception = await Should.ThrowAsync<AbpValidationException>(() => service.UpdateAsync(new UpdateEmailSettingsDto
        {
            SmtpHost = "smtp.example",
            SmtpPort = 25,
            DefaultFromAddress = address
        }));

        exception.ValidationErrors.ShouldContain(error =>
            error.MemberNames.Contains(nameof(UpdateEmailSettingsDto.DefaultFromAddress))
            && error.ErrorMessage == "DefaultFromAddress:Invalid");
        await manager.DidNotReceive().SetForTenantOrGlobalAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string?>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("noreply@example.com")]
    public async Task Host_saves_an_empty_or_real_from_address(string? address)
    {
        var manager = Substitute.For<ISettingManager>();
        var service = Create(Substitute.For<ISettingProvider>(), manager, tenant: false, allowEmailChanges: false);

        await service.UpdateAsync(new UpdateEmailSettingsDto
        {
            SmtpHost = "smtp.example",
            SmtpPort = 25,
            DefaultFromAddress = address
        });

        await manager.Received().SetForTenantOrGlobalAsync(
            null,
            SufiComSenderSettingNames.Email.DefaultFromAddress,
            address);
    }

    [Fact]
    public async Task Host_rejects_an_empty_smtp_host_and_a_port_outside_1_to_65535()
    {
        var manager = Substitute.For<ISettingManager>();
        var service = Create(Substitute.For<ISettingProvider>(), manager, tenant: false, allowEmailChanges: false);

        var exception = await Should.ThrowAsync<AbpValidationException>(() => service.UpdateAsync(new UpdateEmailSettingsDto
        {
            SmtpHost = "  ",
            SmtpPort = 0
        }));

        exception.ValidationErrors.ShouldContain(error =>
            error.MemberNames.Contains(nameof(UpdateEmailSettingsDto.SmtpHost))
            && error.ErrorMessage == "SmtpHost:Required");
        exception.ValidationErrors.ShouldContain(error =>
            error.MemberNames.Contains(nameof(UpdateEmailSettingsDto.SmtpPort))
            && error.ErrorMessage == "SmtpPort:Invalid");
        await manager.DidNotReceive().SetForTenantOrGlobalAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task Host_load_shows_the_default_port_when_none_is_stored()
    {
        var provider = Substitute.For<ISettingProvider>();
        provider.GetAllAsync(Arg.Any<string[]>()).Returns(call =>
            call.Arg<string[]>().Select(name => new SettingValue(name, "")).ToList());
        var service = Create(provider, Substitute.For<ISettingManager>(), tenant: false, allowEmailChanges: false);

        var result = await service.GetAsync();

        result.SmtpPort.ShouldBe(EmailSettingsRules.DefaultSmtpPort);
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
        services.AddSingleton<IStringLocalizerFactory, KeyLocalizerFactory>();
        var service = new EmailSettingsAppService(manager, Substitute.For<ISmtpEmailSender>());
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private sealed class KeyLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => new KeyLocalizer();

        public IStringLocalizer Create(string baseName, string location) => new KeyLocalizer();
    }

    private sealed class KeyLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name, resourceNotFound: false);

        public LocalizedString this[string name, params object[] arguments] => new(name, name, resourceNotFound: false);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
