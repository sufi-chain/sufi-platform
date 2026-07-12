using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Localization;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

/// <summary>
/// Contributes TimeZone settings group to the settings page.
/// </summary>
public class TimeZoneSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiSettingsResource>>();

        context.Groups.Add(new SettingComponentGroup
        {
            Id = "timezone",
            DisplayName = l["TimeZoneSettings"],
            ComponentType = typeof(TimeZoneSettingsGroup),
            Order = 200
        });

        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(SettingsPermissions.TimeZone);
    }
}
