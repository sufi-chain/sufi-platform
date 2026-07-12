using SufiChain.SufiPlatform.UI.Localization;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.AspNetCore.Mvc;
using LocalizationModuleResource = SufiChain.SufiPlatform.Localization.Localization.SufiLocalizationResource;

namespace SufiChain.SufiPlatform.Localization;

[DependsOn(
    typeof(SufiLocalizationApplicationContractsModule),
    typeof(SufiAspNetCoreMvcModule)
)]
public class SufiLocalizationHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<LocalizationModuleResource>()
                .AddBaseTypes(typeof(SufiFrameworkResource), typeof(SufiUiResource));
        });
    }
}
