using SufiChain.SufiPlatform.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Menus;

[DependsOn(typeof(SufiMenusApplicationContractsModule), typeof(SufiAspNetCoreMvcModule))]
public class SufiMenusHttpApiModule : AbpModule
{
}