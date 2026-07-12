using SufiChain.SufiAbp.PermissionManagement.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

using SufiChain.SufiAbp.UI;

namespace SufiChain.SufiAbp.PermissionManagement;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule)
)]
public class SufiAbpPermissionManagementDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpPermissionManagementDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpPermissionManagementResource>("en")
                .AddBaseTypes(typeof(SufiChain.SufiAbp.UI.Localization.SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/PermissionManagement");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiChain.SufiAbp.PermissionManagement", typeof(SufiAbpPermissionManagementResource));
        });
    }
}
