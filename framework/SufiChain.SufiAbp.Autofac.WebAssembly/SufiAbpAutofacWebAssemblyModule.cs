using Volo.Abp.Modularity;
using Volo.Abp.Autofac.WebAssembly;

namespace SufiChain.SufiAbp.Autofac.WebAssembly;

[DependsOn(typeof(AbpAutofacWebAssemblyModule))]
public class SufiAbpAutofacWebAssemblyModule : AbpModule
{
}
