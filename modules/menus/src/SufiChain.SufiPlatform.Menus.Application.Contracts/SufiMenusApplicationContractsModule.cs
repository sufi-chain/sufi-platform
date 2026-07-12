using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Menus;

[DependsOn(typeof(SufiMenusDomainSharedModule), typeof(SufiDddApplicationContractsModule), typeof(SufiAuthorizationModule))]
public class SufiMenusApplicationContractsModule : AbpModule
{
}