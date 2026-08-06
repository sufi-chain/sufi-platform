using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Account;

[DependsOn(
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAccountDomainSharedModule)
)]
public class SufiAccountApplicationContractsModule : AbpModule
{
}
