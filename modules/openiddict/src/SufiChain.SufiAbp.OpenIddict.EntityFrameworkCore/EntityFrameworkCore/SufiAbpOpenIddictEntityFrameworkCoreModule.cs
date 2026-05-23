using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.OpenIddict.Applications;
using SufiChain.SufiAbp.OpenIddict.Authorizations;
using SufiChain.SufiAbp.OpenIddict.Scopes;
using SufiChain.SufiAbp.OpenIddict.Tokens;
using SufiChain.SufiAbp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpOpenIddictDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
)]
public class SufiAbpOpenIddictEntityFrameworkCoreModule : AbpModule
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
