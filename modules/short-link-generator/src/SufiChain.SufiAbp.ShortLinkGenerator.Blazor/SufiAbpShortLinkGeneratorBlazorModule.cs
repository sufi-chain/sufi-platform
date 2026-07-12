using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.SettingManagement.Blazor;
using SufiChain.SufiAbp.SettingManagement.Blazor.Settings;
using SufiChain.SufiAbp.ShortLinkGenerator.Blazor.Menus;
using SufiChain.SufiAbp.ShortLinkGenerator.Blazor.Settings;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorApplicationContractsModule),
    typeof(AbpMapperlyModule),
    typeof(SufiAbpSettingManagementBlazorModule)
)]
public class SufiAbpShortLinkGeneratorBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpShortLinkGeneratorBlazorModule>();

        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new ShortLinkGeneratorMenuContributor());
        });

        Configure<SettingManagementComponentOptions>(options =>
        {
            options.Contributors.Add(new ShortLinkGeneratorSettingsGroupContributor());
        });

        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpShortLinkGeneratorBlazorModule).Assembly);
        });
    }
}
