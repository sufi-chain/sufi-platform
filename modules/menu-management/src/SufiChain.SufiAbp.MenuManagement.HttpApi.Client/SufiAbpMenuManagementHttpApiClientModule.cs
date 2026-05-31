using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.MenuManagement;

[DependsOn(typeof(SufiAbpMenuManagementApplicationContractsModule), typeof(SufiAbpHttpClientModule))]
public class SufiAbpMenuManagementHttpApiClientModule : AbpModule
{
}
