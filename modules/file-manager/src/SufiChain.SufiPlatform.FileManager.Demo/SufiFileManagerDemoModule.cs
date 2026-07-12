using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.FileManager.Blazor;
using SufiChain.SufiPlatform.FileManager.Blazor.Public;
using SufiChain.SufiPlatform.FileManager.Demo.Menus;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager.Demo;

[DependsOn(
    typeof(SufiFileManagerBlazorModule),
    typeof(SufiFileManagerBlazorPublicModule)
)]
public class SufiFileManagerDemoModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiFileManagerDemoModule).Assembly);
        });

        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new FileManagerDemoMenuContributor());
        });
    }
}