using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.FileManager.Blazor;
using SufiChain.SufiAbp.FileManager.Demo.Menus;
using SufiChain.SufiAbp.FileManager.RichTextEditor;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager.Demo;

[DependsOn(
    typeof(SufiAbpFileManagerBlazorModule),
    typeof(SufiAbpFileManagerRichTextEditorModule)
)]
public class SufiAbpFileManagerDemoModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpFileManagerDemoModule).Assembly);
        });

        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new FileManagerDemoMenuContributor());
        });
    }
}
