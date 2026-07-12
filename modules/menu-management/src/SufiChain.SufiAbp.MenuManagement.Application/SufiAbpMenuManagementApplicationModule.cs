using Volo.Abp.Modularity;

using Volo.Abp.Mapperly;
namespace SufiChain.SufiAbp.MenuManagement;

[DependsOn(typeof(SufiAbpMenuManagementDomainModule), typeof(SufiAbpMenuManagementApplicationContractsModule), typeof(AbpMapperlyModule))]
public class SufiAbpMenuManagementApplicationModule : AbpModule
{
}
