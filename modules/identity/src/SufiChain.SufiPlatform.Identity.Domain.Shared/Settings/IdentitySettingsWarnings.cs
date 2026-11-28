namespace SufiChain.SufiPlatform.Identity.Settings;

public static class IdentitySettingsWarnings
{
    public static bool OtpLoginDefaultsToUnavailableSms(
        bool otpEnabled,
        bool allowLogin,
        string? defaultChannel,
        bool smsAvailable) =>
        (otpEnabled || allowLogin)
        && string.Equals(defaultChannel, "Sms", StringComparison.OrdinalIgnoreCase)
        && !smsAvailable;

    public static bool RequireConfirmedEmailWithoutSmtp(bool requireConfirmedEmail, bool emailAvailable) =>
        requireConfirmedEmail && !emailAvailable;
}
