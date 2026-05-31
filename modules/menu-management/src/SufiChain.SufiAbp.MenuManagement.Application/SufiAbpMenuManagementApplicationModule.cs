using SufiChain.SufiAbp.Mapperly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MenuManagement;

[DependsOn(typeof(SufiAbpMenuManagementDomainModule), typeof(SufiAbpMenuManagementApplicationContractsModule), typeof(SufiAbpMapperlyModule))]
public class SufiAbpMenuManagementApplicationModule : AbpModule
{
}
