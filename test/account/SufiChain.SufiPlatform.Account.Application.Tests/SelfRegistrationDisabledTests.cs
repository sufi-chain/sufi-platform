using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Account.Blazor.Pages;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class SelfRegistrationDisabledTests
{
    [Fact]
    public async Task Register_api_rejects_before_creating_a_user_when_the_setting_is_off()
    {
        var settings = DisabledSettings();
        var service = new AccountAppService(
            null!,
            null!,
            Options.Create(new IdentityOptions()),
            null!,
            null!,
            null!,
            null!,
            settings,
            null!,
            null!);

        var exception = await Should.ThrowAsync<BusinessException>(() => service.RegisterAsync(new RegisterDto
        {
            UserName = "new-user",
            EmailAddress = "new-user@example.com",
            Password = "unused-password",
            AppName = "tests"
        }));

        exception.Code.ShouldBe(IdentitySecurityErrorCodes.SelfRegistrationDisabled);
    }

    [Fact]
    public async Task Otp_registration_rejects_before_sending_or_creating_a_user()
    {
        var settings = DisabledSettings();
        var service = new AccountOtpAppService(
            null!,
            null!,
            null!,
            settings,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        var send = await Should.ThrowAsync<BusinessException>(() =>
            service.SendRegistrationOtpAsync(new SendOtpInput { Identifier = "new-user@example.com", AppName = "tests" }));
        var verify = await Should.ThrowAsync<BusinessException>(() =>
            service.VerifyRegistrationOtpAsync(new VerifyOtpInput { Identifier = "new-user@example.com", Code = "000000" }));
        var register = await Should.ThrowAsync<BusinessException>(() =>
            service.RegisterWithOtpAsync(new RegisterWithOtpDto
            {
                UserName = "new-user",
                EmailAddress = "new-user@example.com",
                Password = "unused-password",
                RegistrationToken = "token",
                AppName = "tests"
            }));

        send.Code.ShouldBe(IdentitySecurityErrorCodes.SelfRegistrationDisabled);
        verify.Code.ShouldBe(IdentitySecurityErrorCodes.SelfRegistrationDisabled);
        register.Code.ShouldBe(IdentitySecurityErrorCodes.SelfRegistrationDisabled);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Otp_options_allow_registration_only_when_self_registration_is_on(bool selfRegistration)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        settings.GetOrNullAsync(IdentitySettingNames.Otp.IsEnabled).Returns("true");
        settings.GetOrNullAsync(IdentitySettingNames.Otp.AllowRegistration).Returns("true");
        settings.GetOrNullAsync(IdentitySettingNames.Registration.EnableSelfRegistration)
            .Returns(selfRegistration ? "true" : "false");

        var channels = Substitute.For<IVerificationChannelAvailabilityChecker>();
        channels.GetAvailableChannelsAsync()
            .Returns(new[] { VerificationDeliveryChannel.Email });
        var resolver = Substitute.For<IVerificationChannelResolver>();
        resolver.ResolveAsync(Arg.Any<VerificationPurpose>(), Arg.Any<VerificationDeliveryChannel?>())
            .Returns(VerificationDeliveryChannel.Email);

        var service = new AccountOtpAppService(
            null!,
            null!,
            null!,
            settings,
            null!,
            resolver,
            channels,
            null!,
            null!,
            null!,
            null!,
            null!);

        var options = await service.GetOtpOptionsAsync();

        options.IsEnabled.ShouldBeTrue();
        options.AllowRegistration.ShouldBe(selfRegistration);
    }

    [Fact]
    public void Login_query_error_localizes_a_known_identity_key()
    {
        var identity = new MapLocalizer(new Dictionary<string, string>
        {
            ["Sufi.Identity:SelfRegistrationDisabled"] = "New account registration is currently disabled.",
            ["LoginUnexpectedError"] = "Sign-in could not be completed. Try again.",
            ["Sufi.Identity:EmailConfirmationRequired"] = "Confirm your email before signing in."
        });
        var account = new MapLocalizer(new Dictionary<string, string>
        {
            ["ConfirmPhoneSessionInvalid"] = "The phone confirmation session is no longer valid."
        });

        LoginQueryErrors.Localize(IdentitySecurityErrorCodes.SelfRegistrationDisabled, identity, account)
            .ShouldBe("New account registration is currently disabled.");
        LoginQueryErrors.Localize("EmailConfirmationRequired", identity, account)
            .ShouldBe("Confirm your email before signing in.");
        LoginQueryErrors.Localize("ConfirmPhoneSessionInvalid", identity, account)
            .ShouldBe("The phone confirmation session is no longer valid.");
        LoginQueryErrors.Localize("not-a-known-key", identity, account)
            .ShouldBe("Sign-in could not be completed. Try again.");
    }

    [Theory]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData(null, false)]
    [InlineData("true", true)]
    public async Task Rule_follows_the_current_setting_provider_value(string? stored, bool enabled)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(IdentitySettingNames.Registration.EnableSelfRegistration).Returns(stored);

        (await IdentitySelfRegistrationRules.IsEnabledAsync(settings)).ShouldBe(enabled);
        IdentitySettingNames.Registration.EnableSelfRegistration.ShouldBe("Sufi.Identity.Registration.EnableSelfRegistration");
    }

    [Fact]
    public void Pages_read_the_same_setting_and_saas_signup_is_not_anonymous()
    {
        var platform = PlatformRoot();
        var chain = platform.Parent!;
        File.ReadAllText(Path.Combine(platform.FullName, "modules/account/src/SufiChain.SufiPlatform.Account.Blazor/Pages/Register.razor"))
            .ShouldContain("IsSelfRegistrationEnabled");
        var platformLogin = File.ReadAllText(Path.Combine(platform.FullName, "modules/account/src/SufiChain.SufiPlatform.Account.Blazor/Pages/Login.razor"));
        platformLogin.ShouldContain("private bool _selfRegistrationEnabled;");
        platformLogin.ShouldNotContain("_selfRegistrationEnabled = true");
        platformLogin.ShouldContain("LoginQueryErrors.Localize(ErrorFromQuery, L, AccountL)");
        platformLogin.ShouldNotContain("_ => ErrorFromQuery");
        File.ReadAllText(Path.Combine(chain.FullName, "hosts/SufiChane.SufiPlatform/src/SufiChane.SufiPlatform.Account.Blazor/Pages/Account/Register.razor"))
            .ShouldContain("IsSelfRegistrationEnabled");
        var hostLogin = File.ReadAllText(Path.Combine(chain.FullName, "hosts/SufiChane.SufiPlatform/src/SufiChane.SufiPlatform.Account.Blazor/Pages/Account/Login.razor"));
        hostLogin.ShouldContain("private bool _selfRegistrationEnabled;");
        hostLogin.ShouldNotContain("_selfRegistrationEnabled = true");
        var external = File.ReadAllText(Path.Combine(
            platform.FullName,
            "framework/SufiChain.SufiPlatform.AspNetCore.Authentication.Server/Controllers/SufiAccountController.cs"));
        var callback = external.IndexOf("ExternalLoginCallback", StringComparison.Ordinal);
        var disabledLogin = external.IndexOf("RegistrationDisabledLogin", callback, StringComparison.Ordinal);
        var registerRedirect = external.IndexOf("/account/register?isExternalLogin=true", callback, StringComparison.Ordinal);
        disabledLogin.ShouldBeGreaterThan(callback);
        disabledLogin.ShouldBeLessThan(registerRedirect);
        external.ShouldContain("IdentitySecurityErrorCodes.SelfRegistrationDisabled");
        external.ShouldNotContain("return Redirect(\"/account/register\")");
        var confirmation = external.IndexOf("ExternalLoginConfirmation", StringComparison.Ordinal);
        var create = external.IndexOf("_userManager.CreateAsync(user)", confirmation, StringComparison.Ordinal);
        external.LastIndexOf("IdentitySelfRegistrationRules.IsEnabledAsync", create, StringComparison.Ordinal)
            .ShouldBeGreaterThan(confirmation);

        var saas = File.ReadAllText(Path.Combine(
            chain.FullName,
            "commercial-modules/sufi-saas/src/SufiChain.SufiPlatform.SufiSaas.Application/TenantRequests/TenantRequestAppService.cs"));
        saas.ShouldContain("[Authorize]");
        saas.ShouldContain("[Authorize(SufiSaasPermissions.TenantRequests.Create)]");
        saas.ShouldNotContain("AllowAnonymous");
    }

    private static DirectoryInfo PlatformRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "modules", "identity")))
        {
            dir = dir.Parent;
        }

        dir.ShouldNotBeNull();
        return dir;
    }

    private static ISettingProvider DisabledSettings()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        settings.GetOrNullAsync(IdentitySettingNames.Registration.EnableSelfRegistration).Returns("false");
        return settings;
    }

    private sealed class MapLocalizer : IStringLocalizer
    {
        private readonly Dictionary<string, string> _values;

        public MapLocalizer(Dictionary<string, string> values)
        {
            _values = values;
        }

        public LocalizedString this[string name] =>
            _values.TryGetValue(name, out var value)
                ? new LocalizedString(name, value, resourceNotFound: false)
                : new LocalizedString(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments] => this[name];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _values.Select(pair => new LocalizedString(pair.Key, pair.Value, resourceNotFound: false));
    }
}
