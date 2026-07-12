using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.OpenIddict;

[DependsOn(
    typeof(SufiOpenIddictDomainSharedModule)
)]
public class SufiOpenIddictDomainModule : AbpModule
{
}
