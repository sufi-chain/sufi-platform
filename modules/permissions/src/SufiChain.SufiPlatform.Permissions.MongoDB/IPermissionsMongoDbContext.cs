using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Permissions.MongoDB;

[ConnectionStringName(SufiPermissionsDbProperties.ConnectionStringName)]
public interface IPermissionsMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<PermissionGroupDefinitionRecord> PermissionGroups { get; }

    IMongoCollection<PermissionDefinitionRecord> Permissions { get; }

    IMongoCollection<PermissionGrant> PermissionGrants { get; }

    IMongoCollection<ResourcePermissionGrant> ResourcePermissionGrants { get; }
}
