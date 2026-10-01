using SufiChain.SufiPlatform.Core;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace SufiChain.SufiPlatform.Ddd;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiModule),
    typeof(SufiDddApplicationContractsModule)
)]
public class SufiDddApplicationModule : AbpModule
{
}
