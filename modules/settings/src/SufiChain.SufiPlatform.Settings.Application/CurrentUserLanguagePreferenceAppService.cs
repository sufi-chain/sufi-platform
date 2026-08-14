using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Localization;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Settings;

[Authorize]
public class CurrentUserLanguagePreferenceAppService
    : SettingsAppServiceBase, ICurrentUserLanguagePreferenceAppService
{
    protected ISettingManager SettingManager { get; }
    protected AbpLocalizationOptions LocalizationOptions { get; }

    public CurrentUserLanguagePreferenceAppService(
        ISettingManager settingManager,
        IOptions<AbpLocalizationOptions> localizationOptions)
    {
        SettingManager = settingManager;
        LocalizationOptions = localizationOptions.Value;
    }

    public virtual async Task UpdateAsync(UpdateCurrentUserLanguagePreferenceInput input)
    {
        var cultureName = CultureInfo.GetCultureInfo(input.CultureName).Name;
        if (!LocalizationOptions.Languages.Any(language =>
                string.Equals(language.CultureName, cultureName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new UserFriendlyException($"Language '{cultureName}' is not configured for this application.");
        }

        await SettingManager.SetForUserAsync(
            CurrentUser.GetId(),
            SufiLocalizationSettingNames.DefaultLanguage,
            cultureName);
    }
}
