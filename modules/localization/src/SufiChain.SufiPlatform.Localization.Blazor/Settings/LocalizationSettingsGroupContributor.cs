using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.Localization.Localization;
using SufiChain.SufiPlatform.Localization.Permissions;

namespace SufiChain.SufiPlatform.Localization.Blazor.Settings;

public class LocalizationSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiLocalizationResource>>();
        context.Groups.Add(new SettingComponentGroup
        {
            Id = "localization",
            DisplayName = l["Settings:Localization"],
            Description = l["Settings:Localization:Hint"],
            Icon = "languages",
            ComponentType = typeof(LocalizationSettingsGroup),
            Policy = LocalizationPermissions.Settings.Manage,
            Order = 310
        });
        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(LocalizationPermissions.Settings.Manage);
    }
}
