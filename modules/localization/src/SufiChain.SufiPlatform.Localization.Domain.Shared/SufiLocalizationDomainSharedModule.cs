using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using SufiChain.SufiPlatform.Localization.Localization;
using Volo.Abp.Domain;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiPlatform.Validation;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.Localization;

[DependsOn(
    typeof(SufiUiDomainSharedModule),
    typeof(SufiValidationModule),
    typeof(AbpDddDomainSharedModule)
)]
public class SufiLocalizationDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiLocalizationDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiLocalizationResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiPlatform.UI.Localization.SufiFrameworkResource))
                .AddVirtualJson("/Localization/Localization");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.Localization", typeof(SufiLocalizationResource));
        });
    }
}
