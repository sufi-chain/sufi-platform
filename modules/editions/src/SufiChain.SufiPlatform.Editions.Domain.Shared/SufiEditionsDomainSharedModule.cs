using SufiChain.SufiPlatform.Editions.Localization;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Validation;
using SufiChain.SufiPlatform.Validation.Localization;
using Volo.Abp.Domain;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Editions;

[DependsOn(
    typeof(AbpDddDomainSharedModule),
    typeof(SufiValidationModule),
    typeof(SufiFeaturesModule))]
public class SufiEditionsDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiEditionsDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<EditionsResource>("en")
                .AddBaseTypes(typeof(SufiValidationResource))
                .AddVirtualJson("/Localization/Editions");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiEditions", typeof(EditionsResource));
        });
    }
}
