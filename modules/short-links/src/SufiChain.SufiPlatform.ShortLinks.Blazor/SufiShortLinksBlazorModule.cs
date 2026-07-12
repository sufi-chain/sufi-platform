using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Settings.Blazor;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.ShortLinks.Blazor.Menus;
using SufiChain.SufiPlatform.ShortLinks.Blazor.Settings;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.ShortLinks;

[DependsOn(
    typeof(SufiShortLinksApplicationContractsModule),
    typeof(AbpMapperlyModule),
    typeof(SufiSettingsBlazorModule)
)]
public class SufiShortLinksBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiShortLinksBlazorModule>();

        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new ShortLinksMenuContributor());
        });

        Configure<SettingsComponentOptions>(options =>
        {
            options.Contributors.Add(new ShortLinksSettingsGroupContributor());
        });

        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiShortLinksBlazorModule).Assembly);
        });
    }
}