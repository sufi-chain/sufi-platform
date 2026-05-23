using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Ddd;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiAbpDddDomainSharedModule)
)]
public class SufiAbpDddDomainModule : AbpModule
{
}
