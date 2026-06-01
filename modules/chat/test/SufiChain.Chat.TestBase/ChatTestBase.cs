using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace SufiChain.Chat;

public abstract class ChatTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }
}
