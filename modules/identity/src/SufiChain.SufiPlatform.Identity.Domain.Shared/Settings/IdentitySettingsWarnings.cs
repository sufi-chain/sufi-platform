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

    public static bool RequireConfirmedEmailLocksOutUnconfirmed(bool requireConfirmedEmail, int unconfirmedCount) =>
        requireConfirmedEmail && unconfirmedCount > 0;

    public static bool RequireConfirmedEmailLocksOutCurrentAdmin(bool requireConfirmedEmail, bool currentAdminUnconfirmed) =>
        requireConfirmedEmail && currentAdminUnconfirmed;
}

public interface IUnconfirmedAccountLookup
{
    Task<UnconfirmedAccountNotice> GetAsync();
}

public sealed record UnconfirmedAccountNotice(int Count, bool CurrentUserUnconfirmed);
