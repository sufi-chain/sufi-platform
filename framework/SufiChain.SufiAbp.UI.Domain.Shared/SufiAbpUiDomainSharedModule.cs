using SufiChain.SufiAbp.UI.Localization;
using SufiChain.SufiAbp.Validation;
using SufiChain.SufiAbp.Validation.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.UI;

[DependsOn(
    typeof(AbpLocalizationModule),
    typeof(SufiAbpValidationModule)
)]
public class SufiAbpUiDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpUiDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpFrameworkResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiAbp.Validation.Localization.SufiAbpValidationResource))
                .AddVirtualJson("/Localization/SufiAbpFramework");

            options.Resources
                .Add<SufiAbpAuthorizationResource>("en")
                .AddVirtualJson("/Localization/SufiAbpAuthorization");

            options.Resources
                .Add<SufiAbpExceptionHandlingResource>("en")
                .AddVirtualJson("/Localization/SufiAbpExceptionHandling");

            options.Resources
                .Add<SufiAbpFeatureResource>("en")
                .AddVirtualJson("/Localization/SufiAbpFeature");

            options.Resources
                .Add<SufiAbpGlobalFeatureResource>("en")
                .AddVirtualJson("/Localization/SufiAbpGlobalFeature");

            options.Resources
                .Add<SufiAbpLocalizationResource>("en")
                .AddVirtualJson("/Localization/SufiAbpLocalization");

            options.Resources
                .Add<SufiAbpMultiTenancyResource>("en")
                .AddVirtualJson("/Localization/SufiAbpMultiTenancy");

            options.Resources
                .Add<SufiAbpTimingResource>("en")
                .AddVirtualJson("/Localization/SufiAbpTiming");

            options.Resources
                .Add<SufiAbpUiResource>("en")
                .AddVirtualJson("/Localization/SufiAbpUi");

            options.Resources
                .Add<SufiAbpUiNavigationResource>("en")
                .AddVirtualJson("/Localization/SufiAbpUiNavigation");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.UI", typeof(SufiAbpFrameworkResource));
        });
    }
}
