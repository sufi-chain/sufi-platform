using SufiChain.SufiAbp.Core;
using SufiChain.SufiAbp.UI.Localization;
using SufiChain.SufiAbp.Users.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiAbp.UI;

namespace SufiChain.SufiAbp.Users;

[DependsOn(typeof(SufiAbpUiDomainSharedModule))]
public class SufiAbpUsersDomainSharedModule : SufiAbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpUsersDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpUsersResource>("en")
                .AddBaseTypes(typeof(SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/Users");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.Users", typeof(SufiAbpUsersResource));
        });
    }
}
