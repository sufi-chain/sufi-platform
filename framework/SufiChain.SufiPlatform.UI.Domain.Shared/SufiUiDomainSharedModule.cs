using SufiChain.SufiPlatform.UI.Localization;
using SufiChain.SufiPlatform.Validation;
using SufiChain.SufiPlatform.Validation.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.UI;

[DependsOn(
    typeof(AbpLocalizationModule),
    typeof(SufiValidationModule)
)]
public class SufiUiDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiUiDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiFrameworkResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiPlatform.Validation.Localization.SufiValidationResource))
                .AddVirtualJson("/Localization/SufiFramework");

            options.Resources
                .Add<SufiAuthorizationResource>("en")
                .AddVirtualJson("/Localization/SufiAuthorization");

            options.Resources
                .Add<SufiExceptionHandlingResource>("en")
                .AddVirtualJson("/Localization/SufiExceptionHandling");

            options.Resources
                .Add<SufiFeatureResource>("en")
                .AddVirtualJson("/Localization/SufiFeature");

            options.Resources
                .Add<SufiGlobalFeatureResource>("en")
                .AddVirtualJson("/Localization/SufiGlobalFeature");

            options.Resources
                .Add<SufiLocalizationResource>("en")
                .AddVirtualJson("/Localization/SufiLocalization");

            options.Resources
                .Add<SufiMultiTenancyResource>("en")
                .AddVirtualJson("/Localization/SufiMultiTenancy");

            options.Resources
                .Add<SufiTimingResource>("en")
                .AddVirtualJson("/Localization/SufiTiming");

            options.Resources
                .Add<SufiUiResource>("en")
                .AddVirtualJson("/Localization/SufiUi");

            options.Resources
                .Add<SufiUiNavigationResource>("en")
                .AddVirtualJson("/Localization/SufiUiNavigation");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.UI", typeof(SufiFrameworkResource));
        });
    }
}
