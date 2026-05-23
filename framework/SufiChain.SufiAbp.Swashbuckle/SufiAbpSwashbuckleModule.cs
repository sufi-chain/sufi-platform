using SufiChain.SufiAbp.AspNetCore.Mvc;
using SufiChain.SufiAbp.VirtualFileSystem;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace SufiChain.SufiAbp.Swashbuckle;

/// <summary>
/// Thin wrapper around Volo.Abp.Swashbuckle.
/// This allows SufiAbp modules to depend only on SufiAbp packages, not directly on ABP.
/// </summary>
[DependsOn(
    typeof(AbpSwashbuckleModule),
    typeof(SufiAbpAspNetCoreMvcModule),
    typeof(SufiAbpVirtualFileSystemModule)
)]
public class SufiAbpSwashbuckleModule : AbpModule
{
}
