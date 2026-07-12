using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Validation;
using SufiChain.SufiPlatform.Validation.Localization;
using SufiChain.SufiPlatform.OpenIddict.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.OpenIddict;

[DependsOn(
    typeof(SufiValidationModule),
    typeof(AbpVirtualFileSystemModule),
    typeof(SufiUiDomainSharedModule)
)]
public class SufiOpenIddictDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiOpenIddictDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiOpenIddictResource>("en")
                .AddBaseTypes(typeof(SufiValidationResource))
                .AddVirtualJson("/Localization/SufiOpenIddict");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("OpenIddict", typeof(SufiOpenIddictResource));
        });
    }
}
