using Volo.Abp.Modularity;

using Volo.Abp.Http.Client;
namespace SufiChain.SufiAbp.MenuManagement;

[DependsOn(typeof(SufiAbpMenuManagementApplicationContractsModule), typeof(AbpHttpClientModule))]
public class SufiAbpMenuManagementHttpApiClientModule : AbpModule
{
}
