using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.FileManager.Permissions;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public class FileArchivingSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiFileManagerResource>>();
        context.Groups.Add(new SettingComponentGroup
        {
            Id = "file-archiving",
            DisplayName = l["FileManagerArchivingSettings"],
            Description = l["FileManagerArchivingSettingsHint"],
            Icon = "archive",
            ComponentType = typeof(FileManagerArchivingSettingsGroup),
            Policy = FileManagerPermissions.ArchivingSettings.Manage,
            Order = 195
        });
        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(FileManagerPermissions.ArchivingSettings.Manage);
    }
}
