using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.SettingManagement.Blazor.Settings;
using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using SufiChain.SufiAbp.ShortLinkGenerator.Permissions;

namespace SufiChain.SufiAbp.ShortLinkGenerator.Blazor.Settings;

public class ShortLinkGeneratorSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiAbpShortLinkGeneratorResource>>();

        context.Groups.Add(new SettingComponentGroup
        {
            Id = "short-links",
            DisplayName = l["ShortLinks"],
            ComponentType = typeof(ShortLinkGeneratorSettingsGroup),
            Order = 210
        });

        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(ShortLinkGeneratorPermissions.ShortLinks.Edit);
    }
}
