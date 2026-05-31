using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.FileManager.Permissions;
using SufiChain.SufiAbp.SettingManagement.Blazor.Settings;

namespace SufiChain.SufiAbp.FileManager.Blazor.Settings;

public class FileManagerArchivingSettingsGroupContributor : ISettingComponentContributor
{
    public Task ConfigureAsync(SettingComponentCreationContext context)
    {
        var l = context.GetRequiredService<IStringLocalizer<SufiAbpFileManagerResource>>();

        context.Groups.Add(new SettingComponentGroup
        {
            Id = "file-manager-archiving",
            DisplayName = l["FileManagerArchivingSettings"],
            ComponentType = typeof(FileManagerArchivingSettingsGroup),
            Order = 210
        });

        return Task.CompletedTask;
    }

    public async Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
    {
        var authorizationService = context.GetRequiredService<IAuthorizationService>();
        return await authorizationService.IsGrantedAsync(FileManagerPermissions.Settings.Default);
    }
}
