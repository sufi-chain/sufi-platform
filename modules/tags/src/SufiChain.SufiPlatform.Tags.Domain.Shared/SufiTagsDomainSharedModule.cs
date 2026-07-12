using SufiChain.SufiPlatform.Tags.Localization;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Validation;
using SufiChain.SufiPlatform.Validation.Localization;
using Volo.Abp.Domain;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Tags;

[DependsOn(
    typeof(AbpDddDomainSharedModule),
    typeof(SufiValidationModule),
    typeof(SufiFeaturesModule))]
public class SufiTagsDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiTagsDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiTagsResource>("en")
                .AddBaseTypes(typeof(SufiValidationResource))
                .AddVirtualJson("/Localization/Tags");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace(TagsErrorCodes.Namespace, typeof(SufiTagsResource));
        });
    }
}