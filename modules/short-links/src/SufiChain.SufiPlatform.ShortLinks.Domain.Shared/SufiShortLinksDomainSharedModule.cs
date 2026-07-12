using Volo.Abp.Domain;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.ShortLinks.Localization;
using SufiChain.SufiPlatform.Validation;
using SufiChain.SufiPlatform.Validation.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.ShortLinks;

[DependsOn(
    typeof(AbpDddDomainSharedModule),
    typeof(SufiValidationModule),
    typeof(SufiFeaturesModule)
)]
public class SufiShortLinksDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiShortLinksDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiShortLinksResource>("en")
                .AddBaseTypes(typeof(SufiValidationResource))
                .AddVirtualJson("/Localization/ShortLinks");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("ShortLinks", typeof(SufiShortLinksResource));
        });
    }
}