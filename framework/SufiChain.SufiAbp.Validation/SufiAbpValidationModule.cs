using SufiChain.SufiAbp.Validation.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Validation;

namespace SufiChain.SufiAbp.Validation;

[DependsOn(typeof(AbpValidationModule))]
public class SufiAbpValidationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpValidationModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpValidationResource>("en")
                .AddVirtualJson("/Localization/SufiAbpValidation");
        });
    }
}
