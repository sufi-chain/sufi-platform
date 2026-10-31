using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.Tags.Localization;
using SufiChain.SufiPlatform.Tags.Permissions;

namespace SufiChain.SufiPlatform.Tags.Blazor.Settings;

public class TagsSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiTagsResource>>();
        context.Groups.Add(new SettingComponentGroup
        {
            Id = "tags",
            DisplayName = l["Settings:Tags"],
            Description = l["Settings:Tags:Hint"],
            Icon = "tag",
            ComponentType = typeof(TagsSettingsGroup),
            Policy = TagsPermissions.Settings.Manage,
            Order = 290
        });
        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(TagsPermissions.Settings.Manage);
    }
}
