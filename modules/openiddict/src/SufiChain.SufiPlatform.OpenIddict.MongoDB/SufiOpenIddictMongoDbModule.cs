using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.OpenIddict.Applications;
using SufiChain.SufiPlatform.OpenIddict.Authorizations;
using SufiChain.SufiPlatform.OpenIddict.Scopes;
using SufiChain.SufiPlatform.OpenIddict.Tokens;

namespace SufiChain.SufiPlatform.OpenIddict.MongoDB;

[DependsOn(
    typeof(SufiOpenIddictDomainModule),
    typeof(AbpMongoDbModule)
    )]
public class SufiOpenIddictMongoDbModule : AbpModule
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
