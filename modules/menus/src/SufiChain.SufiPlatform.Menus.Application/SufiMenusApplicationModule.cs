using Volo.Abp.Modularity;

using Volo.Abp.Mapperly;
namespace SufiChain.SufiPlatform.Menus;

[DependsOn(typeof(SufiMenusDomainModule), typeof(SufiMenusApplicationContractsModule), typeof(AbpMapperlyModule))]
public class SufiMenusApplicationModule : AbpModule
{
}