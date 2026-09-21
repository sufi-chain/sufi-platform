using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Localization;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

public class ExternalAuthSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiSettingsResource>>();

        context.Groups.Add(new SettingComponentGroup
        {
            Id = "external-auth",
            DisplayName = l["ExternalAuthSettings"],
            ComponentType = typeof(ExternalAuthSettingsGroup),
            Order = 205
        });

        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(SettingsPermissions.ExternalAuth);
    }
}
