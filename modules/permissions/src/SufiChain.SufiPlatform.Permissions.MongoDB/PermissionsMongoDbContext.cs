using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Permissions.MongoDB;

[ConnectionStringName(SufiPermissionsDbProperties.ConnectionStringName)]
public class PermissionsMongoDbContext : AbpMongoDbContext, IPermissionsMongoDbContext
{
    public IMongoCollection<PermissionGroupDefinitionRecord> PermissionGroups => Collection<PermissionGroupDefinitionRecord>();
    public IMongoCollection<PermissionDefinitionRecord> Permissions => Collection<PermissionDefinitionRecord>();
    public IMongoCollection<PermissionGrant> PermissionGrants => Collection<PermissionGrant>();
    public IMongoCollection<ResourcePermissionGrant> ResourcePermissionGrants => Collection<ResourcePermissionGrant>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigurePermissions();
    }
}
