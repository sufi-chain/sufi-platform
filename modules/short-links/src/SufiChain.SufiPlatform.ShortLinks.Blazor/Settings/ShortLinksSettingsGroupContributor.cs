using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.ShortLinks.Localization;
using SufiChain.SufiPlatform.ShortLinks.Permissions;

namespace SufiChain.SufiPlatform.ShortLinks.Blazor.Settings;

public class ShortLinksSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiShortLinksResource>>();

        context.Groups.Add(new SettingComponentGroup
        {
            Id = "short-links",
            DisplayName = l["ShortLinks"],
            ComponentType = typeof(ShortLinksSettingsGroup),
            Order = 210
        });

        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(ShortLinksPermissions.ShortLinks.Edit);
    }
}