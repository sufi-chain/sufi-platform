using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.OpenIddict.Applications;
using SufiChain.SufiPlatform.OpenIddict.Authorizations;
using SufiChain.SufiPlatform.OpenIddict.Scopes;
using SufiChain.SufiPlatform.OpenIddict.Tokens;

namespace SufiChain.SufiPlatform.OpenIddict.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiOpenIddictDbProperties.ConnectionStringName)]
public interface ISufiOpenIddictMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<OpenIddictApplication> Applications { get; }
    IMongoCollection<OpenIddictAuthorization> Authorizations { get; }
    IMongoCollection<OpenIddictScope> Scopes { get; }
    IMongoCollection<OpenIddictToken> Tokens { get; }
}
