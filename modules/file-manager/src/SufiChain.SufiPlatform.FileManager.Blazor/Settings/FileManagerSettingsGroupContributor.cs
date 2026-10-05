using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.FileManager.Permissions;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public class FileManagerSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiFileManagerResource>>();

        context.Groups.Add(new SettingComponentGroup
        {
            Id = "file-manager",
            DisplayName = l["Menu:SufiFileManager"],
            Icon = "folder",
            ComponentType = typeof(FileManagerSettingsGroup),
            Order = 190
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Visibility is the OR of general settings and storage settings.
    /// The section therefore has no single <see cref="SettingComponentGroup.Policy"/>.
    /// Each H3 still checks its own permission and is omitted when that check fails.
    /// </summary>
    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(FileManagerPermissions.Settings.Default) ||
               await authorizationService.IsGrantedAsync(FileManagerPermissions.StorageSettings.Manage);
    }
}