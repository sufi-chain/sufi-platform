using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Components.Web;

namespace SufiChain.SufiAbp.AspNetCore.Components.Web;

[DependsOn(typeof(AbpAspNetCoreComponentsWebModule))]
public class SufiAbpAspNetCoreComponentsWebModule : AbpModule
{
}
