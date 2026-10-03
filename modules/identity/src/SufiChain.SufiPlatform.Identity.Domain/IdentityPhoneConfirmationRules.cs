using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Phone confirmation is a registration step. It takes effect only when SufiCom has an enabled SMS provider.
/// The stored identity setting does not block password sign-in by itself.
/// </summary>
public static class IdentityPhoneConfirmationRules
{
    public const string SmsChannelEnabledSetting = "SufiCom.Channel.Sms.Enabled";

    public const string SmsProviderCodeSetting = "SufiCom.Channel.Sms.ProviderCode";

    public static async Task<bool> IsSmsProviderConfiguredAsync(ISettingProvider settingProvider)
    {
        if (!await settingProvider.IsTrueAsync(SmsChannelEnabledSetting))
        {
            return false;
        }

        var providerCode = await settingProvider.GetOrNullAsync(SmsProviderCodeSetting);
        return !string.IsNullOrWhiteSpace(providerCode);
    }

    public static async Task<bool> IsRequiredForRegistrationAsync(ISettingProvider settingProvider)
    {
        return await settingProvider.IsTrueAsync(IdentitySettingNames.SignIn.RequireConfirmedPhoneNumber)
               && await IsSmsProviderConfiguredAsync(settingProvider);
    }
}
