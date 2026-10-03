using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Sign-in email confirmation is one rule: <see cref="IdentitySettingNames.SignIn.RequireConfirmedEmail"/>.
/// A previously stored <see cref="IdentitySettingNames.Registration.RequireConfirmedAccount"/> value
/// still counts until settings are saved and that legacy flag is cleared.
/// </summary>
public static class IdentityEmailConfirmationRules
{
    public static async Task<bool> IsSignInBlockedUntilEmailConfirmedAsync(ISettingProvider settingProvider)
    {
        return await settingProvider.IsTrueAsync(IdentitySettingNames.SignIn.RequireConfirmedEmail)
               || await settingProvider.IsTrueAsync(IdentitySettingNames.Registration.RequireConfirmedAccount);
    }

    public static async Task<bool> ShouldSendRegistrationConfirmationEmailAsync(ISettingProvider settingProvider)
    {
        return await settingProvider.IsTrueAsync(IdentitySettingNames.Registration.RequireEmailConfirmation)
               || await IsSignInBlockedUntilEmailConfirmedAsync(settingProvider);
    }
}
