using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Localization;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

public class IdentitySettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiSettingsResource>>();

        context.Groups.Add(new SettingComponentGroup
        {
            Id = "identity",
            DisplayName = l["IdentitySettings"],
            ComponentType = typeof(IdentitySettingsGroup),
            Order = 200
        });

        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(SettingsPermissions.Identity);
    }
}
