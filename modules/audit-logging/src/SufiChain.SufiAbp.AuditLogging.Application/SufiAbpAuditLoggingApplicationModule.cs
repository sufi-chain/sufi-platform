using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using SufiChain.SufiAbp.AuditLogging;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Mapperly;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.AuditLogging;

[DependsOn(
    typeof(SufiAbpAuditLoggingApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpMapperlyModule),
    typeof(SufiAbpAuditLoggingDomainModule)
)]
public class SufiAbpAuditLoggingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpAuditLoggingApplicationModule>();
    }
}
