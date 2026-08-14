using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using SufiChain.SufiPlatform.UI.Localization;
using SufiChain.SufiPlatform.Settings.Blazor.Localization;
using SufiChain.SufiPlatform.Settings.Blazor.Menus;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor;

[DependsOn(typeof(SufiSettingsApplicationContractsModule))]
public class SufiSettingsBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(
            ServiceDescriptor.Scoped<IPreferredLanguageService, SettingsPreferredLanguageService>());

        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiSettingsBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new SettingsMenuContributor());
        });

        // Register setting group contributors
        Configure<SettingsComponentOptions>(options =>
        {
            options.Contributors.Add(new EmailSettingsGroupContributor());
            options.Contributors.Add(new TimeZoneSettingsGroupContributor());
            options.Contributors.Add(new IdentitySettingsGroupContributor());
        });
    }
}
