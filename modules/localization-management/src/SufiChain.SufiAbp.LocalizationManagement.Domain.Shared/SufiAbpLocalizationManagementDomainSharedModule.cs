using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using SufiChain.SufiAbp.LocalizationManagement.Localization;
using Volo.Abp.Domain;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiAbp.UI;
using SufiChain.SufiAbp.Validation;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.LocalizationManagement;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule),
    typeof(SufiAbpValidationModule),
    typeof(SufiAbpDddDomainSharedModule)
)]
public class SufiAbpLocalizationManagementDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpLocalizationManagementDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpLocalizationManagementResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiAbp.UI.Localization.SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/LocalizationManagement");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.LocalizationManagement", typeof(SufiAbpLocalizationManagementResource));
        });
    }
}
