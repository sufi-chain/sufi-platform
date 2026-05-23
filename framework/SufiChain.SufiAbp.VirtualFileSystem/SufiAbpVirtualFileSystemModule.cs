using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.VirtualFileSystem;

[DependsOn(typeof(AbpVirtualFileSystemModule))]
public class SufiAbpVirtualFileSystemModule : AbpModule
{
}
