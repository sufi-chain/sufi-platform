using Volo.Abp.AspNetCore.Mvc.Client;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AspNetCore.Mvc.Client;

[DependsOn(typeof(AbpAspNetCoreMvcClientModule))]
public class SufiAbpAspNetCoreMvcClientModule : AbpModule
{
}
