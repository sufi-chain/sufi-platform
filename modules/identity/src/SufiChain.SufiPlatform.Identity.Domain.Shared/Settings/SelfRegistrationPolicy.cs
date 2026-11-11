using System;
using System.Threading.Tasks;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Identity.Settings;

/// <summary>
/// Self-registration is on only when the Sufi identity setting and ABP's
/// <c>Abp.Account.IsSelfRegistrationEnabled</c> are not false.
/// A missing ABP value follows the Sufi setting, which defaults to enabled.
/// </summary>
public static class SelfRegistrationPolicy
{
    public const string AbpSettingName = "Abp.Account.IsSelfRegistrationEnabled";

    public static bool IsEnabled(string? sufiValue, string? abpValue)
    {
        var sufiEnabled = !IsFalse(sufiValue);
        if (abpValue is null)
        {
            return sufiEnabled;
        }

        return sufiEnabled && !IsFalse(abpValue);
    }

    public static async Task<bool> IsEnabledAsync(ISettingProvider settingProvider)
    {
        var sufi = await settingProvider.GetOrNullAsync(IdentitySettingNames.Registration.EnableSelfRegistration);
        var abp = await settingProvider.GetOrNullAsync(AbpSettingName);
        return IsEnabled(sufi, abp);
    }

    private static bool IsFalse(string? value) =>
        string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) || value == "0";
}
