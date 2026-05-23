using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Http.Client;

[DependsOn(
    typeof(AbpHttpClientModule)
)]
public class SufiAbpHttpClientModule : AbpModule
{
}
