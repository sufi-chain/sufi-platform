using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Identity.Integration;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Reads Identity password settings for cross-module consumers.
/// </summary>
public class IdentitySettingsIntegrationService : SufiApplicationService, IIdentitySettingsIntegrationService
{
    protected ISettingProvider SettingProvider { get; }

    public IdentitySettingsIntegrationService(ISettingProvider settingProvider)
    {
        SettingProvider = settingProvider;
    }

    public virtual async Task<IdentityPasswordRequirementsDto> GetPasswordRequirementsAsync()
    {
        return new IdentityPasswordRequirementsDto
        {
            RequiredLength = await SettingProvider.GetAsync(IdentitySettingNames.Password.RequiredLength, 6),
            RequireDigit = await SettingProvider.GetAsync(IdentitySettingNames.Password.RequireDigit, true),
            RequireLowercase = await SettingProvider.GetAsync(IdentitySettingNames.Password.RequireLowercase, true),
            RequireUppercase = await SettingProvider.GetAsync(IdentitySettingNames.Password.RequireUppercase, true),
            RequireNonAlphanumeric = await SettingProvider.GetAsync(IdentitySettingNames.Password.RequireNonAlphanumeric, true)
        };
    }
}
