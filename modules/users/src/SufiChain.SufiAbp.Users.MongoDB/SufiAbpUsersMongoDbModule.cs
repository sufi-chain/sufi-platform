using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.Users;

[DependsOn(
    typeof(SufiAbpUsersDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiAbpUsersMongoDbModule : AbpModule
{

}
