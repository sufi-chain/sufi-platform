using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using SufiChain.SufiAbp.SettingManagement.Blazor.Menus;
using SufiChain.SufiAbp.SettingManagement.Blazor.Settings;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.SettingManagement;

namespace SufiChain.SufiAbp.SettingManagement.Blazor;

[DependsOn(typeof(SufiAbpSettingManagementApplicationContractsModule))]
public class SufiAbpSettingManagementBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpSettingManagementBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new SettingManagementMenuContributor());
        });

        // Register setting group contributors
        Configure<SettingManagementComponentOptions>(options =>
        {
            options.Contributors.Add(new EmailSettingsGroupContributor());
            options.Contributors.Add(new TimeZoneSettingsGroupContributor());
        });
    }
}
