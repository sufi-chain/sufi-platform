using SufiChain.SufiPlatform.Core;
using SufiChain.SufiPlatform.UI.Localization;
using SufiChain.SufiPlatform.Users.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.Users;

[DependsOn(typeof(SufiUiDomainSharedModule))]
public class SufiUsersDomainSharedModule : SufiModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiUsersDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiUsersResource>("en")
                .AddBaseTypes(typeof(SufiFrameworkResource))
                .AddVirtualJson("/Localization/Users");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiPlatform.Users", typeof(SufiUsersResource));
        });
    }
}
