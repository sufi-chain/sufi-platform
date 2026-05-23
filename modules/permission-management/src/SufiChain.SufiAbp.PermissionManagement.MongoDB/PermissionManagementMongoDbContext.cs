using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.PermissionManagement.MongoDB;

[ConnectionStringName(SufiAbpPermissionManagementDbProperties.ConnectionStringName)]
public class PermissionManagementMongoDbContext : AbpMongoDbContext, IPermissionManagementMongoDbContext
{
    public IMongoCollection<PermissionGroupDefinitionRecord> PermissionGroups => Collection<PermissionGroupDefinitionRecord>();
    public IMongoCollection<PermissionDefinitionRecord> Permissions => Collection<PermissionDefinitionRecord>();
    public IMongoCollection<PermissionGrant> PermissionGrants => Collection<PermissionGrant>();
    public IMongoCollection<ResourcePermissionGrant> ResourcePermissionGrants => Collection<ResourcePermissionGrant>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigurePermissionManagement();
    }
}
