using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Core;

public class SufiAbpModule : AbpModule
{
    public virtual void PreConfigureServices(ServiceConfigurationContext context)
    {
    }

    public override void PreConfigureServices(Volo.Abp.Modularity.ServiceConfigurationContext context)
    {
        PreConfigureServices(new ServiceConfigurationContext(context));
    }

    public virtual void ConfigureServices(ServiceConfigurationContext context)
    {
    }

    public override void ConfigureServices(Volo.Abp.Modularity.ServiceConfigurationContext context)
    {
        ConfigureServices(new ServiceConfigurationContext(context));
    }

    public virtual void OnApplicationInitialization(ApplicationInitializationContext context)
    {
    }

    public override void OnApplicationInitialization(Volo.Abp.ApplicationInitializationContext context)
    {
        OnApplicationInitialization(new ApplicationInitializationContext(context));
    }
}
