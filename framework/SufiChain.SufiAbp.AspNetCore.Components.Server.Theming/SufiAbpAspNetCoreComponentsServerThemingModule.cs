using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Components.Server.Theming;

namespace SufiChain.SufiAbp.AspNetCore.Components.Server.Theming;

[DependsOn(typeof(AbpAspNetCoreComponentsServerThemingModule))]
public class SufiAbpAspNetCoreComponentsServerThemingModule : AbpModule
{
}
