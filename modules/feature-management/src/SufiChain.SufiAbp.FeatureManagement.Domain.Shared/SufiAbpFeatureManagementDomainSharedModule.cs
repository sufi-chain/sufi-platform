using SufiChain.SufiAbp.FeatureManagement.Localization;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.UI;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.FeatureManagement;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule),
    typeof(SufiAbpFeaturesModule)
)]
public class SufiAbpFeatureManagementDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpFeatureManagementDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpFeatureManagementResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiAbp.UI.Localization.SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/FeatureManagement");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.FeatureManagement", typeof(SufiAbpFeatureManagementResource));
        });
    }
}
