using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MenuManagement;

[DependsOn(typeof(SufiAbpDddDomainModule), typeof(SufiAbpMenuManagementDomainSharedModule))]
public class SufiAbpMenuManagementDomainModule : AbpModule
{
}
