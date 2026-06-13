using Volo.Abp.Modularity;

using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.EventBus;
using Volo.Abp.Localization;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.Core;

[DependsOn(
    typeof(AbpCachingModule),
    typeof(AbpDataModule),
    typeof(AbpEventBusModule),
    typeof(AbpLocalizationModule),
    typeof(AbpVirtualFileSystemModule)
)]
public class SufiAbpModule : AbpModule
{
    public virtual void PreConfigureServices(SufiChain.SufiAbp.Modularity.ServiceConfigurationContext context)
    {
    }

    public override void PreConfigureServices(Volo.Abp.Modularity.ServiceConfigurationContext context)
    {
        PreConfigureServices(new SufiChain.SufiAbp.Modularity.ServiceConfigurationContext(context));
    }

    public virtual void ConfigureServices(SufiChain.SufiAbp.Modularity.ServiceConfigurationContext context)
    {
    }

    public override void ConfigureServices(Volo.Abp.Modularity.ServiceConfigurationContext context)
    {
        ConfigureServices(new SufiChain.SufiAbp.Modularity.ServiceConfigurationContext(context));
    }

    public virtual void OnApplicationInitialization(ApplicationInitializationContext context)
    {
    }

    public override void OnApplicationInitialization(Volo.Abp.ApplicationInitializationContext context)
    {
        OnApplicationInitialization(new ApplicationInitializationContext(context));
    }
}
