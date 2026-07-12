using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Authorization;

[DependsOn(
    typeof(AbpAuthorizationModule)
)]
public class SufiAuthorizationModule : AbpModule
{
}
