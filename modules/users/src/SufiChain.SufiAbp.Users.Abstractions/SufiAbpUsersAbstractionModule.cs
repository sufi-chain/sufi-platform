using SufiChain.SufiAbp.EventBus;
using SufiChain.SufiAbp.MultiTenancy;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Users;

[DependsOn(
    typeof(SufiAbpMultiTenancyModule),
    typeof(SufiAbpEventBusModule)
    )]
public class SufiAbpUsersAbstractionModule : AbpModule
{

}
