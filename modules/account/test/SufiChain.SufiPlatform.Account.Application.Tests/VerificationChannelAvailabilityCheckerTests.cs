using NSubstitute;
using Shouldly;
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

    private static VerificationChannelAvailabilityChecker CreateChecker(string? smtpHost)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpHost).Returns(smtpHost);

        return new VerificationChannelAvailabilityChecker(
            new NullSmsSender(),
            new NullVoiceCallSender(),
            settings);
    }
}
