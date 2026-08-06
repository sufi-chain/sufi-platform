using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Users;

[DependsOn(
    typeof(SufiUsersDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiUsersMongoDbModule : AbpModule
{

}
