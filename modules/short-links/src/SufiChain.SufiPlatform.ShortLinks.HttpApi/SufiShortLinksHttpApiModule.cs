using SufiChain.SufiPlatform.UI.Localization;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.AspNetCore.Mvc;
using SufiChain.SufiPlatform.ShortLinks.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.ShortLinks;

[DependsOn(
    typeof(SufiShortLinksApplicationContractsModule),
    typeof(SufiAspNetCoreMvcModule))]
public class SufiShortLinksHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(SufiShortLinksApplicationContractsModule).Assembly);
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<SufiShortLinksResource>()
                .AddBaseTypes(typeof(SufiUiResource));
        });
    }
}