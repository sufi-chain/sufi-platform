using Shouldly;
using SufiChain.SufiPlatform.Account.Otp;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class RegisterOtpStartupTests
{
    [Fact]
    public async Task RegisterWithOtp_OptionsThrow_ShowsUnavailable()
    {
        var result = await RegisterOtpStartup.LoadAsync(
            () => Task.FromResult(true),
            () => throw new InvalidOperationException("otp"),
            _ => { });

        result.Failed.ShouldBeTrue();
        result.Options.ShouldBeNull();
        RegisterOtpStartup.ShowsForm(result).ShouldBeFalse();
    }

    [Fact]
    public async Task RegisterWithOtp_OptionsNull_ShowsUnavailable()
    {
        var result = await RegisterOtpStartup.LoadAsync(
            () => Task.FromResult(true),
            () => Task.FromResult<OtpOptionsDto?>(null),
            _ => { });

        result.Failed.ShouldBeFalse();
        RegisterOtpStartup.ShowsForm(result).ShouldBeFalse();
    }

    [Fact]
    public async Task RegisterWithOtp_Enabled_ShowsForm()
    {
        var result = await RegisterOtpStartup.LoadAsync(
            () => Task.FromResult(true),
            () => Task.FromResult<OtpOptionsDto?>(new OtpOptionsDto
            {
                IsEnabled = true,
                AllowRegistration = true,
                DefaultChannel = VerificationDeliveryChannel.Email
            }),
            _ => throw new InvalidOperationException("should not warn"));

        RegisterOtpStartup.ShowsForm(result).ShouldBeTrue();
    }
}
