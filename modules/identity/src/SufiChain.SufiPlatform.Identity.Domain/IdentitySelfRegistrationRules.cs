using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Self-registration follows ABP's Account setting
/// <see cref="IdentitySettingNames.Registration.EnableSelfRegistration"/>
/// (the persisted value of IsSelfRegistrationEnabled). There is no second flag.
/// <see cref="ISettingProvider"/> applies the current tenant value, then the host value.
/// </summary>
public static class IdentitySelfRegistrationRules
{
    public static Task<bool> IsEnabledAsync(ISettingProvider settingProvider)
    {
        return settingProvider.IsTrueAsync(IdentitySettingNames.Registration.EnableSelfRegistration);
    }

    public static async Task EnsureEnabledAsync(ISettingProvider settingProvider)
    {
        if (!await IsEnabledAsync(settingProvider))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.SelfRegistrationDisabled);
        }
    }
}
