using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Caching;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.EventBus;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Core;

[DependsOn(
    typeof(AbpCachingModule),
    typeof(AbpDataModule),
    typeof(AbpEventBusModule),
    typeof(AbpLocalizationModule),
    typeof(AbpVirtualFileSystemModule)
)]
public class SufiModule : AbpModule
{
    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton(typeof(DistributedCache<,>));
        context.Services.Replace(
            ServiceDescriptor.Singleton(typeof(Volo.Abp.Caching.IDistributedCache<,>), typeof(RevocableDistributedCache<,>)));
    }
}
