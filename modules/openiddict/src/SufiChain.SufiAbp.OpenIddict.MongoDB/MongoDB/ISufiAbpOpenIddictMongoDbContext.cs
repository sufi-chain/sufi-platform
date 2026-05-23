using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiAbp.OpenIddict.Applications;
using SufiChain.SufiAbp.OpenIddict.Authorizations;
using SufiChain.SufiAbp.OpenIddict.Scopes;
using SufiChain.SufiAbp.OpenIddict.Tokens;

namespace SufiChain.SufiAbp.OpenIddict.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpOpenIddictDbProperties.ConnectionStringName)]
public interface ISufiAbpOpenIddictMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<OpenIddictApplication> Applications { get; }
    IMongoCollection<OpenIddictAuthorization> Authorizations { get; }
    IMongoCollection<OpenIddictScope> Scopes { get; }
    IMongoCollection<OpenIddictToken> Tokens { get; }
}
