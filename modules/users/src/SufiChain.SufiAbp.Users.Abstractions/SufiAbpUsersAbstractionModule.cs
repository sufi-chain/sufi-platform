using Volo.Abp.Modularity;

using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;
namespace SufiChain.SufiAbp.Users;

[DependsOn(
    typeof(AbpMultiTenancyModule),
    typeof(AbpEventBusModule)
    )]
public class SufiAbpUsersAbstractionModule : AbpModule
{

}
