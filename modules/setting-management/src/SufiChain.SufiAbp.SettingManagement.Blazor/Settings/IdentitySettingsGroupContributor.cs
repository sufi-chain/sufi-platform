using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.SettingManagement.Localization;

namespace SufiChain.SufiAbp.SettingManagement.Blazor.Settings;

public class IdentitySettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiAbpSettingManagementResource>>();

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
        return await authorizationService.IsGrantedAsync(SettingManagementPermissions.Identity);
    }
}
