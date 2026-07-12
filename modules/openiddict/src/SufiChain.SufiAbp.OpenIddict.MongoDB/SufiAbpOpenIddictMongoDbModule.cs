using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.OpenIddict.Applications;
using SufiChain.SufiAbp.OpenIddict.Authorizations;
using SufiChain.SufiAbp.OpenIddict.Scopes;
using SufiChain.SufiAbp.OpenIddict.Tokens;

namespace SufiChain.SufiAbp.OpenIddict.MongoDB;

[DependsOn(
    typeof(SufiAbpOpenIddictDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiAbpOpenIddictMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<OpenIddictMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<IOpenIddictMongoDbContext>();

            options.AddRepository<OpenIddictApplication, MongoOpenIddictApplicationRepository>();
            options.AddRepository<OpenIddictAuthorization, MongoOpenIddictAuthorizationRepository>();
            options.AddRepository<OpenIddictScope, MongoOpenIddictScopeRepository>();
            options.AddRepository<OpenIddictToken, MongoOpenIddictTokenRepository>();
        });
    }
}
