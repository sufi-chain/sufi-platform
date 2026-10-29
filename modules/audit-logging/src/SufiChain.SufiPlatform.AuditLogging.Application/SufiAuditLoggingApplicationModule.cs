using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using SufiChain.SufiPlatform.AuditLogging;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Ddd;

namespace SufiChain.SufiPlatform.AuditLogging;

[DependsOn(
    typeof(SufiAuditLoggingApplicationContractsModule),
    typeof(SufiDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(SufiAuditLoggingDomainModule),
    typeof(SufiPermissionsDomainModule),
    typeof(SufiSettingsDomainModule)
)]
public class SufiAuditLoggingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAuditLoggingApplicationModule>();
    }
}
