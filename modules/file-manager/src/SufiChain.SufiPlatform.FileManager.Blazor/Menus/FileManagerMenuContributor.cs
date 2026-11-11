using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.FileManager.Permissions;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Menus;

/// <summary>
/// Menu contributor for FileManager module.
/// Items declare RequireFeatures so the menu manager re-checks the tenant
/// feature on every menu build.
/// </summary>
public class FileManagerMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            ConfigureMainMenu(context);
        }

        return Task.CompletedTask;
    }

    private static void ConfigureMainMenu(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiFileManagerResource>();
        var administration = context.Menu.GetAdministration();

        var fileManagerMenu = new ApplicationMenuItem(
            FileManagerMenus.GroupName,
            l["Menu:SufiFileManager"],
            icon: "folder-open",
            order: 40
        ).RequireFeatures(SufiFileManagerFeatures.Enable);

        fileManagerMenu.AddItem(
            new ApplicationMenuItem(
                FileManagerMenus.FileStats,
                l["Menu:FileStats"],
                url: "/panel/admin/file-manager/stats",
                icon: "chart-bar",
                order: 1
            ).RequirePermissions(FileManagerPermissions.FileItems.Default)
             .RequireFeatures(SufiFileManagerFeatures.Enable, SufiFileManagerFeatures.FileItems));

        fileManagerMenu.AddItem(
            new ApplicationMenuItem(
                FileManagerMenus.AssetManager,
                l["Menu:AssetManager"],
                url: "/panel/admin/file-manager/assets",
                icon: "folder-tree",
                order: 2
            ).RequirePermissions(FileManagerPermissions.FileItems.Default)
             .RequireFeatures(SufiFileManagerFeatures.Enable, SufiFileManagerFeatures.FileItems));

        fileManagerMenu.AddItem(
            new ApplicationMenuItem(
                FileManagerMenus.FileStructures,
                l["Menu:FileStructures"],
                url: "/panel/admin/file-manager/structures",
                icon: "layers",
                order: 3
            ).RequirePermissions(FileManagerPermissions.FileStructures.Default)
             .RequireFeatures(SufiFileManagerFeatures.Enable, SufiFileManagerFeatures.FileStructures));

        fileManagerMenu.AddItem(
            new ApplicationMenuItem(
                FileManagerMenus.ArchivedFiles,
                l["Menu:ArchivedFiles"],
                url: "/panel/admin/file-manager/archived",
                icon: "archive",
                order: 4
            ).RequirePermissions(FileManagerPermissions.FileItems.Default)
             .RequireFeatures(
                 SufiFileManagerFeatures.Enable,
                 SufiFileManagerFeatures.FileItems,
                 SufiFileManagerFeatures.Archiving));

        fileManagerMenu.AddItem(
            new ApplicationMenuItem(
                FileManagerMenus.FolderAccess,
                l["Menu:FolderAccess"],
                url: "/panel/admin/file-manager/access",
                icon: "shield",
                order: 5
            ).RequirePermissions(FileManagerPermissions.FileItems.Update)
             .RequireFeatures(SufiFileManagerFeatures.Enable, SufiFileManagerFeatures.FileItems));

        administration.AddItem(fileManagerMenu);
    }
}
