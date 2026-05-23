using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AspNetCore.Mvc;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule)
)]
public class SufiAbpAspNetCoreMvcModule : AbpModule
{
}
