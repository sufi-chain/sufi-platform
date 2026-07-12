using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.EventBus;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
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
}
