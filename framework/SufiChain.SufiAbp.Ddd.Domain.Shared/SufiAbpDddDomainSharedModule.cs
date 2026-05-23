using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Ddd;

[DependsOn(
    typeof(AbpDddDomainSharedModule)
)]
public class SufiAbpDddDomainSharedModule : AbpModule
{
}
