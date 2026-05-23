using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.PermissionManagement.MongoDB;

[ConnectionStringName(SufiAbpPermissionManagementDbProperties.ConnectionStringName)]
public interface IPermissionManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<PermissionGroupDefinitionRecord> PermissionGroups { get; }

    IMongoCollection<PermissionDefinitionRecord> Permissions { get; }

    IMongoCollection<PermissionGrant> PermissionGrants { get; }

    IMongoCollection<ResourcePermissionGrant> ResourcePermissionGrants { get; }
}
