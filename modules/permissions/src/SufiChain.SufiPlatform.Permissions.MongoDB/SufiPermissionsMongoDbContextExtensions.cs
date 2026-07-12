using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Permissions.MongoDB;

public static class SufiPermissionsMongoDbContextExtensions
{
    public static void ConfigurePermissions(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<PermissionGroupDefinitionRecord>(b =>
        {
            b.CollectionName = SufiPermissionsDbProperties.DbTablePrefix + "PermissionGroups";
        });

        builder.Entity<PermissionDefinitionRecord>(b =>
        {
            b.CollectionName = SufiPermissionsDbProperties.DbTablePrefix + "Permissions";
        });

        builder.Entity<PermissionGrant>(b =>
        {
            b.CollectionName = SufiPermissionsDbProperties.DbTablePrefix + "PermissionGrants";
        });

        builder.Entity<ResourcePermissionGrant>(b =>
        {
            b.CollectionName = SufiPermissionsDbProperties.DbTablePrefix + "ResourcePermissionGrants";
        });
    }
}
