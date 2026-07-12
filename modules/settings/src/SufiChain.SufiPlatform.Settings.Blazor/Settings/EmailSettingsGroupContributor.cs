using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Localization;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

/// <summary>
/// Contributes Email settings group to the settings page.
/// </summary>
public class EmailSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiSettingsResource>>();

        context.Groups.Add(new SettingComponentGroup
        {
            Id = "email",
            DisplayName = l["EmailSettings"],
            ComponentType = typeof(EmailSettingsGroup),
            Order = 100
        });

        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(SettingsPermissions.Emailing);
    }
}
