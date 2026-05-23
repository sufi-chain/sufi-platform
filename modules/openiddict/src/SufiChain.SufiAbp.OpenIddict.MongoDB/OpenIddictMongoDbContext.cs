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
public class OpenIddictMongoDbContext : AbpMongoDbContext, IOpenIddictMongoDbContext
{
    public IMongoCollection<OpenIddictApplication> Applications => Collection<OpenIddictApplication>();

    public IMongoCollection<OpenIddictAuthorization> Authorizations => Collection<OpenIddictAuthorization>();

    public IMongoCollection<OpenIddictScope> Scopes => Collection<OpenIddictScope>();

    public IMongoCollection<OpenIddictToken> Tokens => Collection<OpenIddictToken>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureOpenIddict();
    }
}
