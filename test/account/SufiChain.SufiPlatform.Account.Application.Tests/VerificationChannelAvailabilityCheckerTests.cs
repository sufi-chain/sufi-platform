using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Sms;
using SufiChain.SufiPlatform.SufiCom.VoiceCall;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class VerificationChannelAvailabilityCheckerTests
{
    [Fact]
    public async Task Email_Is_Unavailable_When_Platform_Smtp_Host_Is_Empty()
    {
        var checker = CreateChecker(smtpHost: null);

        var channels = await checker.GetAvailableChannelsAsync();

        channels.ShouldNotContain(VerificationDeliveryChannel.Email);
    }

    [Fact]
    public async Task Email_Is_Available_When_Platform_Smtp_Host_Is_Configured()
    {
        var checker = CreateChecker(smtpHost: "smtp.example.com");

        var channels = await checker.GetAvailableChannelsAsync();

        channels.ShouldContain(VerificationDeliveryChannel.Email);
    }

    [Fact]
    public async Task Sms_Is_Unavailable_When_The_SufiCom_Provider_Is_Not_Configured()
    {
        var checker = CreateChecker(smtpHost: null, smsSender: Substitute.For<ISmsSender>());

        var channels = await checker.GetAvailableChannelsAsync();

        channels.ShouldNotContain(VerificationDeliveryChannel.Sms);
    }

    [Fact]
    public async Task Sms_Is_Available_When_The_SufiCom_Provider_Is_Enabled()
    {
        var checker = CreateChecker(
            smtpHost: null,
            smsSender: Substitute.For<ISmsSender>(),
            smsEnabled: true,
            smsProviderCode: "kavenegar");

        var channels = await checker.GetAvailableChannelsAsync();

        channels.ShouldContain(VerificationDeliveryChannel.Sms);
    }

    private static VerificationChannelAvailabilityChecker CreateChecker(
        string? smtpHost,
        ISmsSender? smsSender = null,
        bool smsEnabled = false,
        string? smsProviderCode = null)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpHost).Returns(smtpHost);
        settings.GetOrNullAsync(IdentityPhoneConfirmationRules.SmsChannelEnabledSetting)
            .Returns(smsEnabled ? "true" : "false");
        settings.GetOrNullAsync(IdentityPhoneConfirmationRules.SmsProviderCodeSetting)
            .Returns(smsProviderCode);

        return new VerificationChannelAvailabilityChecker(
            smsSender ?? new NullSmsSender(),
            new NullVoiceCallSender(),
            settings);
    }
}
