using SufiChain.SufiAbp.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AspNetCore.Serilog;

/// <summary>
/// Thin wrapper around Volo.Abp.AspNetCore.Serilog.
/// This allows SufiAbp modules to depend only on SufiAbp packages, not directly on ABP.
/// </summary>
[DependsOn(
    typeof(AbpAspNetCoreSerilogModule),
    typeof(SufiAbpAspNetCoreModule),
    typeof(SufiAbpMultiTenancyModule)
)]
public class SufiAbpAspNetCoreSerilogModule : AbpModule
{
}
