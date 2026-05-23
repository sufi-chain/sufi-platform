using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Authorization;

[DependsOn(
    typeof(AbpAuthorizationModule)
)]
public class SufiAbpAuthorizationModule : AbpModule
{
}
