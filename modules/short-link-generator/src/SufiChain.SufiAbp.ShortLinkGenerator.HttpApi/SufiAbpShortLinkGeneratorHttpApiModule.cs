using SufiChain.SufiAbp.UI.Localization;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorApplicationContractsModule),
    typeof(SufiAbpAspNetCoreMvcModule))]
public class SufiAbpShortLinkGeneratorHttpApiModule : AbpModule
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
            options.ConventionalControllers.Create(typeof(SufiAbpShortLinkGeneratorApplicationContractsModule).Assembly);
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<SufiAbpShortLinkGeneratorResource>()
                .AddBaseTypes(typeof(SufiAbpUiResource));
        });
    }
}


