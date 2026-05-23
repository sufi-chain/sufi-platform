using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.OpenIddict;

[DependsOn(
    typeof(SufiAbpOpenIddictDomainSharedModule)
)]
public class SufiAbpOpenIddictDomainModule : AbpModule
{
}
