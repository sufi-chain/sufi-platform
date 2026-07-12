using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace SufiChain.SufiAbp.MenuManagement;

[DependsOn(typeof(AbpDddDomainModule), typeof(SufiAbpMenuManagementDomainSharedModule))]
public class SufiAbpMenuManagementDomainModule : AbpModule
{
}
