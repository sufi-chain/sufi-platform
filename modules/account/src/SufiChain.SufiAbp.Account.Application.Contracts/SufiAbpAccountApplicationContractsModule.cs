using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Account;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpAccountDomainSharedModule)
)]
public class SufiAbpAccountApplicationContractsModule : AbpModule
{
}
