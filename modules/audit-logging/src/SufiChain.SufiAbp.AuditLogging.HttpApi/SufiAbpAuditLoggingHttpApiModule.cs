using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AuditLogging;

[DependsOn(
    typeof(SufiAbpAuditLoggingApplicationContractsModule),
    typeof(SufiAbpAspNetCoreMvcModule)
)]
public class SufiAbpAuditLoggingHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }
}
