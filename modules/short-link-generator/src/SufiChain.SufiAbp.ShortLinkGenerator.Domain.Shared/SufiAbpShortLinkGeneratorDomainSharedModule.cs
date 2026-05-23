using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using SufiChain.SufiAbp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using SufiChain.SufiAbp.Validation;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpValidationModule)
)]
public class SufiAbpShortLinkGeneratorDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpShortLinkGeneratorDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpShortLinkGeneratorResource>("en")
                .AddBaseTypes(typeof(SufiAbpValidationResource))
                .AddVirtualJson("/Localization/ShortLinkGenerator");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("ShortLinkGenerator", typeof(SufiAbpShortLinkGeneratorResource));
        });
    }
}


