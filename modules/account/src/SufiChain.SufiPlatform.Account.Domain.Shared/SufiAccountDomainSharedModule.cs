using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.UI.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.Account;

[DependsOn(
    typeof(SufiUiDomainSharedModule)
)]
public class SufiAccountDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAccountDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                   .Add<SufiAccountResource>("en")
                   .AddBaseTypes(
                       typeof(SufiFrameworkResource)
                       //typeof(AccountResource)
                   )
                   .AddVirtualJson("/Localization/Account");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.Account", typeof(SufiAccountResource));
            options.MapCodeNamespace("SufiAccount", typeof(SufiAccountResource));
        });
    }
}
