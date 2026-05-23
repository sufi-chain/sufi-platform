using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;
using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerDomainSharedModule),
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpAuthorizationModule)
    )]
public class SufiAbpFileManagerApplicationContractsModule : AbpModule
{

}
