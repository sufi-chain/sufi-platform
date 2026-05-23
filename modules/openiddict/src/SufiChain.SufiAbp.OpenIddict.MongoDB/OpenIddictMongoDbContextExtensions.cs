using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.OpenIddict.Applications;
using SufiChain.SufiAbp.OpenIddict.Authorizations;
using SufiChain.SufiAbp.OpenIddict.Scopes;
using SufiChain.SufiAbp.OpenIddict.Tokens;
using Volo.Abp;

namespace SufiChain.SufiAbp.OpenIddict.MongoDB;

public static class OpenIddictMongoDbContextExtensions
{
    public static void ConfigureOpenIddict(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        
        builder.Entity<OpenIddictApplication>(b =>
        {
            b.CollectionName = SufiAbpOpenIddictDbProperties.DbTablePrefix + "Applications";
        });
        
        builder.Entity<OpenIddictAuthorization>(b =>
        {
            b.CollectionName = SufiAbpOpenIddictDbProperties.DbTablePrefix + "Authorizations";
        });
        
        builder.Entity<OpenIddictScope>(b =>
        {
            b.CollectionName = SufiAbpOpenIddictDbProperties.DbTablePrefix + "Scopes";
        });
        
        builder.Entity<OpenIddictToken>(b =>
        {
            b.CollectionName = SufiAbpOpenIddictDbProperties.DbTablePrefix + "Tokens";
        });
    }
}
