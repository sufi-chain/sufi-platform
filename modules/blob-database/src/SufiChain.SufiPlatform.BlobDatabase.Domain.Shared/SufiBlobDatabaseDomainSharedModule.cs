using SufiChain.SufiPlatform.BlobDatabase.Localization;
using SufiChain.SufiPlatform.Validation;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.BlobDatabase;

[DependsOn(typeof(SufiValidationModule))]
public class SufiBlobDatabaseDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiBlobDatabaseDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiBlobDatabaseResource>("en")
                .AddBaseTypes(typeof(SufiValidationResource))
                .AddVirtualJson("/Localization/BlobDatabase");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.BlobDatabase", typeof(SufiBlobDatabaseResource));
        });
    }
}
