using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Validation.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(typeof(SufiFeaturesModule))]
public class SufiAIDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAIDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<AIResource>("en")
                .AddBaseTypes(typeof(SufiValidationResource))
                .AddVirtualJson("/Localization/AI");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("AI", typeof(AIResource));
        });
    }
}
