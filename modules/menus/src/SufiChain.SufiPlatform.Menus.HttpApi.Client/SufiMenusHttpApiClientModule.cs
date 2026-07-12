using Volo.Abp.Modularity;

using Volo.Abp.Http.Client;
namespace SufiChain.SufiPlatform.Menus;

[DependsOn(typeof(SufiMenusApplicationContractsModule), typeof(AbpHttpClientModule))]
public class SufiMenusHttpApiClientModule : AbpModule
{
}