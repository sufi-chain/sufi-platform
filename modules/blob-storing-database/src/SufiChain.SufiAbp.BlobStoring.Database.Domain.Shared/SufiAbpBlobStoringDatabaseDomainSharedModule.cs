using SufiChain.SufiAbp.BlobStoring.Database.Localization;
using SufiChain.SufiAbp.Validation;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.BlobStoring.Database;

[DependsOn(typeof(SufiAbpValidationModule))]
public class SufiAbpBlobStoringDatabaseDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpBlobStoringDatabaseDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpBlobStoringDatabaseResource>("en")
                .AddBaseTypes(typeof(SufiAbpValidationResource))
                .AddVirtualJson("/Localization/BlobStoringDatabase");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.BlobStoring.Database", typeof(SufiAbpBlobStoringDatabaseResource));
        });
    }
}
