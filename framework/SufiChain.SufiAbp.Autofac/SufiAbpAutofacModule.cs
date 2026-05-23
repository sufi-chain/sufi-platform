using Volo.Abp.Modularity;
using Volo.Abp.Autofac;

namespace SufiChain.SufiAbp.Autofac;

[DependsOn(typeof(AbpAutofacModule))]
public class SufiAbpAutofacModule : AbpModule
{
}
