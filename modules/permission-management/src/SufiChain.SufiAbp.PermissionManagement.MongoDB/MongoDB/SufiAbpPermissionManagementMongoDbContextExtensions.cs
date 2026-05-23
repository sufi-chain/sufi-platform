using Volo.Abp;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.PermissionManagement;

namespace SufiChain.SufiAbp.PermissionManagement.MongoDB;

public static class SufiAbpPermissionManagementMongoDbContextExtensions
{
    public static void ConfigureSufiAbpPermissionManagement(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<PermissionGroupDefinitionRecord>(b =>
        {
            b.CollectionName = SufiAbpPermissionManagementDbProperties.DbTablePrefix + "PermissionGroups";
        });

        builder.Entity<PermissionDefinitionRecord>(b =>
        {
            b.CollectionName = SufiAbpPermissionManagementDbProperties.DbTablePrefix + "Permissions";
        });

        builder.Entity<PermissionGrant>(b =>
        {
            b.CollectionName = SufiAbpPermissionManagementDbProperties.DbTablePrefix + "PermissionGrants";
        });

        builder.Entity<ResourcePermissionGrant>(b =>
        {
            b.CollectionName = SufiAbpPermissionManagementDbProperties.DbTablePrefix + "ResourcePermissionGrants";
        });
    }
}
