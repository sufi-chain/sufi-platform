using SufiChain.SufiPlatform.Validation.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.Validation;

[DependsOn(typeof(AbpValidationModule))]
public class SufiValidationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiValidationModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiValidationResource>("en")
                .AddVirtualJson("/Localization/SufiValidation");
        });
    }
}
