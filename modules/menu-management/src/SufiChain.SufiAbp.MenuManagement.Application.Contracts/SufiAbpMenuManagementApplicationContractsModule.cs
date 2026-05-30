using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MenuManagement;

[DependsOn(typeof(SufiAbpMenuManagementDomainSharedModule), typeof(SufiAbpDddApplicationContractsModule), typeof(SufiAbpAuthorizationModule))]
public class SufiAbpMenuManagementApplicationContractsModule : AbpModule
{
}
