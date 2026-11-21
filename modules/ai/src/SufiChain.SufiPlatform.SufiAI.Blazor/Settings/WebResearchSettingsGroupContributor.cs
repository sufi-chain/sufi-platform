using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;
using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Settings;

public class WebResearchSettingsGroupContributor : ISettingComponentContributor
{
    public async Task ConfigureAsync(SettingComponentCreationContext context)
    {
        if (!IsHost(context))
        {
            return;
        }

        var l = context.GetRequiredService<IStringLocalizer<AIResource>>();
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        var canEdit = await authorizationService.IsGrantedAsync(AIPermissions.WebSearchSettings.Update);

        context.Groups.Add(new SettingComponentGroup
        {
            Id = WebResearchSettingsSection.Id,
            DisplayName = l["WebResearchSettings"],
            Description = l["WebResearchSetupHint"],
            Icon = "search",
            ComponentType = typeof(WebResearchSettings),
            Policy = AIPermissions.WebSearchSettings.Default,
            ReadOnly = !canEdit,
            Order = WebResearchSettingsSection.Order
        });
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        if (!IsHost(context))
        {
            return false;
        }

        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(AIPermissions.WebSearchSettings.Default);
    }

    private static bool IsHost(SettingComponentCreationContext context)
    {
        var currentTenant = context.GetRequiredService<ICurrentTenant>();
        return currentTenant.IsAvailable != true;
    }
}
