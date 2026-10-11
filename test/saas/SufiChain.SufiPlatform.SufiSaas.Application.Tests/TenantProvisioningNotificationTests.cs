using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiCom.Notifications;
using SufiChain.SufiPlatform.SufiSaas.Localization;
using SufiChain.SufiPlatform.SufiSaas.Provisioning;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public class TenantProvisioningNotificationTests
{
    [Fact]
    public async Task NotifyCompleted_Should_Not_Expose_Any_Credential_Secret()
    {
        var publisher = Substitute.For<ISensitiveInAppNotificationPublisher>();
        SensitiveInAppNotificationRequest? captured = null;
        publisher.PublishAsync(Arg.Any<SensitiveInAppNotificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                captured = call.Arg<SensitiveInAppNotificationRequest>();
                return Task.FromResult(true);
            });

        var cultureResolver = new TenantProvisioningNotificationCultureResolver(
            Substitute.For<ISettingManager>(),
            Options.Create(new AbpLocalizationOptions()),
            Substitute.For<ICurrentTenant>());
        var localizer = Substitute.For<IStringLocalizer<SufiSaasResource>>();

        var service = new SufiComTenantProvisioningNotificationService(
            publisher,
            cultureResolver,
            localizer);

        await service.NotifyCompletedAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Arian group",
            "arian.sufichain.com",
            SufiSaasConsts.TenantAdminUserName,
            "https://arian.sufichain.com/account/login");

        captured.ShouldNotBeNull();
        captured!.TemplateData.Keys
            .Select(key => key.ToLowerInvariant())
            .ShouldNotContain("password");
        captured.TemplateData.Values
            .Select(value => value?.ToString() ?? string.Empty)
            .ShouldNotContain(value => value.Contains("P@ssw0rd", StringComparison.Ordinal));
    }
}
