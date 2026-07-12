using SufiChain.SufiPlatform.BlobDatabase;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.Validation;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Domain;
using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(SufiUiDomainSharedModule),
    typeof(SufiValidationModule),
    typeof(AbpDddDomainSharedModule),
    typeof(SufiFeaturesModule),
    typeof(SufiBlobDatabaseDomainSharedModule)
)]
public class SufiFileManagerDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiFileManagerDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiFileManagerResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiPlatform.UI.Localization.SufiFrameworkResource))
                .AddVirtualJson("/Localization/FileManager");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.FileManager", typeof(SufiFileManagerResource));
        });
    }
}