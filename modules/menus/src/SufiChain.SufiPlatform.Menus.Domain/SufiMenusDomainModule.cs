using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace SufiChain.SufiPlatform.Menus;

[DependsOn(typeof(AbpDddDomainModule), typeof(SufiMenusDomainSharedModule))]
public class SufiMenusDomainModule : AbpModule
{
}