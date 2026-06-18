using SufiChain.SufiAbp.AuditLogging;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using SufiChain.SufiAbp.AuditLogging.Localization;
using SufiChain.SufiAbp.UI;
using SufiChain.SufiAbp.UI.Localization;

namespace SufiChain.SufiAbp.AuditLogging;

[DependsOn(
    typeof(SufiAbpUiDomainSharedModule)
)]
public class SufiAbpAuditLoggingDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpAuditLoggingDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpAuditLoggingResource>("en")
                .AddBaseTypes(typeof(SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/AuditLogging");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("AuditLogging", typeof(SufiAbpAuditLoggingResource));
        });
    }
}
