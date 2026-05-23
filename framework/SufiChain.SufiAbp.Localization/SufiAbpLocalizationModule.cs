using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Localization;

[DependsOn(
    typeof(AbpLocalizationModule)
)]
public class SufiAbpLocalizationModule : AbpModule
{
}
