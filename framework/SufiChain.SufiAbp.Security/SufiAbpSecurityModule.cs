using Volo.Abp.Modularity;
using Volo.Abp.Security;

namespace SufiChain.SufiAbp.Security;

[DependsOn(typeof(AbpSecurityModule))]
public class SufiAbpSecurityModule : AbpModule
{
}
