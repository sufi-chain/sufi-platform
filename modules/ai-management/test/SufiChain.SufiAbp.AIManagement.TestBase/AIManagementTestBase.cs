using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace SufiChain.SufiAbp.AIManagement;

public abstract class AIManagementTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }
}
