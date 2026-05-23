using SufiChain.SufiAbp.BlobStoring.Database;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.UI;
using SufiChain.SufiAbp.Validation;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule),
    typeof(SufiAbpValidationModule),
    typeof(SufiAbpDddDomainSharedModule),
    typeof(SufiAbpBlobStoringDatabaseDomainSharedModule)
)]
public class SufiAbpFileManagerDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpFileManagerDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpFileManagerResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiAbp.UI.Localization.SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/FileManager");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.FileManager", typeof(SufiAbpFileManagerResource));
        });
    }
}
