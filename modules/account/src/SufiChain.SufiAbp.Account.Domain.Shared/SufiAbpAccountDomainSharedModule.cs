using SufiChain.SufiAbp.Account.Localization;
using SufiChain.SufiAbp.UI;
using SufiChain.SufiAbp.UI.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.Account;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule)
)]
public class SufiAbpAccountDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpAccountDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                   .Add<SufiAbpAccountResource>("en")
                   .AddBaseTypes(
                       typeof(SufiAbpFrameworkResource)
                       //typeof(AccountResource)
                   )
                   .AddVirtualJson("/Localization/Account");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.Account", typeof(SufiAbpAccountResource));
        });
    }
}
