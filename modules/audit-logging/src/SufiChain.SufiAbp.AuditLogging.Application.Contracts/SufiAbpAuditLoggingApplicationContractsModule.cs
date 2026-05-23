using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AuditLogging;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpAuthorizationModule),
    typeof(SufiAbpAuditLoggingDomainSharedModule)
)]
public class SufiAbpAuditLoggingApplicationContractsModule : AbpModule
{
}
