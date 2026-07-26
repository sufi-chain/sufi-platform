using SufiChain.SufiPlatform.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Editions;

[DependsOn(
    typeof(SufiEditionsApplicationContractsModule),
    typeof(SufiAspNetCoreMvcModule)
)]
public class SufiEditionsHttpApiModule : AbpModule
{
}
