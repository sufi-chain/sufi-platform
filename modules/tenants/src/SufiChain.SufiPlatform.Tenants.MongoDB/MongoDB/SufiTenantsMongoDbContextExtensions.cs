using Volo.Abp;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.Tenants;

namespace SufiChain.SufiPlatform.Tenants.MongoDB;

public static class SufiTenantsMongoDbContextExtensions
{
    public static void ConfigureSufiTenants(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Tenant>(b =>
        {
            b.CollectionName = SufiTenantsDbProperties.DbTablePrefix + "Tenants";
        });
    }
}
