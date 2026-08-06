using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace SufiChain.SufiPlatform.Tags;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiTagsDomainSharedModule)
)]
public class SufiTagsDomainModule : AbpModule
{
}