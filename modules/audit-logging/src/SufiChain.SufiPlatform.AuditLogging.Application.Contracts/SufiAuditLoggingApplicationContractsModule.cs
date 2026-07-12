using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.AuditLogging;

[DependsOn(
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAuthorizationModule),
    typeof(SufiAuditLoggingDomainSharedModule)
)]
public class SufiAuditLoggingApplicationContractsModule : AbpModule
{
}
