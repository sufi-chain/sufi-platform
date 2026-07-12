using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.OpenIddict.Applications;
using SufiChain.SufiPlatform.OpenIddict.Authorizations;
using SufiChain.SufiPlatform.OpenIddict.Scopes;
using SufiChain.SufiPlatform.OpenIddict.Tokens;
using Volo.Abp;

namespace SufiChain.SufiPlatform.OpenIddict.MongoDB;

public static class OpenIddictMongoDbContextExtensions
{
    public static void ConfigureOpenIddict(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        
        builder.Entity<OpenIddictApplication>(b =>
        {
            b.CollectionName = SufiOpenIddictDbProperties.DbTablePrefix + "Applications";
        });
        
        builder.Entity<OpenIddictAuthorization>(b =>
        {
            b.CollectionName = SufiOpenIddictDbProperties.DbTablePrefix + "Authorizations";
        });
        
        builder.Entity<OpenIddictScope>(b =>
        {
            b.CollectionName = SufiOpenIddictDbProperties.DbTablePrefix + "Scopes";
        });
        
        builder.Entity<OpenIddictToken>(b =>
        {
            b.CollectionName = SufiOpenIddictDbProperties.DbTablePrefix + "Tokens";
        });
    }
}
