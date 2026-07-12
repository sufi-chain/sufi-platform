using SufiChain.SufiPlatform.Features.Localization;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.Features;

[DependsOn(
    typeof(SufiUiDomainSharedModule),
    typeof(SufiFeaturesModule)
)]
public class SufiFeaturesDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiFeaturesDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiFeaturesResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiPlatform.UI.Localization.SufiFrameworkResource))
                .AddVirtualJson("/Localization/Features");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.Features", typeof(SufiFeaturesResource));
        });
    }
}
