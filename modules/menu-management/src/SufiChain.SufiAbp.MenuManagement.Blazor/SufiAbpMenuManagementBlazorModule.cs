using SufiChain.SufiAbp.MenuManagement.Blazor.Menus;
using SufiChain.SufiAbp.LocalizationManagement.Blazor.Public;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MenuManagement.Blazor;

[DependsOn(
    typeof(SufiAbpMenuManagementApplicationContractsModule),
    typeof(SufiAbpLocalizationManagementBlazorPublicModule))]
public class SufiAbpMenuManagementBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpRouterOptions>(options => options.AdditionalAssemblies.Add(typeof(SufiAbpMenuManagementBlazorModule).Assembly));

        Configure<SufiAbpNavigationOptions>(options => options.MenuContributors.Add(new MenuManagementMenuContributor()));
    }
}
