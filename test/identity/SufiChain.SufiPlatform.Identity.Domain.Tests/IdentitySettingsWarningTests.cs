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
    public void IdentitySettings_RequireConfirmedEmail_WarnsWithUnconfirmedUserCount()
    {
        IdentitySettingsWarnings.RequireConfirmedEmailLocksOutUnconfirmed(true, 3).ShouldBeTrue();
        IdentitySettingsWarnings.RequireConfirmedEmailLocksOutUnconfirmed(true, 0).ShouldBeFalse();
        IdentitySettingsWarnings.RequireConfirmedEmailLocksOutUnconfirmed(false, 3).ShouldBeFalse();
    }

    [Fact]
    public void IdentitySettings_RequireConfirmedEmail_WarnsWhenCurrentAdminIsUnconfirmed()
    {
        IdentitySettingsWarnings.RequireConfirmedEmailLocksOutCurrentAdmin(true, true).ShouldBeTrue();
        IdentitySettingsWarnings.RequireConfirmedEmailLocksOutCurrentAdmin(true, false).ShouldBeFalse();
    }

    [Fact]
    public void IdentitySettings_RequireConfirmedEmailWithoutSmtp_StillShowsExistingWarning()
    {
        IdentitySettingsWarnings.RequireConfirmedEmailWithoutSmtp(true, emailAvailable: false).ShouldBeTrue();
        IdentitySettingsWarnings.RequireConfirmedEmailWithoutSmtp(true, emailAvailable: true).ShouldBeFalse();
    }
}
