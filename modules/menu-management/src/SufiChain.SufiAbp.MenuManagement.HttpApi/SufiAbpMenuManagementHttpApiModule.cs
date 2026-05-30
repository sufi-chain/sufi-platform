using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MenuManagement;

[DependsOn(typeof(SufiAbpMenuManagementApplicationContractsModule), typeof(SufiAbpAspNetCoreMvcModule))]
public class SufiAbpMenuManagementHttpApiModule : AbpModule
{
}
