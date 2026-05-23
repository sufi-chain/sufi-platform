using Volo.Abp;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.TenantManagement;

namespace SufiChain.SufiAbp.TenantManagement.MongoDB;

public static class SufiAbpTenantManagementMongoDbContextExtensions
{
    public static void ConfigureSufiAbpTenantManagement(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Tenant>(b =>
        {
            b.CollectionName = SufiAbpTenantManagementDbProperties.DbTablePrefix + "Tenants";
        });
    }
}
