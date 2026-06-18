using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(SufiAbpAIManagementApplicationContractsModule),
    typeof(SufiAbpAIManagementApplicationModule),
    typeof(SufiAbpAspNetCoreMvcModule)
)]
public class SufiAbpAIManagementHttpApiModule : AbpModule
{
}
