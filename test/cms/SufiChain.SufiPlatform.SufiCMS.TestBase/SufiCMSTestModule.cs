using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace SufiChain.SufiPlatform.SufiCMS;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(SufiCMSDomainModule))]
public class SufiCMSTestModule : AbpModule
{
}
