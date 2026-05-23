using SufiChain.SufiAbp.Localization;
using SufiChain.SufiAbp.Validation;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiAbp.ObjectExtending;

/// <summary>
/// Thin wrapper around Volo.Abp.ObjectExtending.
/// This allows SufiAbp modules to depend only on SufiAbp packages, not directly on ABP.
/// </summary>
[DependsOn(
    typeof(AbpObjectExtendingModule),
    typeof(SufiAbpValidationModule),
    typeof(SufiAbpLocalizationModule)
)]
public class SufiAbpObjectExtendingModule : AbpModule
{
}
