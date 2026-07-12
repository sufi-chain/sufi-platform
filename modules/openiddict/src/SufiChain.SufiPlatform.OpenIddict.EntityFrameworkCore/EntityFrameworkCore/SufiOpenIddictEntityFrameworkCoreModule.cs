using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.OpenIddict.Applications;
using SufiChain.SufiPlatform.OpenIddict.Authorizations;
using SufiChain.SufiPlatform.OpenIddict.Scopes;
using SufiChain.SufiPlatform.OpenIddict.Tokens;

namespace SufiChain.SufiPlatform.OpenIddict.EntityFrameworkCore;

[DependsOn(
    typeof(SufiOpenIddictDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiOpenIddictEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<OpenIddictDbContext>(options =>
        {
            options.AddDefaultRepositories<IOpenIddictDbContext>();

            options.AddRepository<OpenIddictApplication, EfCoreOpenIddictApplicationRepository>();
            options.AddRepository<OpenIddictAuthorization, EfCoreOpenIddictAuthorizationRepository>();
            options.AddRepository<OpenIddictScope, EfCoreOpenIddictScopeRepository>();
            options.AddRepository<OpenIddictToken, EfCoreOpenIddictTokenRepository>();
        });

        Configure<AbpEntityChangeOptions>(options =>
        {
            options.IgnoredNavigationEntitySelectors.Add("DisableOpenIddictApplication", type => type == typeof(OpenIddictApplication));
        });
    }
}
