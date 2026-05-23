using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.FileManager.Permissions;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.FileManager.Blazor.Menus;

/// <summary>
/// Menu contributor for FileManager module.
/// </summary>
public class FileManagerMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiAbpFileManagerResource>();
        var administration = context.Menu.GetAdministration();

        var fileManagerMenu = new ApplicationMenuItem(
            FileManagerMenus.GroupName,
            l["Menu:FileManager"],
            icon: "folder-open",
            order: 20
        );

        fileManagerMenu.AddItem(
            new ApplicationMenuItem(
                FileManagerMenus.FileStats,
                l["Menu:FileStats"],
                url: "/admin/file-manager/stats",
                icon: "chart-bar",
                order: 1
            ).RequirePermissions(FileManagerPermissions.FileItems.Default)
        );

        fileManagerMenu.AddItem(
            new ApplicationMenuItem(
                FileManagerMenus.AssetManager,
                l["Menu:AssetManager"],
                url: "/admin/file-manager/assets",
                icon: "folder-tree",
                order: 2
            ).RequirePermissions(FileManagerPermissions.FileItems.Default)
        );

        fileManagerMenu.AddItem(
            new ApplicationMenuItem(
                FileManagerMenus.FileStructures,
                l["Menu:FileStructures"],
                url: "/admin/file-manager/structures",
                icon: "layers",
                order: 3
            ).RequirePermissions(FileManagerPermissions.FileStructures.Default)
        );

        fileManagerMenu.AddItem(
            new ApplicationMenuItem(
                FileManagerMenus.ArchivedFiles,
                l["Menu:ArchivedFiles"],
                url: "/admin/file-manager/archived",
                icon: "archive",
                order: 4
            ).RequirePermissions(FileManagerPermissions.FileItems.Default)
        );

        administration.AddItem(fileManagerMenu);

        return Task.CompletedTask;
    }
}

