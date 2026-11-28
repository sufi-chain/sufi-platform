using Shouldly;
using SufiChain.SufiPlatform.Identity.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Identity;

public class IdentitySettingsWarningTests
{
    [Fact]
    public void IdentitySettings_OtpLoginDefaultSms_WhenSmsSendingOff_ShowsWarning()
    {
        IdentitySettingsWarnings.OtpLoginDefaultsToUnavailableSms(true, true, "Sms", smsAvailable: false).ShouldBeTrue();
    }

    [Fact]
    public void IdentitySettings_OtpLoginDefaultSms_WhenSmsConfigured_NoWarning()
    {
        IdentitySettingsWarnings.OtpLoginDefaultsToUnavailableSms(true, true, "Sms", smsAvailable: true).ShouldBeFalse();
    }

    [Fact]
    public void IdentitySettings_RequireConfirmedEmailWithoutSmtp_StillShowsExistingWarning()
    {
        IdentitySettingsWarnings.RequireConfirmedEmailWithoutSmtp(true, emailAvailable: false).ShouldBeTrue();
        IdentitySettingsWarnings.RequireConfirmedEmailWithoutSmtp(true, emailAvailable: true).ShouldBeFalse();
    }
}
