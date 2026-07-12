using Volo.Abp.Modularity;

using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;
namespace SufiChain.SufiPlatform.Users;

[DependsOn(
    typeof(AbpMultiTenancyModule),
    typeof(AbpEventBusModule)
    )]
public class SufiUsersAbstractionModule : AbpModule
{

}
