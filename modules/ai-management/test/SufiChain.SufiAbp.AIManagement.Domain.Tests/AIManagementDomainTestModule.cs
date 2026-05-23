using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(AIManagementTestBaseModule),
    typeof(SufiAbpAIManagementDomainModule)
)]
public class AIManagementDomainTestModule : AbpModule
{
}
