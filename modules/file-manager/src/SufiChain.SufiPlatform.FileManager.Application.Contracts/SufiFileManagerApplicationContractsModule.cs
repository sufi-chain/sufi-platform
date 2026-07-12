using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;
using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;

namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(SufiFileManagerDomainSharedModule),
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAuthorizationModule)
    )]
public class SufiFileManagerApplicationContractsModule : AbpModule
{

}