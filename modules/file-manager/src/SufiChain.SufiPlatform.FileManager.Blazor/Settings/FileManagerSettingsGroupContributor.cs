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
            ComponentType = typeof(FileManagerSettingsGroup),
            Order = 190
        });

        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(FileManagerPermissions.Settings.Default) ||
               await authorizationService.IsGrantedAsync(FileManagerPermissions.StorageSettings.Manage);
    }
}