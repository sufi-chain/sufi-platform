using SufiChain.SufiPlatform.AuditLogging;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiPlatform.AuditLogging.Localization;
using SufiChain.SufiPlatform.UI.Localization;

using SufiChain.SufiPlatform.UI;

namespace SufiChain.SufiPlatform.AuditLogging;

[DependsOn(
    typeof(SufiUiDomainSharedModule)
)]
public class SufiAuditLoggingDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAuditLoggingDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAuditLoggingResource>("en")
                .AddBaseTypes(typeof(SufiFrameworkResource))
                .AddVirtualJson("/Localization/AuditLogging");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("AuditLogging", typeof(SufiAuditLoggingResource));
        });
    }
}
