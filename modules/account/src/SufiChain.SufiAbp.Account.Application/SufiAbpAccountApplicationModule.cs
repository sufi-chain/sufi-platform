using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Account;

[DependsOn(
    typeof(SufiAbpAccountApplicationContractsModule)
)]
public class SufiAbpAccountApplicationModule : AbpModule
{
}
