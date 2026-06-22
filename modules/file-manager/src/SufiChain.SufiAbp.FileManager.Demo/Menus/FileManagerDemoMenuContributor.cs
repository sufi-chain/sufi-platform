using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.FileManager.Demo.Menus;

/// <summary>
/// Menu contributor for File Manager Demo pages.
/// </summary>
public class FileManagerDemoMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return Task.CompletedTask;
        }

        var demoMenu = context.Menu.GetDemo();
        var l = context.GetLocalizer<SufiAbpFileManagerResource>();

        var fileManagerDemo = new ApplicationMenuItem(
            name: FileManagerDemoMenus.GroupName,
            displayName: l["Menu:FileManagerDemo"],
            url: "/demo/file-manager",
            icon: "folder-open",
            order: 2
        )
        {
            IsCollapsed = false
        };

        fileManagerDemo.AddItem(new ApplicationMenuItem(
            FileManagerDemoMenus.Overview,
            l["Menu:FileManagerDemoOverview"],
            "/demo/file-manager",
            icon: "file-text",
            order: 1
        ));
        fileManagerDemo.AddItem(new ApplicationMenuItem(
            FileManagerDemoMenus.AssetManager,
            l["Menu:FileManagerDemoAssetManager"],
            "/demo/file-manager/asset-manager",
            icon: "folder-open",
            order: 2
        ));
        fileManagerDemo.AddItem(new ApplicationMenuItem(
            FileManagerDemoMenus.FileBrowser,
            l["Menu:FileManagerDemoFileBrowser"],
            "/demo/file-manager/browser",
            icon: "table",
            order: 3
        ));
        fileManagerDemo.AddItem(new ApplicationMenuItem(
            FileManagerDemoMenus.Upload,
            l["Menu:FileManagerDemoUpload"],
            "/demo/file-manager/upload",
            icon: "upload",
            order: 4
        ));
        fileManagerDemo.AddItem(new ApplicationMenuItem(
            FileManagerDemoMenus.RichTextEditor,
            l["Menu:FileManagerDemoRichTextEditor"],
            "/demo/file-manager/rich-text-editor",
            icon: "align-left",
            order: 5
        ));
        fileManagerDemo.AddItem(new ApplicationMenuItem(
            FileManagerDemoMenus.MarkdownEditor,
            l["Menu:FileManagerDemoMarkdownEditor"],
            "/demo/file-manager/markdown-editor",
            icon: "file-text",
            order: 6
        ));
        fileManagerDemo.AddItem(new ApplicationMenuItem(
            FileManagerDemoMenus.FileStructure,
            l["Menu:FileManagerDemoFileStructure"],
            "/demo/file-manager/file-structure",
            icon: "layers",
            order: 7
        ));

        demoMenu.AddItem(fileManagerDemo);

        return Task.CompletedTask;
    }
}
