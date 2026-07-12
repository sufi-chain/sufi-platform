using Volo.Abp.Domain;
using SufiChain.SufiPlatform.Menus.Localization;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Validation;
using SufiChain.SufiPlatform.Validation.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Menus;

[DependsOn(
    typeof(AbpDddDomainSharedModule),
    typeof(SufiValidationModule),
    typeof(SufiFeaturesModule))]
public class SufiMenusDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options => options.FileSets.AddEmbedded<SufiMenusDomainSharedModule>());
        Configure<AbpLocalizationOptions>(options => options.Resources.Add<SufiMenusResource>("en").AddBaseTypes(typeof(SufiValidationResource)).AddVirtualJson("/Localization/Menus"));
        Configure<AbpExceptionLocalizationOptions>(options => options.MapCodeNamespace(MenusErrorCodes.Namespace, typeof(SufiMenusResource)));
    }
}